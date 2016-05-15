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

        private KeyboardState _lastKeyboardState;

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

            StabilizeCascades = true;
            VisualizeCascades = false;

            SplitDistance0 = 0.05f;
            SplitDistance1 = 0.15f;
            SplitDistance2 = 0.50f;
            SplitDistance3 = 1.0f;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.F) && !_lastKeyboardState.IsKeyDown(Keys.F))
            {
                FixedFilterSize++;
                if (FixedFilterSize > FixedFilterSize.Filter7x7)
                    FixedFilterSize = FixedFilterSize.Filter2x2;
            }

            if (keyboardState.IsKeyDown(Keys.C) && !_lastKeyboardState.IsKeyDown(Keys.C))
                StabilizeCascades = !StabilizeCascades;

            if (keyboardState.IsKeyDown(Keys.V) && !_lastKeyboardState.IsKeyDown(Keys.V))
                VisualizeCascades = !VisualizeCascades;

            if (keyboardState.IsKeyDown(Keys.K) && !_lastKeyboardState.IsKeyDown(Keys.K))
                FilterAcrossCascades = !FilterAcrossCascades;

            if (keyboardState.IsKeyDown(Keys.B) && !_lastKeyboardState.IsKeyDown(Keys.B))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
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

            if (keyboardState.IsKeyDown(Keys.O) && !_lastKeyboardState.IsKeyDown(Keys.O))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
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

            _lastKeyboardState = keyboardState;
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
