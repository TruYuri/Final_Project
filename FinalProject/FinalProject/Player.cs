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
    class Player
    {
        public string playerName;
        public Vector3 Position;
        public Vector3 Forward;

        Camera camera; // all movement
        Map map;
        GameObject gameObject;
        bool localPlayer;

        public Player(Game game, Camera c, Map t, bool local, BasicModel m, string n)
        {
            playerName = n;
            camera = c;
            map = t;
            localPlayer = local;

            if(local)
                gameObject = new GameObject(m, false);
            else
                gameObject = new GameObject(m, true);
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
                Camera cam2 = camera.clone();
                var oldPos = camera.view;
                var oldTar = camera.target;
                camera.Update(gameTime);

                if (GameObjectManager.Instance.CheckCollision(gameObject))
                {
                    camera = cam2;
                }

                Position = camera.cameraPosition;
                Forward = camera.target;
                gameObject.world = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
            }
            else
            {
                var matrix = Matrix.CreateWorld(Position, -(Forward - Position), Vector3.Up);
                gameObject.world = matrix;
                gameObject.Update(gameTime);
            }
        }

        public void Delete()
        {
            GameObjectManager.Instance.RemoveGameObject(gameObject);
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