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
    public enum PlayerState { Alive, WeaponFired, CrashedGround, CrashedVehicle, Killed, Respawn, Left, OutOfBounds, DiedOOB }
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
        List<string> availableWeapons;
        Vector3 forward;

        float weaponChangeTime;
        int weaponIndex;
        int prevMouseWheel;
        public float boundsTimer;
        bool outOfBounds;

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
            AudioManager.Instance.AddPlayer(name);
            outOfBounds = false;
        }

        public void Initialize()
        {
            if (lives <= 0)
                return;

            if (localPlayer)
            {
                camera.Update(new GameTime());
                Position = camera.cameraPosition;
                Velocity = camera.velocity;
                forward = Velocity;
                forward.Normalize();
                gameObject = new GameObject(new BasicModel(Game1.ContentManager.Load<Model>("spaceship"), new Vector3(0, 0, 0)), false, "vehicle", name);
            }
            else
            {
                gameObject = new GameObject(new BasicModel(Game1.ContentManager.Load<Model>("spaceship"), new Vector3(0, 0, 0)), true, "vehicle", name);
                forward = Vector3.Forward;
            }
            gameObject.world = Matrix.CreateWorld(Position, forward, Vector3.Up);
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
            boundsTimer -= time;

            if (status == PlayerState.Respawn || status == PlayerState.WeaponFired)
                status = PlayerState.Alive;

            if (localPlayer && lives > 0)
            {
                if (!alive)
                {
                    respawnTimer -= time;
                    if(respawnTimer <= 0.0f)
                    {
                        var pos = map.CreateRandomSpawnAtHeight(600, new Random());
                        var c = map.Center(600);
                        camera.PlaceCamera(pos, c - pos, Vector3.Up);
                        Initialize();
                        status = PlayerState.Respawn;
                    }
                }
                else
                {
                    camera.Update(gameTime);
                    //AudioManager.Instance.Play("engine", name, false);
                    Position = camera.cameraPosition;
                    forward = -Matrix.Transpose(camera.view).Forward; // camera.target;
                    Velocity = camera.velocity;
                    var def = Projectile.definitions[weaponType];
                    AudioManager.Instance.UpdatePosition(Position, Velocity, camera.view.Up, forward);

                    if (gameObject != null)
                        gameObject.world = Matrix.CreateWorld(Position, forward, Vector3.Up);

                    bool hit = false;
                    if (gameObject != null)
                    {
                        var colliders = GameObjectManager.Instance.CheckCollision(gameObject);
                        this.collider = null;

                        foreach (var collider in colliders)
                        {
                            switch (collider.type)
                            {
                                case "health_orb":
                                    health = (float)Math.Min(healthMax, health + healthMax * 0.25);
                                    GameObjectManager.Instance.Delete(collider);
                                    break;
                                case "shield_orb":
                                    shield = shieldMax * 2;
                                    GameObjectManager.Instance.Delete(collider);
                                    break;
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
                                        var def2 = Projectile.definitions[collider.type];
                                        this.collider = collider.owner;
                                        GameObjectManager.Instance.Delete(collider);
                                        AudioManager.Instance.Play(def2.hitSound, name, Position, Vector3.Zero, collider.world.Up, collider.world.Forward);

                                        if (shield > 0.0f)
                                            shield = Math.Max(0.0f, shield - def2.damage);
                                        else // damage health
                                            health = Math.Max(0.0f, health - def2.damage);

                                        if(health <= 0.0f)
                                            Kill(5.0f, PlayerState.Killed);
                                    }
                                    break;
                            }
                        }
                    }

                    if (!alive)
                        return;

                    if(!hit && shieldRechargeDelay <= 0.0f && shield < shieldMax)
                    {
                        shield = Math.Min(shieldMax, shield + shieldRechargeRate * shieldMax * time);
                    }

                    float? height = null;
                    foreach (var t in map.terrainPieces)
                    {
                        height = t.Intersects(new Ray(Position, Vector3.Down));

                        if (height != null)
                            break;
                    }

                    if (height == null)
                    {
                        if (outOfBounds)
                        {
                            if (boundsTimer <= 0.0f)
                            {
                                Kill(10.0f, PlayerState.DiedOOB);
                            }
                            else
                            {
                                status = PlayerState.OutOfBounds;
                            }
                        }
                        else
                        {
                            outOfBounds = true;
                            boundsTimer = 10.0f;
                            status = PlayerState.OutOfBounds;
                        }
                    }
                    else if (height - 25.0 < 0.0f)
                    {
                        outOfBounds = false;
                        this.collider = "the ground";
                        Kill(5.0f, PlayerState.CrashedGround);
                    }
                    else
                        outOfBounds = false;

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
                        def = Projectile.definitions[weaponType];
                    }

                    if ((mState.LeftButton == ButtonState.Pressed || kState.IsKeyDown(Keys.Space)) && def.fireTime - timeToNextFire <= 0.0f)
                    {
                        timeToNextFire = 0.0f;
                        FireWeapon(weaponType);
                    }

                    prevMouseWheel = mState.ScrollWheelValue;
                }
            }
            else
            {
                forward = -Velocity;
                if (forward == Vector3.Zero)
                    forward = Vector3.Forward;
                else
                    forward.Normalize();


                if (alive)
                {
                    var matrix = Matrix.CreateWorld(Position, forward, Vector3.Up);
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
                            case "health_orb":
                                GameObjectManager.Instance.Delete(collider);
                                break;
                            case "shield_orb":
                                GameObjectManager.Instance.Delete(collider);
                                break;
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
                                    AudioManager.Instance.Play(def.hitSound, name, Position, Vector3.Zero, collider.world.Up, collider.world.Forward);
                                    GameObjectManager.Instance.Delete(collider);
                                }
                                break;
                        }
                    }
                }

                if (alive)
                {
                    status = PlayerState.Alive;
                }
                else
                {
                    status = PlayerState.Killed;
                }
            }
        }

        public void FireWeapon(string weapon)
        {
            var def = Projectile.definitions[weaponType];
            Random rand = new Random();
            var m = Matrix.CreateWorld(Position, forward, Vector3.Up);
            weaponType = weapon;

            AudioManager.Instance.Play(def.fireSound, name, Position, Velocity, camera.view.Up, m.Forward);
            var projectile = new Projectile(Position, m.Forward, Velocity, weaponType, name);
            status = PlayerState.WeaponFired;
        }

        public void Kill(float respawn, PlayerState reason)
        {
            if (!alive)
                return;

            lives--;
            status = reason;
            respawnTimer = respawn;
            Delete();
            alive = false;
            health = 0.0f;
            shield = 0.0f;
            AudioManager.Instance.Play("explode", name, Position, Vector3.Zero, Vector3.Up, forward);
            AudioManager.Instance.Pause("engine", name);
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