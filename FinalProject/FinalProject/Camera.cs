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

namespace FinalProject
{
    class Camera
    {
        public Matrix view { get; set; }
        public Matrix projection { get; protected set; }

        public Vector3 cameraPosition;
        public Vector3 cameraDirection;
        public Vector3 cameraUp;
        public Vector3 target;
        public Vector3 velocity;

        MouseState prevMouseState;
        Map map;
        float throttle;
        float throttleRate;

        public Camera(Game game, Map t)
        {
            Mouse.SetPosition(0, 0);
            prevMouseState = Mouse.GetState();

            map = t;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 
                                                             (float)game.Window.ClientBounds.Width / (float)game.Window.ClientBounds.Height, 
                                                             1, 10000);
            throttle = 0.05f;
            throttleRate = 0.1f;
        }

        private void CreateLookAt()
        {
            view = Matrix.CreateLookAt(cameraPosition, target, cameraUp);
        }

        public void Update(GameTime gameTime)
        {
            var kState = Keyboard.GetState();
            var mState = Mouse.GetState();
            float time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            float s = 1.0f;
            if (kState.IsKeyDown(Keys.LeftShift))
                s = 3.0f;

            // change throttle by an acceleration value;
            if (kState.IsKeyDown(Keys.W))
                throttle = Math.Min(throttle + throttleRate * time, 1.0f);
            else if (kState.IsKeyDown(Keys.S))
                throttle = Math.Max(throttle - throttleRate * time, 0.05f);

            velocity = cameraDirection * 1000 * throttle * s * time;

            cameraPosition += velocity;
            target += velocity;

            // Mouse
            mState = Mouse.GetState();
            Vector3 dist = cameraPosition - target;
            cameraDirection = -dist;
            if (cameraPosition != Vector3.Zero)
                cameraDirection.Normalize();

            // rotate an object around the camera, make that the target
            var xZRotation = (float)Math.Atan2(dist.Z, dist.X) - MathHelper.Pi;
            var yRotation = (float)Math.Atan2(dist.Y, Math.Sqrt(dist.X * dist.X + dist.Z * dist.Z));
            yRotation += (float)MathHelper.PiOver2;
            
            xZRotation += (mState.X - prevMouseState.X) * (float)Math.PI / 50;
            yRotation += (mState.Y - prevMouseState.Y) * (float)Math.PI / 50;

            if (yRotation < 0.01f)
                yRotation = 0.01f;
            else if (yRotation > (float)MathHelper.Pi - 0.01f)
                yRotation = (float)MathHelper.Pi - 0.01f;

            target.X = cameraPosition.X + dist.Length() * (float)Math.Cos(xZRotation) * (float)Math.Sin(yRotation);
            target.Z = cameraPosition.Z + dist.Length() * (float)Math.Sin(xZRotation) * (float)Math.Sin(yRotation);
            target.Y = cameraPosition.Y + dist.Length() * (float)Math.Cos(yRotation);

            CreateLookAt();
            prevMouseState = mState;
        }

        public void PlaceCamera(Vector3 pos, Vector3 direction, Vector3 up)
        {
            cameraPosition = pos;
            cameraDirection = direction;
            cameraDirection.Normalize();
            target = cameraPosition + cameraDirection * new Vector3(1, 1, 1);
            cameraUp = up;
            throttle = 0.05f;

            float? height = null;

            foreach (var t in map.terrainPieces)
            {
                height = t.Intersects(new Ray(cameraPosition, Vector3.Down));

                if (height != null)
                    break;
            }
            if (height != null)
            {
                cameraPosition.Y = cameraPosition.Y - (float)height + 300;
                target.Y = target.Y - (float)height + 300;
            }

            CreateLookAt();
        }
    }
}