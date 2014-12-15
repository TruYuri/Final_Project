using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;

namespace FinalProject
{
    public enum PlayerState { Alive, WeaponFired, CrashedGround, CrashedVehicle, Killed, Respawn, Left }
    class Player
    {
        public PlayerState status;
        public string collider;
        public string weaponType;
        public float health;
        public float  shield;
        public float healthMax;
        public float shieldMax;
        public bool alive;
        public string name;
        public Vector3 Position;
        public Vector3 Forward;
        public Vector3 Velocity;
        public int lives;
        public float respawnTimer;

        Camera camera; // all movement
        Map map;
        GameObject gameObject;
        bool localPlayer;
        float timeToNextFire;
        float shieldRechargeRate;
        float shieldRechargeDelay;
        BasicModel model;
        List<string> availableWeapons;

        float weaponChangeTime;
        int weaponIndex;
        int prevMouseWheel;

        public Player(Game game, Camera c, Map t, bool local, string n)
        {
            health = healthMax = 1000.0f;
            shield = shieldMax = 1000.0f;
            shieldRechargeRate = 0.10f;
            name = n;
            camera = c;
            map = t;
            localPlayer = local;
            alive = true;
            status = PlayerState.Alive;
            weaponType = "bullet";
            timeToNextFire = Projectile.definitions[weaponType].fireTime;
            lives = 5;
            availableWeapons = new List<string>() { "bullet", "rocket" };
            weaponChangeTime = 0.0f;
            prevMouseWheel = Mouse.GetState().ScrollWheelValue;
        }

        public void Initialize()
        {
            if (localPlayer)
            {
                camera.Update(new GameTime());
                Position = camera.cameraPosition;
                Forward = -Matrix.Transpose(camera.view).Forward;
                gameObject = new GameObject(new BasicModel(Game1.ContentManager.Load<Model>("spaceship"), new Vector3(0, 600, 0)), false, "vehicle", name);
            }
            else
                gameObject = new GameObject(new BasicModel(Game1.ContentManager.Load<Model>("spaceship"), new Vector3(0, 600, 0)), true, "vehicle", name);
            gameObject.world = Matrix.CreateWorld(Position, Forward, Vector3.Up);
            alive = true;
            health = healthMax;
            shield = healthMax;
        }

        public void Update(GameTime gameTime)
        {
            var mState = Mouse.GetState();
            var kState = Keyboard.GetState();
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            timeToNextFire += time;
            weaponChangeTime -= time;
            shieldRechargeDelay -= time;

            if (status == PlayerState.Respawn || status == PlayerState.WeaponFired)
                status = PlayerState.Alive;

            if (localPlayer)
            {
                if (!alive)
                {
                    respawnTimer -= time;
                    if(respawnTimer <= 0.0f)
                    {
                        var pos = map.CreateRandomSpawnAtHeight(600);
                        var c = map.Center(600);
                        camera.PlaceCamera(map.CreateRandomSpawnAtHeight(600), c - pos, Vector3.Up);
                        Initialize();
                        status = PlayerState.Respawn;
                    }
                }
                else
                {
                    camera.Update(gameTime);

                    bool hit = false;
                    if (gameObject != null)
                    {
                        var colliders = GameObjectManager.Instance.CheckCollision(gameObject);
                        this.collider = null;

                        foreach (var collider in colliders)
                        {
                            switch (collider.type)
                            {
                                case "vehicle":
                                    this.collider = collider.owner;
                                    Kill(5.0f, PlayerState.CrashedVehicle);
                                    break;
                                case "bullet":
                                case "rocket":
                                    hit = true;
                                    shieldRechargeDelay = 5.0f;
                                    if (collider.owner != name)
                                    {
                                        this.collider = collider.owner;
                                        var def = Projectile.definitions[collider.type];
                                        GameObjectManager.Instance.Delete(collider);

                                        if (shield > 0.0f)
                                            shield = Math.Max(0.0f, shield - def.damage);
                                        else // damage health
                                            health = Math.Max(0.0f, health - def.damage);

                                        if(health <= 0.0f)
                                            Kill(5.0f, PlayerState.Killed);
                                    }
                                    break;
                            }
                        }
                    }

                    if(!hit && shieldRechargeDelay <= 0.0f)
                    {
                        shield = Math.Min(shieldMax, shield + shieldRechargeRate * shieldMax * time);
                    }

                    Position = camera.cameraPosition;
                    Forward = -Matrix.Transpose(camera.view).Forward; // camera.target;
                    Velocity = camera.velocity;

                    float? height = null;
                    foreach (var t in map.terrainPieces)
                    {
                        height = t.Intersects(new Ray(Position, Vector3.Down));

                        if (height != null)
                            break;
                    }

                    if (height == null)
                    {
                        // out of bounds checking

                    }
                    else if (height - 25.0 < 0.0f)
                    {
                        this.collider = "the ground";
                        Kill(5.0f, PlayerState.CrashedGround);
                    }

                    if (alive)
                    {
                        var def = Projectile.definitions[weaponType];

                        if(weaponChangeTime <= 0.0f)
                        {
                            if(mState.ScrollWheelValue < prevMouseWheel || kState.IsKeyDown(Keys.Q))
                            {
                                weaponChangeTime = 0.5f;
                                weaponIndex = (weaponIndex + 1) % availableWeapons.Count();
                                timeToNextFire = 0.0f;
                            }
                            else if(mState.ScrollWheelValue > prevMouseWheel || kState.IsKeyDown(Keys.E))
                            {
                                weaponChangeTime = 0.0f;
                                weaponIndex = (weaponIndex - 1 < 0 ? availableWeapons.Count - 1 : weaponIndex - 1);
                            }

                            weaponType = availableWeapons[weaponIndex];
                        }

                        if ((mState.LeftButton == ButtonState.Pressed || kState.IsKeyDown(Keys.Space)) && def.fireTime - timeToNextFire <= 0.0f)
                        {
                            timeToNextFire = 0.0f;
                            var transpose = Matrix.Transpose(camera.view);
                            var projectile = new Projectile(Position, Forward, Velocity, weaponType, name);
                            status = PlayerState.WeaponFired;
                        }
                    }

                    if (gameObject != null)
                        gameObject.world = Matrix.CreateWorld(Position, Forward, Vector3.Up);
                }
            }
            else
            {
                if (alive)
                {
                    var matrix = Matrix.CreateWorld(Position, Forward, Vector3.Up);
                    if (gameObject != null)
                    {
                        gameObject.world = matrix;
                    }

                    // note: mostly need this to do sounds on both sides
                    var colliders = GameObjectManager.Instance.CheckCollision(gameObject);
                    foreach (var collider in colliders)
                    {
                        switch (collider.type)
                        {
                            case "vehicle":
                                // play crash sound
                                break;
                            case "bullet":
                            case "rocket":
                                // play bullet hit metal sound
                                if (collider.owner != name)
                                {
                                    this.collider = collider.owner;
                                    var def = Projectile.definitions[collider.type];
                                    GameObjectManager.Instance.Delete(collider);
                                }
                                break;
                        }
                    }
                }

                if (alive)
                    status = PlayerState.Alive;
                else
                    status = PlayerState.Killed;
            }
        }

        public void FireWeapon(string weapon)
        {
            weaponType = weapon;
            var matrix = Matrix.CreateWorld(Position, Forward, Vector3.Up);
            var def = Projectile.definitions[weaponType];
            var projectile = new Projectile(Position, matrix.Forward, Velocity, weaponType, name);
        }

        public void Kill(float respawn, PlayerState reason)
        {
            if (!alive)
                return;

            status = reason;
            respawnTimer = respawn;
            Delete();
            alive = false;
            health = 0.0f;
            shield = 0.0f;
        }

        private void Delete()
        {
            GameObjectManager.Instance.Delete(gameObject);
            gameObject = null;
        }

        public void Draw()
        {
            if (localPlayer)
            {
                // draw weapon
            }
            else
            {
            }
        }
    }
}