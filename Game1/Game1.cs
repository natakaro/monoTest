using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Text;

namespace Game1
{

    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Camera camera;
        private SpriteFont spriteFont;

        private Texture2D cross;
        //float skala = 0.5f;
        float skala = 10f;
        float szerokosc;
        float wysokosc;
        float odleglosc;

        //Camera
        float x;
        float y;
        float z;
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

        private const float CAMERA_FOVX = 85.0f;
        private const float CAMERA_ZNEAR = 0.01f;
        private const float CAMERA_ZFAR = 1000.0f;
        private const float CAMERA_PLAYER_EYE_HEIGHT = 110.0f;
        private const float CAMERA_ACCELERATION_X = 800.0f;
        private const float CAMERA_ACCELERATION_Y = 800.0f;
        private const float CAMERA_ACCELERATION_Z = 800.0f;
        private const float CAMERA_VELOCITY_X = 200.0f;
        private const float CAMERA_VELOCITY_Y = 200.0f;
        private const float CAMERA_VELOCITY_Z = 200.0f;
        private const float CAMERA_RUNNING_MULTIPLIER = 2.0f;
        private const float CAMERA_RUNNING_JUMP_MULTIPLIER = 1.5f;

        private KeyboardState currentKeyboardState;
        private KeyboardState prevKeyboardState;
        private Vector2 fontPos;
        private int windowWidth;
        private int windowHeight;
        private int frames;
        private int framesPerSecond;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private bool displayHelp;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            Content.RootDirectory = "Content";

            camera = new Camera(this);
            Components.Add(camera); //?
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Setup the window to be a quarter the size of the desktop.
            windowWidth = GraphicsDevice.DisplayMode.Width / 2;
            windowHeight = GraphicsDevice.DisplayMode.Height / 2;

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            // Initial position for text rendering.
            fontPos = new Vector2(1.0f, 1.0f);

            //Setup Camera
            //camera.cameraTarget = new Vector3(0f, 0f, 0f);
            //camera.cameraPosition = new Vector3(0f, 0f, -10);
            //camera.cameraUp = new Vector3(0f, 1f, 0f);
            //camera.cameraDirection = camera.cameraTarget - camera.cameraPosition;
            //camera.cameraDirection.Normalize();
            //camera.CreateLookAt();

            //camera.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Window.ClientBounds.Width / (float)Window.ClientBounds.Height, 1, 100);
            //projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            //                   MathHelper.ToRadians(45f), graphics.
            //                   GraphicsDevice.Viewport.AspectRatio,
            //    1f, 1000f);
            //viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
            //             new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(new Vector3(0, 100, 0), Vector3.Forward, Vector3.Up);



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

            //Setup the camera.
            camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT;
            camera.Acceleration = new Vector3(
                CAMERA_ACCELERATION_X,
                CAMERA_ACCELERATION_Y,
                CAMERA_ACCELERATION_Z);
            camera.VelocityWalking = new Vector3(
                CAMERA_VELOCITY_X,
                CAMERA_VELOCITY_Y,
                CAMERA_VELOCITY_Z);
            camera.VelocityRunning = new Vector3(
                camera.VelocityWalking.X * CAMERA_RUNNING_MULTIPLIER,
                camera.VelocityWalking.Y * CAMERA_RUNNING_JUMP_MULTIPLIER,
                camera.VelocityWalking.Z * CAMERA_RUNNING_MULTIPLIER);
            camera.Perspective(
                CAMERA_FOVX,
                (float)windowWidth / (float)windowHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);

            currentKeyboardState = Keyboard.GetState();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"fonts\DemoFont");
        }

        protected override void UnloadContent()
        {
        }

        private void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            graphics.IsFullScreen = !graphics.IsFullScreen;

            if (graphics.IsFullScreen)
            {
                newWidth = GraphicsDevice.DisplayMode.Width;
                newHeight = GraphicsDevice.DisplayMode.Height;
            }
            else
            {
                newWidth = windowWidth;
                newHeight = windowHeight;
            }

            graphics.PreferredBackBufferWidth = newWidth;
            graphics.PreferredBackBufferHeight = newHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            float aspectRatio = (float)newWidth / (float)newHeight;

            camera.Perspective(CAMERA_FOVX, aspectRatio, CAMERA_ZNEAR, CAMERA_ZFAR);
        }
    
        private bool KeyJustPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }

        private void ProcessKeyboard()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (KeyJustPressed(Keys.Escape))
                this.Exit();

            if (KeyJustPressed(Keys.H))
                displayHelp = !displayHelp;

            if (KeyJustPressed(Keys.M))
                camera.EnableMouseSmoothing = !camera.EnableMouseSmoothing;

            //if (KeyJustPressed(Keys.P))
            //    enableParallax = !enableParallax;

            //if (KeyJustPressed(Keys.T))
            //    enableColorMap = !enableColorMap;

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) || currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }

            if (KeyJustPressed(Keys.Add))
            {
                camera.RotationSpeed += 0.01f;

                if (camera.RotationSpeed > 1.0f)
                    camera.RotationSpeed = 1.0f;
            }

            if (KeyJustPressed(Keys.Subtract))
            {
                camera.RotationSpeed -= 0.01f;

                if (camera.RotationSpeed <= 0.0f)
                    camera.RotationSpeed = 0.01f;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive)
                return;

            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back ==
            //    ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
            //    Keys.Escape))
            //{
            //    Exit();
            //}


            //if (Keyboard.GetState().IsKeyDown(Keys.V))
            //{

            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.A))
            //{
            //    camPosition.X += 0.1f;
            //    camTarget.X += 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.D))
            //{
            //    camPosition.X -= 0.1f;
            //    camTarget.X -= 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.S))
            //{
            //    camPosition.Z -= 0.1f;
            //    camTarget.Z -= 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.W))
            //{
            //    camPosition.Z += 0.1f;
            //    camTarget.Z += 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.Left))
            //{
            //    camPosition.X -= 0.1f;
            //    camTarget.X -= 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.Right))
            //{
            //    camPosition.X += 0.1f;
            //    camTarget.X += 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.Up))
            //{
            //    camPosition.Y += 0.1f;
            //    camTarget.Y += 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.Down))
            //{
            //    camPosition.Y -= 0.1f;
            //    camTarget.Y -= 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            //{
            //    camPosition.Z += 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            //{
            //    camPosition.Z -= 0.1f;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.Space))
            //{
            //    orbit = !orbit;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.D1))
            //{
            //    graphics.ToggleFullScreen();
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.D7))
            //{
            //    kolor = new Vector4(1, 0, 0, 1);
            //}

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
            //if (orbit)
            //{
            //    Matrix rotationMatrix = Matrix.CreateRotationY(
            //                            MathHelper.ToRadians(1f));
            //    camPosition = Vector3.Transform(camPosition,
            //                  rotationMatrix);
            //}
            /*if (Mouse.GetState().X <= 0 || Mouse.GetState().Y <= 0 || Mouse.GetState().X >= graphics.PreferredBackBufferWidth || Mouse.GetState().Y >= graphics.PreferredBackBufferHeight)
            {
                Mouse.SetPosition(100, 100);
            }*/
            //viewMatrix = Matrix.CreateLookAt(camPosition, camTarget,
            //             Vector3.Up);

            //mm = Mouse.GetState().Position.X;
            //Debug.WriteLine(Mouse.GetState().X);

            base.Update(gameTime);

            ProcessKeyboard();
            UpdateFrameRate(gameTime);
        }

        private void UpdateFrameRate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                framesPerSecond = frames;
                frames = 0;
            }
        }

        private void IncrementFrameCounter()
        {
            ++frames;
        }

        private void DrawText()
        {
            StringBuilder buffer = new StringBuilder();

            if (displayHelp)
            {
                buffer.AppendLine("Move mouse to free look");
                buffer.AppendLine();
                buffer.AppendLine("Press W and S to move forwards and backwards");
                buffer.AppendLine("Press A and D to strafe left and right");
                buffer.AppendLine("Press SPACE to jump");
                buffer.AppendLine("Press and hold LEFT CTRL to crouch");
                buffer.AppendLine("Press and hold LEFT SHIFT to run");
                buffer.AppendLine();
                buffer.AppendLine("Press M to toggle mouse smoothing");
                //buffer.AppendLine("Press P to toggle between parallax normal mapping and normal mapping");
                buffer.AppendLine("Press NUMPAD +/- to change camera rotation speed");
                buffer.AppendLine("Press ALT + ENTER to toggle full screen");
                buffer.AppendLine();
                buffer.AppendLine("Press H to hide help");
            }
            else
            {
                buffer.AppendFormat("FPS: {0}\n", framesPerSecond);
                //buffer.AppendFormat("Technique: {0}\n",
                //    (enableParallax ? "Parallax normal mapping" : "Normal mapping"));
                buffer.AppendFormat("Mouse smoothing: {0}\n\n",
                    (camera.EnableMouseSmoothing ? "on" : "off"));
                buffer.Append("Camera:\n");
                buffer.AppendFormat("  Position: x:{0} y:{1} z:{2}\n",
                    camera.Position.X.ToString("f2"),
                    camera.Position.Y.ToString("f2"),
                    camera.Position.Z.ToString("f2"));
                buffer.AppendFormat("  Orientation: heading:{0} pitch:{1}\n",
                    camera.HeadingDegrees.ToString("f2"),
                    camera.PitchDegrees.ToString("f2"));
                buffer.AppendFormat("  Velocity: x:{0} y:{1} z:{2}\n",
                    camera.CurrentVelocity.X.ToString("f2"),
                    camera.CurrentVelocity.Y.ToString("f2"),
                    camera.CurrentVelocity.Z.ToString("f2"));
                buffer.AppendFormat("  Rotation speed: {0}\n",
                    camera.RotationSpeed.ToString("f2"));
                buffer.Append("\nPress H to display help");
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
            spriteBatch.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

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
                    temp.Draw(Matrix.CreateScale(skala) * Matrix.CreateTranslation(i * odleglosc, 0, (j * wysokosc) + (i % 2) * wysokosc / 2) * worldMatrix, camera.ViewMatrix, camera.ProjectionMatrix);
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

            DrawText();
            
            base.Draw(gameTime);
            IncrementFrameCounter();
        }
    }
}