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
    public enum VehicleState { Alive, WeaponFired, CrashedGround, CrashedVehicle, TookDamage, Died, Respawn }
    class Player
    {
        public VehicleState status;
        public string collider;
        public string weaponType;
        public float health;
        public int shield;
        public bool alive;
        public string name;
        public Vector3 Position;
        public Vector3 Forward;
        public Vector3 Velocity;
        public int lives;

        float respawnTimer;
        Camera camera; // all movement
        Map map;
        GameObject gameObject;
        bool localPlayer;
        float timeToNextFire;
        BasicModel model;

        public Player(Game game, Camera c, Map t, bool local, string n)
        {
            health = 1000.0f;
            name = n;
            camera = c;
            map = t;
            localPlayer = local;
            alive = true;
            status = VehicleState.Alive;
            weaponType = "projectile";
            timeToNextFire = Projectile.definitions[weaponType].fireTime;
            lives = 5;
        }

        public void Initialize()
        {
            if (localPlayer)
            {
                camera.Update(new GameTime());
                Position = camera.cameraPosition;
                Forward = camera.target;
                gameObject = new GameObject(new BasicModel(Game1.ContentManager.Load<Model>("spaceship"), new Vector3(0, 600, 0)), false, "vehicle", name);
            }
            else
                gameObject = new GameObject(new BasicModel(Game1.ContentManager.Load<Model>("spaceship"), new Vector3(0, 600, 0)), true, "vehicle", name);
            gameObject.world = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
            alive = true;
            status = VehicleState.Alive;
        }

        public void Update(GameTime gameTime)
        {
            var mState = Mouse.GetState();
            var kState = Keyboard.GetState();
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeToNextFire += time;
            if (localPlayer)
            {
                if (alive)
                    status = VehicleState.Alive;
                else
                {
                    status = VehicleState.Died;
                    respawnTimer -= time;
                    if(respawnTimer <= 0.0f)
                    {
                        status = VehicleState.Respawn;
                        camera.PlaceCamera(new Vector3(0, 600, 0), new Vector3(0, 0, 1), Vector3.Up);
                        Initialize();
                    }
                }

                if (alive)
                {
                    camera.Update(gameTime);

                    if (gameObject != null)
                    {
                        var colliders = GameObjectManager.Instance.CheckCollision(gameObject);
                        this.collider = null;
                        foreach (var collider in colliders)
                        {
                            switch (collider.type)
                            {
                                case "vehicle":
                                    status = VehicleState.CrashedVehicle;
                                    this.collider = collider.owner;
                                    alive = false;
                                    break;
                                case "projectile":
                                    if (collider.owner != name)
                                    {
                                        var def = Projectile.definitions[collider.type];
                                        health -= def.damage;
                                        health = Math.Max(health, 0.0f);
                                        GameObjectManager.Instance.Delete(collider);

                                        if (health <= 0.0f)
                                        {
                                            alive = false;
                                            status = VehicleState.Died;
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    Position = camera.cameraPosition;
                    Forward = camera.target;
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
                        status = VehicleState.CrashedGround;
                        this.collider = "the ground";
                        alive = false;
                    }

                    if (alive)
                    {
                        var def = Projectile.definitions[weaponType];

                        if ((mState.LeftButton == ButtonState.Pressed || kState.IsKeyDown(Keys.Space)) && def.fireTime - timeToNextFire <= 0.0f)
                        {
                            timeToNextFire = 0.0f;
                            var transpose = Matrix.Transpose(camera.view);
                            var projectile = new Projectile(Position, -transpose.Forward, Velocity, weaponType, name);
                            status = VehicleState.WeaponFired;
                        }
                    }

                    if(!alive)
                    {
                        respawnTimer = 5.0f;
                    }

                    if (gameObject != null)
                        gameObject.world = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
                }
                else
                {
                    status = VehicleState.Died;
                    Delete();
                }
            }
            else
            {
                if (alive)
                {
                    var matrix = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
                    if (gameObject != null)
                    {
                        gameObject.world = matrix;
                    }

                    var colliders = GameObjectManager.Instance.CheckCollision(gameObject);
                    this.collider = null;
                    foreach (var collider in colliders)
                    {
                        switch (collider.type)
                        {
                            case "vehicle":
                                status = VehicleState.CrashedVehicle;
                                this.collider = collider.owner;
                                alive = false;
                                break;
                            case "projectile":
                                if (collider.owner != name)
                                {
                                    var def = Projectile.definitions[collider.type];
                                    health -= def.damage;
                                    GameObjectManager.Instance.Delete(collider);
                                }
                                break;
                        }
                    }

                    switch (status)
                    {
                        case VehicleState.WeaponFired:
                            var def = Projectile.definitions[weaponType];
                            var projectile = new Projectile(Position, matrix.Forward, Velocity, weaponType, name);
                            break;
                    }
                }
                else
                {
                    Delete();
                }

                if (alive)
                    status = VehicleState.Alive;
                else
                    status = VehicleState.Died;
            }
        }

        public void Delete()
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