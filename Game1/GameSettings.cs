using Game1.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class GameSettings
    {
        private static readonly int[] KernelSizes = { 2, 3, 5, 7 };

        public Vector3 LightDirection;
        public Vector3 LightColor;

        public bool Shadows;
        public FixedFilterSize FixedFilterSize;
        public bool VisualizeCascades;
        public bool StabilizeCascades;
        public bool FilterAcrossCascades;
        public float SplitDistance0;
        public float SplitDistance1;
        public float SplitDistance2;
        public float SplitDistance3;
        public float Bias;
        public float OffsetScale;

        public bool ShowGBuffer;
        public bool DrawDebugShapes;

        public bool FXAA;
        public bool DrawSSAO;
        public bool ToneMap;
        public bool ReflectObjects;
        public float WaterHeight;

        public bool Vignette;

        public float SSAORadius;
        public float SSAOPower;

        public Point ScreenResolution;
        Game1 game;

        public bool Polish;

        public enum FogEffect
        {
            Off = 0,
            Linear = 1,
            Exponential = 2,
            Exponential2 = 3
        }

        public FogEffect fog;

        public enum DOFType
        {
            Off = 0,
            Downscale = 1,
            DownscaleWithDepth = 2,
            DiscBlur = 3
        }

        public DOFType dofType;

        public float FocalDistance = 7.0f;
        public float FocalWidth = 2000.0f;

        private KeyboardState _prevKeyboardState;
        private KeyboardState _currentKeyboardState;

        public int FixedFilterKernelSize
        {
            get { return KernelSizes[(int)FixedFilterSize]; }
        }

        public GameSettings(Game1 game)
        {
            this.game = game;
            Shadows = true;

            LightDirection = Vector3.Normalize(new Vector3(1, 1, -1));
            LightColor = new Vector3(3, 3, 3);
            Bias = 0.004f;
            OffsetScale = 0.01f;
            FixedFilterSize = FixedFilterSize.Filter3x3;

            StabilizeCascades = true;
            VisualizeCascades = false;

            SplitDistance0 = 0.05f;
            SplitDistance1 = 0.15f;
            SplitDistance2 = 0.50f;
            SplitDistance3 = 1.0f;

            ShowGBuffer = false;
            DrawDebugShapes = false;

            fog = FogEffect.Exponential;
            dofType = DOFType.DiscBlur;

            FXAA = true;
            DrawSSAO = true;
            ToneMap = true;
            ReflectObjects = true;
            WaterHeight = 5;

            Vignette = true;

            SSAORadius = 5;
            SSAOPower = 2;

            Polish = false;
        }

        public void Update(GameTime gameTime, InputState state)
        {
            _prevKeyboardState = _currentKeyboardState;
            _currentKeyboardState = state.CurrentKeyboardState;

            if (state.IsNewKeyPress(Keys.F))
            {
                FixedFilterSize++;
                if (FixedFilterSize > FixedFilterSize.Filter7x7)
                    FixedFilterSize = FixedFilterSize.Filter2x2;
            }

            if (state.IsNewKeyPress(Keys.C))
                StabilizeCascades = !StabilizeCascades;

            if (state.IsNewKeyPress(Keys.V))
                VisualizeCascades = !VisualizeCascades;

            if (state.IsNewKeyPress(Keys.K))
                FilterAcrossCascades = !FilterAcrossCascades;

            if (state.IsNewKeyPress(Keys.B))
            {
                if (_currentKeyboardState.IsKeyDown(Keys.LeftShift) || _currentKeyboardState.IsKeyDown(Keys.RightShift))
                {
                    Bias += 0.001f;
                }
                else
                {
                    Bias -= 0.001f;
                    Bias = Math.Max(Bias, 0.0f);
                }
                Bias = (float)Math.Round(Bias, 3);
            }

            if (state.IsNewKeyPress(Keys.O))
            {
                if (_currentKeyboardState.IsKeyDown(Keys.LeftShift) || _currentKeyboardState.IsKeyDown(Keys.RightShift))
                {
                    OffsetScale += 0.1f;
                }
                else
                {
                    OffsetScale -= 0.1f;
                    OffsetScale = Math.Max(OffsetScale, 0.0f);
                }
                OffsetScale = (float)Math.Round(OffsetScale, 1);
            }

            if (state.IsNewKeyPress(Keys.G))
            {
                ShowGBuffer = !ShowGBuffer;
            }

            if (state.IsNewKeyPress(Keys.X))
                FXAA = !FXAA;

            if (state.IsNewKeyPress(Keys.Y))
            {
                DrawDebugShapes = !DrawDebugShapes;
            }

            if (state.IsNewKeyPress(Keys.D5))
            {
                fog++;
                if ((int)fog == 4)
                    fog = FogEffect.Off;
            }

            if (state.IsNewKeyPress(Keys.D6))
            {
                dofType++;
                if ((int)dofType == 4)
                    dofType = DOFType.Off;
            }

            if (state.IsNewKeyPress(Keys.D9))
            {
                ReflectObjects = !ReflectObjects;
            }

            if (state.IsNewKeyPress(Keys.D8))
                DrawSSAO = !DrawSSAO;

            if (state.IsNewKeyPress(Keys.F1))
                SSAORadius -= 0.1f;

            if (state.IsNewKeyPress(Keys.F2))
                SSAORadius += 0.1f;

            if (state.IsNewKeyPress(Keys.F3))
                SSAOPower -= 0.1f;

            if (state.IsNewKeyPress(Keys.F4))
                SSAOPower += 0.1f;

            if (state.IsNewKeyPress(Keys.F5))
                ToneMap = !ToneMap;

            if (state.IsNewKeyPress(Keys.R))
                ReflectObjects = !ReflectObjects;

            if (state.IsNewKeyPress(Keys.Q))
                Vignette = !Vignette;

            if (state.IsNewKeyPress(Keys.T))
            {
                //game.Window.IsBorderless = !game.Window.IsBorderless;
                //game.Window.BeginScreenDeviceChange(true);
            }

        }
    }
    public enum FixedFilterSize
    {
        Filter2x2,
        Filter3x3,
        Filter5x5,
        Filter7x7
    }
}
