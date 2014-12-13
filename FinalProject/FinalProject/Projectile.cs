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
    class Projectile : GameObject
    {
        Vector3 dir;
        float speed;
        float timeToLive;

        public Projectile(BasicModel m, Vector3 d, float s, string name) : base(m, true, name)
        {
            model = m;
            dir = d;
            speed = s;
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(Camera c)
        {
            base.Draw(c);
        }
    }
}
