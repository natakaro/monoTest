using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.HUD
{
    public class HUDTargetBar : HUDElement
    {
        private float valueMax;
        private float valueCurrent;
        private Color barColor;
        private Texture2D barBackground;
        private Texture2D cornerMask;
        private float alpha;
        private Stopwatch stopwatch;

        public HUDTargetBar(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, float value, Vector2 position, Vector2 dimension, Color color, float valueMax) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.valueMax = valueMax;
            this.valueCurrent = value;
            this.barColor = color;
            this.enabled = false;
        }

        public override void LoadContent(ContentManager Content)
        {
            barBackground = Content.Load<Texture2D>("Hud/bar_background");
            cornerMask = Content.Load<Texture2D>("Hud/bar_background_cornermask");
            stopwatch = new Stopwatch();
        }

        public void Update(float? valueCurrent, float? valueMax, bool current)
        {
            if (valueCurrent == null)
            {
                stopwatch.Reset();
                enabled = false;
            }
            else
            {
                this.valueCurrent = (float)valueCurrent;
                this.valueMax = (float)valueMax;

                if (current == true)
                {
                    stopwatch.Reset();
                    enabled = true;
                }
                else
                {
                    stopwatch.Start();

                    if (stopwatch.ElapsedMilliseconds < 1000)
                        alpha = ALPHA;
                    else
                    {

                        alpha = MathHelper.Lerp(ALPHA, 0, (stopwatch.ElapsedMilliseconds - 1000f) / 1000f);

                        if (stopwatch.ElapsedMilliseconds > 2000)
                        {
                            stopwatch.Reset();
                            enabled = false;
                        }
                    }
                }
            }
        }

        public override void Draw()
        {
            if (enabled)
            {
                float percent = valueCurrent / valueMax;

                Rectangle rectangle = new Rectangle();
                rectangle.Width = (int)(dimension.X * percent);
                rectangle.Height = (int)dimension.Y;
                rectangle.X = (int)position.X;
                rectangle.Y = (int)position.Y;

                Rectangle backgroundRectangle = new Rectangle();
                backgroundRectangle.Width = (int)dimension.X + 10;
                backgroundRectangle.Height = (int)dimension.Y + 10 ;
                backgroundRectangle.X = (int)position.X - 5;
                backgroundRectangle.Y = (int)position.Y - 5;

                Texture2D dummyTexture = new Texture2D(graphicsDevice, 1, 1);
                dummyTexture.SetData(new Color[] { barColor });

                spriteBatch.Draw(dummyTexture, rectangle, barColor * alpha);
                spriteBatch.Draw(barBackground, backgroundRectangle, Color.White * alpha);
            }
        }

        public void DrawMask()
        {
            if (enabled)
            {
                Rectangle maskRectangle = new Rectangle();
                maskRectangle.Width = 10;
                maskRectangle.Height = 20;
                maskRectangle.X = (int)position.X;
                maskRectangle.Y = (int)position.Y;
                spriteBatch.Draw(cornerMask, maskRectangle, Color.White);
                maskRectangle.X = (int)position.X + (int)dimension.X - maskRectangle.Width;
                maskRectangle.Y = (int)position.Y;
                spriteBatch.Draw(cornerMask, maskRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }
        }
    }
}
