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
    class GameObjectManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private static GameObjectManager instance;

        private List<GameObject> gameObjects;
        private Camera camera;

        public static GameObjectManager Instance
        {
            get 
            { 
                return instance;
            }

            set
            {
                instance = value;
            }
        }

        public GameObjectManager(Game game, Camera c) : base(game)
        {
            gameObjects = new List<GameObject>();
            camera = c;
        }

        public bool CheckCollision(GameObject obj)
        {
            foreach (var go in gameObjects)
            {
                if (obj == go)
                    continue;
                if (obj.sphere.Intersects(go.sphere))
                    return true;
            }

            return false;
        }

        public override void Update(GameTime gameTime)
        {
 	         base.Update(gameTime);

            foreach(var go in gameObjects)
                go.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            foreach (var go in gameObjects)
                go.Draw(camera);
        }

        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            gameObjects.Remove(gameObject);
        }

        public void Reset()
        {
            gameObjects.Clear();
        }
    }
}
