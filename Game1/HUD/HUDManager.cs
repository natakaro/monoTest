using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Game1.Helpers.HexCoordinates;

namespace Game1.HUD
{
    public class HUDManager
    {
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphicsDevice;
        private ContentManager content;
        private Stats stats;
        private TimeOfDay timeOfDay;
        private Dictionary<AxialCoordinate, Tile> map;

        private List<HUDElement> elements;

        private float backbufferWidth;
        private float backbufferHeight;

        private HUDBar healthBar;
        private HUDBar manaBar;
        private HUDBar essenceBar;
        private HUDBar expBar;
        private HUDTargetBar targetHealthBar;
        private HUDCrosshair crosshair;
        private HUDPhaseMessage phaseMessage;
        private HUDIcons hudIcons;
        private HUDMinimap minimap;

        private Matrix m;
        private AlphaTestEffect a;
        private DepthStencilState s1;
        private DepthStencilState s2;
        private BlendState mask;

        public HUDManager(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ContentManager content, Stats stats, TimeOfDay timeOfDay, Dictionary<AxialCoordinate, Tile> map)
        {
            this.spriteBatch = spriteBatch;
            this.graphicsDevice = graphicsDevice;
            this.content = content;
            this.stats = stats;
            this.timeOfDay = timeOfDay;
            this.map = map;
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

            mask = new BlendState
            {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                AlphaSourceBlend = Blend.One,
                BlendFactor = Color.White,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.InverseSourceAlpha,
                ColorWriteChannels = ColorWriteChannels.None,
            };
        }

        public void LoadContent()
        {
            healthBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentHealth, new Vector2(backbufferWidth / 2 - backbufferWidth / 2 + 100, backbufferHeight - 160), new Vector2(250, 20), new Color(192, 57, 43), stats.maxHealth);
            manaBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentMana, new Vector2(backbufferWidth / 2 - backbufferWidth / 2 + 100, backbufferHeight - 120), new Vector2(250, 20), new Color(41, 128, 185), stats.maxMana);
            essenceBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentEssence, new Vector2(backbufferWidth / 2 - backbufferWidth / 2 + 100, backbufferHeight - 80), new Vector2(250, 20), new Color(142, 68, 173), stats.maxEssence);
            expBar = new HUDBar(spriteBatch, graphicsDevice, stats.currentExp, new Vector2(backbufferWidth - 300 - 100, backbufferHeight - 80), new Vector2(250, 20), new Color(241, 196, 15), stats.maxExp);
            targetHealthBar = new HUDTargetBar(spriteBatch, graphicsDevice, 100, new Vector2(backbufferWidth / 2 - 125, backbufferHeight / 8), new Vector2(250, 20), new Color(192, 57, 43), 100);
            crosshair = new HUDCrosshair(spriteBatch, graphicsDevice, new Vector2(backbufferWidth / 2 - 32, backbufferHeight / 2 - 32), new Vector2(64, 64), stats);
            phaseMessage = new HUDPhaseMessage(spriteBatch, graphicsDevice, new Vector2(backbufferWidth / 2 - 740 / 2, backbufferHeight / 4 - 100 / 2), new Vector2(740, 100));
            hudIcons = new HUDIcons(spriteBatch, graphicsDevice, new Vector2(backbufferWidth / 2, backbufferHeight - 100), new Vector2(0, 0), this);
            minimap = new HUDMinimap(spriteBatch, graphicsDevice, new Vector2(backbufferWidth - 250, 0), new Vector2(250, 250), timeOfDay, map);

            elements.Add(healthBar);
            elements.Add(manaBar);
            elements.Add(essenceBar);
            elements.Add(expBar);
            elements.Add(targetHealthBar);
            elements.Add(crosshair);
            elements.Add(phaseMessage);
            elements.Add(hudIcons);
            elements.Add(minimap);

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
            expBar.Update(stats.currentExp, stats.maxExp);
            if (stats.currentTargetedEnemy != null)
                targetHealthBar.Update(stats.currentTargetedEnemy.CurrentHealth, stats.currentTargetedEnemy.MaxHealth, true);
            else if (stats.lastTargetedEnemy != null)
                targetHealthBar.Update(stats.lastTargetedEnemy.CurrentHealth, stats.lastTargetedEnemy.MaxHealth, false);
            else
                targetHealthBar.Update(null, null, false);
            crosshair.Update();
            phaseMessage.Update();

            //special case for bars
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, s2, null);
            foreach (HUDElement element in elements)
            {
                if (element is HUDBar || element is HUDTargetBar || element is HUDMinimap)
                {
                    element.Draw();
                }
            }
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (HUDElement element in elements)
            {
                if(element is HUDBar)
                {
                    HUDBar bar = element as HUDBar;
                    bar.DrawBackground();
                }
                else if (element is HUDTargetBar)
                {
                    HUDTargetBar targetBar = element as HUDTargetBar;
                    targetBar.DrawBackground();
                }
                else if (element is HUDMinimap)
                {
                    HUDMinimap minimap = element as HUDMinimap;
                    minimap.DrawBackground();
                }
                else
                    element.Draw();
            }
            spriteBatch.End();
        }

        public void DrawMask()
        {
            graphicsDevice.Clear(ClearOptions.Stencil, Color.Transparent, 0, 1);

            spriteBatch.Begin(SpriteSortMode.Deferred, mask, null, s1, null, a);
            healthBar.DrawMask();
            manaBar.DrawMask();
            essenceBar.DrawMask();
            expBar.DrawMask();
            targetHealthBar.DrawMask();
            minimap.DrawMask();
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
        public HUDBar EssenceBar
        {
            get { return essenceBar; }
        }
        public HUDBar ExpBar
        {
            get { return expBar; }
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
        public HUDIcons Icons
        {
            get { return hudIcons; }
        }
        public HUDMinimap Minimap
        {
            get { return minimap; }
        }

        public float BackbufferWidth
        {
            get { return backbufferWidth; }
        }

        public float BackbufferHeight
        {
            get { return backbufferHeight; }
        }
        #endregion
    }
}
