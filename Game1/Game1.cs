using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Game1.Spells;
using Game1.Helpers;
using Game1.Shadows;
using Game1.Lights;
using Game1.Sky;
using Game1.HUD;
using System.Diagnostics;
using System.Threading;
using static Game1.Helpers.HexCoordinates;
using Game1.Items;

namespace Game1
{
    public class Game1 : Game
    {
        public static GraphicsDeviceManager graphics; //Ustawione na razie na static, żeby nie trzeba było dogrzebywać się do tego z każdej klasy
        private SpriteBatch spriteBatch;
        public Camera camera;
        public QuadRenderComponent quadRenderer;
        private SpriteFont spriteFont;
        private InstancingManager instancingManager;
        public Octree octree;
        public ObjectManager objectManager;
        public ItemManager itemManager;
        public PhaseManager phaseManager;

        public static Dictionary<AxialCoordinate, Tile> map;
        public static Dictionary<AxialCoordinate, DrawableObject> mapAsset;

        #region Object lists
        private FrustumIntersections reflectionObjects;
        private FrustumIntersections drawObjects;
        #endregion

        private Matrix reflectionViewMatrix;
        private Plane reflectionPlane;
        private Vector3 reflectionCameraPosition;

        private Effect fxaaEffect;
        private Effect fogEffect;

        public RenderTarget2D colorTarget;
        public RenderTarget2D normalTarget;
        public RenderTarget2D depthTarget;
        public RenderTarget2D lightTarget;
        public RenderTarget2D finalTarget;
        public RenderTarget2D waterTarget;
        public RenderTarget2D dofTarget;
        public RenderTarget2D fxaaTarget;
        
        private Effect clearBufferEffect;
        private Effect finalCombineEffect;

        private LightManager lightManager;
        private ShadowRenderer shadowRenderer;
        private SSAO ssao;
        private HDRProcessor hdrProcessor;
        private DepthOfFieldProcessor dofProcessor;
        private Water water;
        public GameSettings settings;

        private float lastFocalDistance;
        private float lastFocalWidth;
        private float targetFocalDistance;
        private float targetFocalWidth;

        private Model tileModel;
        private Model hands;
        private Model coreModel;
        private Texture2D handstex;
        private Texture2D tileTexture;
        private Texture2D coreTexture;

        private HUDManager hudManager;
        private Stats stats;

        //map
        private Texture2D mapTex;
        private Texture2D mapTexAsset;

        private Core core;

        private SkyDome sky;
        private Vector3 lightDirection;
        private Vector3 lightColor;

        private TimeOfDay timeOfDay;
        
        float acceleration = 100.0f; // przyspieszenie przy wspinaniu i opadaniu

        private const float CAMERA_FOVX = 90.0f;
        private const float CAMERA_ZNEAR = 1.0f;
        private const float CAMERA_ZFAR = 4000.0f;
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

        public DrawableObject selectedObject = null;
        private Stopwatch stopwatchLastTargetedEnemy;

        private enum SkyStatus
        {
            Manual = 0,
            Automatic = 1,
        }

        SkyStatus stat;

        #region Spells
        public enum SpellType
        {
            MoveTerrain = 1,
            Fireball = 2,
            CreateTurret = 3
        };

        private SpellMoveTerrain spellMoveTerrain;
        private SpellFireball spellFireball;
        private SpellCreateTurret spellCreateTurret;

        public SpellType selectedSpell = SpellType.MoveTerrain;
        #endregion

        #region Diagnostics
        Stopwatch swUpdate;
        Stopwatch swDraw;
        Stopwatch swGPU;

        double updateMs = 0;
        double drawMs = 0;
        double gpuMs = 0;
        #endregion


        PathFinder pathfinder;
        List<Tile> path;

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
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferMultiSampling = false;
            graphics.PreferredBackBufferFormat = SurfaceFormat.HdrBlendable;
            graphics.ApplyChanges();

            Window.Position = new Point(0, 0);
            
            //graphics.ToggleFullScreen();

            // Initial position for text rendering.
            fontPos = new Vector2(1.0f, 1.0f);

            //camera.worldMatrix = Matrix.CreateWorld(new Vector3(), Vector3.Forward, Vector3.Up);           

            //Setup the camera.
            camera.Position = new Vector3(2000, 200, 2000); //player position
            camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT + 200;
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

            stat = SkyStatus.Automatic;

            timeOfDay = new TimeOfDay(15, 00, 0);

            sky = new SkyDome(this, ref camera);
            // Set skydome parameters here
            //sky.Theta = 2.4f;// (float)Math.PI / 2.0f - 0.3f;
            sky.Theta = timeOfDay.TimeFloat * (float)(Math.PI) / 12.0f;
            sky.Parameters.NumSamples = 10;
            sky.Initialize();

            objectManager = new ObjectManager();

            base.Initialize();
            
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            int backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            colorTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            normalTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            lightTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);

            finalTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
            waterTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
            dofTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);

            fxaaTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);

            stats = new Stats();

            stopwatchLastTargetedEnemy = new Stopwatch();

            hudManager = new HUDManager(spriteBatch, GraphicsDevice, Content, stats);
            hudManager.LoadContent();

            phaseManager = new PhaseManager(this, timeOfDay, hudManager);

            spriteFont = Content.Load<SpriteFont>("Fonts/DemoFont");

            tileModel = Content.Load<Model>("Models/Tile");
            tileTexture = Content.Load<Texture2D>("Textures/gradient");
            hands = Content.Load<Model>("Models/hands");
            handstex = Content.Load<Texture2D>("Textures/handstex");
            coreModel = Content.Load<Model>("Models/core");

            coreTexture = Content.Load<Texture2D>("Textures/core");

            //map
            mapTex = Content.Load<Texture2D>("Textures/map");
            mapTexAsset = Content.Load<Texture2D>("Textures/mapAssets");

            clearBufferEffect = Content.Load<Effect>("Effects/ClearGBuffer");
            finalCombineEffect = Content.Load<Effect>("Effects/CombineFinal");

            shadowRenderer = new ShadowRenderer(GraphicsDevice, settings, Content);

            fxaaEffect = Content.Load<Effect>("Effects/fxaa");
            fogEffect = Content.Load<Effect>("Effects/Fog");
            fogEffect.CurrentTechnique = fogEffect.Techniques["FogExp"];
            fogEffect.Parameters["FogDensity"].SetValue(0.25f);
            fogEffect.Parameters["FogColor"].SetValue(Color.CornflowerBlue.ToVector4());

            ssao = new SSAO(GraphicsDevice, Content, settings, quadRenderer, camera, normalTarget, depthTarget);

            hdrProcessor = new HDRProcessor(GraphicsDevice, Content, quadRenderer);
            hdrProcessor.FlushCache();

            dofProcessor = new DepthOfFieldProcessor(GraphicsDevice, Content, quadRenderer);
            dofProcessor.FlushCache();

            water = new Water(GraphicsDevice, Content, settings, quadRenderer);

            lightManager = new LightManager(this, lightTarget, Content);
            //TestLights();

            instancingManager = new InstancingManager(this, camera, Content, tileModel, tileTexture);

            //octree
            octree = new Octree();

            map = Map.CreateMapFromTex(this, mapTex, tileModel, octree);

            List<DrawableObject> tileList = new List<DrawableObject>();
            foreach(var item in map)
            {
                tileList.Add(item.Value);
            }

            octree.m_objects.AddRange(tileList);

            core = new Core(this, Matrix.CreateTranslation(1100, 50, 1700), coreModel, octree, coreTexture);
            octree.m_objects.Add(core);

            camera.Octree = octree;

            spellMoveTerrain = new SpellMoveTerrain(octree, stats, phaseManager, map);
            spellFireball = new SpellFireball(this, camera, octree, objectManager, lightManager, hudManager, stats);
            spellCreateTurret = new SpellCreateTurret(this, camera, octree, objectManager, lightManager, stats);

            camera.DebugModel = spellFireball.fireballModel;

            swDraw = new Stopwatch();
            swUpdate = new Stopwatch();
            swGPU = new Stopwatch();

            sky.LoadContent();

            pathfinder = new PathFinder();
            path = new List<Tile>();

            itemManager = new ItemManager(this, Content, octree, objectManager, lightManager, stats);


            mapAsset = Map.CreateAssetMapFromTex(this, mapTexAsset, Content, octree, itemManager, core.Position, phaseManager);

            List<DrawableObject> assetList = new List<DrawableObject>();
            foreach (var item in mapAsset)
            {
                assetList.Add(item.Value);
            }
            //if(assetList.Count == 0){ throw new Exception("lolll"); }
            octree.m_objects.AddRange(assetList);

            /*
            List<DrawableObject> abc = new List<DrawableObject>();
            foreach(Tile tile in path.Pathfind((Tile)octree.AllObjects(DrawableObject.ObjectType.Terrain)[0], (Tile)octree.AllObjects(DrawableObject.ObjectType.Terrain)[846], octree))
            {
                Vector3 temp = tile.Position;
                temp.Y += 50;
                tile.Position = temp;
                abc.Add(tile);
            }

            octree.m_objects.AddRange(abc);
            */
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
            quadRenderer.Render();
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

            if (KeyJustPressed(Keys.D3))
            {
                stat++;
                if ((int)stat == 2)
                    stat = SkyStatus.Manual;
            }

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) || currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    graphics.ToggleFullScreen();
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
            if (KeyJustPressed(Keys.P))
            {
                /*
                Tile start = tileFromPosition(camera.Position, tileDictionary);
                Tile end = tileFromPosition(core.Position, tileDictionary);
                path = pathfinder.Pathfind(start, end, tileDictionary, settings.Instancing);
                */
                //IntersectionRecord ir = octree.NearestIntersection(camera.GetMouseRay(graphics.GraphicsDevice.Viewport));
                Spawn temp = new Spawn(this, Matrix.CreateTranslation(camera.Position+Vector3.Normalize(core.Position - camera.Position)*100), coreModel, octree, itemManager, Content, core.Position, phaseManager);
                
                //path = pathfinder.Pathfind((Tile)octree.HighestIntersection(camera.GetDownwardRay(), DrawableObject.ObjectType.Terrain).DrawableObjectObject, (Tile)octree.HighestIntersection(core, DrawableObject.ObjectType.Terrain).DrawableObjectObject, octree, settings);
            }

            //if (KeyJustPressed(Keys.F8))
            //{
            //    water.reflectionTarget.SaveAsPng(new System.IO.FileStream("../Reflection.png", System.IO.FileMode.Create), 1280, 720);
            //    finalTarget.SaveAsPng(new System.IO.FileStream("../Final.png", System.IO.FileMode.Create), 1280, 720);
            //}

            if (currentMouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue && currentMouseState.LeftButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Released)
            {
                if(selectedSpell != SpellType.CreateTurret)
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
                    spellMoveTerrain.Start(leftButton, rightButton, selectedObject);
                    break;
                case SpellType.Fireball:
                    spellFireball.Start(leftButton, rightButton);
                    break;
                case SpellType.CreateTurret:
                    spellCreateTurret.Start(leftButton, rightButton, selectedObject);
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
                case SpellType.CreateTurret:
                    spellCreateTurret.Continue(leftButton, rightButton, selectedObject);
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
                case SpellType.CreateTurret:
                    spellCreateTurret.Stop(leftButton, rightButton, selectedObject);
                    break;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive)
                return;

            swGPU.Stop();
            gpuMs = swGPU.Elapsed.TotalMilliseconds;

            swUpdate.Reset();
            swUpdate.Start();

            stats.Update(gameTime);

            sky.Update(gameTime);
            base.Update(gameTime);

            timeOfDay.Update((float)gameTime.ElapsedGameTime.TotalSeconds, 100);
            phaseManager.Update(gameTime);

            ProcessKeyboard();

            //Tile tile = tileFromPosition(camera.Position, tileDictionary);

            //if (tile != null)
            //{
            //    tileStandingOn = tile;
            //    distance = tileStandingOn.BoundingBox.Max.Y;
            //}

            //if (tile == null)
            //    distance = 0;

            Ray yRay = camera.DownwardRay;
            IntersectionRecord ir = octree.HighestIntersection(yRay);

            if (ir != null && ir.DrawableObjectObject != null)//..ujowy if ale działa
            {
                distance = ir.DrawableObjectObject.BoundingBox.Max.Y;
                tileStandingOn = ir.DrawableObjectObject;
            }
            if (ir.DrawableObjectObject == null)
                distance = 0;

            float eyeHeight = camera.EyeHeightStanding - (CAMERA_PLAYER_EYE_HEIGHT + distance);

            //if (-eyeHeight > 30)
            //    camera.Block();
            if (eyeHeight > 30)
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

            //depth of field adaptation
            lastFocalDistance = settings.FocalDistance;
            lastFocalWidth = settings.FocalWidth;

            targetFocalDistance = Math.Min(Math.Max((float)mouse_ir.Distance, 50), 2000);
            targetFocalWidth = targetFocalDistance * 6;

            if (lastFocalDistance < targetFocalDistance)
                settings.FocalDistance = MathHelper.Lerp(lastFocalDistance, targetFocalDistance, ((targetFocalDistance - lastFocalDistance) / targetFocalDistance) / 20);
            else
                settings.FocalDistance = MathHelper.Lerp(lastFocalDistance, targetFocalDistance, ((lastFocalDistance - targetFocalDistance) / lastFocalDistance) / 20);
            settings.FocalWidth = settings.FocalDistance * 6;
            //===

            if (selectedObject == null)
            {
                if (mouse_ir.DrawableObjectObject != null)
                {
                    selectedObject = mouse_ir.DrawableObjectObject;
                }
            }
            else
            {
                if (mouse_ir.DrawableObjectObject == null)
                {
                    selectedObject.Selected = false;
                    selectedObject = null;
                }
                else if (mouse_ir.DrawableObjectObject != selectedObject)
                {
                    selectedObject.Selected = false;
                    selectedObject = mouse_ir.DrawableObjectObject;
                    selectedObject.Selected = true;
                }
            }

            if (selectedObject != null)
            {
                if (selectedObject.Type == DrawableObject.ObjectType.Enemy)
                {
                    stopwatchLastTargetedEnemy.Reset();
                    stats.currentTargetedEnemy = (Enemy)selectedObject;
                }
            }

            if (stats.currentTargetedEnemy != null)
            {
                if (selectedObject == null || selectedObject != null && selectedObject.Type != DrawableObject.ObjectType.Enemy)
                {
                    stopwatchLastTargetedEnemy.Start();
                    stats.lastTargetedEnemy = stats.currentTargetedEnemy;
                    stats.currentTargetedEnemy = null;
                }

                if (stopwatchLastTargetedEnemy.ElapsedMilliseconds > 2000)
                {
                    stats.lastTargetedEnemy = null;
                    stopwatchLastTargetedEnemy.Reset();
                }
            }
         
            octree.Update(gameTime);
            objectManager.Update(gameTime);
            itemManager.Update(gameTime, camera.Position);

            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (stat)
            {
                //case SkyStatus.Manual:
                //    if (currentKeyboardState.IsKeyDown(Keys.Down))
                //        sky.Theta -= 0.4f * step;
                //    if (currentKeyboardState.IsKeyDown(Keys.Up))
                //        sky.Theta += 0.4f * step;

                //    if (currentKeyboardState.IsKeyDown(Keys.Left))
                //        sky.Phi -= 0.4f * step;
                //    if (currentKeyboardState.IsKeyDown(Keys.Right))
                //        sky.Phi += 0.4f * step;

                //    if (sky.Theta > (float)Math.PI * 2.0f)
                //        sky.Theta = sky.Theta - (float)Math.PI * 2.0f;
                //    if (sky.Theta < 0.0f)
                //        sky.Theta = (float)Math.PI * 2.0f + sky.Theta;

                //    if (sky.Phi > (float)Math.PI * 2.0f)
                //        sky.Phi = sky.Phi - (float)Math.PI * 2.0f;
                //    if (sky.Phi < 0.0f)
                //        sky.Phi = (float)Math.PI * 2.0f + sky.Phi;
                //    break;
                case SkyStatus.Manual:
                    if (currentKeyboardState.IsKeyDown(Keys.Down))
                        timeOfDay.Minutes -= 1;
                    if (currentKeyboardState.IsKeyDown(Keys.Up))
                        timeOfDay.Minutes += 1;

                    if (currentKeyboardState.IsKeyDown(Keys.Left))
                        sky.Phi -= 0.4f * step;
                    if (currentKeyboardState.IsKeyDown(Keys.Right))
                        sky.Phi += 0.4f * step;

                    sky.Theta = timeOfDay.TimeFloat * (float)(Math.PI) / 12.0f;

                    if (sky.Phi > (float)Math.PI * 2.0f)
                        sky.Phi = sky.Phi - (float)Math.PI * 2.0f;
                    if (sky.Phi < 0.0f)
                        sky.Phi = (float)Math.PI * 2.0f + sky.Phi;
                    break;
                case SkyStatus.Automatic:
                    sky.Theta = timeOfDay.TimeFloat * (float)(Math.PI) / 12.0f; 
                    break;
            }

            hdrProcessor.ToneMapKey = timeOfDay.LogisticTime(0.05f, 0.8f, 2.0f);
            //hdrProcessor.MaxLuminance = 512.0f * timeOfDay.LogisticTime(0f, 1f, 1f);

            //sky.Theta = timeOfDay.TotalMinutes * (float)(Math.PI) / 12.0f / 60.0f;       

            UpdateFrameRate(gameTime);

            swUpdate.Stop();
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
                //buffer.AppendLine("Move mouse to free look");
                //buffer.AppendLine();
                //buffer.AppendLine("Press W and S to move forwards and backwards");
                //buffer.AppendLine("Press A and D to strafe left and right");
                //buffer.AppendLine("Press SPACE to jump");
                //buffer.AppendLine("Press and hold LEFT CTRL to crouch");
                //buffer.AppendLine("Press and hold LEFT SHIFT to run");
                //buffer.AppendLine();
                //buffer.AppendLine("Press M to toggle mouse smoothing");
                //buffer.AppendLine("Press NUMPAD +/- to change camera rotation speed");
                //buffer.AppendLine("Press ALT + ENTER to toggle full screen");
                //buffer.AppendLine();
                //buffer.AppendLine("Press H to hide help");
                buffer.AppendFormat("Instancing: {0}\n",
                    settings.Instancing.ToString());
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
                buffer.AppendFormat(" Normal offset (o / O): {0}\n",
                    settings.OffsetScale.ToString());
                buffer.AppendFormat(" SSAORadius (F1/F2): {0}\n",
                    settings.SSAORadius.ToString());
                buffer.AppendFormat(" SSAOPower (F3/F4): {0}\n\n",
                    settings.SSAOPower.ToString());
                buffer.AppendFormat(" FocalWidth (<-/->): {0}\n",
                    settings.FocalWidth.ToString());
                buffer.AppendFormat(" FocalDistance (^/v): {0}\n\n",
                    settings.FocalDistance.ToString());
                buffer.AppendLine("Press H to return");
            }
            else
            {
                buffer.AppendFormat("FPS: {0}\n", framesPerSecond);
                buffer.AppendFormat(" Update: {0} ms\n", updateMs);
                buffer.AppendFormat(" Draw:     {0} ms\n", drawMs);
                buffer.AppendFormat(" GPU:      {0} ms\n", gpuMs);
                buffer.AppendFormat("  Total: {0} ms\n\n", updateMs + drawMs + gpuMs);
                buffer.AppendFormat("Mouse smoothing: {0}\n\n",
                    (camera.EnableMouseSmoothing ? "on" : "off"));
                buffer.Append("Camera:\n");
                buffer.AppendFormat("  Position: x:{0} y:{1} z:{2}\n",
                    camera.Position.X.ToString("f2"),
                    camera.Position.Y.ToString("f2"),
                    camera.Position.Z.ToString("f2"));
                //buffer.AppendFormat("  Orientation: heading:{0} pitch:{1}\n",
                //    camera.HeadingDegrees.ToString("f2"),
                //    camera.PitchDegrees.ToString("f2"));
                //buffer.AppendFormat("  Velocity: x:{0} y:{1} z:{2}\n",
                //    camera.CurrentVelocity.X.ToString("f2"),
                //    camera.CurrentVelocity.Y.ToString("f2"),
                //    camera.CurrentVelocity.Z.ToString("f2"));
                //buffer.AppendFormat("  Rotation speed: {0}\n\n",
                //    camera.RotationSpeed.ToString("f2"));

                buffer.AppendFormat(" Time of day: {0}:{1}:{2}\n\n",
                    timeOfDay.Hours.ToString("D2"),
                    timeOfDay.Minutes.ToString("D2"),
                    timeOfDay.Seconds.ToString("00"));
                buffer.AppendFormat(" Time of day Logistic: {0}\n\n",
                    timeOfDay.LogisticTime(0.1f, 0.8f, 2.0f).ToString("f2"));

                buffer.AppendFormat(" Non-octree objects: {0}\n\n",
                    objectManager.List.Count);

                buffer.AppendFormat(" MouseWheel: {0}\n",
                    currentMouseState.ScrollWheelValue.ToString("f2"));
                buffer.AppendFormat("  Selected Spell: {0}\n",
                    selectedSpell.ToString());
                buffer.AppendFormat("  Selected Spell: {0}\n\n",
                    camera.GetMouseRay(GraphicsDevice.Viewport).Direction.ToString());
                buffer.AppendFormat(" Health: {0}\n",
                    stats.currentHealth.ToString());
                buffer.AppendFormat(" Mana: {0}\n\n",
                    stats.currentMana.ToString());
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
            quadRenderer.Render();
        }

        private void DrawFog()
        {
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            //GraphicsDevice.DepthStencilState = DepthStencilState.None;
            fogEffect.Parameters["depthMap"].SetValue(depthTarget);
            fogEffect.Parameters["FogColor"].SetValue(sky.SunColor);
            fogEffect.Parameters["FogDensity"].SetValue(1 - timeOfDay.LogisticTime(0.2f, 0.8f, 1.5f));
            fogEffect.CurrentTechnique = fogEffect.Techniques[settings.fog.ToString()];
            fogEffect.CurrentTechnique.Passes[0].Apply();
            quadRenderer.Render();
            GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        protected override bool BeginDraw()
        {
            swDraw.Reset();
            swDraw.Start();

            return base.BeginDraw();
        }

        private void FrustumLists()
        {
            //reflection
            if(settings.Reflect)
                reflectionObjects = octree.AllFrustumIntersections(new BoundingFrustum(reflectionViewMatrix * camera.ProjectionMatrix));

            //draw
            drawObjects = octree.AllFrustumIntersections(camera.Frustum);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            

            lightDirection = sky.Parameters.LightDirection.ToVector3();
            if (lightDirection.Y < 0)
            {
                lightDirection = Vector3.Negate(lightDirection); //odwrocenie kierunku kiedy zrodlem swiatla jest ksiezyc
            }

            lightColor = sky.SunColor.ToVector3();

            float skyIntensity = timeOfDay.LogisticTime(2, 4, 2.0f);

            reflectionPlane = new Plane(new Vector3(0, 1, 0), -settings.WaterHeight);
            reflectionViewMatrix = Matrix.CreateReflection(reflectionPlane) * camera.ViewMatrix;
            reflectionCameraPosition = camera.Position;
            reflectionCameraPosition.Y = -camera.Position.Y + settings.WaterHeight * 2;

            FrustumLists();

            if (settings.Reflect)
                water.RenderReflectionMap(gameTime, camera, reflectionCameraPosition, reflectionViewMatrix, reflectionPlane, sky, lightDirection, lightColor, skyIntensity, reflectionObjects, instancingManager);

            SetGBuffer();
            ClearGBuffer();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            GraphicsDevice.Clear(Color.Transparent);

            sky.Draw(gameTime, camera.ViewMatrix, camera.Position);

            instancingManager.DrawModelHardwareInstancing(drawObjects.IntersectionsInstanced);
            foreach (IntersectionRecord ir in drawObjects.Intersections)
            {
                ir.DrawableObjectObject.Draw(camera);
            }
            modelsDrawn = drawObjects.Intersections.Count;
            modelsDrawnInstanced = drawObjects.IntersectionsInstanced.Count;

            objectManager.Draw(camera);
            itemManager.Draw(camera);

            if (settings.DrawDebugShapes)
            {
                octree.DrawBounds(camera.Frustum);                
            }
            DebugShapeRenderer.Draw(gameTime, camera.ViewMatrix, camera.ProjectionMatrix);

            //rączki na razie tutaj
            foreach (ModelMesh mesh in hands.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * Matrix.CreateScale(0.03f) * camera.WeaponWorldMatrix(0, -0.7f, 2.5f));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Texture"].SetValue(handstex);
                }
                mesh.Draw();
            }

            //ścieżka
            foreach(Tile tile in path)
            {
                tile.Draw(camera);
            }

            ResolveGBuffer();

            shadowRenderer.RenderShadowMap(GraphicsDevice, camera, lightDirection, Matrix.Identity, octree);

            ssao.DrawSSAO();

            GraphicsDevice.SetRenderTarget(lightTarget);
            GraphicsDevice.Clear(Color.Transparent);

            shadowRenderer.Render(GraphicsDevice, camera, Matrix.Identity, lightDirection, lightColor, colorTarget, normalTarget, depthTarget, ssao.BlurTarget, timeOfDay);
            quadRenderer.Render();

            lightManager.Draw();

            GraphicsDevice.SetRenderTarget(finalTarget);
            GraphicsDevice.Clear(Color.White);
            DrawFinal();

            GraphicsDevice.SetRenderTarget(waterTarget);
            GraphicsDevice.Clear(Color.White);

            water.DrawWater(camera, (float)gameTime.TotalGameTime.TotalMilliseconds*5, finalTarget, depthTarget, lightDirection, lightColor);

            if (settings.DrawFog)
                DrawFog();

            if (settings.dofType != GameSettings.DOFType.None)
                dofProcessor.DOF(waterTarget, dofTarget, depthTarget, camera, settings.dofType, settings.FocalDistance, settings.FocalWidth);

            if (settings.ToneMap)
            {
                if(settings.dofType != GameSettings.DOFType.None)
                    hdrProcessor.ToneMap(dofTarget, fxaaTarget, (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f, false);
                else
                    hdrProcessor.ToneMap(waterTarget, fxaaTarget, (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f, false);
            }

            GraphicsDevice.SetRenderTarget(null);

            hudManager.DrawMask();

            if (settings.FXAA)
            {
                float w = fxaaTarget.Width;
                float h = fxaaTarget.Height;
                fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];
                fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
                fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
                fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);
                fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / w);
                fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / h);
                fxaaEffect.Parameters["texScreen"].SetValue(fxaaTarget);
            
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, fxaaEffect);
                if(settings.ToneMap)
                    spriteBatch.Draw(fxaaTarget, new Rectangle(0, 0, fxaaTarget.Width, fxaaTarget.Height), Color.White);
                else if (settings.dofType != GameSettings.DOFType.None)
                    spriteBatch.Draw(dofTarget, new Rectangle(0, 0, fxaaTarget.Width, fxaaTarget.Height), Color.White);
                else
                    spriteBatch.Draw(waterTarget, new Rectangle(0, 0, fxaaTarget.Width, fxaaTarget.Height), Color.White);
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                if (settings.ToneMap)
                    spriteBatch.Draw(fxaaTarget, Vector2.Zero, Color.White);
                else if (settings.dofType != GameSettings.DOFType.None)
                    spriteBatch.Draw(dofTarget, Vector2.Zero, Color.White);
                else
                    spriteBatch.Draw(waterTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            //camera.Update(gameTime);

            swDraw.Stop();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            DrawText();
            if(settings.ShowGBuffer)
                DrawGBuffer();
            spriteBatch.End();
            
            hudManager.Draw();

            IncrementFrameCounter();

            updateMs = swUpdate.Elapsed.TotalMilliseconds;
            drawMs = swDraw.Elapsed.TotalMilliseconds;

            swGPU.Reset();
            swGPU.Start();
        }

        private void DrawGBuffer()
        {
            int halfWidth = GraphicsDevice.Viewport.Width / 2;
            int halfHeight = GraphicsDevice.Viewport.Height / 2;
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Draw(colorTarget, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(normalTarget, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(depthTarget, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(lightTarget, new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight), Color.White);

            //ssaoTarget.SaveAsPng(new System.IO.FileStream("../Tex.png", System.IO.FileMode.Create), 1280, 720);
        }
    }
}