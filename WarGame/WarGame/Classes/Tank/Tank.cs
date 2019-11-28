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

        public GestorBalas balas;
        bool fierAux;

        public GestorParticulas po;

        public Matrix worldMatrix;
        Matrix transform;

        public Model tankModel;

        Vector3 lastPos;

        public Vector3 pos, dir;
        public Vector3 normal;
        private float speed = 5f, rotationSpeed = 3f;
        public float scale;
        public float yaw;
        public int moving;

        Vector3 vNext;

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

        public int hp;
        public bool alive;

        //Construtor tank
        public Tank(GraphicsDevice device, Model tankModel, Model balaModel, float scale, Vector3 pos)
        {

            this.tankModel = tankModel;
            this.scale = scale;
            this.pos = pos;

            worldMatrix = Matrix.Identity;
            dir = Vector3.Forward;
            hp = 100;
            alive = true;

            //particulas
            balas = new GestorBalas(balaModel, Vector3.Down, 1f, 0.0015f, 0.1f, 10);
            po = new GestorParticulas(device, Vector3.Down, 0.5f, 0.5f, 1f, 1, 0.07f, 0.005f, 10);

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

            //Obtem as matrizes "Transform"
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;

            boneTransforms = new Matrix[tankModel.Bones.Count];

            //set tank scale
            tankModel.Root.Transform = Matrix.CreateScale(scale);

        }

        public void UpdatePlayer(KeyboardState keyboard, GameTime gameTime, ref Map map, ref Tank enemyTank)
        {

            lastPos = pos;
            moving = 0;

            //movimento do tanke
            if (keyboard.IsKeyDown(Keys.W))
            {
                pos -= dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                moving = 1;
            }

            if (keyboard.IsKeyDown(Keys.S))
            {
                pos += dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                moving = 1;
            }

            if (keyboard.IsKeyDown(Keys.A))
            {
                yaw += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                moving = 1;
            }

            if (keyboard.IsKeyDown(Keys.D))
            {
                yaw -= rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                moving = 1;
            }

            //colisoes
            if (Colides(ref enemyTank))
                pos = enemyTank.pos + Vector3.Normalize(pos - enemyTank.pos) * 50 * (float)gameTime.ElapsedGameTime.TotalSeconds;




            //otimizaçao da variavel encarregue pela rotaçao do tanke
            if (yaw > 2 * Math.PI || yaw < -2 * Math.PI)
                yaw = 0;

            //fazer restriçao das bordas do mapa
            if (pos.X > map.width - 2)
                pos.X = map.width - 2f;
            if (pos.X < 2)
                pos.X = 2f;
            if (pos.Z > map.height - 2)
                pos.Z = map.height - 2f;
            if (pos.Z < 2)
                pos.Z = 2f;

            //obter y e normal do tanke nuuma determinada posiçao
            pos.Y = map.GetY(pos);
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
            transform = tankModel.Root.Transform;


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
            if (cannonAng < -Math.PI / 4f)
                cannonAng = -(float)Math.PI / 4f;
            if (cannonAng > 0)
                cannonAng = 0;

            //Aplicar as transformaçoes na torre e no canhao
            turretBone.Transform = Matrix.CreateRotationY(turretAng) * turretTransform;
            cannonBone.Transform = Matrix.CreateRotationX(cannonAng) * cannonTransform;




            //adiciona as mudanças "boneTransforms"
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            //Fire Cannon
            if (keyboard.IsKeyUp(Keys.Space))
                fierAux = true;
            if (fierAux == true && keyboard.IsKeyDown(Keys.Space))
            {
                balas.Disparar(boneTransforms[cannonBone.Index].Translation, -Vector3.Normalize(boneTransforms[cannonBone.Index].Forward));
                fierAux = false;
            }

            balas.Update(gameTime, ref map, ref enemyTank);

            //Particles
            if (moving == 1)
            {
                po.SpawnParticles(gameTime, boneTransforms[rBWheelBone.Index].Translation + (Vector3.Normalize(boneTransforms[rBWheelBone.Index].Right) * 0.1f), tankModel.Root.Transform.Backward);
                po.SpawnParticles(gameTime, boneTransforms[lBWheelBone.Index].Translation + (Vector3.Normalize(boneTransforms[lBWheelBone.Index].Left) * 0.1f), tankModel.Root.Transform.Backward);
            }
            po.Update(gameTime, ref map);
        }

        public void UpdateEnemyPlayer(KeyboardState keyboard, GameTime gameTime,ref Tank enemyTank, ref Map map)
        {

            if (alive)
            {
                lastPos = pos;
                moving = 0;
                //movimento do tanke
                if (keyboard.IsKeyDown(Keys.T))
                {
                    pos -= dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    moving = 1;
                }

                if (keyboard.IsKeyDown(Keys.G))
                {
                    pos += dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    moving = 1;
                }

                if (keyboard.IsKeyDown(Keys.F))
                {
                    yaw += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    moving = 1;
                }

                if (keyboard.IsKeyDown(Keys.H))
                {
                    yaw -= rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    moving = 1;
                }

                //Colisoes
                if (Colides(ref enemyTank))
                    pos = enemyTank.pos + Vector3.Normalize(pos - enemyTank.pos) * 50 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                //otimizaçao da variavel encarregue pela rotaçao do tanke
                if (yaw > 2 * Math.PI || yaw < -2 * Math.PI)
                    yaw = 0;

                //fazer restriçao das bordas do mapa
                if (pos.X > map.width - 2)
                    pos.X = map.width - 2f;
                if (pos.X < 2)
                    pos.X = 2f;
                if (pos.Z > map.height - 2)
                    pos.Z = map.height - 2f;
                if (pos.Z < 2)
                    pos.Z = 2f;

                //obter y e normal do tanke nuuma determinada posiçao
                pos.Y = map.GetY(pos);
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
                if (keyboard.IsKeyDown(Keys.J))
                    turretAng += MathHelper.ToRadians(1.0f);
                if (keyboard.IsKeyDown(Keys.L))
                    turretAng -= MathHelper.ToRadians(1.0f);
                if (keyboard.IsKeyDown(Keys.I))
                    cannonAng += MathHelper.ToRadians(1.0f);
                if (keyboard.IsKeyDown(Keys.K))
                    cannonAng -= MathHelper.ToRadians(1.0f);

                //otimizaçao da variavel encarregue da rotaçao da torre
                if (turretAng > 2 * Math.PI || turretAng < -2 * Math.PI)
                    turretAng = 0;

                //otimizaçao da variavel encarregue da rotaçao do canhao
                if (cannonAng < -Math.PI / 4f)
                    cannonAng = -(float)Math.PI / 4f;
                if (cannonAng > 0)
                    cannonAng = 0;

                //Aplicar as transformaçoes na torre e no canhao
                turretBone.Transform = Matrix.CreateRotationY(turretAng) * turretTransform;
                cannonBone.Transform = Matrix.CreateRotationX(cannonAng) * cannonTransform;

                //adiciona as mudanças "boneTransforms"
                tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

                //hp
                if (hp <= 0)
                    alive = false;
                

                if (moving == 1)
                {
                    po.SpawnParticles(gameTime, boneTransforms[rBWheelBone.Index].Translation + (Vector3.Normalize(boneTransforms[rBWheelBone.Index].Right) * 0.1f), tankModel.Root.Transform.Backward);
                    po.SpawnParticles(gameTime, boneTransforms[lBWheelBone.Index].Translation + (Vector3.Normalize(boneTransforms[lBWheelBone.Index].Left) * 0.1f), tankModel.Root.Transform.Backward);
                }
                po.Update(gameTime, ref map);

            }
            
        }

        public void UpdateEnemyBot(GameTime gameTime, ref Map map, ref Tank playerTank)
        {
            if (alive)
            {
               
               // if (Vector3.Distance(playerTank.pos, pos) >= 2)
                {
                    moving = 1;
                    float aMax = 4f;
                    float velMax = 4.5f;
                    Vector3 nextPosVec = -playerTank.dir * playerTank.speed * 3 * playerTank.moving;
                    Vector3 vecVSeek = Vector3.Normalize(((playerTank.pos + nextPosVec)) - pos) * velMax;
                    Vector3 a = Vector3.Normalize((vecVSeek - vNext)) * aMax;
                    vNext += a * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    pos = pos + (vNext * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
               /* else
                {
                    moving = 1;
                    float aMax = 10f;
                    float velMax = 5f;
                    Vector3 vecVSeek = -Vector3.Normalize((playerTank.pos) - pos) * velMax;
                    Vector3 a = Vector3.Normalize(vecVSeek - vNext) * aMax;
                    vNext += a * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    pos = pos + vNext * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }*/
                
                //fazer restriçao das bordas do mapa
                if (pos.X > map.width - 2)
                    pos.X = map.width - 2f;
                if (pos.X < 2)
                    pos.X = 2f;
                if (pos.Z > map.height - 2)
                    pos.Z = map.height - 2f;
                if (pos.Z < 2)
                    pos.Z = 2f;

                ////obter y e normal do tanke nuuma determinada posiçao
                pos.Y = map.GetY(pos);
                normal = map.GetNormal(pos);

                ////obter vetor right e dir
                Vector3 right = Vector3.Cross(-vNext, normal);
                dir = Vector3.Cross(normal, right);
                dir.Normalize();
                right.Normalize();

                ////Criar matriz rotaçao(rotaçao do tank)
                Matrix rotationMatrix = Matrix.Identity;
                rotationMatrix.Forward = dir;
                rotationMatrix.Up = normal;
                rotationMatrix.Right = right;

                ////Criar matriz translaçao (movimento do tank)
                Matrix translationMatrix = Matrix.CreateTranslation(pos);

                ////Criar matriz escala (escala do tank)
                Matrix scaleMatrix = Matrix.CreateScale(scale);

                ////adiciona modificaçoes ao tank
                tankModel.Root.Transform = scaleMatrix * rotationMatrix * translationMatrix;

               
                ////Aplicar as transformaçoes na torre e no canhao
                turretBone.Transform = Matrix.CreateRotationY(turretAng) * turretTransform;
                cannonBone.Transform = Matrix.CreateRotationX(cannonAng) * cannonTransform;

                ////adiciona as mudanças "boneTransforms"
                tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

                if (hp <= 0)
                    alive = false;

                if (moving == 1)
                {
                    po.SpawnParticles(gameTime, boneTransforms[rBWheelBone.Index].Translation + (Vector3.Normalize(boneTransforms[rBWheelBone.Index].Right) * 0.1f), tankModel.Root.Transform.Backward);
                    po.SpawnParticles(gameTime, boneTransforms[lBWheelBone.Index].Translation + (Vector3.Normalize(boneTransforms[lBWheelBone.Index].Left) * 0.1f), tankModel.Root.Transform.Backward);
                }
                po.Update(gameTime, ref map);
            }
        }

        public bool Colides(ref Tank enemyTank)
        {
            if (enemyTank.alive)
            {
                Vector3[] points = new Vector3[4];
                Vector3[] enemyColisionPoints = new Vector3[4];
                Vector3[] normals = new Vector3[4];

                Vector3 auxVec;

                normals[0] = Vector3.Normalize(transform.Backward);
                normals[1] = Vector3.Normalize(transform.Left);
                normals[2] = Vector3.Normalize(transform.Forward);
                normals[3] = Vector3.Normalize(transform.Right);

                points[0] = boneTransforms[enemyTank.rFWheelBone.Index].Translation + normals[0] * 0.3f;
                points[1] = boneTransforms[enemyTank.rBWheelBone.Index].Translation + normals[1] * 0.3f;
                points[2] = boneTransforms[enemyTank.lBWheelBone.Index].Translation + normals[2] * 0.3f;
                points[3] = boneTransforms[enemyTank.lFWheelBone.Index].Translation + normals[3] * 0.3f;

                enemyColisionPoints[0] = enemyTank.boneTransforms[enemyTank.rFWheelBone.Index].Translation;
                enemyColisionPoints[1] = enemyTank.boneTransforms[enemyTank.rBWheelBone.Index].Translation;
                enemyColisionPoints[2] = enemyTank.boneTransforms[enemyTank.lBWheelBone.Index].Translation;
                enemyColisionPoints[3] = enemyTank.boneTransforms[enemyTank.lFWheelBone.Index].Translation;

                for (int ePointsI = 0; ePointsI < 4; ePointsI++)
                {
                    int n = 0;
                    for (int pointsI = 0; pointsI < 4; pointsI++)
                    {
                        auxVec = enemyColisionPoints[ePointsI] - points[pointsI];
                        auxVec.Normalize();


                        if (Vector3.Dot(auxVec, normals[pointsI]) < 0)
                            n++;
                    }
                    if (n == 4) return true;
                }
            }


            return false;

        }

        public void Draw(GraphicsDevice device, Matrix view, Matrix projection)
        {
            if (alive)
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
            po.Draw(device, view, projection);
            balas.Draw(view, projection);
        }

        
    }
}
