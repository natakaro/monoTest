using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.HUD
{
    public abstract class HUDElement
    {
        protected SpriteBatch spriteBatch;
        protected GraphicsDevice graphicsDevice;
        protected Vector2 position;
        protected Vector2 dimension;

        public HUDElement(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension)
        {
            this.spriteBatch = spriteBatch;
            this.graphicsDevice = graphicsDevice;
            this.position = position;
            this.dimension = dimension;
        }

        public abstract void Update(float value);
        public abstract void Draw();
    }
}
