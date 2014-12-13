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
        public Quaternion Rotation;
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
                Rotation = Quaternion.CreateFromRotationMatrix(camera.view);
            }
            else
            {
                float q0 = Rotation.W;
                float q1 = Rotation.Y;
                float q2 = Rotation.X;
                float q3 = Rotation.Z;
                Vector3 radAngles = new Vector3();
                radAngles.X = (float)Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (Math.Pow(q1, 2) + Math.Pow(q2, 2)));
                radAngles.Y = (float)Math.Asin(2 * (q0 * q2 - q3 * q1));
                radAngles.Z = (float)Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (Math.Pow(q2, 2) + Math.Pow(q3, 2)));
                Vector3 angles = new Vector3();
                angles.X = MathHelper.ToDegrees(radAngles.X);
                angles.Y = MathHelper.ToDegrees(radAngles.Y);
                angles.Z = MathHelper.ToDegrees(radAngles.Z);

                var matrix = Matrix.CreateFromYawPitchRoll(radAngles.Y, radAngles.X, radAngles.Z);
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