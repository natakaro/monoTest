using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.HUD
{
    class HUDIcons : HUDElement
    {
        Texture2D healthIcon;
        Texture2D manaIcon;
        Texture2D essenceIcon;

        HUDManager hudManager;

        private new const float ALPHA = 100f / 255f;

        public HUDIcons(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension, HUDManager hudManager) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.enabled = true;
            this.hudManager = hudManager;
        }

        public override void Draw()
        {
            if(enabled)
            {
                Vector2 offset = new Vector2(-34, -6);
                spriteBatch.Draw(healthIcon, hudManager.HealthBar.Position + offset, Color.White * ALPHA);
                spriteBatch.Draw(manaIcon, hudManager.ManaBar.Position + offset, Color.White * ALPHA);
                spriteBatch.Draw(essenceIcon, hudManager.EssenceBar.Position + offset, Color.White * ALPHA);
            }
        }

        public override void LoadContent(ContentManager Content)
        {
            healthIcon = Content.Load<Texture2D>("Interface/HUD/icons/health");
            manaIcon = Content.Load<Texture2D>("Interface/HUD/icons/mana");
            essenceIcon = Content.Load<Texture2D>("Interface/HUD/icons/essence");
        }
    }
}
