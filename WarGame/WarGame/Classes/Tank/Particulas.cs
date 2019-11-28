using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarGame
{
    class Particula
    {
        Vector3 pos, dir, vel, acc, accDir;
        float accRate;
        float lifeTime;


        public Particula(Vector3 posInicial, Vector3 dirInicial, Vector3 accDir, float velInicial, float accRate, float lifeTime)
        {
            this.pos = posInicial;
            this.accDir = accDir;
            this.accRate = accRate;
            this.lifeTime = lifeTime;

            //Velocidade inicial
            this.vel = dirInicial * velInicial * (1f / 60f);

            //aceleraçao
            acc += accDir * accRate * (1f / 60f);
        }

        public bool Update(GameTime gt, ref Map map)
        {
            //Phisics
            vel += acc * (float)gt.ElapsedGameTime.TotalSeconds;
            pos += vel;

            lifeTime -= (float)gt.ElapsedGameTime.TotalSeconds;
            if (pos.Y <= map.GetY(pos) || lifeTime <= 0)
                return true;
            return false;
        }

        public void Draw(GraphicsDevice device)
        {
            //cria vertices
            VertexPositionColor[] pontos = new VertexPositionColor[2];
            pontos[0] = new VertexPositionColor(pos, Color.Red);
            pontos[1] = new VertexPositionColor(pos + (Vector3.Up * 0.01f), Color.Red);

            //desenha linha
            device.DrawUserPrimitives(PrimitiveType.LineList, pontos, 0, 1);
        }
    }

    class GestorParticulas
    {
        public List<Particula> particulas = new List<Particula>();
        Vector3 dir;
        Vector3 vecGravidade;
        
        float dirDeviation;
        float velInicial;
        float aceleracao;
        float tVida;
        float spawningTime, time;
        float raio;
        float vel;

        int nParticulas;
        Random r;

        BasicEffect effect;

        public GestorParticulas(GraphicsDevice device, Vector3 vecGravidade, float dirDeviation, float velInicial, float aceleracao, float tVida, float raio, float spawningTime, int nParticulas)
        {
            effect = new BasicEffect(device);

            //Var
            this.vecGravidade = vecGravidade;
            this.dirDeviation = dirDeviation;
            this.velInicial = velInicial;
            this.aceleracao = aceleracao;
            this.tVida = tVida;
            this.raio = raio;
            this.spawningTime = spawningTime;
            this.nParticulas = nParticulas;

            //inicializa random
            r = new Random();
        }

        public void SpawnParticles(GameTime gt, Vector3 pos, Vector3 normal)
        {
            //controlo de spawn por unidade de tempo
            time += (float)gt.ElapsedGameTime.TotalSeconds;
            if (time >= spawningTime)
            {
                for (int n = 0; n <= nParticulas; n++)
                {
                    //rand do angulo
                    float alpha = (float)r.NextDouble() * MathHelper.TwoPi;

                    //rand de distancia em x (auxRaio)
                    float auxRaio = (float)r.NextDouble() * raio;

                    //Calcular direcao
                    dir = normal + ((dirDeviation > 0) ? ((new Vector3((float)(dirDeviation * ((r.NextDouble() * 2f) - 1f)), (float)(dirDeviation * ((r.NextDouble() * 2f) - 1f)), (float)(dirDeviation * ((r.NextDouble() * 2f) - 1f))))) : Vector3.Zero);


                    //adicionar particula á lista
                    particulas.Add(
                        new Particula(
                        new Vector3(((float)Math.Cos(alpha) * auxRaio) + pos.X, pos.Y, ((float)Math.Sin(alpha) * auxRaio) + pos.Z),
                        dir,
                        vecGravidade,
                        velInicial,
                        aceleracao,
                        tVida)
                        );
                }
                time = 0;
            }
        }

        public void Update(GameTime gt, ref Map map)
        {
            //fazer update as particulas e caso estejao mortas sao elininadas da lista
            for (int i = 0; i < particulas.Count; i++)
                if (particulas[i].Update(gt, ref map))
                    particulas.RemoveAt(i--);
        }

        public void Draw(GraphicsDevice device, Matrix view, Matrix projection)
        {
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;
            effect.CurrentTechnique.Passes[0].Apply();
            //faz draw de todas as particulas na lista "particulas"
            foreach (Particula p in particulas)
                p.Draw(device);
        }

    }
}
