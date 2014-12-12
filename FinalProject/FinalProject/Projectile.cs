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
    class Projectile : GameObject
    {
        BasicModel model;
        Vector3 dir;
        float speed;
        float timeToLive;

        public Projectile(BasicModel m, Vector3 d, float s)
        {
            model = m;
            dir = d;
            speed = s;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(Camera camera)
        {

        }
    }
}
