using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace FinalProject
{
    class Map
    {
        // string is a LRUP combo, determining which direction to render the piece from the center. 
        public List<Terrain> terrainPieces;
        public List<Vector3> startingPositions;
        public Terrain BottomLeft;
        public Vector2 Size;

        public Map(Vector2 s)
        {
            Size = s;
            terrainPieces = new List<Terrain>();
            startingPositions = new List<Vector3>();
        }

        public Vector3 CreateRandomSpawnAtHeight(float y)
        {
            Random rand = new Random();
            float m = (float)Math.Abs(BottomLeft.startPosition.X * 2);
            double x = -(m / 2.0f) + m * Size.X * rand.NextDouble();
            double z = (m / 2.0f) - m * Size.Y * rand.NextDouble();

            return new Vector3((float)x, y, (float)z);
        }

        public Vector3 Center(float y)
        {
            float m = (float)Math.Abs(BottomLeft.startPosition.X * 2);
            float x = (-(m / 2.0f) + (m * Size.X)) / 2.0f;
            float z = (-(m / 2.0f) - (m * Size.Y)) / 2.0f;

            return new Vector3(x, y, z);
        }

        public List<GameObject> MakePowerups(int n)
        {
            List<GameObject> orbs = new List<GameObject>();
            Random rand = new Random();
            for (int i = 0; i < n; i++)
            {
                int b = rand.Next() % 2;
                string name = (b == 0 ? "health_orb" : "shield_orb");
                var orb = new GameObject(new BasicModel(Game1.ContentManager.Load<Model>(name), Vector3.Zero), true, name, "");
                orbs.Add(orb);
                var pos = CreateRandomSpawnAtHeight(rand.Next() % 400 + 100);
                orb.world = Matrix.CreateWorld(pos, Vector3.Forward, Vector3.Up);
                orb.model.World = orb.world;
            }

            return orbs;
        }

        public void Load()
        {
            BottomLeft.Load(256, 256, 10.0f, 1.0f, 0.0f);
            float m = (float)Math.Abs(BottomLeft.startPosition.X * 2);
            foreach(var t in terrainPieces)
            {
                t.Load(256, 256, 10.0f, 1.0f, m);
            }
        }

        public void Draw(Camera camera)
        {
            foreach (var t in terrainPieces)
            {
                t.Draw(camera);
            }
        }
    }

    class Terrain // : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Vector3 startPosition;
        // 
        int vertexCountX;
        // Number of vertices of Vertex Grid in X direction, equivilent to the width of the height map image in pixels
        int vertexCountZ;
        // Number of vertices of Vertex Grid in Z direction, equivilent to the height of the height map image in pixels
        float blockScale;
        // Space in both X and Z direction between verteces in Vertex Grid
        float heightScale;
        // Heights provided by height map will be values 0 – 255, amount at which to scale these values
        byte[] heightmap;
        // Array of heights from height map
        int numVertices;
        // Number of vertices in vertex grid
        int numTriangles;
        // Number of triangles in vertex grid
        VertexBuffer vb;
        // All the vertices that make up the vertex grid
        IndexBuffer ib;
        // Indices of the vertices that make up the primitives that make up the terrain mesh
        BasicEffect effect;
        // Used for drawing the terrain
        Texture2D texture;
        // Texture placed over the terrain mesh
        string heightMap;

        int x; // grid x
        int y; // grid y

        public Terrain(string heightmap, string texture, int x, int y) 
        {
            this.texture = Game1.ContentManager.Load<Texture2D>(texture);
            heightMap = heightmap;
            this.x = x;
            this.y = y;
        }

        public void Load(int vertexCountX, int vertexCountZ, float blockScale, float heightScale, float offsetAmount)
        {
            var offset = new Vector3(x * offsetAmount, 0, -y * offsetAmount);
            effect = new BasicEffect(Game1.GraphicsDeviceRef);
            this.vertexCountX = vertexCountX;
            this.vertexCountZ = vertexCountZ;
            this.blockScale = blockScale;
            this.heightScale = heightScale;

            FileStream filestream = File.OpenRead(Game1.ContentManager.RootDirectory + "/" + heightMap);

            int heightmapSize = vertexCountX * vertexCountZ;
            this.heightmap = new byte[heightmapSize];

            filestream.Read(heightmap, 0, heightmapSize);
            filestream.Close();

            GenerateTerrainMesh(offset);
        }

        private void GenerateTerrainMesh(Vector3 offset)
        {
            numVertices = vertexCountX * vertexCountZ;
            numTriangles = (vertexCountX - 1) * (vertexCountZ - 1) * 2;
            short[] indices = GenerateTerrainIndices();
            VertexPositionTexture[] vertices = GenerateTerrainVertices(indices, offset);

            startPosition = vertices[0].Position;

            vb = new VertexBuffer(Game1.GraphicsDeviceRef, typeof(VertexPositionTexture), numVertices, BufferUsage.WriteOnly);
            vb.SetData<VertexPositionTexture>(vertices);

            ib = new IndexBuffer(Game1.GraphicsDeviceRef, typeof(short), numTriangles * 3, BufferUsage.WriteOnly);
            ib.SetData<short>(indices);
        }

        private short[] GenerateTerrainIndices()
        {
            int numIndices = numTriangles * 3;
            short[] indices = new short[numIndices];

            int indicesCount = 0;
            for (int i = 0; i < (vertexCountZ - 1); i++) //All Rows except last
                for (int j = 0; j < (vertexCountX - 1); j++) //All Columns except last
                {
                    short index = (short)(j + i * vertexCountZ); //2D coordinates to linear
                    //First Triangle Vertices
                    indices[indicesCount++] = index;
                    indices[indicesCount++] = (short)(index + 1);
                    indices[indicesCount++] = (short)(index + vertexCountX + 1);

                    //Second Triangle Vertices
                    indices[indicesCount++] = (short)(index + vertexCountX + 1);
                    indices[indicesCount++] = (short)(index + vertexCountX);
                    indices[indicesCount++] = index;
                }

            return indices;
        }

        private VertexPositionTexture[] GenerateTerrainVertices(short[] terrainIndices, Vector3 offset)
        {
            float halfTerrainWidth = (vertexCountX - 1) * blockScale * .5f;
            float halfTerrainDepth = (vertexCountZ - 1) * blockScale * .5f;
            float tuDerivative = 1.0f / (vertexCountX - 1);
            float tvDerivative = 1.0f / (vertexCountZ - 1);
            VertexPositionTexture[] vertices = new VertexPositionTexture[vertexCountX * vertexCountZ];
            int vertexCount = 0;
            float tu = 0;
            float tv = 0;

            for (float i = -halfTerrainDepth; i <= halfTerrainDepth; i += blockScale)
            {
                tu = 0.0f;
                for (float j = -halfTerrainWidth; j <= halfTerrainWidth; j += blockScale)
                {
                    vertices[vertexCount].Position = new Vector3(j, heightmap[vertexCount] * heightScale, i) + offset;
                    vertices[vertexCount].TextureCoordinate = new Vector2(tu, tv);

                    tu += tuDerivative;
                    vertexCount++;
                }
                tv += tvDerivative;
            }

            return vertices;
        }

        public float getHeight(float x, float z)
        {
            Vector2 positionInGrid = new Vector2(x - startPosition.X, z - startPosition.Z);
            Vector2 blockPosition = new Vector2(positionInGrid.X / blockScale, positionInGrid.Y / blockScale);

            if (blockPosition.X >= 0 && blockPosition.X < (vertexCountX - 1) &&
                blockPosition.Y >= 0 && blockPosition.Y < (vertexCountZ - 1))
            {
                Vector2 blockOffset = new Vector2(blockPosition.X - (int)blockPosition.X, blockPosition.Y - (int)blockPosition.Y);

                int vertexIndex = (int)blockPosition.X + (int)blockPosition.Y * vertexCountX;
                float height1 = heightmap[vertexIndex + 1];
                float height2 = heightmap[vertexIndex];
                float height3 = heightmap[vertexIndex + vertexCountX + 1];
                float height4 = heightmap[vertexIndex + vertexCountX];

                float heightIncX, heightIncY;
                //Top triangle
                if (blockOffset.X > blockOffset.Y)
                {
                    heightIncX = height1 - height2;
                    heightIncY = height3 - height1;
                }
                //Bottom triangle
                else
                {
                    heightIncX = height3 - height4;
                    heightIncY = height4 - height2;
                }

                float lerpHeight = height2 + heightIncX * blockOffset.X + heightIncY * blockOffset.Y;
                
                return lerpHeight * heightScale;
            }

            return -999999;
        }

        public float getHeight(Vector2 position)
        {
            return getHeight(position.X, position.Y);
        }

        public float getHeight(Vector3 position)
        {
            return getHeight(position.X, position.Z);
        }

        public float? Intersects(Ray ray)
        {
            //This won't be changed if the Ray doesn't collide with terrain            
            float? collisionDistance = null;
            //Size of step is half of blockScale
            Vector3 rayStep = ray.Direction * blockScale * 0.5f;
            //Need to save start position to find total distance once collision point is found
            Vector3 rayStartPosition = ray.Position;

            Vector3 lastRayPosition = ray.Position;
            ray.Position += rayStep;
            float height = getHeight(ray.Position);
            while (ray.Position.Y > height && height >= 0)
            {
                lastRayPosition = ray.Position;
                ray.Position += rayStep;
                height = getHeight(ray.Position);
            }

            if (height >= 0) //Lowest possible point of terrain
            {
                Vector3 startPosition = lastRayPosition;
                Vector3 endPosition = ray.Position;
                // Binary search. Find the exact collision point
                for (int i = 0; i < 32; i++)
                {
                    // Binary search pass
                    Vector3 middlePoint = (startPosition + endPosition) * 0.5f;
                    if (middlePoint.Y < height)
                        endPosition = middlePoint;
                    else
                        startPosition = middlePoint;
                }

                Vector3 collisionPoint = (startPosition + endPosition) * 0.5f;
                collisionDistance = Vector3.Distance(rayStartPosition, collisionPoint);
            }

            return collisionDistance;
        }

        public void Draw(Camera camera)
        {
            effect.World = Matrix.Identity; //No transformation of the terrain
            effect.View = camera.view;
            effect.Projection = camera.projection;
            effect.Texture = texture;
            effect.TextureEnabled = true;

            Game1.GraphicsDeviceRef.SetVertexBuffer(vb);
            Game1.GraphicsDeviceRef.Indices = ib;

            foreach (EffectPass CurrentPass in effect.CurrentTechnique.Passes)
            {
                CurrentPass.Apply();

                // triangle list, 
                // so every 3 indices = 1 vertex
                // every 3 vertices = 1 triangle
                // maximize triangles per render,
                
                int nTriangles = (int)MathHelper.Min(numTriangles, 65535);
                int index = 0;
                for(int i = 1; index != ib.IndexCount; i++)
                {
                    Game1.GraphicsDeviceRef.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, nTriangles * 3, index, nTriangles);
                    //Draw all triangles that make up the mesh

                    index += 3 * nTriangles;
                    nTriangles = (int)MathHelper.Min(numTriangles - i * 65535, 65535);
                }
            }
        }
    }
}
