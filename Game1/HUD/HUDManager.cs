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
        private HUDBar essenceBar;
        private HUDBar coreBar;
        private HUDTargetBar targetHealthBar;
        private HUDCrosshair crosshair;
        private HUDPhaseMessage phaseMessage;

        private Matrix m;
        private AlphaTestEffect a;
        private DepthStencilState s1;
        private DepthStencilState s2;

        public HUDManager(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ContentManager content, Stats stats)
        {
            this.spriteBatch = spriteBatch;
            this.graphicsDevice = graphicsDevice;
            this.content = content;
            this.stats = stats;
            backbufferWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
            elements = new List<HUDElement>();

            m = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, 0, 0, 1);
            a = new AlphaTestEffect(graphicsDevice)
            {
                Projection = m
            };

            s1 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.Always,
                StencilPass = StencilOperation.Replace,
                ReferenceStencil = 0,
                DepthBufferEnable = false,
            };

            s2 = new DepthStencilState
            {
                StencilEnable = true,
                StencilFunction = CompareFunction.NotEqual,
                StencilPass = StencilOperation.Keep,
                ReferenceStencil = 0,
                DepthBufferEnable = false,
            };
        }

        public void LoadContent()
        {
            healthBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentHealth, new Vector2(backbufferWidth / 2 - backbufferWidth / 2 + 100, backbufferHeight - 100), new Vector2(250, 20), new Color(192, 57, 43), stats.maxHealth);
            manaBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentMana, new Vector2(backbufferWidth / 2 - 125, backbufferHeight - 100), new Vector2(250, 20), new Color(41, 128, 185), stats.maxMana);
            essenceBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentEssence, new Vector2(backbufferWidth / 2 + backbufferWidth / 2 - 350, backbufferHeight - 100), new Vector2(250, 20), new Color(142, 68, 173), stats.maxEssence);
            targetHealthBar = new HUDTargetBar(spriteBatch, graphicsDevice, 100, new Vector2(backbufferWidth / 2 - 125, backbufferHeight / 6), new Vector2(250, 20), new Color(192, 57, 43), 100);
            crosshair = new HUDCrosshair(spriteBatch, graphicsDevice, new Vector2(backbufferWidth / 2 - 32, backbufferHeight / 2 - 32), new Vector2(64, 64), stats);
            phaseMessage = new HUDPhaseMessage(spriteBatch, graphicsDevice, new Vector2(backbufferWidth / 2 - 740 / 2, backbufferHeight / 4 - 100 / 2), new Vector2(740, 100));

            elements.Add(healthBar);
            elements.Add(manaBar);
            elements.Add(essenceBar);
            elements.Add(targetHealthBar);
            elements.Add(crosshair);
            elements.Add(phaseMessage);

            foreach(HUDElement element in elements)
            {
                element.LoadContent(content);
            }
        }

        public void Draw()
        {
            healthBar.Update(stats.currentHealth, stats.maxHealth);
            manaBar.Update(stats.currentMana, stats.maxMana);
            essenceBar.Update(stats.currentEssence, stats.maxEssence);
            if (stats.currentTargetedEnemy != null)
                targetHealthBar.Update(stats.currentTargetedEnemy.CurrentHealth, stats.currentTargetedEnemy.MaxHealth, true);
            else if (stats.lastTargetedEnemy != null)
                targetHealthBar.Update(stats.lastTargetedEnemy.CurrentHealth, stats.lastTargetedEnemy.MaxHealth, false);
            else
                targetHealthBar.Update(null, null, false);
            crosshair.Update();
            phaseMessage.Update();

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, s2, null);
            foreach (HUDElement element in elements)
            {
                element.Draw();
            }
            spriteBatch.End();
        }

        public void DrawMask()
        {
            graphicsDevice.Clear(ClearOptions.Stencil, Color.Transparent, 0, 1);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, s1, null, a);
            healthBar.DrawMask();
            manaBar.DrawMask();
            essenceBar.DrawMask();
            targetHealthBar.DrawMask();
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
        public HUDTargetBar TargetHealthBar
        {
            get { return targetHealthBar; }
        }
        public HUDCrosshair Crosshair
        {
            get { return crosshair; }
        }
        public HUDPhaseMessage PhaseMessage
        {
            get { return phaseMessage; }
        }
        #endregion
    }
}
