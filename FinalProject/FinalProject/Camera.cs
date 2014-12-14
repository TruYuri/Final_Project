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
        int screenCenterX;
        int screenCenterY;
        int prevMouseX;
        int prevMouseY;
        Map map;
        float throttle;
        float throttleRate;

        public Camera(Game game, Map t)
        {
            Mouse.SetPosition(0, 0);
            prevMouseState = Mouse.GetState();

            map = t;
            screenCenterX = game.Window.ClientBounds.Width / 2;
            screenCenterY = game.Window.ClientBounds.Height / 2;
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
            var transpose = Matrix.Transpose(view);

            mState = Mouse.GetState();
            Vector3 dist = cameraPosition - target;
            cameraDirection = -dist;
            if (cameraDirection != Vector3.Zero)
                cameraDirection.Normalize();

            float roll = 0.0f;
            if (kState.IsKeyDown(Keys.A))
                roll += (float)MathHelper.PiOver4 * time;
            else if (kState.IsKeyDown(Keys.D))
                roll -= (float)MathHelper.PiOver4 * time;
            cameraUp = Vector3.Lerp(cameraUp, Vector3.Cross(cameraUp, cameraDirection), roll / (float)MathHelper.PiOver2);

            // rotate an object around the camera, make that the target
            var xZRotation = (float)Math.Atan2(dist.Z, dist.X) - MathHelper.Pi;
            var yRotation = (float)Math.Atan2(dist.Y, Math.Sqrt(dist.X * dist.X + dist.Z * dist.Z));
            yRotation += MathHelper.PiOver2;

            var xDiff = mState.X - screenCenterX;
            var yDiff = mState.Y - screenCenterY;
            xZRotation += (xDiff) * MathHelper.PiOver4 / 4 * time;
            yRotation += (yDiff) * MathHelper.PiOver4 / 4 * time;

            if (yRotation < 0.01f)
                yRotation = 0.01f;
            else if (yRotation > MathHelper.Pi - 0.01f)
                yRotation = MathHelper.Pi - 0.01f;

            target.X = cameraPosition.X + dist.Length() * (float)Math.Cos(xZRotation) * (float)Math.Sin(yRotation);
            target.Z = cameraPosition.Z + dist.Length() * (float)Math.Sin(xZRotation) * (float)Math.Sin(yRotation);
            target.Y = cameraPosition.Y + dist.Length() * (float)Math.Cos(yRotation);

            CreateLookAt();
            Mouse.SetPosition(screenCenterX, screenCenterY);
        }

        public void PlaceCamera(Vector3 pos, Vector3 direction, Vector3 up)
        {
            cameraPosition = pos;
            cameraDirection = direction;
            cameraDirection.Normalize();
            target = cameraPosition + cameraDirection;
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