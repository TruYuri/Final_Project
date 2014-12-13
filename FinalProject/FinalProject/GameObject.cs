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
    class GameObject
    {
        public BasicModel model;
        public Matrix world;
        public BoundingSphere sphere;
        bool drawModel;

        public GameObject(BasicModel m, bool draw) 
        {
            drawModel = draw;
            model = m;
            if(model != null)
                sphere = model.model.Meshes[0].BoundingSphere;

            GameObjectManager.Instance.AddGameObject(this);
        }

        public virtual void Update(GameTime gameTime) 
        {
            if(model != null)
                model.World = world;
        }

        public virtual void Draw(Camera c)
        {
            if (model != null && drawModel)
                model.Draw(c);
        }
    }
}
