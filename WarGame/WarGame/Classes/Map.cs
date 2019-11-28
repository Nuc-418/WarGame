using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarGame
{
    class Map
    {
        public float[] vertexHeights;
        public Vector3[] vertexNormals;
        private short[] indexVertex;
        private Color[] vertexColor;
        public Texture2D heightMapTexture, terrainTexture;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        BasicEffect effect;
        public int width, height;
        public Matrix mapMatrix;
        float mountainScale;
        int textureMultiplier;
        
        public Map(GraphicsDevice device, Texture2D mapTexture, Texture2D terrainTexture, float mountainScale, int textureMultiplier)
        {

            //Effect
            effect = new BasicEffect(device);
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
            effect.LightingEnabled = true;

            //luz ambiente
            effect.AmbientLightColor = new Vector3(1f, 1f, 1f) * 0.3f;
            effect.SpecularColor = new Vector3(0, 0, 0);
            effect.DiffuseColor = new Vector3(0.64f, 0.64f, 0.64f);

            //luz direcional
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f) * 2;
            effect.DirectionalLight0.Direction = new Vector3(0, -0.57f, 1f);
            effect.DirectionalLight0.SpecularColor = new Vector3(1f, 0.96f, 0.81f);

            //Set Vars
            this.heightMapTexture = mapTexture;
            this.width = mapTexture.Width;
            this.height = mapTexture.Height;
            this.terrainTexture = terrainTexture;
            this.mapMatrix = Matrix.Identity;
            this.mountainScale = mountainScale;
            this.textureMultiplier = textureMultiplier;

            CreateMap(device);
        }

        private void CreateMap(GraphicsDevice device)
        {
            //Get heightMapTexture info
            int width = heightMapTexture.Width;
            int height = heightMapTexture.Height;

            //Get texture delta per coordenate
            double widthT = (1f / width) * textureMultiplier;
            double heightT = (1f / height) * textureMultiplier;

            //get number of texels in heightMapTexture
            int nTexels = width * height;

            //Initializate Arrays
            VertexPositionNormalTexture[] vertexArray = new VertexPositionNormalTexture[nTexels];
            vertexHeights = new float[nTexels];
            vertexNormals = new Vector3[nTexels];
            vertexColor = new Color[nTexels];
            indexVertex = new short[height * (2 * width - 2)];

            //Import Texel color to vertexColor
            heightMapTexture.GetData<Color>(vertexColor);

            //Import info to vertexArray
            for (int z = 0; z < height; z++)
                for (int x = 0; x < width; x++)
                {
                    vertexArray[x + (z * width)] = new VertexPositionNormalTexture(new Vector3(x, vertexColor[x + (z * width)].R * mountainScale, z), Vector3.Up, new Vector2((float)(widthT * x), (float)(heightT * z)));
                    vertexHeights[x + (z * width)] = vertexColor[x + (z * width)].R * mountainScale;
                }
            //Get Normals
            for (int z = 1; z < height - 1; z++)
                for (int x = 1; x < width - 1; x++)
                {
                    Vector3[] normals = new Vector3[4];
                    Vector3[] points = new Vector3[5];
                    Vector3[] vecs = new Vector3[4];

                    //Get points
                    points[0] = vertexArray[x + (z * width)].Position;
                    points[1] = vertexArray[(x + 1) + (z * width)].Position;
                    points[2] = vertexArray[x + ((z + 1) * width)].Position;
                    points[3] = vertexArray[(x - 1) + (z * width)].Position;
                    points[4] = vertexArray[x + ((z - 1) * width)].Position;

                    for (int indexVec = 0; indexVec < 4; indexVec++)
                    {
                        //Calculate Vectors
                        vecs[indexVec] = points[indexVec + 1] - points[0];
                        //Normalize Vectors
                        vecs[indexVec].Normalize();
                    }

                    //Cross Vectors
                    for (int normalN = 0; normalN < 4; normalN++)
                        normals[normalN] = Vector3.Cross(vecs[(normalN < 3) ? normalN + 1 : 0], vecs[normalN]);

                    //Obter Vector medio e atribuir normal 
                    vertexArray[x + (z * width)].Normal = new Vector3(
                        (normals[0].X + normals[1].X + normals[2].X + normals[3].X) / 2f,
                        (normals[0].Y + normals[1].Y + normals[2].Y + normals[3].Y) / 2f,
                        (normals[0].Z + normals[1].Z + normals[2].Z + normals[3].Z) / 2f
                        );
                    vertexNormals[x + (z * width)] = new Vector3(
                        (normals[0].X + normals[1].X + normals[2].X + normals[3].X) / 2f,
                        (normals[0].Y + normals[1].Y + normals[2].Y + normals[3].Y) / 2f,
                        (normals[0].Z + normals[1].Z + normals[2].Z + normals[3].Z) / 2f
                        );
                }

            //Build index array (indexVertex)
            int i = 0;
            for (int x = 0; x < width - 1; x++)
                for (int z = 0; z < height; z++)
                {
                    indexVertex[i++] = (short)((z * width) + x);
                    indexVertex[i++] = (short)((z * width) + x + 1);
                }

            //Buffers
            vertexBuffer = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, vertexArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexArray);
            indexBuffer = new IndexBuffer(device, typeof(short), indexVertex.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indexVertex);


        }


        public void Draw(GraphicsDevice device, Matrix view, Matrix projection)
        {
            //Add view matrix (cam)
            effect.View = view;
            effect.Projection = projection;
            //Add mapMatrix
            effect.World = mapMatrix;

            //Add terrain texture
            effect.Texture = terrainTexture;

            effect.CurrentTechnique.Passes[0].Apply();

            //set device Indices 
            device.Indices = indexBuffer;

            //set device VertexBuffer
            device.SetVertexBuffer(vertexBuffer);

            //draw map
            for (int x = 0; x < heightMapTexture.Width; x++)
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 2 * heightMapTexture.Height * x, heightMapTexture.Height * 2 - 2);
        }
        
        ////////////////////////////////////////////
        
        //Calculates the y of the position "pos"
        public float GetY(Vector3 pos)
        {
           
            float[] auxHeight = new float[2];

            //get the height of 4 points near the point "pos"
            float aHeight = vertexHeights[((int)pos.X) + (((int)pos.Z) * width)];
            float bHeight = vertexHeights[((int)pos.X + 1) + (((int)pos.Z) * width)];
            float cHeight = vertexHeights[((int)pos.X) + (((int)pos.Z + 1) * width)];
            float dHeight = vertexHeights[((int)pos.X + 1) + (((int)pos.Z + 1) * width)];

            //Calculate deltaX
            float deltaX = (pos.X - (int)pos.X);

            //Get the haights of 2 points (horizontal)
            auxHeight[0] = aHeight * (1 - deltaX) + bHeight * deltaX;
            auxHeight[1] = cHeight * (1 - deltaX) + dHeight * deltaX;

            //Calculate deltaZ
            float deltaZ = (pos.Z - (int)pos.Z);

            //Get the haight of the position "pos" (Vertical)
            return auxHeight[0] * (1 - deltaZ) + auxHeight[1] * deltaZ;


        }

        //Calculates the normal in the position "pos"
        public Vector3 GetNormal(Vector3 pos)
        {

            Vector3[] auxVec = new Vector3[2];

            //get the normal of 4 points near the point "pos"
            Vector3 aNormal = vertexNormals[((int)pos.X) + (((int)pos.Z) * width)];
            Vector3 bNormal = vertexNormals[((int)pos.X + 1) + (((int)pos.Z) * width)];
            Vector3 cNormal = vertexNormals[((int)pos.X) + (((int)pos.Z + 1) * width)];
            Vector3 dNormal = vertexNormals[((int)pos.X + 1) + (((int)pos.Z + 1) * width)];

            //Calculate deltaX
            float deltaX = (pos.X - (int)pos.X);

            //Get the normals of 2 points (horizontal)
            auxVec[0] = Vector3.Normalize(aNormal * (1 - deltaX) + bNormal * deltaX);
            auxVec[1] = Vector3.Normalize(cNormal * (1 - deltaX) + dNormal * deltaX);

            //Calculate deltaZ
            float deltaZ = (pos.Z - (int)pos.Z);


            //Get the normal of the position "pos" (Vertical)
            return Vector3.Normalize(auxVec[0] * (1 - deltaZ) + auxVec[1] * deltaZ);
        }
    }
}
