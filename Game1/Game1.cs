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
using Game1.Particles;
using Game1.Screens;

namespace Game1
{
    public class Game1 : Game
    {
        public GraphicsDeviceManager graphics;
        private ScreenManager screenManager;
        public GameSettings settings;
        public static Random random;
        
        public int frames;
        public int framesPerSecond;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        #region Diagnostics
        Stopwatch swUpdate;
        Stopwatch swDraw;
        Stopwatch swGPU;

        public double updateMs = 0;
        public double drawMs = 0;
        public double gpuMs = 0;
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false; //to ustawione na false albo vsync na true potrzebne

            settings = new GameSettings(this);

            random = new Random();

            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
            AddInitialScreens();
        }

        protected override void Initialize()
        {
            DebugShapeRenderer.Initialize(graphics.GraphicsDevice);

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false; //vsync
            graphics.PreferredBackBufferWidth = GameSettings.ScreenResolution.X;
            graphics.PreferredBackBufferHeight = GameSettings.ScreenResolution.Y;
            graphics.PreferMultiSampling = false;
            graphics.PreferredBackBufferFormat = SurfaceFormat.HdrBlendable;
            graphics.ApplyChanges();

            Window.Position = new Point(0, 0);
            
            //graphics.ToggleFullScreen();

            swDraw = new Stopwatch();
            swUpdate = new Stopwatch();
            swGPU = new Stopwatch();

            base.Initialize();
        }

        private void AddInitialScreens()
        {
            screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            //if (!this.IsActive)
            //    return;

            swGPU.Stop();
            gpuMs = swGPU.Elapsed.TotalMilliseconds;

            swUpdate.Reset();
            swUpdate.Start();

            base.Update(gameTime);

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

        protected override bool BeginDraw()
        {
            swDraw.Reset();
            swDraw.Start();

            return base.BeginDraw();
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            IncrementFrameCounter();

            updateMs = swUpdate.Elapsed.TotalMilliseconds;
            drawMs = swDraw.Elapsed.TotalMilliseconds;
            swDraw.Stop();

            swGPU.Reset();
            swGPU.Start();
        }

        public ScreenManager ScreenManager
        {
            get { return screenManager; }
        }
    }
}