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
        public Matrix view { get; protected set; }
        public Matrix projection { get; protected set; }

        public Vector3 cameraPosition;
        public Vector3 cameraDirection;
        Vector3 cameraUp;
        public Vector3 target;
        Vector3 velocity;
        bool jumping;
        MouseState prevMouseState;
        List<Terrain> terrains;

        public Camera(Game game, Vector3 pos, Vector3 direction, Vector3 up, List<Terrain> t)
        {
            Mouse.SetPosition(0, 0);
            prevMouseState = Mouse.GetState();

            terrains = t;
            cameraPosition = pos;
            cameraDirection = direction;
            cameraDirection.Normalize();
            target = cameraPosition + cameraDirection * new Vector3(1, 1, 1);
            cameraUp = up;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 
                                                             (float)game.Window.ClientBounds.Width / (float)game.Window.ClientBounds.Height, 
                                                             1, 1000);
            CreateLookAt();
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
            Vector3 oldCamPos = cameraPosition;
            Vector3 oldTarPos = target;

            Vector3 forward = Vector3.Zero;
            Vector3 left = Vector3.Zero;
            Vector3 right = Vector3.Zero;
            Vector3 backward = Vector3.Zero;

            float slopeF = 0.0f;
            float slopeB = 0.0f;
            float slopeL = 0.0f;
            float slopeR = 0.0f;

            float? height = null;

            if(!jumping)
            {
                velocity = Vector3.Zero;

                float s = 1.0f;
                if (kState.IsKeyDown(Keys.LeftShift))
                    s = 3.0f;

                if (kState.IsKeyDown(Keys.W))
                    forward = s * new Vector3(cameraDirection.X, 0.0f, cameraDirection.Z) * 100 * time;
                if (kState.IsKeyDown(Keys.S))
                    backward = 0.5f * new Vector3(cameraDirection.X, 0.0f, cameraDirection.Z) * 100 * time;
                if (kState.IsKeyDown(Keys.A))
                    left = 0.7f * Vector3.Cross(new Vector3(cameraDirection.X, 0.0f, cameraDirection.Z), Vector3.Up) * 100 * time;
                if (kState.IsKeyDown(Keys.D))
                    right = 0.7f * Vector3.Cross(new Vector3(cameraDirection.X, 0.0f, cameraDirection.Z), Vector3.Up) * 100 * time;

                // forward
                var newPos = cameraPosition + forward;
                newPos.Y = 1000.0f;
                foreach (var t in terrains)
                {
                    height = t.Intersects(new Ray(newPos, Vector3.Down));

                    if (height != null)
                        break;
                }

                if (height != null)
                {
                    newPos.Y = newPos.Y - (float)height + 50.0f;
                    var ch = newPos - cameraPosition;
                    slopeF = (float)Math.Cos(Math.Atan2(ch.Y, Math.Sqrt(ch.X * ch.X + ch.Z * ch.Z)));
                    if (newPos.Y < cameraPosition.Y) slopeF = 2.0f - slopeF;
                }

                // backward
                newPos = cameraPosition - backward;
                newPos.Y = 1000.0f;
                foreach (var t in terrains)
                {
                    height = t.Intersects(new Ray(newPos, Vector3.Down));

                    if (height != null)
                        break;
                }
                if (height != null)
                {
                    newPos.Y = newPos.Y - (float)height + 50.0f;
                    var ch = newPos - cameraPosition;
                    slopeB = (float)Math.Cos(Math.Atan2(ch.Y, Math.Sqrt(ch.X * ch.X + ch.Z * ch.Z)));
                    if (newPos.Y < cameraPosition.Y) slopeB = 2.0f - slopeB;
                }

                // left
                newPos = cameraPosition - left;
                newPos.Y = 1000.0f;
                foreach (var t in terrains)
                {
                    height = t.Intersects(new Ray(newPos, Vector3.Down));

                    if (height != null)
                        break;
                }
                if (height != null)
                {
                    newPos.Y = newPos.Y - (float)height + 50.0f;
                    var ch = newPos - cameraPosition;
                    slopeL = (float)Math.Cos(Math.Atan2(ch.Y, Math.Sqrt(ch.X * ch.X + ch.Z * ch.Z)));
                    if (newPos.Y < cameraPosition.Y) slopeL = 2.0f - slopeL;
                }

                // right
                newPos = cameraPosition + right;
                newPos.Y = 1000.0f;
                foreach (var t in terrains)
                {
                    height = t.Intersects(new Ray(newPos, Vector3.Down));

                    if (height != null)
                        break;
                }
                if (height != null)
                {
                    newPos.Y = newPos.Y - (float)height + 50.0f;
                    var ch = newPos - cameraPosition;
                    slopeR = (float)Math.Cos(Math.Atan2(ch.Y, Math.Sqrt(ch.X * ch.X + ch.Z * ch.Z)));
                    if (newPos.Y < cameraPosition.Y) slopeR = 2.0f - slopeR;
                }

                velocity = (forward * slopeF) - (backward * slopeB) - (left * slopeL) + (right * slopeR);

                if (kState.IsKeyDown(Keys.Space) && !jumping)
                {
                    jumping = true;
                    velocity.Y = 5.0f;
                }
            }

            cameraPosition += velocity;
            target += velocity;

            if (jumping)
            {
                var Vi = velocity.Y;
                velocity.Y = velocity.Y - time * 9.8f;
                cameraPosition.Y += ((Vi + velocity.Y) / 2) * time;
                target.Y += ((Vi + velocity.Y) / 2) * time;
            }

            foreach (var t in terrains)
            {
                height = t.Intersects(new Ray(cameraPosition, Vector3.Down));

                if (height != null)
                    break;
            }
            if (height != null)
            {
                if(jumping && height < 50.0f)
                    jumping = false;

                if (!jumping)
                {
                    // gravity right here
                    cameraPosition.Y = cameraPosition.Y - (float)height + 50;
                    target.Y = target.Y - (float)height + 50;

                    var change = cameraPosition - oldCamPos;
                    if ((float)Math.Atan2(change.Y, Math.Sqrt(change.X * change.X + change.Z * change.Z)) > MathHelper.PiOver4)
                    {
                        cameraPosition = oldCamPos;
                        target = oldTarPos;
                    }
                }
            }
            else
            {
                if (jumping)
                {
                    cameraPosition = new Vector3(oldCamPos.X, cameraPosition.Y, oldCamPos.Z);
                    target = new Vector3(oldTarPos.X, target.Y, oldTarPos.Z);
                    velocity.X = velocity.Z = 0.0f;
                }
                else
                {
                    cameraPosition = oldCamPos;
                    target = oldTarPos;
                }
            }

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
    }
}