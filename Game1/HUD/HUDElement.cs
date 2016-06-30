using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        protected bool enabled;

        protected const float ALPHA = 200f / 255f;

        public HUDElement(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension)
        {
            this.spriteBatch = spriteBatch;
            this.graphicsDevice = graphicsDevice;
            this.position = position;
            this.dimension = dimension;
            this.enabled = false;
        }

        public abstract void LoadContent(ContentManager Content);
        public abstract void Draw();

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector2 Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }
        public bool Enable
        {
            get { return enabled; }
            set { enabled = value; }
        }
    }
}
