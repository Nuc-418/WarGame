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
    class TankCam
    {
        public Matrix projection;
        public Vector3 camPos;
        bool switchedAR;

        //Construtor Cam
        public TankCam(GraphicsDevice device)
        {
            camPos = new Vector3(50, 0, 50);
            float aspectRatio = ((float)device.Viewport.Width / 2) / device.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.001f, 1000.0f);
        }

        //Update
        public Matrix UpdateCam(ref Tank tank, GameTime gameTime)
        {
            //obter posiçao da camara 
            camPos = 
                tank.boneTransforms[tank.turretBone.Index].Translation + 
                Vector3.Normalize(tank.boneTransforms[tank.turretBone.Index].Forward)*1.5f+
                Vector3.Normalize(tank.boneTransforms[tank.turretBone.Index].Up)*0.5f ;

            //obter view
            return Matrix.CreateLookAt
                (
                camPos, 
                tank.boneTransforms[tank.turretBone.Index].Translation + 
                Vector3.Normalize(tank.boneTransforms[tank.cannonBone.Index].Backward), 
                tank.boneTransforms[tank.turretBone.Index].Up
                );
        }

        public void SwitchAspectRatio(GraphicsDevice device)
        {
            float aspectRatio = ((switchedAR) ? ((float)device.Viewport.Width / 2) : (device.Viewport.Width)) / device.Viewport.Height;
            switchedAR = !switchedAR;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.001f, 1000.0f);
        }
    }
}

