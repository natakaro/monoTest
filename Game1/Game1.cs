using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Text;

namespace Game1
{

    public class Game1 : Game
    {
        public static GraphicsDeviceManager graphics; //Ustawione na razie na static, żeby nie trzeba było dogrzebywać się do tego z każdej klasy
        private SpriteBatch spriteBatch;
        private Camera camera;
        private SpriteFont spriteFont;

        private Octree octree;
        InstancingDraw temp;
        private Texture2D cross;
        //float skala = 0.5f;
        

        //Matrix worldMatrix;

        //Geometric info
        //Model model;
        //Model model2;
        //Model model3;

        //Mapa
        //int size = 30;
        //int[,] mapa;
        Map mapa;

        private const float CAMERA_FOVX = 85.0f;
        private const float CAMERA_ZNEAR = 0.01f;
        private const float CAMERA_ZFAR = 2000.0f;
        private const float CAMERA_PLAYER_EYE_HEIGHT = 30.0f;
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

        private int modelsDrawn;

        private float distance;
        private DrawableObject tileStandingOn;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;

            Content.RootDirectory = "Content";
            
            camera = new Camera(this);
            Components.Add(camera);

            IsFixedTimeStep = false; //to ustawione na false albo vsync na true potrzebne
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Setup the window to be a quarter the size of the desktop.
            windowWidth = GraphicsDevice.DisplayMode.Width / 2;
            windowHeight = GraphicsDevice.DisplayMode.Height / 2;

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false; //vsync
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            // Initial position for text rendering.
            fontPos = new Vector2(1.0f, 1.0f);

            camera.worldMatrix = Matrix.CreateWorld(new Vector3(), Vector3.Forward, Vector3.Up);           

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

            mapa = new Map(this, 30, camera.worldMatrix);
            mapa.Initialize(Content);

            //octree
            octree = new Octree(mapa.TileList);
        }

        protected override void LoadContent()
        {
            cross = Content.Load<Texture2D>("cross_cross");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"fonts\DemoFont");
            temp = new InstancingDraw(this, camera, Content);
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

            if (KeyJustPressed(Keys.D1))
            {
                mapa.reload();
                Map.efekt = !Map.efekt;
            }
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

            octree.Update(gameTime);
            base.Update(gameTime);
            
            ProcessKeyboard();

            //poruszanie po y w zaleznosci od pozycji tila - czy powinno to byc w update a nie gdzies indziej?
            if (camera.CurrentVelocity != Vector3.Zero)
            {
                Ray yRay = camera.GetDownwardRay();
                IntersectionRecord ir = octree.NearestIntersection(yRay);
                if (ir != null && ir.DrawableObjectObject != null && tileStandingOn != ir.DrawableObjectObject)//..ujowy if ale działa
                {
                     //dla debugu, wywalic potem
                    //distance = (float)yRay.Intersects(ir.DrawableObjectObject.BoundingBox); //paskudne
                    //camera.Move(0, (distance-20)*-1, 0);
                    distance = ir.DrawableObjectObject.Position.Y; //prosciej
                    //camera.Move(0, (camera.Position.Y - distance-20) * -1, 0); //move jest natychmiastowy a nie plynny jak cala reszta kamery wiec wyglada niefajnie, plus do tego psuje kucanie - uzyc czegos z velocity w kamerze?
                    camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT + distance;
                    //camera.CurrentY = distance*10;
                    tileStandingOn = ir.DrawableObjectObject;
                }
            }

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
            else if (tileStandingOn != null) //chwilowe obejscie, wywalic potem caly elseif
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

                //buffer.AppendFormat("  Ray Position: x:{0} y:{1} z:{2}\n",
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Position.X.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Position.Y.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Position.Z.ToString("f2"));
                //buffer.AppendFormat("  Ray Direction: x:{0} y:{1} z:{2}\n",
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Direction.X.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Direction.Y.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Direction.Z.ToString("f2"));
                //buffer.AppendFormat(mapa.mapa[0, 0].model.Meshes[0].BoundingSphere.Radius.ToString());
                buffer.AppendFormat(" Models drawn: {0}",
                    modelsDrawn.ToString("f2"));
                buffer.AppendFormat("  Ray Position: x:{0} y:{1} z:{2}\n",
                    camera.GetDownwardRay().Position.X.ToString("f2"),
                    camera.GetDownwardRay().Position.Y.ToString("f2"),
                    camera.GetDownwardRay().Position.Z.ToString("f2"));
                buffer.AppendFormat("  Ray Direction: x:{0} y:{1} z:{2}\n",
                    camera.GetDownwardRay().Direction.X.ToString("f2"),
                    camera.GetDownwardRay().Direction.Y.ToString("f2"),
                    camera.GetDownwardRay().Direction.Z.ToString("f2"));

                buffer.AppendFormat(" Distance: {0}\n",
                    distance.ToString("f2"));
                buffer.AppendFormat(" TileStandingOn Y: {0}\n",
                    tileStandingOn.Position.Y.ToString("f2"));

                buffer.Append("\nPress H to display help");
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

                //buffer.AppendFormat("  Ray Position: x:{0} y:{1} z:{2}\n",
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Position.X.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Position.Y.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Position.Z.ToString("f2"));
                //buffer.AppendFormat("  Ray Direction: x:{0} y:{1} z:{2}\n",
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Direction.X.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Direction.Y.ToString("f2"),
                //    camera.GetMouseRay(graphics.GraphicsDevice.Viewport).Direction.Z.ToString("f2"));
                //buffer.AppendFormat(mapa.mapa[0, 0].model.Meshes[0].BoundingSphere.Radius.ToString());
                buffer.AppendFormat(" Models drawn: {0}",
                    modelsDrawn.ToString("f2"));
                buffer.AppendFormat("  Ray Position: x:{0} y:{1} z:{2}\n",
                    camera.GetDownwardRay().Position.X.ToString("f2"),
                    camera.GetDownwardRay().Position.Y.ToString("f2"),
                    camera.GetDownwardRay().Position.Z.ToString("f2"));
                buffer.AppendFormat("  Ray Direction: x:{0} y:{1} z:{2}\n",
                    camera.GetDownwardRay().Direction.X.ToString("f2"),
                    camera.GetDownwardRay().Direction.Y.ToString("f2"),
                    camera.GetDownwardRay().Direction.Z.ToString("f2"));

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
            //GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            //GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            modelsDrawn = 0;

            //Renders all visible objects by iterating through the oct tree recursively and testing for intersection 
            //with the current camera view frustum

            /*
            foreach (IntersectionRecord ir in octree.AllIntersections(camera.Frustum))
            {
                // ir.DrawableObjectObject.SetDirectionalLight(m_globalLight[0].Direction, m_globalLight[0].Color);
                // ir.DrawableObjectObject.UpdateLOD(camera);
                ir.DrawableObjectObject.Draw(camera);
                modelsDrawn++;
            }*/

            temp.DrawModelHardwareInstancing(octree.AllIntersections(camera.Frustum));
            modelsDrawn = octree.AllIntersections(camera.Frustum).Count;

            spriteBatch.Begin();
            spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
            spriteBatch.End();

            DrawText();
            
            base.Draw(gameTime);
            IncrementFrameCounter();
        }
    }
}