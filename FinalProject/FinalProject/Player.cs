﻿using System;
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
                    status = VehicleState.Alive;
                else
                    status = VehicleState.Died;

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
                                        GameObjectManager.Instance.Delete(collider);
                                    }
                                    break;
                            }
                        }
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
                            var projectile = new Projectile(new BasicModel(Game1.ContentManager.Load<Model>(def.modelName), Vector3.Zero),
                                                            Position, matrix.Forward, weaponType, name);
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