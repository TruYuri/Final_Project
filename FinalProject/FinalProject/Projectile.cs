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
    struct ProjectileDefinition
    {
        public string name;
        public string modelName;
        public float timeToLive;
        public float speed;
        public float damage;
        public float fireTime;

        public ProjectileDefinition(string n, string m, float t, float s, float d, float f)
        {
            name = n;
            modelName = m;
            timeToLive = t;
            speed = s;
            damage = d;
            fireTime = f;
        }
    }

    class Projectile : GameObject
    {
        public static Dictionary<string, ProjectileDefinition> definitions = new Dictionary<string, ProjectileDefinition>()
        {
            { "projectile", new ProjectileDefinition("projectile", "spaceship", 20.0f, -10.0f, 5.0f, 0.5f) }
        };

        Vector3 dir;
        float lifeTime;

        public Projectile(BasicModel m, Vector3 pos, Vector3 d, string name, string owner) : base(m, true, name, owner)
        {
            lifeTime = 0.0f;
            model = m;
            dir = d;
            world = Matrix.CreateWorld(pos, d, Vector3.Up);
        }

        public override void Update(GameTime gameTime)
        {
            var def = Projectile.definitions[type];
            lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(lifeTime >= def.timeToLive)
            {
                GameObjectManager.Instance.Delete(this);
            }

            var pos = world.Translation;
            pos += dir * (float)gameTime.ElapsedGameTime.TotalSeconds * def.speed;
            world = Matrix.CreateWorld(pos, dir, Vector3.Up);

            base.Update(gameTime);
        }

        public override void Draw(Camera c)
        {
            base.Draw(c);
        }
    }
}
