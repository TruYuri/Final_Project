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
        Camera camera; // all movement
        List<Terrain> terrains;
        BasicModel model;

        bool localPlayer;

        public Vector3 Position;
        public Quaternion rotation;

        public Player(Game game, Camera c, List<Terrain> t, bool local, BasicModel m, string n)
        {
            playerName = n;
            camera = c;
            terrains = t;
            localPlayer = local;
            model = m;
        }

        public void Update(GameTime gameTime)
        {
            if (localPlayer)
            {
                camera.Update(gameTime);
                Position = camera.cameraPosition;
                rotation = Quaternion.CreateFromRotationMatrix(camera.view);
            }
            else
            {
                var matrix = Matrix.CreateFromQuaternion(rotation);
                matrix.Translation = Position;
                model.World = matrix;
            }
            // send network stuff
        }

        public void Draw()
        {
            if (localPlayer)
            {
                // draw weapon
            }
            else
            {
                model.Draw(camera);
            }
        }
    }
}