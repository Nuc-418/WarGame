using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarGame
{

    class Tank
    {
        //var

        public Matrix worldMatrix;
        public Model tankModel;

        public Vector3 pos, dir ;
        public Vector3 normal;
        private float speed = 0.005f, rotationSpeed = 0.002f;
        private float scale;
        private float yaw;

        public ModelBone rFWheelBone;
        public ModelBone lFWheelBone;
        public ModelBone rBWheelBone;
        public ModelBone lBWheelBone;
        public ModelBone lSteerBone;
        public ModelBone rSteerBone;
        public ModelBone turretBone;
        public ModelBone cannonBone;
        public ModelBone hatchBone;

        public Matrix cannonTransform;
        public Matrix turretTransform;

        public float turretAng;
        public float cannonAng;

        public Matrix[] boneTransforms;

        //Construtor tank
        public Tank( Model tankModel, float scale, Vector3 pos)
        {
            this.tankModel = tankModel;
            worldMatrix = Matrix.Identity;

            this.scale = scale;
            this.pos = pos;
            dir = Vector3.Forward;

            //associar bones as devidas variaveis
            rFWheelBone = tankModel.Bones["r_front_wheel_geo"];
            lFWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rBWheelBone = tankModel.Bones["r_back_wheel_geo"];
            lBWheelBone = tankModel.Bones["l_back_wheel_geo"];
            lSteerBone = tankModel.Bones["l_steer_geo"];
            rSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];

            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;

            boneTransforms = new Matrix[tankModel.Bones.Count];
            tankModel.Root.Transform = Matrix.CreateScale(scale);

        }

        public void Update(KeyboardState keyboard, GameTime gameTime, ref Map map)
        {


            //movimento do tanke
            if (keyboard.IsKeyDown(Keys.W))
                pos -= dir * speed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.S))
                pos += dir * speed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.A))
                yaw += rotationSpeed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.D))
                yaw -= rotationSpeed * gameTime.ElapsedGameTime.Milliseconds;

            //otimizaçao da variavel encarregue pela rotaçao do tanke
            if (yaw > 2*Math.PI || yaw < -2 * Math.PI)
                yaw = 0;


            
            //fazer restriçao das bordas do mapa
            if (pos.X > map.width - 2)
                pos.X = map.width - 2f;
            if (pos.X <  2)
                pos.X = 2f;
            if (pos.Z > map.height - 2)
                pos.Z = map.height - 2f;
            if (pos.Z <  2)
                pos.Z = 2f;
            
            //obter y e normal do tanke nuuma determinada posiçao
            pos.Y = map.GetY(pos) ;
            normal = map.GetNormal(pos);

            //obter vetor right e dir
            Vector3 diretionHorizontal = Vector3.Transform(Vector3.UnitX, Matrix.CreateRotationY(yaw));
            Vector3 right = Vector3.Cross(diretionHorizontal, normal);
            dir = Vector3.Cross(normal, right);
            dir.Normalize();
            right.Normalize();

            //Criar matriz rotaçao(rotaçao do tank)
            Matrix rotationMatrix = Matrix.Identity;
            rotationMatrix.Forward = dir;
            rotationMatrix.Up = normal;
            rotationMatrix.Right = right;

            //Criar matriz translaçao (movimento do tank)
            Matrix translationMatrix = Matrix.CreateTranslation(pos);

            //Criar matriz escala (escala do tank)
            Matrix scaleMatrix = Matrix.CreateScale(scale);

            //adiciona modificaçoes ao tank
            tankModel.Root.Transform = scaleMatrix * rotationMatrix * translationMatrix;

            //movimento da torre e do canhao
            if (keyboard.IsKeyDown(Keys.Left))
                turretAng += MathHelper.ToRadians(1.0f);
            if (keyboard.IsKeyDown(Keys.Right))
                turretAng -= MathHelper.ToRadians(1.0f);
            if (keyboard.IsKeyDown(Keys.Up))
                cannonAng += MathHelper.ToRadians(1.0f);
            if (keyboard.IsKeyDown(Keys.Down))
                cannonAng -= MathHelper.ToRadians(1.0f);

            //otimizaçao da variavel encarregue da rotaçao da torre
            if (turretAng > 2 * Math.PI || turretAng < -2 * Math.PI)
                turretAng = 0;

            //otimizaçao da variavel encarregue da rotaçao do canhao
            if (cannonAng < -Math.PI/4f )
                cannonAng = -(float)Math.PI / 4f;
            if (cannonAng > 0)
                cannonAng = 0;
            
            //Aplicar as transformaçoes na torre e no canhao
            turretBone.Transform = Matrix.CreateRotationY(turretAng) * turretTransform;
            cannonBone.Transform = Matrix.CreateRotationX(cannonAng) * cannonTransform;


            //adiciona as mudanças "boneTransforms"
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //Aplicar alteraçoes na mesh
                    effect.World = boneTransforms[mesh.ParentBone.Index];

                    //View
                    effect.View = view;
                    effect.Projection = projection;

                    //Luz para cada mesh do model "tankModel"
                    effect.LightingEnabled = true;

                    effect.AmbientLightColor = new Vector3(1f, 1f, 1f) * 0.3f;
                    effect.SpecularColor = new Vector3(0, 0, 0);
                    effect.DiffuseColor = new Vector3(0.64f, 0.64f, 0.64f);

                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.DiffuseColor = new Vector3(1f, 1f, 1f) * 2;
                    effect.DirectionalLight0.Direction = new Vector3(0, -0.57f, 1f);
                    effect.DirectionalLight0.SpecularColor = new Vector3(1f, 0.96f, 0.81f);
                }

                //desenha mesh
                mesh.Draw();
            }
        }
    }
}
