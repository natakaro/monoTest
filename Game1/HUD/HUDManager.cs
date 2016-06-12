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
    public class HUDManager
    {
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphicsDevice;
        private ContentManager content;
        private Stats stats;
        private List<HUDElement> elements;

        private float backbufferWidth;
        private float backbufferHeight;

        private HUDBar healthBar;
        private HUDBar manaBar;
        private HUDCrosshair crosshair;

        public HUDManager(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ContentManager content, Stats stats)
        {
            this.spriteBatch = spriteBatch;
            this.graphicsDevice = graphicsDevice;
            this.content = content;
            this.stats = stats;
            backbufferWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
            elements = new List<HUDElement>();
        }

        public void LoadContent()
        {
            healthBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentHealth, new Vector2(50, backbufferHeight - 100), new Vector2(100, 10), new Color(255, 0, 0, 200), 100);
            manaBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentMana, new Vector2(50, backbufferHeight - 85), new Vector2(200, 10), new Color(0, 0, 255, 200), 200);
            crosshair = new HUDCrosshair(spriteBatch, graphicsDevice, new Vector2(backbufferWidth / 2 - 32, backbufferHeight / 2 - 32), new Vector2(64, 64), stats);

            elements.Add(healthBar);
            elements.Add(manaBar);
            elements.Add(crosshair);

            foreach(HUDElement element in elements)
            {
                element.LoadContent(content);
            }
        }

        public void Draw()
        {
            healthBar.Update(stats.currentHealth);
            manaBar.Update(stats.currentMana);
            crosshair.Update();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach(HUDElement element in elements)
            {
                element.Draw();
            }
            spriteBatch.End();
        }

        #region Properties
        public HUDBar HealthBar
        {
            get { return healthBar; }
        }
        public HUDBar ManaBar
        {
            get { return manaBar; }
        }
        public HUDCrosshair Crosshair
        {
            get { return crosshair; }
        }
        #endregion
    }
}
