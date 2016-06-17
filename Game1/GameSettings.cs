using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class GameSettings : GameComponent
    {
        private static readonly int[] KernelSizes = { 2, 3, 5, 7 };
        public Vector3 LightDirection;
        public Vector3 LightColor;
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

        public bool Instancing;
        public bool ShowGBuffer;
        public bool DrawDebugShapes;

        public bool FXAA;
        public bool DrawFog;
        public bool DrawSSAO;
        public bool ToneMap;
        public bool Reflect;
        public float WaterHeight;

        public float SSAORadius;
        public float SSAOPower;

        public bool EnemyMove;

        public enum FogEffect
        {
            FogLinear = 0,
            FogExp = 1,
            FogExp2 = 2
        }

        public FogEffect fog;

        public enum DOFType
        {
            None = 0,
            BlurBuffer = 1,
            BlurBufferDepthCorrection = 2,
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

        public GameSettings(Game game)
            : base(game)
        {
            LightDirection = Vector3.Normalize(new Vector3(1, 1, -1));
            LightColor = new Vector3(3, 3, 3);
            Bias = 0.002f;
            OffsetScale = 0.0f;
            FixedFilterSize = FixedFilterSize.Filter3x3;

            StabilizeCascades = false;
            VisualizeCascades = false;

            SplitDistance0 = 0.05f;
            SplitDistance1 = 0.15f;
            SplitDistance2 = 0.50f;
            SplitDistance3 = 1.0f;

            Instancing = true;
            ShowGBuffer = false;
            DrawDebugShapes = false;

            fog = FogEffect.FogExp;
            dofType = DOFType.DiscBlur;

            FXAA = true;
            DrawFog = true;
            DrawSSAO = true;
            ToneMap = true;
            Reflect = true;
            WaterHeight = 5;

            EnemyMove = false;

            SSAORadius = 5;
            SSAOPower = 2;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        private bool KeyJustPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _prevKeyboardState.IsKeyUp(key);
        }

        public override void Update(GameTime gameTime)
        {
            _prevKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            if (KeyJustPressed(Keys.F))
            {
                FixedFilterSize++;
                if (FixedFilterSize > FixedFilterSize.Filter7x7)
                    FixedFilterSize = FixedFilterSize.Filter2x2;
            }

            if (KeyJustPressed(Keys.C))
                StabilizeCascades = !StabilizeCascades;

            if (KeyJustPressed(Keys.V))
                VisualizeCascades = !VisualizeCascades;

            if (KeyJustPressed(Keys.K))
                FilterAcrossCascades = !FilterAcrossCascades;

            if (KeyJustPressed(Keys.B))
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

            if (KeyJustPressed(Keys.O))
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

            if (KeyJustPressed(Keys.G))
            {
                ShowGBuffer = !ShowGBuffer;
            }

            if (KeyJustPressed(Keys.X))
                FXAA = !FXAA;

            if (KeyJustPressed(Keys.D1))
            {
                Instancing = !Instancing;
            }

            if (KeyJustPressed(Keys.D2))
            {
                DrawDebugShapes = !DrawDebugShapes;
            }

            if (KeyJustPressed(Keys.D4))
                DrawFog = !DrawFog;

            if (KeyJustPressed(Keys.D5))
            {
                fog++;
                if ((int)fog == 3)
                    fog = FogEffect.FogLinear;
            }

            if (KeyJustPressed(Keys.D6))
            {
                dofType++;
                if ((int)dofType == 4)
                    dofType = DOFType.None;
            }

            if (KeyJustPressed(Keys.D9))
            {
                Reflect = !Reflect;
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Left))
                FocalWidth -= 0.1f;

            if (_currentKeyboardState.IsKeyDown(Keys.Right))
                FocalWidth += 0.1f;

            if (_currentKeyboardState.IsKeyDown(Keys.Down))
                FocalDistance -= 0.1f;

            if (_currentKeyboardState.IsKeyDown(Keys.Up))
                FocalDistance += 0.1f;

            if (KeyJustPressed(Keys.D8))
                DrawSSAO = !DrawSSAO;

            if (KeyJustPressed(Keys.F1))
                SSAORadius -= 0.1f;

            if (KeyJustPressed(Keys.F2))
                SSAORadius += 0.1f;

            if (KeyJustPressed(Keys.F3))
                SSAOPower -= 0.1f;

            if (KeyJustPressed(Keys.F4))
                SSAOPower += 0.1f;

            if (KeyJustPressed(Keys.F5))
                ToneMap = !ToneMap;

            if (KeyJustPressed(Keys.R))
                Reflect = !Reflect;
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
