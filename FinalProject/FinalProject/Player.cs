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
    public enum VehicleState { Alive, WeaponFired, CrashedGround, CrashedVehicle, TookDamage, Died }
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

        Camera camera; // all movement
        Map map;
        GameObject gameObject;
        bool localPlayer;
        float timeToNextFire;

        public Player(Game game, Camera c, Map t, bool local, BasicModel m, string n)
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
            if(local)
                gameObject = new GameObject(m, false, "vehicle", name);
            else
                gameObject = new GameObject(m, true, "vehicle", name);
        }

        public void Initialize()
        {
            camera.Update(new GameTime());
            Position = camera.cameraPosition;
            Forward = camera.target;
            gameObject.world = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
        }

        public void Update(GameTime gameTime)
        {
            timeToNextFire += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (localPlayer)
            {
                if (alive)
                {
                    status = VehicleState.Alive;
                    camera.Update(gameTime);

                    if (gameObject != null)
                    {
                        var colliders = GameObjectManager.Instance.CheckCollision(gameObject);
                        List<Projectile> projectiles2 = new List<Projectile>();
                        this.collider = null;
                        foreach (var collider in colliders)
                        {
                            switch (collider.type)
                            {
                                case "vehicle":
                                    status = VehicleState.CrashedVehicle;
                                    this.collider = collider.owner;
                                    alive = false;
                                    Delete();
                                    break;
                                case "projectile":
                                    if (collider.owner != name)
                                    {
                                        var def = Projectile.definitions[collider.type];
                                        health -= def.damage;
                                        projectiles2.Add((Projectile)collider);
                                    }
                                    break;
                            }
                        }

                        foreach (var p in projectiles2)
                            GameObjectManager.Instance.RemoveGameObject(p);
                    }

                    Position = camera.cameraPosition;
                    Forward = camera.target;

                    if (alive)
                    {
                        var def = Projectile.definitions[weaponType];

                        if (Mouse.GetState().LeftButton == ButtonState.Pressed && def.fireTime - timeToNextFire <= 0.0f)
                        {
                            timeToNextFire = 0.0f;
                            var transpose = Matrix.Transpose(camera.view);
                            var projectile = new Projectile(new BasicModel(Game1.ContentManager.Load<Model>(def.modelName), Vector3.Zero),
                                                            Position, -transpose.Forward, weaponType, name);
                            status = VehicleState.WeaponFired;
                        }
                    }

                    if (gameObject != null)
                        gameObject.world = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
                }
                else
                {
                    status = VehicleState.Died;
                    // dead stuff here
                }
            }
            else
            {
                var matrix = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
                if (gameObject != null)
                {
                    gameObject.world = matrix;
                }

                switch(status)
                {
                    case VehicleState.WeaponFired:
                        var def = Projectile.definitions[weaponType];
                        var projectile = new Projectile(new BasicModel(Game1.ContentManager.Load<Model>(def.modelName), Vector3.Zero),
                                                        Position, matrix.Forward, weaponType, name);
                        break;
                }

                if (alive)
                    status = VehicleState.Alive;
                else
                    status = VehicleState.Died;
            }
        }

        public void Delete()
        {
            GameObjectManager.Instance.RemoveGameObject(gameObject);
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