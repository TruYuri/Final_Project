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
    public enum ProjectileType { Model, Line, None };
    struct ProjectileDefinition
    {
        public string name;
        public string modelName;
        public float timeToLive;
        public float speed;
        public float damage;
        public float fireTime;
        public ProjectileType type;

        public ProjectileDefinition(string n, string m, float t, float s, float d, float f, ProjectileType p)
        {
            name = n;
            modelName = m;
            timeToLive = t;
            speed = s;
            damage = d;
            fireTime = f;
            type = p;
        }
    }

    class Projectile : GameObject
    {
        public static Dictionary<string, ProjectileDefinition> definitions = new Dictionary<string, ProjectileDefinition>()
        {
            { "bullet", new ProjectileDefinition("bullet", "", 0.5f, -5000.0f, 500.0f, 1.0f, ProjectileType.Line) },
            { "rocket", new ProjectileDefinition("rocket", "spaceship", 20.0f, -50.0f, 500.0f, 0.1f, ProjectileType.Model) }
        };

        Vector3 dir;
        Vector3 baseVelocity;
        float lifeTime;
        VertexBuffer buffer;

        public Projectile(Vector3 pos, Vector3 d, Vector3 baseVel, string name, string owner)
            : base(definitions[name].modelName == "" ? null : new BasicModel(Game1.ContentManager.Load<Model>(definitions[name].modelName), Vector3.Zero), true, name, owner)
        {
            lifeTime = 0.0f;
            dir = d;
            baseVelocity = baseVel;
            world = Matrix.CreateWorld(pos, d, Vector3.Up);

            buffer = new VertexBuffer(Game1.GraphicsDeviceRef, typeof(VertexPositionColor), 2, BufferUsage.None);

            var vertices = new VertexPositionColor[2];
            vertices[0].Position = Vector3.Zero;
            vertices[0].Color = Color.Gold;
            vertices[1].Position = new Vector3(0.0f, 0.0f, -100.0f);
            vertices[1].Color = Color.Gold;
            buffer.SetData<VertexPositionColor>(vertices);
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
            world = Matrix.CreateWorld(pos, dir, Vector3.Up);

            base.Update(gameTime);
        }

        public override void Draw(Camera c)
        {
            var def = Projectile.definitions[type];
            if(def.type == ProjectileType.Model)
                base.Draw(c);
            else if(def.type == ProjectileType.Line)
            {
                var basicEffect = new BasicEffect(Game1.GraphicsDeviceRef);
                basicEffect.VertexColorEnabled = true;
                basicEffect.Projection = c.projection;
                basicEffect.View = c.view;
                basicEffect.World = world;
                basicEffect.TextureEnabled = false;
                basicEffect.CurrentTechnique.Passes[0].Apply();
                Game1.GraphicsDeviceRef.SetVertexBuffer(buffer);
                Game1.GraphicsDeviceRef.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 2, 0, 1);
            }
        }
    }
}
