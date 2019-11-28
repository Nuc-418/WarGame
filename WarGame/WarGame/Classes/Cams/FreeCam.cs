using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarGame.Classes.Cams
{
    class FreeCam
    {

        public Matrix rotation, projection;
        public Vector3 camPos;
        Vector3 direction;
        Vector2 screenCenter;
        Vector2 mouseVec;
        float speed, mouseSensibility = 0.0001f;
        float yaw, pitch;
        
        //Construtor Cam
        public FreeCam(GraphicsDevice device,float speed)
        {
            //projection
            float aspectRatio = (float)device.Viewport.Width / device.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.01f, 10000.0f);

            //inicializar vars
            direction = Vector3.One;
            screenCenter = new Vector2(device.Viewport.Width / 2, device.Viewport.Height / 2);
            camPos = new Vector3(50, 20, 50);

            this.speed = speed;
        }

        //Update
        public Matrix UpdateCam(KeyboardState keyboard, MouseState mouse, GameTime gameTime)
        {

            //movimento da camara
            if (keyboard.IsKeyDown(Keys.NumPad8))
                camPos += direction * speed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.NumPad5))
                camPos -= direction * speed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.NumPad6))
                camPos += Vector3.Cross(direction, Vector3.Up) * speed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.NumPad4))
                camPos -= Vector3.Cross(direction, Vector3.Up) * speed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.NumPad7))
                camPos += Vector3.Up * speed * gameTime.ElapsedGameTime.Milliseconds;

            if (keyboard.IsKeyDown(Keys.NumPad1))
                camPos += Vector3.Down * speed * gameTime.ElapsedGameTime.Milliseconds;

            //Fazer set da posiçao do cursor para o centro
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);

            //obter o vector Delta da posiçao do cursor
            mouseVec = mouse.Position.ToVector2() - screenCenter;

            //modificar yaw
            yaw = yaw + (mouseVec.X * mouseSensibility * gameTime.ElapsedGameTime.Milliseconds);

            //optimizar variavel yaw
            if (yaw >= 2 * Math.PI || yaw <= -2 * Math.PI)
                yaw = 0;

            //modificar pitch
            pitch = pitch + (mouseVec.Y * mouseSensibility * gameTime.ElapsedGameTime.Milliseconds);

            //condicionar movimento em pitch
            if (pitch > Math.PI / 3)
                pitch = (float)(Math.PI / 3);
            if (pitch < -(Math.PI / 3))
                pitch = (float)-(Math.PI / 3);

            //obter matriz rotaçao
            rotation = Matrix.CreateFromYawPitchRoll(-yaw, -pitch, 0.0f);

            //obter vector direçao
            direction = Vector3.Transform(-Vector3.UnitZ, rotation);
            direction.Normalize();

            //obter view
            return Matrix.CreateLookAt(camPos, direction + camPos/*Obter ponto "LookAt"*/, Vector3.Up);
        }
    }
}
