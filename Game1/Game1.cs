﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Game1.Spells;
using Game1.Helpers;
using Game1.Shadows;
using Game1.Lights;

namespace Game1
{

    public class Game1 : Game
    {
        public static GraphicsDeviceManager graphics; //Ustawione na razie na static, żeby nie trzeba było dogrzebywać się do tego z każdej klasy
        private SpriteBatch spriteBatch;
        public Camera camera;
        public QuadRenderComponent quadRenderer;
        private SpriteFont spriteFont;

        public Octree octree;
        InstancingDraw temp;
        private Texture2D cross;

        private Effect fxaaEffect;
        private RenderTarget2D fxaaTarget;

        public RenderTarget2D colorTarget;
        public RenderTarget2D normalTarget;
        public RenderTarget2D depthTarget;
        public RenderTarget2D lightTarget;

        private Effect clearBufferEffect;
        private Effect finalCombineEffect;

        private LightManager lightManager;
        private ShadowRenderer shadowRenderer;
        public GameSettings settings;

        Model tileModel;
        Model hands;
        Texture2D handstex;
        Texture2D tileTexture;

        float acceleration = 100.0f; // przyspieszenie przy wspinaniu i opadaniu

        private bool instancing = true;
        private bool useFXAA = true;
        private bool showgbuffer = false;
        private bool debugShapes = false;
        private bool raybox = false;

        private const float CAMERA_FOVX = 90.0f;
        private const float CAMERA_ZNEAR = 0.1f;
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
            
            

            IsFixedTimeStep = false; //to ustawione na false albo vsync na true potrzebne
        }

        protected override void Initialize()
        {
            DebugShapeRenderer.Initialize(graphics.GraphicsDevice);

            camera = new Camera(this);
            Components.Add(camera);

            Components.Add(quadRenderer = new QuadRenderComponent(this));
            Components.Add(settings = new GameSettings(this));

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

            //camera.worldMatrix = Matrix.CreateWorld(new Vector3(), Vector3.Forward, Vector3.Up);           

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

            PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
            fxaaTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            currentKeyboardState = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            cross = Content.Load<Texture2D>("Hud/cross_cross");
            spriteFont = Content.Load<SpriteFont>("Fonts/DemoFont");

            int backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            colorTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            lightTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            tileModel = Content.Load<Model>("Models/Tile");
            tileTexture = Content.Load<Texture2D>("Textures/gradient");
            hands = Content.Load<Model>("Models/hands");
            handstex = Content.Load<Texture2D>("Textures/handstex");

            clearBufferEffect = Content.Load<Effect>("Effects/ClearGBuffer");
            finalCombineEffect = Content.Load<Effect>("Effects/CombineFinal");

            shadowRenderer = new ShadowRenderer(GraphicsDevice, settings, Content);

            fxaaEffect = Content.Load<Effect>("Effects/fxaa");

            spriteBatch = new SpriteBatch(GraphicsDevice);

            lightManager = new LightManager(this, lightTarget, Content);
            TestLights();

            //octree
            octree = new Octree(Map.CreateMap(this, 30, tileModel));

            spellMoveTerrain = new SpellMoveTerrain(octree);
            spellFireball = new SpellFireball(this, camera, octree, lightManager);


        }

        protected override void UnloadContent()
        {
            fxaaTarget.Dispose();
        }

        private void SetGBuffer()
        {
            GraphicsDevice.SetRenderTargets(colorTarget, normalTarget, depthTarget);
        }

        private void ResolveGBuffer()
        {
            GraphicsDevice.SetRenderTargets(null);
        }

        private void ClearGBuffer()
        {
            clearBufferEffect.Techniques[0].Passes[0].Apply();
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
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

            if (KeyJustPressed(Keys.G))
            {
                showgbuffer = !showgbuffer;
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

            if (KeyJustPressed(Keys.X))
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

            Ray yRay = camera.MovingRay();
            IntersectionRecord ir = octree.HighestIntersection(yRay);

            if (ir != null && ir.DrawableObjectObject != null)//..ujowy if ale działa
            {
                distance = ir.DrawableObjectObject.BoundingBox.Max.Y;
                tileStandingOn = ir.DrawableObjectObject;
            }

            if (ir.DrawableObjectObject == null)
                distance = 0;

            float eyeHeight = camera.EyeHeightStanding - (CAMERA_PLAYER_EYE_HEIGHT + distance);

            if (-eyeHeight > 30)
                camera.Block();
            else if (eyeHeight > 30)
                camera.EyeHeightStanding += -Math.Sign(eyeHeight) * acceleration * 2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            else if (Math.Truncate(eyeHeight) == 0)
                camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT + distance;
            else if (Math.Abs(eyeHeight) < 5)
                camera.EyeHeightStanding += -Math.Sign(eyeHeight) * acceleration / 2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            else
                camera.EyeHeightStanding += -Math.Sign(eyeHeight) * acceleration * (float)(gameTime.ElapsedGameTime.TotalSeconds);

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

            //obrot slonca
            //var rotationY = (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.00025f;
            //var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotationY);
            //var lightDirection = settings.LightDirection;
            //lightDirection = Vector3.Transform(lightDirection, rotation);
            //settings.LightDirection = lightDirection;
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
                buffer.AppendFormat("  Rotation speed: {0}\n\n",
                    camera.RotationSpeed.ToString("f2"));

                buffer.AppendFormat("Instancing: {0}\n",
                    instancing.ToString());
                buffer.AppendFormat(" Models drawn: {0}\n",
                    modelsDrawn.ToString("f2"));
                buffer.AppendFormat(" Models drawn instanced: {0}\n\n",
                    modelsDrawnInstanced.ToString("f2"));

                buffer.Append("Shadows:\n");
                buffer.AppendFormat(" Filter size (F): {0}\n", 
                    settings.FixedFilterSize.ToString());
                buffer.AppendFormat(" Stabilize cascades? (C): {0}\n",
                    settings.StabilizeCascades.ToString());
                buffer.AppendFormat(" Visualize cascades? (V): {0}\n",
                    settings.VisualizeCascades.ToString());
                buffer.AppendFormat(" Filter across cascades? (K): {0}\n",
                    settings.FilterAcrossCascades.ToString());
                buffer.AppendFormat(" Bias (b / B): {0}\n",
                    settings.Bias.ToString());
                buffer.AppendFormat(" Normal offset (o / O): {0}\n\n",
                    settings.OffsetScale.ToString());

                buffer.AppendFormat(" MouseWheel: {0}\n",
                    currentMouseState.ScrollWheelValue.ToString("f2"));
                buffer.AppendFormat("  Selected Spell: {0}\n",
                    selectedSpell.ToString());
                buffer.AppendFormat("  Selected Spell: {0}\n",
                    camera.GetMouseRay(GraphicsDevice.Viewport).Direction.ToString());

                buffer.Append("\nPress H to display help");
            }
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
        }

        private void TestLights()
        {
            //draw some lights
            Color[] colors = new Color[10];
            colors[0] = Color.Red; colors[1] = Color.Blue;
            colors[2] = Color.IndianRed; colors[3] = Color.CornflowerBlue;
            colors[4] = Color.Gold; colors[5] = Color.Green;
            colors[6] = Color.Crimson; colors[7] = Color.SkyBlue;
            colors[8] = Color.Red; colors[9] = Color.ForestGreen;
            int n = 15;

            for (int i = 0; i < n; i++)
            {
                Vector3 pos = new Vector3((float)Math.Sin(i * MathHelper.TwoPi / n), 0.30f, (float)Math.Cos(i * MathHelper.TwoPi / n));
                lightManager.AddLight(new PointLight(pos * 40, colors[i % 10], 15, 2));
                pos = new Vector3((float)Math.Cos((i + 5) * MathHelper.TwoPi / n), 0.30f, (float)Math.Sin((i + 5) * MathHelper.TwoPi / n));
                lightManager.AddLight(new PointLight(pos * 20, colors[i % 10], 20, 1));
                pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n), 0.10f, (float)Math.Sin(i * MathHelper.TwoPi / n));
                lightManager.AddLight(new PointLight(pos * 75, colors[i % 10], 45, 2));
                pos = new Vector3((float)Math.Cos(i * MathHelper.TwoPi / n), -0.3f, (float)Math.Sin(i * MathHelper.TwoPi / n));
                lightManager.AddLight(new PointLight(pos * 20, colors[i % 10], 20, 2));
            }

            lightManager.AddLight(new PointLight(new Vector3(0, (float)Math.Sin(0.8) * 40, 0), Color.Red, 30, 5));
            lightManager.AddLight(new PointLight(new Vector3(0, 25, 0), Color.White, 30, 1));
            lightManager.AddLight(new PointLight(new Vector3(0, 0, 70), Color.Wheat, 55 + 10 * (float)Math.Sin(5), 3));
        }

        private void DrawFinal()
        {
            //Combine everything
            finalCombineEffect.Parameters["colorMap"].SetValue(colorTarget);
            finalCombineEffect.Parameters["lightMap"].SetValue(lightTarget);

            finalCombineEffect.Techniques[0].Passes[0].Apply();
            //render a full-screen quad
            quadRenderer.Render(Vector2.One * -1, Vector2.One);
        }

        protected override void Draw(GameTime gameTime)
        {
            SetGBuffer();
            ClearGBuffer();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            modelsDrawn = 0;
            modelsDrawnInstanced = 0;

            //Renders all visible objects by iterating through the oct tree recursively and testing for intersection 
            //with the current camera view frustum
            List<IntersectionRecord> list = octree.AllIntersections(camera.Frustum);
            if (instancing)
            {
                List<IntersectionRecord> instanceList = list.FindAll(ir => ir.DrawableObjectObject.IsInstanced == true);
                list.RemoveAll(ir => ir.DrawableObjectObject.IsInstanced == true);
                InstancingDraw.DrawModelHardwareInstancing(GraphicsDevice, camera, tileModel, tileTexture, instanceList);
                modelsDrawnInstanced = instanceList.Count;
                foreach (IntersectionRecord ir in list)
                {
                    // ir.DrawableObjectObject.UpdateLOD(camera);
                    ir.DrawableObjectObject.Draw(camera);
                    modelsDrawn++;
                }
            }
            else
            {
                foreach (IntersectionRecord ir in list)
                {
                    // ir.DrawableObjectObject.UpdateLOD(camera);
                    ir.DrawableObjectObject.Draw(camera);
                    modelsDrawn++;
                }
            }

            if (debugShapes)
            {
                octree.DrawBounds();
                DebugShapeRenderer.Draw(gameTime, camera.ViewMatrix, camera.ProjectionMatrix);
            }

            //rączki na razie tutaj
            foreach (ModelMesh mesh in hands.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * Matrix.CreateScale(0.01f) * camera.WeaponWorldMatrix(0, -0.1f, 0.4f));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["Texture"].SetValue(handstex);
                }
                mesh.Draw();
            }

            //foreach (ModelMesh mesh in skySphereModel.Meshes)
            //{
            //    foreach (Effect effect in mesh.Effects)
            //    {
            //        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * camera.worldMatrix * Matrix.CreateScale(5000));
            //        effect.Parameters["View"].SetValue(camera.ViewMatrix);
            //        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            //        effect.Parameters["Texture"].SetValue(handstex);
            //    }
            //    mesh.Draw();
            //}

            //rysowanie gdzie znajduje się movingRay do kolizji
            if (raybox)
            {
                Content.Load<Model>("1").Draw(camera.worldMatrix * Matrix.CreateTranslation(camera.MovingRay().Position), camera.viewMatrix, camera.projMatrix);
            }

            ResolveGBuffer();
            shadowRenderer.RenderShadowMap(GraphicsDevice, camera, Matrix.Identity, octree);

            GraphicsDevice.SetRenderTarget(lightTarget);
            GraphicsDevice.Clear(Color.Transparent);

            shadowRenderer.Render(GraphicsDevice, camera, camera.ProjectionMatrix, colorTarget, normalTarget, depthTarget);
            quadRenderer.Render(Vector2.One * -1, Vector2.One);

            lightManager.Draw();

            GraphicsDevice.SetRenderTarget(fxaaTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawFinal();
            GraphicsDevice.SetRenderTarget(null);

            if (useFXAA)
            {
                float w = fxaaTarget.Width;
                float h = fxaaTarget.Height;
                fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];
                fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
                fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
                fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);
                fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / w);
                fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / h);
                fxaaEffect.Parameters["texScreen"].SetValue((Texture2D)fxaaTarget);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, fxaaEffect);
                spriteBatch.Draw((Texture2D)fxaaTarget, new Rectangle(0, 0, fxaaTarget.Width, fxaaTarget.Height), Color.White);
                //spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
                //DrawText();
                //DrawGBuffer();
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw((Texture2D)fxaaTarget, Vector2.Zero, Color.White);
                //spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
                //DrawText();
                //DrawGBuffer();
                spriteBatch.End();
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(cross, new Rectangle(graphics.PreferredBackBufferWidth / 2 - 25, graphics.PreferredBackBufferHeight / 2 - 25, 50, 50), Color.Red);
            DrawText();
            if(showgbuffer)
                DrawGBuffer();
            spriteBatch.End();

            base.Draw(gameTime);
            IncrementFrameCounter();
        }

        private void DrawGBuffer()
        {
            int halfWidth = GraphicsDevice.Viewport.Width / 2;
            int halfHeight = GraphicsDevice.Viewport.Height / 2;
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Draw(colorTarget, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(normalTarget, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(depthTarget, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(lightTarget, new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight), Color.White);
        }
    }
}