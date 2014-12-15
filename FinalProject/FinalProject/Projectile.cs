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
        public string fireSound;
        public string hitSound;
        public float timeToLive;
        public float speed;
        public float damage;
        public float fireTime;

        public ProjectileDefinition(string n, string m, string fS, string hS, float t, float s, float d, float f)
        {
            name = n;
            modelName = m;
            fireSound = fS;
            hitSound = hS;
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
            { "bullet", new ProjectileDefinition("bullet", "bullet", "machinegun", "bullet", 1.0f, -1000.0f, 50.0f, 0.1f) },
            { "rocket", new ProjectileDefinition("rocket", "spaceship", "missile", "explode2", 50.0f, -50.0f, 500.0f, 1.0f) }
        };

        Vector3 dir;
        Vector3 baseVelocity;
        float lifeTime;

        public Projectile(Vector3 pos, Vector3 d, Vector3 baseVel, string name, string owner)
            : base(new BasicModel(Game1.ContentManager.Load<Model>(definitions[name].modelName), Vector3.Zero), true, name, owner)
        {
            lifeTime = 0.0f;
            dir = d;
            baseVelocity = baseVel;
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
            pos += baseVelocity + dir * (float)gameTime.ElapsedGameTime.TotalSeconds * def.speed;
            world = (type == "bullet" ? Matrix.CreateRotationY(-MathHelper.PiOver2) : Matrix.Identity ) * Matrix.CreateWorld(pos, dir, Vector3.Up);

            base.Update(gameTime);
        }

        public override void Draw(Camera c)
        {
            base.Draw(c);
        }
    }
}
