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

        //public Vector3 Position;
        //public Vector3 YPR;
        public Matrix world;
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
                world = camera.view;
                //Position = camera.cameraPosition;

                /*
                 * float rotx = atanf( Z.y / Z.z );
                    float roty = asinf( -Z.x );
                    float rotz = atanf( Y.x / X.x );
                 * */
                //YPR.X = (float)Math.Asin(-camera.view.M13);
               // YPR.Y = (float)
                //YPR.X = (float)Math.Atan2(-camera.view.M31, Math.Sqrt(camera.view.M32 * camera.view.M32 + camera.view.M33 * camera.view.M33));
                //YPR.Y = (float)Math.Atan2(camera.view.M32, camera.view.M32);
                //YPR.Z = (float)Math.Atan2(camera.view.M21, camera.view.M11);
            }
            else
            {
                //var matrix = Matrix.CreateFromYawPitchRoll(YPR.X, YPR.Y, YPR.Z);
                //matrix.Translation = Position;
                //model.World = matrix;
                model.World = world;
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