using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using WarGame.Classes;
using WarGame.Classes.Cams;

namespace WarGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Map map;
        Tank tank;
        Tank enemyTank;



        Matrix view;
        Matrix projection;
        Matrix enemyView;

        int U, D;

        int viewWidth;
        int viewHeight;

        bool toggleAux;
        bool toggleFlag;

        private Viewport Viewport;
        private Viewport enemyViewport;

        CamSF camSF;
        TankCam tankCam;
        FreeCam freeCam;

        short selectedCam;

        float camHeight;


        public Game1()
        {
            //graphics
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            //graphics.PreferredBackBufferWidth = 1920;
            //graphics.PreferredBackBufferHeight = 1080;
            viewWidth = graphics.PreferredBackBufferWidth;
            viewHeight = graphics.PreferredBackBufferHeight;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //altura da camara surface follow
            camHeight = 3;

            //Criaçao das viewports para fazer split screen
            Viewport = new Viewport();
            Viewport.X = 0;
            Viewport.Y = 0;
            Viewport.Width = (int)(viewWidth / 2f);
            Viewport.Height = viewHeight;
            Viewport.MinDepth = 0;
            Viewport.MaxDepth = 1;

            enemyViewport = new Viewport();
            enemyViewport.X = (int)(viewWidth / 2f);
            enemyViewport.Y = 0;
            enemyViewport.Width = (int)(viewWidth / 2f);
            enemyViewport.Height = viewHeight;
            enemyViewport.MinDepth = 0;
            enemyViewport.MaxDepth = 1;


            base.Initialize();
        }

        protected override void LoadContent()
        {
            //New camara
            tankCam = new TankCam(GraphicsDevice);
            camSF = new CamSF(GraphicsDevice, /*Speed*/0.005f);
            freeCam = new FreeCam(GraphicsDevice,/*Speed*/ 0.005f);

            //New mapa
            map = new Map(GraphicsDevice, Content.Load<Texture2D>("map"), Content.Load<Texture2D>("terrainTexture"), 0.05f, 100);

            //New tank
            tank = new Tank(GraphicsDevice,Content.Load<Model>("tank\\tank"), Content.Load<Model>("tank\\bala"), 0.001f, new Vector3(50.0f, 10.0f, 50.0f));

            //New enemy tank
            enemyTank = new Tank(GraphicsDevice,Content.Load<Model>("tank\\tank"), Content.Load<Model>("tank\\bala"), 0.001f, new Vector3(50.0f, 10.0f, 51.0f));

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);




        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Console.WriteLine("Draw - update = " + (D - U));
            U++;



            KeyboardState keyboard = Keyboard.GetState();

            //Selecçao da camara
            if (keyboard.IsKeyDown(Keys.F1))
                selectedCam = 0;
            else if (keyboard.IsKeyDown(Keys.F2))
                selectedCam = 1;
            else if (keyboard.IsKeyDown(Keys.F3))
                selectedCam = 2;

            /////////////////////////////////////////
            if (keyboard.IsKeyUp(Keys.Tab))
                toggleAux = true;
            if (toggleAux == true && keyboard.IsKeyDown(Keys.Tab))
            {
                toggleFlag = !toggleFlag;
                tankCam.SwitchAspectRatio(GraphicsDevice);
                toggleAux = false;
            }

            switch (selectedCam)
            {
                case 0://Camara do tank



                    //update do tank player1
                    tank.UpdatePlayer(keyboard, gameTime, ref map, ref enemyTank);
                    //Update da camara do player1
                    view = tankCam.UpdateCam(ref tank, gameTime);

                    if (enemyTank.alive)
                        if (toggleFlag)
                            enemyTank.UpdateEnemyBot(gameTime, ref map, ref tank);
                        else
                        {
                            //update do tank player2
                            enemyTank.UpdateEnemyPlayer(keyboard, gameTime, ref tank, ref map);
                            //Update da camara do player2
                            enemyView = tankCam.UpdateCam(ref enemyTank, gameTime);
                        }

                    break;

                case 1: //Surface Follow

                    //Update dos tanks

                    tank.UpdatePlayer(keyboard, gameTime, ref map, ref enemyTank);

                    if (enemyTank.alive)
                        if (toggleFlag)
                            enemyTank.UpdateEnemyBot(gameTime, ref map, ref tank);
                        else
                            enemyTank.UpdateEnemyPlayer(keyboard, gameTime, ref tank, ref map);
                    
                    //update da camara "Surface Follow" e atribuir a "view" da "camSF" a view
                    view = camSF.UpdateCam(keyboard, Mouse.GetState(), ref map, gameTime, map.heightMapTexture.Width, camHeight);


                    break;

                case 2:

                    //Update dos tanks
                    tank.UpdatePlayer(keyboard, gameTime, ref map, ref enemyTank);

                    if (enemyTank.alive)
                        if (toggleFlag)
                            enemyTank.UpdateEnemyBot(gameTime, ref map, ref tank);
                        else
                            enemyTank.UpdateEnemyPlayer(keyboard, gameTime, ref tank, ref map);

                    //update da camara "Surface Follow" e atribuir a "view" da "camSF" a view
                    view = freeCam.UpdateCam(keyboard, Mouse.GetState(), gameTime);

                    break;
            }



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            U = 0;
            D = 0;

            D++;



            switch (selectedCam)
            {
                case 0://Camara do tank

                    //Bot/player2
                    if (toggleFlag)
                    {
                        projection = tankCam.projection;

                        map.Draw(GraphicsDevice, view, projection);

                        tank.Draw(GraphicsDevice,view, projection);

                        enemyTank.Draw(GraphicsDevice, view, projection);


                    }
                    else
                    {
                        Viewport original = graphics.GraphicsDevice.Viewport;

                        //obter projeçao da camara "tankCam"
                        projection = tankCam.projection;

                        //Alterar viewport/////////////
                        graphics.GraphicsDevice.Viewport = Viewport;

                        //Desenha mapa
                        map.Draw(GraphicsDevice, view, projection);

                        //Desenha "tank"
                        tank.Draw(GraphicsDevice, view, projection);


                        //Desenha "enemyTank"
                        enemyTank.Draw(GraphicsDevice, view, projection);

                        //Alterar viewport/////////////
                        graphics.GraphicsDevice.Viewport = enemyViewport;

                        //Desenha mapa
                        map.Draw(GraphicsDevice, enemyView, projection);

                        //Desenha "tank"
                        tank.Draw(GraphicsDevice, enemyView, projection);


                        //Desenha "enemyTank"
                        enemyTank.Draw(GraphicsDevice, enemyView, projection);

                        //Reset do "GraphicsDevice.Viewport"
                        GraphicsDevice.Viewport = original;
                    }
                    break;

                case 1: //Surface Follow
                    //obter projeçao da camara "camSF"
                    projection = camSF.projection;

                    //Desenha mapa
                    map.Draw(GraphicsDevice, view, projection);

                    //Desenha "tank"
                    tank.Draw(GraphicsDevice, view, projection);

                    //Desenha "enemyTank"
                    enemyTank.Draw(GraphicsDevice, view, projection);

                    break;

                case 2:
                    //obter projeçao da camara "freeCam"
                    projection = freeCam.projection;
                    //Desenha mapa
                    map.Draw(GraphicsDevice, view, projection);
                    //Desenha "enemyTank"
                    enemyTank.Draw(GraphicsDevice, view, projection);
                    //Desenha "tank"
                    tank.Draw(GraphicsDevice, view, projection);
                    break;
            }
            
            base.Draw(gameTime);
        }
    }
}
