using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Game1.Spells;

namespace Game1
{

    public class Game1 : Game
    {
        public static GraphicsDeviceManager graphics; //Ustawione na razie na static, żeby nie trzeba było dogrzebywać się do tego z każdej klasy
        private SpriteBatch spriteBatch;
        private Camera camera;
        private SpriteFont spriteFont;

        Octree octree;
        public static Vector3 slonce = new Vector3(300, 300, -500);
        InstancingDraw temp;
        private Texture2D cross;

        private Effect fxaaEffect;
        private RenderTarget2D renderTarget;

        BoundingSphere cameraSphere;
        TestBox testbox;
        Wall wall;
        Model skybox;

        private bool instancing = false;
        private bool useFXAA = true;
        private bool debugShapes = false;
        private bool raybox = false;

        private const float CAMERA_FOVX = 90.0f;
        private const float CAMERA_ZNEAR = 0.01f;
        private const float CAMERA_ZFAR = 15000.0f;
        private const float CAMERA_PLAYER_EYE_HEIGHT = 30.0f;
        private const float CAMERA_ACCELERATION_X = 800.0f;
        private const float CAMERA_ACCELERATION_Y = 800.0f;
        private const float CAMERA_ACCELERATION_Z = 800.0f;
        private const float CAMERA_VELOCITY_X = 200.0f;
        private const float CAMERA_VELOCITY_Y = 200.0f;
        private const float CAMERA_VELOCITY_Z = 200.0f;
        private const float CAMERA_RUNNING_MULTIPLIER = 2.0f;
        private const float CAMERA_RUNNING_JUMP_MULTIPLIER = 1.5f;

        #region FXAA SETTINGS

        private float fxaaQualitySubpix = 0.5f;
        private float fxaaQualityEdgeThreshold = 0.166f;
        private float fxaaQualityEdgeThresholdMin = 0.0625f;

        #endregion

        private KeyboardState currentKeyboardState;
        private KeyboardState prevKeyboardState;
        private MouseState currentMouseState;
        private MouseState prevMouseState;
        private bool currentLeftButton;
        private bool currentRightButton;
        private bool prevLeftButton;
        private bool prevRightButton;
        private Vector2 fontPos;
        private int windowWidth;
        private int windowHeight;
        private int frames;
        private int framesPerSecond;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private bool displayHelp;

        private int modelsDrawn;
        private int modelsDrawnInstanced;

        private float distance;
        private DrawableObject tileStandingOn;

        public DrawableObject selected_obj = null;
        public enum SpellType
        {
            MoveTerrain = 1,
            Fireball = 2
        };

        private SpellMoveTerrain spellMoveTerrain;
        private SpellFireball spellFireball;

        public SpellType selectedSpell = SpellType.MoveTerrain;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            
            camera = new Camera(this);
            Components.Add(camera);

            IsFixedTimeStep = false; //to ustawione na false albo vsync na true potrzebne
        }

        protected override void Initialize()
        {
            
            base.Initialize();

            DebugShapeRenderer.Initialize(graphics.GraphicsDevice);

            // Setup the window to be a quarter the size of the desktop.
            windowWidth = GraphicsDevice.DisplayMode.Width;
            windowHeight = GraphicsDevice.DisplayMode.Height;

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false; //vsync
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            Window.Position = new Point(0, 0);

            //graphics.ToggleFullScreen();

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

            cameraSphere = new BoundingSphere(camera.Position - new Vector3(0, 30, 0), 10);

            PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            currentKeyboardState = Keyboard.GetState();

            testbox = new TestBox(this, camera.worldMatrix);
            wall = new Wall(this, camera.worldMatrix);
            //octree
            octree = new Octree(Map.CreateMap(this, 30, camera.worldMatrix));
            octree.m_objects.Add(testbox);
            octree.m_objects.Add(wall);

            spellMoveTerrain = new SpellMoveTerrain(octree);
            spellFireball = new SpellFireball(this, camera, octree);
            
        }

        protected override void LoadContent()
        {
            cross = Content.Load<Texture2D>("cross_cross");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"fonts\DemoFont");
            temp = new InstancingDraw(this, camera, Content);
            skybox = Content.Load<Model>("SkySphere");

            fxaaEffect = Content.Load<Effect>("Effects/fxaa");
        }

        protected override void UnloadContent()
        {
            renderTarget.Dispose();
        }

        private bool KeyJustPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }

        private void ProcessKeyboard()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            prevMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            prevLeftButton = currentLeftButton;
            if (currentMouseState.LeftButton == ButtonState.Pressed)
                currentLeftButton = true;
            else
                currentLeftButton = false;

            prevRightButton = currentRightButton;
            if (currentMouseState.RightButton == ButtonState.Pressed)
                currentRightButton = true;
            else
                currentRightButton = false;

            if (KeyJustPressed(Keys.Escape))
                this.Exit();

            if (KeyJustPressed(Keys.H))
                displayHelp = !displayHelp;

            if (KeyJustPressed(Keys.M))
                camera.EnableMouseSmoothing = !camera.EnableMouseSmoothing;

            if (KeyJustPressed(Keys.D1))
            {
                instancing = !instancing;
            }

            if (KeyJustPressed(Keys.D2))
            {
                debugShapes = !debugShapes;
            }

            if (KeyJustPressed(Keys.D3))
            {
                raybox = !raybox;
            }
            //if (KeyJustPressed(Keys.P))
            //    enableParallax = !enableParallax;

            //if (KeyJustPressed(Keys.T))
            //    enableColorMap = !enableColorMap;

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) || currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    graphics.ToggleFullScreen();
            }

            //if (KeyJustPressed(Keys.Enter))
            //    graphics.ToggleFullScreen();

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

            if (KeyJustPressed(Keys.F))
            {
                useFXAA = !useFXAA;
            }

            if (currentMouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue && currentMouseState.LeftButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Released)
            {
                if(selectedSpell != SpellType.Fireball)
                {
                    selectedSpell++;
                }
            }

            if (currentMouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue && currentMouseState.LeftButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Released)
            {
                if (selectedSpell != SpellType.MoveTerrain)
                {
                    selectedSpell--;
                }
            }
            
            if ((currentLeftButton && prevLeftButton == false) || (currentRightButton && prevRightButton == false))
                StartSpellcasting(currentLeftButton, currentRightButton);
            if ((currentLeftButton && prevLeftButton) || (currentRightButton && prevRightButton))
                ContinueSpellcasting(currentLeftButton, currentRightButton);
            if ((currentLeftButton == false && prevLeftButton) || (currentRightButton == false && prevRightButton))
                StopSpellcasting(currentLeftButton, currentRightButton);
        }

        private void StartSpellcasting(bool leftButton, bool rightButton)
        {
            switch (selectedSpell)
            {
                case SpellType.MoveTerrain:
                    spellMoveTerrain.Start(leftButton, rightButton, selected_obj);
                    break;
                case SpellType.Fireball:
                    spellFireball.Start(leftButton, rightButton);
                    break;
            }
        }

        private void ContinueSpellcasting(bool leftButton, bool rightButton)
        {
            switch (selectedSpell)
            {
                case SpellType.MoveTerrain:
                    spellMoveTerrain.Continue(leftButton, rightButton);
                    break;
                case SpellType.Fireball:
                    spellFireball.Continue(leftButton, rightButton);
                    break;
            }
        }

        private void StopSpellcasting(bool leftButton, bool rightButton)
        {
            switch (selectedSpell)
            {
                case SpellType.MoveTerrain:
                    spellMoveTerrain.Stop(leftButton, rightButton);
                    break;
                case SpellType.Fireball:
                    spellFireball.Stop(leftButton, rightButton);
                    break;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive)
                return;

            base.Update(gameTime);

            ProcessKeyboard();

            //poruszanie po y w zaleznosci od pozycji tila - czy powinno to byc w update a nie gdzies indziej?
            //cameraSphere.Center = camera.Position - new Vector3(0, 30, 0);
            //Ray yRay = camera.GetDownwardRay();
            Ray yRay = camera.MovingRay();
            IntersectionRecord ir = octree.HighestIntersection(yRay);

            if (ir != null && ir.DrawableObjectObject != null)//..ujowy if ale działa
            {
                //dla debugu, wywalic potem
                //distance = (float)yRay.Intersects(ir.DrawableObjectObject.BoundingBox); //paskudne
                //camera.Move(0, (distance-20)*-1, 0);
                //distance = ir.DrawableObjectObject.Position.Y; //prosciej
                distance = ir.DrawableObjectObject.BoundingBox.Max.Y;
                //camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT + distance;
                //camera.Move(0, (camera.Position.Y - distance-20) * -1, 0); //move jest natychmiastowy a nie plynny jak cala reszta kamery wiec wyglada niefajnie, plus do tego psuje kucanie - uzyc czegos z velocity w kamerze?
                //camera.CurrentY = distance*10;
                tileStandingOn = ir.DrawableObjectObject;
            }
            if (ir.DrawableObjectObject == null)
            {
                distance = 0;
            }
            float temp = camera.EyeHeightStanding - (CAMERA_PLAYER_EYE_HEIGHT + distance);
            if (temp < -30)
            {
                camera.Block();
            }
            else if (temp > 0.2)
            {
                camera.EyeHeightStanding -= (125f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (temp < -0.2)
            {
                camera.EyeHeightStanding += (100f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT + distance;
            }


            /*if (Math.Abs(camera.EyeHeightStanding - CAMERA_PLAYER_EYE_HEIGHT + distance) <= 0.1f)
            {
                camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT + distance;
            }
            if (Math.Abs(camera.EyeHeightStanding - CAMERA_PLAYER_EYE_HEIGHT + distance) > 100)
            {
                temp = (distance / 1000);
            }
            if (camera.EyeHeightStanding > (CAMERA_PLAYER_EYE_HEIGHT + distance))
            {
                camera.EyeHeightStanding -= 0.1f + temp;
            }
            else if (camera.EyeHeightStanding < (CAMERA_PLAYER_EYE_HEIGHT + distance))
            {
                camera.EyeHeightStanding += 0.1f + temp;
            }*/

            //obrót słońca wokół punktu 0,0
            slonce = Vector3.Transform(slonce, Matrix.CreateRotationY((float)(gameTime.ElapsedGameTime.TotalSeconds)));

            //wykrywanie obiektu na ktory celujesz
            Ray mouseRay = camera.GetMouseRay(graphics.GraphicsDevice.Viewport);
            IntersectionRecord mouse_ir = octree.NearestIntersection(mouseRay);

            if (selected_obj == null)
            {
                if (mouse_ir.DrawableObjectObject != null)
                    selected_obj = mouse_ir.DrawableObjectObject;
            }
            else
            {
                if (mouse_ir.DrawableObjectObject == null)
                {
                    selected_obj.Selected = false;
                    selected_obj = null;
                }
                else if (mouse_ir.DrawableObjectObject != selected_obj)
                {
                    selected_obj.Selected = false;
                    selected_obj = mouse_ir.DrawableObjectObject;
                    selected_obj.Selected = true;
                }
            }


            octree.Update(gameTime);
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
                buffer.AppendFormat("Instancing: {0}\n",
                    instancing.ToString());
                buffer.AppendFormat(" Models drawn: {0}\n",
                    modelsDrawn.ToString("f2"));
                buffer.AppendFormat(" Models drawn instanced: {0}\n",
                    modelsDrawnInstanced.ToString("f2"));
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

                buffer.AppendFormat("  MovingRay Position: x:{0} y:{1} z:{2}\n",
                    camera.MovingRay().Position.X.ToString("f2"),
                    camera.MovingRay().Position.Y.ToString("f2"),
                    camera.MovingRay().Position.Z.ToString("f2"));
                Vector3 temp = slonce - camera.Position;
                buffer.AppendFormat(" Sun distance Y: {0}\n",
                    temp.Length().ToString("f2"));

                buffer.AppendFormat(" MouseWheel: {0}\n",
                    currentMouseState.ScrollWheelValue.ToString("f2"));

                buffer.AppendFormat("  Selected Spell: {0}\n",
                    selectedSpell.ToString());

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
                buffer.AppendFormat("Instancing: {0}\n",
                    instancing.ToString());
                buffer.AppendFormat(" Models drawn: {0}\n",
                    modelsDrawn.ToString("f2"));
                buffer.AppendFormat(" Models drawn instanced: {0}\n",
                    modelsDrawnInstanced.ToString("f2"));
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
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            //GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            //GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            modelsDrawn = 0;
            modelsDrawnInstanced = 0;
            skybox.Draw(camera.worldMatrix * Matrix.CreateScale(5000), camera.viewMatrix, camera.projMatrix);


            //Renders all visible objects by iterating through the oct tree recursively and testing for intersection 
            //with the current camera view frustum

            //if (instancing)
            //{
            //    List<IntersectionRecord> list = octree.AllIntersections(camera.Frustum);
            //    temp.DrawModelHardwareInstancing(list);
            //    modelsDrawn = list.Count;
            //}
            //else
            //{
            //    foreach (IntersectionRecord ir in octree.AllIntersections(camera.Frustum))
            //    {
            //        // ir.DrawableObjectObject.SetDirectionalLight(m_globalLight[0].Direction, m_globalLight[0].Color);
            //        // ir.DrawableObjectObject.UpdateLOD(camera);
            //        ir.DrawableObjectObject.Draw(camera);
            //        modelsDrawn++;
            //    }
            //}

            if (instancing)
            {
                List<IntersectionRecord> list = octree.AllIntersections(camera.Frustum);
                List<IntersectionRecord> instanceList = list.FindAll(ir => ir.DrawableObjectObject.IsInstanced == true);
                list.RemoveAll(ir => ir.DrawableObjectObject.IsInstanced == true);
                temp.DrawModelHardwareInstancing(instanceList);
                modelsDrawnInstanced = instanceList.Count;
                foreach (IntersectionRecord ir in list)
                {
                    // ir.DrawableObjectObject.SetDirectionalLight(m_globalLight[0].Direction, m_globalLight[0].Color);
                    // ir.DrawableObjectObject.UpdateLOD(camera);
                    ir.DrawableObjectObject.Draw(camera);
                    modelsDrawn++;
                }
            }
            else
            {
                foreach (IntersectionRecord ir in octree.AllIntersections(camera.Frustum))
                {
                    // ir.DrawableObjectObject.SetDirectionalLight(m_globalLight[0].Direction, m_globalLight[0].Color);
                    // ir.DrawableObjectObject.UpdateLOD(camera);
                    ir.DrawableObjectObject.Draw(camera);
                    modelsDrawn++;
                }
            }

            if (debugShapes)
            {
                DebugShapeRenderer.AddBoundingSphere(cameraSphere, Color.Red);
                octree.DrawBounds();
                DebugShapeRenderer.Draw(gameTime, camera.ViewMatrix, camera.ProjectionMatrix);
            }

            //rączki na razie po chuju tutaj
            Effect effect = Content.Load<Effect>("Effects/test").Clone();
            Model model = Content.Load<Model>("hands");
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Vector3 temp = slonce - camera.Position;
                    effect.Parameters["Texture"].SetValue(part.Effect.Parameters["Texture"].GetValueTexture2D());
                    part.Effect = effect;
                    //slonce
                    effect.Parameters["DiffuseLightDirection"].SetValue(temp);
                    effect.Parameters["DiffuseIntensity"].SetValue(3-(temp.Length()/1000));
                    
                    effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * Matrix.CreateScale(0.01f) * camera.WeaponWorldMatrix(0, -0.1f, 0.4f));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["WorldInverseTranspose"].SetValue(
                                            Matrix.Transpose(camera.worldMatrix * mesh.ParentBone.Transform));
                }
                mesh.Draw();
            }
            //rysowanie gdzie znajduje się movingRay do kolizji
            if (raybox)
            {
                Content.Load<Model>("Monocube").Draw(camera.worldMatrix * Matrix.CreateTranslation(camera.MovingRay().Position), camera.viewMatrix, camera.projMatrix);
            }

            //slonce
            Content.Load<Model>("Monocube").Draw(camera.worldMatrix * Matrix.CreateTranslation(slonce), camera.viewMatrix, camera.projMatrix);

            GraphicsDevice.SetRenderTarget(null);

            if (useFXAA)
            {
                float w = renderTarget.Width;
                float h = renderTarget.Height;
                fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];
                fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
                fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
                fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);
                fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / w);
                fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / h);
                fxaaEffect.Parameters["texScreen"].SetValue((Texture2D)renderTarget);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, fxaaEffect);
                spriteBatch.Draw((Texture2D)renderTarget, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
                //spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
                //DrawText();
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw((Texture2D)renderTarget, Vector2.Zero, Color.White);
                spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
                DrawText();
                spriteBatch.End();
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
            DrawText();
            spriteBatch.End();

            base.Draw(gameTime);
            IncrementFrameCounter();
        }

    }
}