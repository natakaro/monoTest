using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Game1
{

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D cross;
        //float skala = 0.5f;
        float skala = 10f;
        float szerokosc;
        float wysokosc;
        float odleglosc;




        //Camera
        float x;
        float y;
        float z;
        Vector3 camTarget;
        Vector3 camPosition;
        Matrix projectionMatrix;
        Matrix viewMatrix;
        Matrix worldMatrix;
        Vector4 kolor = new Vector4(0, 0, 1, 1);
        //Geometric info
        Model model;
        Model model2;
        Model model3;

        //Mapa
        int size = 30;
        int[,] mapa;
        //Orbit
        bool orbit = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();

            //Setup Camera
            camTarget = new Vector3(0f, 0f, 0f);
            camPosition = new Vector3(0f, 0f, -10);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f), graphics.
                               GraphicsDevice.Viewport.AspectRatio,
                1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);

            model = Content.Load<Model>("1");
            model2 = Content.Load<Model>("2");
            model3 = Content.Load<Model>("3");
            cross = Content.Load<Texture2D>("cross_cross");
            szerokosc = 2 * skala;
            wysokosc = (float)Math.Sqrt(3) / 2 * szerokosc;
            odleglosc = 0.75f * szerokosc;
            mapa = new int[size, size];
            Random a = new Random();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    mapa[i, j] = a.Next(1, 4);
                }
            }
        }

        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
                ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
                Keys.Escape))
            {
                Exit();
            }


            if (Keyboard.GetState().IsKeyDown(Keys.V))
            {

            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                camPosition.X += 0.1f;
                camTarget.X += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                camPosition.X -= 0.1f;
                camTarget.X -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                camPosition.Z -= 0.1f;
                camTarget.Z -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                camPosition.Z += 0.1f;
                camTarget.Z += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                camPosition.X -= 0.1f;
                camTarget.X -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                camPosition.X += 0.1f;
                camTarget.X += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                camPosition.Y += 0.1f;
                camTarget.Y += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                camPosition.Y -= 0.1f;
                camTarget.Y -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                camPosition.Z += 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                camPosition.Z -= 0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                orbit = !orbit;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                graphics.ToggleFullScreen();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D7))
            {
                kolor = new Vector4(1, 0, 0, 1);
            }

            /*if (Mouse.GetState().X > mm) 
            {
                Matrix rotationMatrix = Matrix.CreateRotationY(
                                        MathHelper.ToRadians(1f));
                camPosition = Vector3.Transform(camPosition,
                              rotationMatrix);

                viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
                base.Update(gameTime);
            }
            if (Mouse.GetState().X < mm)
            {
                Matrix rotationMatrix = Matrix.CreateRotationY(
                                        MathHelper.ToRadians(-1f));
                camPosition = Vector3.Transform(camPosition,
                              rotationMatrix);

                viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
                base.Update(gameTime);
            }*/
            if (orbit)
            {
                Matrix rotationMatrix = Matrix.CreateRotationY(
                                        MathHelper.ToRadians(1f));
                camPosition = Vector3.Transform(camPosition,
                              rotationMatrix);
            }
            /*if (Mouse.GetState().X <= 0 || Mouse.GetState().Y <= 0 || Mouse.GetState().X >= graphics.PreferredBackBufferWidth || Mouse.GetState().Y >= graphics.PreferredBackBufferHeight)
            {
                Mouse.SetPosition(100, 100);
            }*/
            viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
                         Vector3.Up);
            base.Update(gameTime);
            //mm = Mouse.GetState().Position.X;
            //Debug.WriteLine(Mouse.GetState().X);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            //GraphicsDevice.Clear(new Color(kolor));
            /*foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1f, 1f, 1f);
                    effect.View = viewMatrix;
                    //effect.World = worldMatrix * Matrix.CreateScale(scalee) * Matrix.CreateTranslation(camtarget) * Matrix.CreateRotationX(MathHelper.PiOver2);
                    effect.World = worldMatrix * Matrix.CreateScale(scalee);// * Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f));
                    effect.Projection = projectionMatrix;
                    //effect.DiffuseColor = Color.Red.ToVector3();
                    //effect.SpecularPower = 70;
                    effect.TextureEnabled = true;
                    
                }
                mesh.Draw();
            }*/

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Model temp = Content.Load<Model>(mapa[i, j].ToString());
                    temp.Draw(Matrix.CreateScale(skala) * Matrix.CreateTranslation(i * odleglosc, 0, (j * wysokosc) + (i % 2) * wysokosc / 2) * worldMatrix, viewMatrix, projectionMatrix);
                    /*switch (mapa[i, j])
                    {
                        case 2:
                            model2.Draw(Matrix.CreateScale(skala) * Matrix.CreateTranslation(i * odleglosc, 0, (j * wysokosc) + (i % 2) * wysokosc / 2) * worldMatrix, viewMatrix, projectionMatrix);
                            break;
                        case 3:
                            model3.Draw(Matrix.CreateScale(skala) * Matrix.CreateTranslation(i * odleglosc, 0, (j * wysokosc) + (i % 2) * wysokosc / 2) * worldMatrix, viewMatrix, projectionMatrix);
                            break;
                        default:
                            model.Draw(Matrix.CreateScale(skala) * Matrix.CreateTranslation(i * odleglosc, 0, (j * wysokosc) + (i % 2) * wysokosc / 2) * worldMatrix, viewMatrix, projectionMatrix);
                            break;
                    }*/
                }

            }

            spriteBatch.Begin();
            spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);

        }
    }
}