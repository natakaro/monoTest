using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace Game1.HUD
{
    public class HUDBar : HUDElement
    {
        private float valueMax;
        private float valueCurrent;
        private Color barColor;
        private float alpha;
        private Texture2D barBackground;
        private Texture2D cornerMask;

        private new const float ALPHA = 100f / 255f;

        public HUDBar(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, float value, Vector2 position, Vector2 dimension, Color color, float valueMax) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.valueMax = valueMax;
            this.valueCurrent = value;
            this.barColor = color;
            this.enabled = true;
        }

        public override void LoadContent(ContentManager Content)
        {
            barBackground = Content.Load<Texture2D>("Interface/HUD/bar_background");
            cornerMask = Content.Load<Texture2D>("Interface/HUD/bar_background_cornermask");
            alpha = ALPHA;
        }

        public void Update(float valueCurrent, float valueMax)
        {
            this.valueCurrent = valueCurrent;
            this.valueMax = valueMax;
        }

        public override void Draw()
        {
            if(enabled)
            {
                float percent = valueCurrent / valueMax;

                Rectangle rectangle = new Rectangle();
                rectangle.Width = (int)dimension.X;
                rectangle.Height = (int)dimension.Y;
                rectangle.X = (int)position.X;
                rectangle.Y = (int)position.Y;

                Texture2D dummyTexture = new Texture2D(graphicsDevice, 1, 1);
                dummyTexture.SetData(new Color[] { Color.Black });

                spriteBatch.Draw(dummyTexture, rectangle, Color.Black * alpha);

                rectangle.Width = (int)(dimension.X * percent);
                dummyTexture = new Texture2D(graphicsDevice, 1, 1);
                dummyTexture.SetData(new Color[] { barColor });

                spriteBatch.Draw(dummyTexture, rectangle, barColor * alpha);
            }
        }

        public void DrawBackground()
        {
            if (enabled)
            {
                Rectangle backgroundRectangle = new Rectangle();
                backgroundRectangle.Width = (int)(dimension.X + 10);
                backgroundRectangle.Height = (int)(dimension.Y + 10);
                backgroundRectangle.X = (int)position.X - 5;
                backgroundRectangle.Y = (int)position.Y - 5;

                spriteBatch.Draw(barBackground, backgroundRectangle, Color.White * alpha);
            }
        }

        public void DrawMask()
        {
            if(enabled)
            {
                Rectangle maskRectangle = new Rectangle();
                maskRectangle.Width = 10;
                maskRectangle.Height = (int)dimension.Y;
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
