using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Game1.Screens;
using static Game1.Screens.GameplayScreen;

namespace Game1.HUD
{
    class HUDIcons : HUDElement
    {
        HUDIcon healthIcon;
        Texture2D healthIconTexture;
        HUDIcon manaIcon;
        Texture2D manaIconTexture;
        HUDIcon essenceIcon;
        Texture2D essenceIconTexture;

        HUDIcon moveTerrainIcon;
        Texture2D moveTerrainIconTexture;
        HUDIcon fireIcon;
        Texture2D fireIconTexture;
        HUDIcon createTurretIcon;
        Texture2D createTurretIconTexture;

        List<HUDIcon> icons;
        List<HUDIcon> spellIcons;

        HUDManager hudManager;

        private new const float ALPHA = 100f / 255f;

        public HUDIcons(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension, HUDManager hudManager) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.enabled = true;
            this.hudManager = hudManager;

            icons = new List<HUDIcon>();
            spellIcons = new List<HUDIcon>();
        }

        public override void Draw()
        {
            if(enabled)
            {
                foreach(HUDIcon icon in icons)
                {
                    icon.Draw();
                }

                for (int i = 0; i < spellIcons.Count; i++)
                {
                    if (i == (int)selectedSpell)
                        spellIcons[i].Draw(true);
                    else
                        spellIcons[i].Draw(false);
                }

                //spriteBatch.Draw(healthIcon, hudManager.HealthBar.Position + offset, Color.White * ALPHA);
                //spriteBatch.Draw(manaIcon, hudManager.ManaBar.Position + offset, Color.White * ALPHA);
                //spriteBatch.Draw(essenceIcon, hudManager.EssenceBar.Position + offset, Color.White * ALPHA);
            }
        }

        public Vector2 CalculateSpellIconsPosition(int count, float iconSize)
        {
            Vector2 ret = new Vector2(hudManager.BackbufferWidth / 2, hudManager.BackbufferHeight - 100);

            ret.X -= (float)(iconSize * count) / 2f;
            return ret;
        }

        public override void LoadContent(ContentManager Content)
        {
            healthIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/health");
            manaIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/mana");
            essenceIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/essence");

            moveTerrainIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/moveTerrainIcon");
            fireIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/fireIcon");
            createTurretIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/createTurretIcon");

            Vector2 offset = new Vector2(-34, -6);

            healthIcon = new HUDIcon(spriteBatch, graphicsDevice, hudManager.HealthBar.Position + offset, dimension, healthIconTexture);
            manaIcon = new HUDIcon(spriteBatch, graphicsDevice, hudManager.ManaBar.Position + offset, dimension, manaIconTexture);
            essenceIcon = new HUDIcon(spriteBatch, graphicsDevice, hudManager.EssenceBar.Position + offset, dimension, essenceIconTexture);

            Vector2 pos = CalculateSpellIconsPosition(3, 64);
            Vector2 spellOffset = new Vector2(64, 0);

            fireIcon = new HUDIcon(spriteBatch, graphicsDevice, pos, dimension, fireIconTexture);
            moveTerrainIcon = new HUDIcon(spriteBatch, graphicsDevice, pos + spellOffset, dimension, moveTerrainIconTexture);
            createTurretIcon = new HUDIcon(spriteBatch, graphicsDevice, pos + spellOffset * 2, dimension, createTurretIconTexture);

            icons.Add(healthIcon);
            icons.Add(manaIcon);
            icons.Add(essenceIcon);

            spellIcons.Add(fireIcon);
            spellIcons.Add(moveTerrainIcon);
            spellIcons.Add(createTurretIcon);
        }
    }
}
