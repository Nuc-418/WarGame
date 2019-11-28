using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WarGame
{
    class Bala
    {
        private Matrix transform;
        public Model balaModel;
        public Vector3 pos, dir;
        private Vector3 vel;
        private Vector3 acceleration, accDir;
        private float accRate;
        private float scale;
        private float lifeTime;


        public Bala(Model balaModel, Vector3 pos, Vector3 dir, Vector3 accelerationDir, float accRate,
            float scale, float speed, float lifeTime)
        {
            this.balaModel = balaModel;
            this.pos = pos;
            this.dir = dir;
            this.accDir = accelerationDir;
            this.accRate = accRate;
            this.scale = scale;
            this.lifeTime = lifeTime;

            //Velocidade inicial
            vel = dir * speed;
            //aceleraçao
            acceleration = accDir * accRate * (1f / 60f);

            //set tank scale
            this.balaModel.Root.Transform = Matrix.CreateScale(scale);
        }

        public bool Update(GameTime gt, ref Map map, ref Tank enemyTank)
        {

            //Phisics
            vel += acceleration * (float)gt.ElapsedGameTime.TotalSeconds;
            pos += vel;

            //destruir bala
            float dist = Vector3.Distance(enemyTank.pos, pos);
            lifeTime -= (float)gt.ElapsedGameTime.TotalSeconds;

            //limites do mapa
            if (pos.X > map.width - 2)
                return true;
            if (pos.X < 2)
                return true;
            if (pos.Z > map.height - 2)
                return true;
            if (pos.Z < 2)
                return true;

            // tempo de vida e colisao com o mapa
            if (pos.Y <= map.GetY(pos) || lifeTime <= 0)
                return true;

            //colisao com tank enimigo
            if (dist <= 0.5f && enemyTank.alive)
            {
                enemyTank.hp -= 20;//Damage done
                return true;
            }

            //obter vetor right e dir
            Vector3 right = Vector3.Cross(Vector3.Up, Vector3.Normalize(vel));
            right.Normalize();
            Vector3 normal = Vector3.Cross(right, Vector3.Normalize(vel));
            normal.Normalize();

            //Criar matriz rotaçao
            Matrix rotationMatrix = Matrix.Identity;
            rotationMatrix.Forward = right;
            rotationMatrix.Up = normal;
            rotationMatrix.Right = -Vector3.Normalize(vel);

            //Criar matriz translaçao
            Matrix translationMatrix = Matrix.CreateTranslation(pos);

            //Criar matriz escala
            Matrix scaleMatrix = Matrix.CreateScale(scale);

            //adiciona modificaçoes 
            balaModel.Root.Transform = scaleMatrix * rotationMatrix * translationMatrix;
            transform = balaModel.Root.Transform;

            return false;
        }

        public void Draw(Matrix view, Matrix projection)
        {

            foreach (ModelMesh mesh in balaModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    //View
                    effect.World = transform;
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

    class GestorBalas
    {
        Model balaModel;
        List<Bala> balas = new List<Bala>();
        Vector3 accelerationDir;
        float accRate;
        float scale;
        float speed;
        float lifeTime;

        public GestorBalas(Model balaModel, Vector3 accelerationDir, float accRate, float scale, float speed, float lifeTime)
        {
            this.balaModel = balaModel;
            this.accelerationDir = accelerationDir;
            this.accRate = accRate;
            this.scale = scale;
            this.speed = speed;
            this.lifeTime = lifeTime;
        }
        public void Disparar(Vector3 pos, Vector3 dir)
        {
            //adiciona nova bala
            balas.Add(new Bala(balaModel, pos, dir, accelerationDir, accRate, scale, speed, lifeTime));
        }
        public void Update(GameTime gt, ref Map map, ref Tank enemyTank)
        {
            //Gestao das balas em "balas"
            for (int i = 0; i < balas.Count; i++)
                if (balas[i].Update(gt, ref map, ref enemyTank))
                    balas.RemoveAt(i--);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            //draw das balas
            foreach (Bala bala in balas)
                bala.Draw(view, projection);
        }
    }

}
