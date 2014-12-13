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
        public int health;
        public int shield;
        public bool alive;
        public string name;
        public Vector3 Position;
        public Vector3 Forward;

        Camera camera; // all movement
        Map map;
        GameObject gameObject;
        bool localPlayer;

        public Player(Game game, Camera c, Map t, bool local, BasicModel m, string n)
        {
            name = n;
            camera = c;
            map = t;
            localPlayer = local;
            alive = true;
            status = VehicleState.Alive;
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
            if (localPlayer)
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
                                Delete();
                                break;
                            case "projectile":
                                //health = min()
                                break;
                        }
                    }
                }

                Position = camera.cameraPosition;
                Forward = camera.target;

                if(gameObject != null)
                    gameObject.world = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
            }
            else
            {
                if (gameObject != null)
                {
                    gameObject.world = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
                }
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