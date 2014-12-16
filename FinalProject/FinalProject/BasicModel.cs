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
    class BasicModel
    {
        public Model model { get; protected set; }
        protected Matrix world = Matrix.Identity;
        public BoundingSphere sphere;

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }
        
        public BasicModel(Model m, Vector3 pos)
        {
            model = m;
            world.Translation += pos;

            if (model.Meshes.Count > 0)
                sphere = model.Meshes[0].BoundingSphere;
            else
                sphere = new BoundingSphere();
            sphere.Center = pos;
        }

        public virtual void Update()
        {
            //Base class does nothing here
        }

        public void Draw(Camera camera)
        {
            //Set transforms
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            //Loop through meshes and their effects 
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    //Set BasicEffect information
                    be.TextureEnabled = false;
                    be.EnableDefaultLighting();
                    be.Projection = camera.projection;
                    be.View = camera.view;
                    be.World = World * mesh.ParentBone.Transform;
                }
                //Draw
                mesh.Draw();
            }
        }
    }
}
