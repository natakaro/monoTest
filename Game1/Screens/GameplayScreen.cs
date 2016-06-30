using Game1.Helpers;
using Game1.HUD;
using Game1.Input;
using Game1.Items;
using Game1.Lights;
using Game1.Particles;
using Game1.Postprocess;
using Game1.Shadows;
using Game1.Sky;
using Game1.Spells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Game1.Helpers.HexCoordinates;

namespace Game1.Screens
{
    /// <summary>
    /// This screen implements the actual game logic.
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        public static Tutorial tutorial;

        private GraphicsDevice GraphicsDevice;
        private ContentManager Content;
        private SpriteBatch spriteBatch;

        public static Random random;

        private GameSettings settings;
        private QuadRenderComponent quadRenderer;
        private SpriteFont spriteFont;
        private InstancingManager instancingManager;
        private Octree octree;
        public static ObjectManager objectManager;
        public static ItemManager itemManager;
        public static PhaseManager phaseManager;
        private PathFinder pathfinder;
        public static ParticleManager particleManager;
        public static HUDManager hudManager;
        public static Stats stats;
        public static AssetContentContainer assetContentContainer;

        public static Dictionary<AxialCoordinate, Tile> map;
        public static Dictionary<AxialCoordinate, DrawableObject> mapAsset;

        public static Camera camera;
        float acceleration = 100.0f; // przyspieszenie przy wspinaniu i opadaniu
        private float distance;
        public static DrawableObject tileStandingOn;
        Vector2 fontPos = new Vector2(1.0f, 1.0f);
        private SkyDome sky;
        private Vector3 lightDirection;
        private Vector3 lightColor;

        public static Core core;
        public static TimeOfDay timeOfDay;
        public static List<Spawn> spawns;
        public static List<List<Wave>> wavesList;

        #region ScreenActions
        InputAction pauseAction;
        InputAction menuAction;
        float pauseAlpha;
        #endregion

        #region Camera consts
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
        #endregion

        #region FXAA SETTINGS
        private float fxaaQualitySubpix = 0.5f;
        private float fxaaQualityEdgeThreshold = 0.166f;
        private float fxaaQualityEdgeThresholdMin = 0.0625f;
        #endregion

        #region Object lists
        private FrustumIntersections reflectionObjects;
        private FrustumIntersections drawObjects;
        #endregion

        #region Rendertargets & Postprocessing
        public RenderTarget2D colorTarget;
        public RenderTarget2D normalTarget;
        public RenderTarget2D depthTarget;
        public RenderTarget2D emissiveTarget;
        public RenderTarget2D lightTarget;
        public RenderTarget2D postProcessTarget1;
        public RenderTarget2D postProcessTarget2;
        public RenderTarget2D finalTarget;

        private Effect clearBufferEffect;
        private Effect finalCombineEffect;
        private Effect fxaaEffect;
        private Effect fogEffect;

        public static LightManager lightManager;
        private ShadowRenderer shadowRenderer;
        private SSAO ssao;
        private HDRProcessor hdrProcessor;
        private DepthOfFieldProcessor dofProcessor;
        private Water water;

        private Matrix reflectionViewMatrix;
        private Plane reflectionPlane;
        private Vector3 reflectionCameraPosition;

        private float lastFocalDistance;
        private float lastFocalWidth;
        private float targetFocalDistance;
        private float targetFocalWidth;
        #endregion

        #region Assets
        private Model tileModel;
        private Model hands;
        private Model coreModel;
        private Texture2D handstex;
        private Texture2D tileTexture;
        private Texture2D coreTexture;
        public static Texture2D mapTex;
        public static Texture2D mapTexAsset;
        #endregion    

        #region Diagnostics
        private bool displayHelp;

        private int modelsDrawn;
        private int modelsDrawnInstanced;
        #endregion
        
        public DrawableObject selectedObject = null;
        private Stopwatch stopwatchLastTargetedEnemy;

        #region Spells
        public enum SpellType
        {
            Fire = 0,
            Ice = 1,
            MoveTerrain = 2,
            CreateTurret = 3
        };

        
        private SpellFire spellFire;
        private SpellIce spellIce;
        private SpellMoveTerrain spellMoveTerrain;
        private SpellCreateTurret spellCreateTurret;

        public static SpellType selectedSpell = SpellType.Fire;

        private bool currentLeftButton;
        private bool currentRightButton;
        private bool prevLeftButton;
        private bool prevRightButton;
        #endregion

        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Keys[] { Keys.Escape },
                null,
                true);

            menuAction = new InputAction(
                new Keys[] { Keys.Tab, Keys.M },
                null,
                true);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate()
        {
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            if (GraphicsDevice == null)
                GraphicsDevice = ScreenManager.Game.GraphicsDevice;

            #region Camera setup
            camera = new Camera(ScreenManager.Game);

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
                (float)ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth / (float)ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);

            camera.Initialize();
            #endregion

            random = new Random();

            timeOfDay = new TimeOfDay(3, 00, 0);
            quadRenderer = new QuadRenderComponent(ScreenManager.Game, (IGraphicsDeviceService)ScreenManager.Game.Services.GetService(typeof(IGraphicsDeviceService)));
            quadRenderer.LoadContent();

            sky = new SkyDome(ScreenManager.Game, ref camera, quadRenderer);
            sky.Theta = timeOfDay.TimeFloat * (float)(Math.PI) / 12.0f;
            sky.Parameters.NumSamples = 10;
            sky.Initialize();
            sky.LoadContent();

            objectManager = new ObjectManager();
            particleManager = new ParticleManager(ScreenManager.Game, Content);
            settings = (ScreenManager.Game as Game1).settings;

            spawns = new List<Spawn>();
            wavesList = new List<List<Wave>>();
            LoadContent();

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        private void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            int backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            colorTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            normalTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            emissiveTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
            lightTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);

            postProcessTarget1 = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
            postProcessTarget2 = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);

            finalTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);



            List<Wave> jeden = new List<Wave>();
            List<Wave> dwa = new List<Wave>();
            List<Wave> trzy = new List<Wave>();
            List<Wave> cztery = new List<Wave>();

            wavesList.Add(jeden);
            wavesList.Add(dwa);
            wavesList.Add(trzy);
            wavesList.Add(cztery);

            var path = @"Content\waves.txt";

            var stream = TitleContainer.OpenStream(path);
            var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                string text = reader.ReadLine();
                string[] bits = text.Split(' ');

                int i = int.Parse(bits[0]);
                int type = int.Parse(bits[1]);
                float time = float.Parse(bits[2]);
                int number = int.Parse(bits[3]);
                float stopwatch = float.Parse(bits[4]);

                wavesList[i].Add(new Wave(type, time, number, stopwatch));
            }
             
            //map
            mapTex = Content.Load<Texture2D>("Textures/map");
            mapTexAsset = Content.Load<Texture2D>("Textures/mapAssets");

            stats = new Stats();

            stopwatchLastTargetedEnemy = new Stopwatch();

            hudManager = new HUDManager(spriteBatch, GraphicsDevice, Content, stats, timeOfDay, map);
            hudManager.LoadContent();

            assetContentContainer = new AssetContentContainer();
            assetContentContainer.LoadContent(Content);

            phaseManager = new PhaseManager(ScreenManager.Game, timeOfDay, hudManager);

            spriteFont = Content.Load<SpriteFont>("Fonts/DemoFont");

            tileModel = Content.Load<Model>("Models/Tile");
            tileTexture = Content.Load<Texture2D>("Textures/gradient");
            hands = Content.Load<Model>("Models/hands");
            handstex = Content.Load<Texture2D>("Textures/handstex");
            coreModel = Content.Load<Model>("Models/core");

            coreTexture = Content.Load<Texture2D>("Textures/core");

            clearBufferEffect = Content.Load<Effect>("Effects/ClearGBuffer");
            finalCombineEffect = Content.Load<Effect>("Effects/CombineFinal");

            shadowRenderer = new ShadowRenderer(GraphicsDevice, settings, Content);

            fxaaEffect = Content.Load<Effect>("Effects/fxaa");
            fogEffect = Content.Load<Effect>("Effects/Fog");
            fogEffect.CurrentTechnique = fogEffect.Techniques["FogExp"];
            fogEffect.Parameters["FogDensity"].SetValue(0.25f);
            fogEffect.Parameters["FogColor"].SetValue(Color.CornflowerBlue.ToVector4());

            ssao = new SSAO(GraphicsDevice, Content, settings, quadRenderer, camera, normalTarget, depthTarget);

            hdrProcessor = new HDRProcessor(GraphicsDevice, Content, quadRenderer, settings);
            hdrProcessor.FlushCache();

            dofProcessor = new DepthOfFieldProcessor(GraphicsDevice, Content, quadRenderer);
            dofProcessor.FlushCache();

            water = new Water(GraphicsDevice, Content, settings, quadRenderer);
            lightManager = new LightManager(ScreenManager.Game, ScreenManager, lightTarget, Content);
            instancingManager = new InstancingManager(ScreenManager.Game, camera, Content, tileModel, tileTexture);

            octree = new Octree();

            map = Map.CreateMapFromTex(ScreenManager.Game, mapTex, tileModel, octree);

            foreach (var item in map)
            {
                Octree.AddObject(item.Value);
            }

            core = new Core(ScreenManager.Game, Matrix.CreateTranslation(1100, 50, 1700), coreModel, octree, coreTexture);
            objectManager.Add(core);
            //Octree.AddObject(core);

            camera.Octree = octree;

            
            spellFire = new SpellFire(ScreenManager.Game, camera, octree, objectManager, lightManager, particleManager, hudManager, stats);
            spellIce = new SpellIce(ScreenManager.Game, camera, octree, objectManager, lightManager, particleManager, hudManager, stats);
            spellMoveTerrain = new SpellMoveTerrain(octree, stats, phaseManager, map);
            spellCreateTurret = new SpellCreateTurret(ScreenManager.Game, camera, octree, objectManager, lightManager, particleManager, stats);

            pathfinder = new PathFinder();

            itemManager = new ItemManager(ScreenManager.Game, Content, octree, objectManager, lightManager, stats);

            mapAsset = Map.CreateAssetMapFromTex(ScreenManager.Game, mapTexAsset, Content, octree, itemManager, core.Position, phaseManager, assetContentContainer);

            List<DrawableObject> assetList = new List<DrawableObject>();
            foreach (var item in mapAsset)
            {
                if (item.Value is Spawn)
                    objectManager.Add(item.Value);
                else
                    Octree.AddObject(item.Value);
            }

            tutorial = new Tutorial(ScreenManager.Game, ScreenManager, hudManager, stats);
        }

        #region GBuffer
        private void SetGBuffer()
        {
            GraphicsDevice.SetRenderTargets(colorTarget, normalTarget, depthTarget, emissiveTarget);
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
        #endregion

        #region Spellcasting
        private void StartSpellcasting(bool leftButton, bool rightButton)
        {
            switch (selectedSpell)
            {
                case SpellType.Fire:
                    spellFire.Start(leftButton, rightButton);
                    break;
                case SpellType.Ice:
                    spellIce.Start(leftButton, rightButton);
                    break;
                case SpellType.MoveTerrain:
                    spellMoveTerrain.Start(leftButton, rightButton, selectedObject);
                    break;
                case SpellType.CreateTurret:
                    spellCreateTurret.Start(leftButton, rightButton, selectedObject);
                    break;
            }
        }

        private void ContinueSpellcasting(bool leftButton, bool rightButton, GameTime gameTime)
        {
            switch (selectedSpell)
            {
                case SpellType.Fire:
                    spellFire.Continue(leftButton, rightButton, gameTime);
                    break;
                case SpellType.Ice:
                    spellIce.Continue(leftButton, rightButton, gameTime);
                    break;
                case SpellType.MoveTerrain:
                    spellMoveTerrain.Continue(leftButton, rightButton);
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
                case SpellType.Fire:
                    spellFire.Stop(leftButton, rightButton);
                    break;
                case SpellType.Ice:
                    spellIce.Stop(leftButton, rightButton);
                    break;
                case SpellType.MoveTerrain:
                    spellMoveTerrain.Stop(leftButton, rightButton);
                    break;
                case SpellType.CreateTurret:
                    spellCreateTurret.Stop(leftButton, rightButton, selectedObject);
                    break;
            }
        }
        #endregion

        public override void Deactivate()
        {
            base.Deactivate();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            Content.Unload();
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                stats.Update(gameTime);

                sky.Update(gameTime);
                int scale=0;
                if(phaseManager.Phase == Phase.Day)
                {
                    scale = 1100;
                }
                else
                {
                    scale = 250;
                }
                timeOfDay.Update((float)gameTime.ElapsedGameTime.TotalSeconds, scale);
                phaseManager.Update(gameTime);

                octree.Update(gameTime);
                objectManager.Update(gameTime);
                itemManager.Update(gameTime, camera.Position);

                sky.Theta = timeOfDay.TimeFloat * (float)(Math.PI) / 12.0f;

                hdrProcessor.ToneMapKey = timeOfDay.LogisticTime(0.05f, 0.8f, 2.0f);
                particleManager.Update(gameTime);

                //hdrProcessor.MaxLuminance = 512.0f * timeOfDay.LogisticTime(0f, 1f, 1f);

                tutorial.Update(gameTime);
            }
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            ScreenManager.Game.IsMouseVisible = false;

            if (input == null)
                throw new ArgumentNullException("input");

            KeyboardState keyboardState = input.CurrentKeyboardState;
            MouseState mouseState = input.CurrentMouseState;

            if (pauseAction.Evaluate(input))
            {
                ScreenManager.AddScreen(new PauseMenuScreen());
            }
            else if (menuAction.Evaluate(input))
            {
                ScreenManager.AddScreen(new MapScreen());
            }
            else
            {
                camera.Update(gameTime, input);

                settings.Update(gameTime, input);

                prevLeftButton = currentLeftButton;
                if (mouseState.LeftButton == ButtonState.Pressed)
                    currentLeftButton = true;
                else
                    currentLeftButton = false;

                prevRightButton = currentRightButton;
                if (mouseState.RightButton == ButtonState.Pressed)
                    currentRightButton = true;
                else
                    currentRightButton = false;

                if (input.IsNewKeyPress(Keys.H))
                    displayHelp = !displayHelp;

                //if (input.IsNewKeyPress(Keys.M))
                //    camera.EnableMouseSmoothing = !camera.EnableMouseSmoothing;

                if (input.IsNewKeyPress(Keys.Add))
                {
                    camera.RotationSpeed += 0.01f;

                    if (camera.RotationSpeed > 1.0f)
                        camera.RotationSpeed = 1.0f;
                }

                if (input.IsNewKeyPress(Keys.Subtract))
                {
                    camera.RotationSpeed -= 0.01f;

                    if (camera.RotationSpeed <= 0.0f)
                        camera.RotationSpeed = 0.01f;
                }
                if (input.IsNewKeyPress(Keys.P))
                {
                    ScreenManager.AddScreen(new Credits());
                }

                if (input.IsNewMouseScrollDown && mouseState.LeftButton == ButtonState.Released && mouseState.RightButton == ButtonState.Released)
                {
                    if (selectedSpell == SpellType.Fire)
                    {
                        if (stats.iceEnabled)
                            selectedSpell = SpellType.Ice;
                    }

                    else if (selectedSpell == SpellType.Ice)
                    {
                        if (stats.moveTerrainEnabled)
                            selectedSpell = SpellType.MoveTerrain;
                    }

                    else if (selectedSpell == SpellType.MoveTerrain)
                    {
                        if (stats.createTurretEnabled)
                            selectedSpell = SpellType.CreateTurret;
                    }
                }

                if (input.IsNewMouseScrollUp && mouseState.LeftButton == ButtonState.Released && mouseState.RightButton == ButtonState.Released)
                {
                    if (selectedSpell == SpellType.CreateTurret)
                    {
                        if (stats.moveTerrainEnabled)
                            selectedSpell = SpellType.MoveTerrain;
                    }

                    else if (selectedSpell == SpellType.MoveTerrain)
                    {
                        if (stats.iceEnabled)
                            selectedSpell = SpellType.Ice;
                    }

                    else if (selectedSpell == SpellType.Ice)
                    {
                        if (stats.fireEnabled)
                            selectedSpell = SpellType.Fire;
                    }
                }

                if (input.IsNewKeyPress(Keys.D1))
                {
                    if (stats.fireEnabled)
                        selectedSpell = SpellType.Fire;
                }

                if (input.IsNewKeyPress(Keys.D2))
                {
                    if (stats.iceEnabled)
                        selectedSpell = SpellType.Ice;
                }

                if (input.IsNewKeyPress(Keys.D3))
                {
                    if (stats.moveTerrainEnabled)
                        selectedSpell = SpellType.MoveTerrain;
                }

                if (input.IsNewKeyPress(Keys.D4))
                {
                    if (stats.createTurretEnabled)
                        selectedSpell = SpellType.CreateTurret;
                }

                if ((currentLeftButton == true && prevLeftButton == false) || (currentRightButton == true && prevRightButton == false))
                    StartSpellcasting(currentLeftButton, currentRightButton);
                if ((currentLeftButton && prevLeftButton) || (currentRightButton && prevRightButton))
                    ContinueSpellcasting(currentLeftButton, currentRightButton, gameTime);
                if ((currentLeftButton == false && prevLeftButton) || (currentRightButton == false && prevRightButton))
                    StopSpellcasting(currentLeftButton, currentRightButton);

                if (input.IsKeyPressed(Keys.Down))
                    timeOfDay.Minutes -= 1;
                if (input.IsKeyPressed(Keys.Up))
                    timeOfDay.Minutes += 1;

                if (input.IsKeyPressed(Keys.Left))
                {
                    sky.Phi -= 0.4f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (sky.Phi > (float)Math.PI * 2.0f)
                        sky.Phi = sky.Phi - (float)Math.PI * 2.0f;
                    if (sky.Phi < 0.0f)
                        sky.Phi = (float)Math.PI * 2.0f + sky.Phi;
                }
                if (input.IsKeyPressed(Keys.Right))
                {
                    sky.Phi += 0.4f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (sky.Phi > (float)Math.PI * 2.0f)
                        sky.Phi = sky.Phi - (float)Math.PI * 2.0f;
                    if (sky.Phi < 0.0f)
                        sky.Phi = (float)Math.PI * 2.0f + sky.Phi;
                }

                UpdateCameraY(gameTime);

                IntersectionRecord mouse_ir = octree.NearestIntersection(camera.GetMouseRay(GraphicsDevice.Viewport));

                UpdateDepthOfFieldAdaptation(mouse_ir);
                UpdateSelectedObject(mouse_ir);
            }
        }



        private void UpdateDepthOfFieldAdaptation(IntersectionRecord mouse_ir)
        {
            lastFocalDistance = settings.FocalDistance;
            lastFocalWidth = settings.FocalWidth;

            targetFocalDistance = Math.Min(Math.Max((float)mouse_ir.Distance, 50), 2000);
            targetFocalWidth = targetFocalDistance * 6;

            if (lastFocalDistance < targetFocalDistance)
                settings.FocalDistance = MathHelper.Lerp(lastFocalDistance, targetFocalDistance, ((targetFocalDistance - lastFocalDistance) / targetFocalDistance) / 20);
            else
                settings.FocalDistance = MathHelper.Lerp(lastFocalDistance, targetFocalDistance, ((lastFocalDistance - targetFocalDistance) / lastFocalDistance) / 20);
            settings.FocalWidth = settings.FocalDistance * 6;
        }

        private void UpdateCameraY(GameTime gameTime)
        {
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

            if ((-eyeHeight > 30) == false)
            {

                if (eyeHeight > 30)
                    camera.EyeHeightStanding += -Math.Sign(eyeHeight) * acceleration * 2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                else if (Math.Truncate(eyeHeight) == 0)
                    camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT + distance;
                else if (Math.Abs(eyeHeight) < 5)
                    camera.EyeHeightStanding += -Math.Sign(eyeHeight) * acceleration / 2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                else
                    camera.EyeHeightStanding += -Math.Sign(eyeHeight) * acceleration * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        private void UpdateSelectedObject(IntersectionRecord mouse_ir)
        {
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
        }

        private void DrawText()
        {
            StringBuilder buffer = new StringBuilder();

            if (displayHelp)
            {
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
                buffer.AppendFormat(" FocalWidth: {0}\n",
                    settings.FocalWidth.ToString());
                buffer.AppendFormat(" FocalDistance: {0}\n\n",
                    settings.FocalDistance.ToString());
                buffer.AppendLine("Press H to return");
            }
            else
            {
                buffer.AppendFormat("FPS: {0}\n", (ScreenManager.Game as Game1).framesPerSecond);
                buffer.AppendFormat(" Update: {0} ms\n", (ScreenManager.Game as Game1).updateMs);
                buffer.AppendFormat(" Draw:     {0} ms\n", (ScreenManager.Game as Game1).drawMs);
                buffer.AppendFormat(" GPU:      {0} ms\n", (ScreenManager.Game as Game1).gpuMs);
                buffer.AppendFormat("  Total: {0} ms\n\n", (ScreenManager.Game as Game1).updateMs + (ScreenManager.Game as Game1).drawMs + (ScreenManager.Game as Game1).gpuMs);
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
                buffer.AppendFormat(" Time of day TimeFloat: {0}\n\n",
                    timeOfDay.TimeFloat.ToString("f2"));

                buffer.AppendFormat(" Non-octree objects: {0}\n\n",
                    objectManager.List.Count);

                buffer.AppendFormat("  Selected Spell: {0}\n",
                    selectedSpell.ToString());
                buffer.AppendFormat("  Selected Spell: {0}\n\n",
                    camera.GetMouseRay(GraphicsDevice.Viewport).Direction.ToString());
                buffer.AppendFormat(" Health: {0}\n",
                    stats.currentHealth.ToString());
                buffer.AppendFormat(" Mana: {0}\n\n",
                    stats.currentMana.ToString());
                buffer.AppendFormat(" Essence: {0}\n\n",
                    stats.currentEssence.ToString());
                buffer.AppendFormat(" Experience: {0}\n\n",
                    stats.currentExp.ToString());
            }
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
        }

        private void DrawFinal()
        {
            //Combine everything
            finalCombineEffect.Parameters["colorMap"].SetValue(colorTarget);
            finalCombineEffect.Parameters["lightMap"].SetValue(lightTarget);
            finalCombineEffect.Parameters["emissionMap"].SetValue(emissiveTarget);

            finalCombineEffect.Techniques[0].Passes[0].Apply();
            //render a full-screen quad
            quadRenderer.Render();
        }

        private void DrawFog()
        {
            if (settings.fog != GameSettings.FogEffect.Off)
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
        }

        private void FrustumLists()
        {
            //reflection
            if (settings.ReflectObjects)
                reflectionObjects = octree.AllFrustumIntersections(new BoundingFrustum(reflectionViewMatrix * camera.ProjectionMatrix));

            //draw
            drawObjects = octree.AllFrustumIntersections(camera.Frustum);
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (IsActive)
            {

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

                if (settings.ReflectObjects)
                    water.RenderReflectionMap(gameTime, camera, reflectionCameraPosition, reflectionViewMatrix, reflectionPlane, sky, lightDirection, lightColor, skyIntensity, reflectionObjects, instancingManager);
                else
                    water.RenderReflectionMapSkyOnly(gameTime, reflectionCameraPosition, reflectionViewMatrix, sky);

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
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * camera.WeaponWorldMatrix(0, -0.7f, 2.5f, 0.03f));
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                        effect.Parameters["FarClip"].SetValue(camera.FarZ);
                        effect.Parameters["Texture"].SetValue(handstex);
                    }
                    mesh.Draw();
                }

                ResolveGBuffer();

                shadowRenderer.RenderShadowMap(GraphicsDevice, camera, lightDirection, Matrix.Identity, octree);

                ssao.DrawSSAO();

                GraphicsDevice.SetRenderTarget(lightTarget);
                GraphicsDevice.Clear(Color.Transparent);

                shadowRenderer.Render(GraphicsDevice, camera, Matrix.Identity, lightDirection, lightColor, colorTarget, normalTarget, depthTarget, ssao.BlurTarget, timeOfDay);
                quadRenderer.Render();

                lightManager.Draw(camera, colorTarget, normalTarget, depthTarget);

                GraphicsDevice.SetRenderTarget(postProcessTarget1);
                GraphicsDevice.Clear(Color.White);
                DrawFinal();

                GraphicsDevice.SetRenderTarget(postProcessTarget2);
                GraphicsDevice.Clear(Color.White);

                water.DrawWater(camera, (float)gameTime.TotalGameTime.TotalMilliseconds * 5, postProcessTarget1, depthTarget, lightDirection, lightColor);

                particleManager.Draw(gameTime, camera, camera.FarZ, depthTarget);

                DrawFog();

                if (settings.dofType != GameSettings.DOFType.Off)
                {
                    if(settings.ToneMap)
                        dofProcessor.DOF(postProcessTarget2, postProcessTarget1, depthTarget, camera, settings.dofType, settings.FocalDistance, settings.FocalWidth);
                    else
                        dofProcessor.DOF(postProcessTarget2, finalTarget, depthTarget, camera, settings.dofType, settings.FocalDistance, settings.FocalWidth);
                }

                if (settings.ToneMap)
                {
                    if (settings.dofType != GameSettings.DOFType.Off)
                        hdrProcessor.ToneMap(postProcessTarget1, finalTarget, (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f, false);
                    else
                        hdrProcessor.ToneMap(postProcessTarget2, finalTarget, (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000f, false);
                }

                GraphicsDevice.SetRenderTarget(null);

                

                if (settings.FXAA)
                {
                    float w = finalTarget.Width;
                    float h = finalTarget.Height;
                    fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];
                    fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
                    fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
                    fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);
                    fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / w);
                    fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / h);
                    fxaaEffect.Parameters["texScreen"].SetValue(finalTarget);

                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwise, fxaaEffect);
                    if (settings.ToneMap)
                        spriteBatch.Draw(finalTarget, new Rectangle(0, 0, finalTarget.Width, finalTarget.Height), Color.White);
                    else if (settings.dofType != GameSettings.DOFType.Off)
                        spriteBatch.Draw(finalTarget, new Rectangle(0, 0, finalTarget.Width, finalTarget.Height), Color.White);
                    else
                        spriteBatch.Draw(postProcessTarget1, new Rectangle(0, 0, postProcessTarget1.Width, postProcessTarget1.Height), Color.White);
                    spriteBatch.End();
                }
                else
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    if (settings.ToneMap)
                        spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
                    else if (settings.dofType != GameSettings.DOFType.Off)
                        spriteBatch.Draw(finalTarget, Vector2.Zero, Color.White);
                    else
                        spriteBatch.Draw(postProcessTarget1, Vector2.Zero, Color.White);
                    spriteBatch.End();
                }

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                DrawText();
                if (settings.ShowGBuffer)
                    DrawGBuffer();
                spriteBatch.End();

                hudManager.DrawMask();
                hudManager.Draw();

                // If the game is transitioning on or off, fade it out to black.
                if (TransitionPosition > 0 || pauseAlpha > 0)
                {
                    float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                    ScreenManager.FadeBackBufferToBlack(alpha);
                }
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                if(settings.ToneMap || settings.dofType != GameSettings.DOFType.Off)
                    spriteBatch.Draw(finalTarget, new Rectangle(0, 0, finalTarget.Width, finalTarget.Height), Color.White);
                else
                    spriteBatch.Draw(postProcessTarget1, new Rectangle(0, 0, postProcessTarget1.Width, postProcessTarget1.Height), Color.White);
                spriteBatch.End();

                hudManager.DrawMask();
                hudManager.Draw();

                // If the game is transitioning on or off, fade it out to black.
                if (TransitionPosition > 0 || pauseAlpha > 0)
                {
                    float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                    ScreenManager.FadeBackBufferToBlack(alpha);
                }
            }
        }

        private void DrawGBuffer()
        {
            int halfWidth = GraphicsDevice.Viewport.Width / 2;
            int halfHeight = GraphicsDevice.Viewport.Height / 2;
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Draw(shadowRenderer.ShadowMap, new Rectangle(0, 0, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(normalTarget, new Rectangle(0, halfHeight, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(depthTarget, new Rectangle(halfWidth, 0, halfWidth, halfHeight), Color.White);
            spriteBatch.Draw(lightTarget, new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight), Color.White);

            //ssaoTarget.SaveAsPng(new System.IO.FileStream("../Tex.png", System.IO.FileMode.Create), 1280, 720);
        }

        #endregion
    }
}
