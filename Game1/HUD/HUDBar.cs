using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.HUD
{
    public class HUDBar : HUDElement
    {
        private float valueMax;
        private float valueCurrent;
        private Color barColor;

        private bool enabled;

        public HUDBar(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, float value, Vector2 position, Vector2 dimension, Color color, float valueMax) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.valueMax = valueMax;
            this.valueCurrent = value;
            this.barColor = color;
            this.enabled = true;
        }

        public void Show(bool value)
        {
            this.enabled = value;
        }

        public override void Update(float value)
        {
            this.valueCurrent = value;
        }

        public override void Draw()
        {
            if(enabled)
            {
                float percent = valueCurrent / valueMax;

                Rectangle rectangle = new Rectangle();
                rectangle.Width = (int)(dimension.X * percent);
                rectangle.Height = (int)dimension.Y;
                rectangle.X = (int)position.X;
                rectangle.Y = (int)position.Y;

                Texture2D dummyTexture = new Texture2D(graphicsDevice, 1, 1);
                dummyTexture.SetData(new Color[] { barColor });

                spriteBatch.Draw(dummyTexture, rectangle, barColor);
            }
        }
    }
}
