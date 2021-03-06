﻿using System;
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
    public class HUDIcons : HUDElement
    {
        HUDIcon healthIcon;
        Texture2D healthIconTexture;
        HUDIcon manaIcon;
        Texture2D manaIconTexture;
        HUDIcon essenceIcon;
        Texture2D essenceIconTexture;
        HUDIcon levelIcon;
        Texture2D levelIconTexture;

        HUDIcon moveTerrainIcon;
        Texture2D moveTerrainIconTexture;
        HUDIcon fireIcon;
        Texture2D fireIconTexture;
        HUDIcon iceIcon;
        Texture2D iceIconTexture;
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

        public void UpdateIconPositions()
        {
            int i = 0;
            foreach (HUDIcon icon in spellIcons)
            {
                if (icon.Enable)
                    i++;
            }

            Vector2 spellPosition = CalculateSpellIconsPosition(i, 64);
            Vector2 spellOffset = new Vector2(64, 0);

            FireIcon.Position = spellPosition + spellOffset * 0;
            IceIcon.Position = spellPosition + spellOffset * 1;
            MoveTerrainIcon.Position = spellPosition + spellOffset * 2;
            CreateTurretIcon.Position = spellPosition + spellOffset * 3;
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
            levelIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/level");

            moveTerrainIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/moveTerrainIcon");
            fireIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/fireIcon");
            iceIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/iceIcon");
            createTurretIconTexture = Content.Load<Texture2D>("Interface/HUD/icons/createTurretIcon");

            Vector2 offset = new Vector2(-34, -6);
            Vector2 offsetRight = new Vector2(252, -6);

            healthIcon = new HUDIcon(spriteBatch, graphicsDevice, hudManager.HealthBar.Position + offset, dimension, healthIconTexture);
            manaIcon = new HUDIcon(spriteBatch, graphicsDevice, hudManager.ManaBar.Position + offset, dimension, manaIconTexture);
            essenceIcon = new HUDIcon(spriteBatch, graphicsDevice, hudManager.EssenceBar.Position + offset, dimension, essenceIconTexture);
            levelIcon = new HUDIcon(spriteBatch, graphicsDevice, hudManager.ExpBar.Position + offsetRight, dimension, levelIconTexture);

            icons.Add(healthIcon);
            icons.Add(manaIcon);
            icons.Add(essenceIcon);
            icons.Add(levelIcon);

            Vector2 spellPosition = CalculateSpellIconsPosition(4, 64);
            Vector2 spellOffset = new Vector2(64, 0);

            fireIcon = new HUDIcon(spriteBatch, graphicsDevice, spellPosition + spellOffset * spellIcons.Count, dimension, fireIconTexture);
            spellIcons.Add(fireIcon);
            iceIcon = new HUDIcon(spriteBatch, graphicsDevice, spellPosition + spellOffset * spellIcons.Count, dimension, iceIconTexture);
            spellIcons.Add(iceIcon);
            moveTerrainIcon = new HUDIcon(spriteBatch, graphicsDevice, spellPosition + spellOffset * spellIcons.Count, dimension, moveTerrainIconTexture);
            spellIcons.Add(moveTerrainIcon);
            createTurretIcon = new HUDIcon(spriteBatch, graphicsDevice, spellPosition + spellOffset * spellIcons.Count, dimension, createTurretIconTexture);
            spellIcons.Add(createTurretIcon);
        }

        #region accessors
        public HUDIcon HealthIcon
        {
            get { return healthIcon; }
        }
        public HUDIcon ManaIcon
        {
            get { return manaIcon; }
        }
        public HUDIcon EssenceIcon
        {
            get { return essenceIcon; }
        }
        public HUDIcon LevelIcon
        {
            get { return levelIcon; }
        }
        public HUDIcon FireIcon
        {
            get { return fireIcon; }
        }
        public HUDIcon IceIcon
        {
            get { return iceIcon; }
        }
        public HUDIcon MoveTerrainIcon
        {
            get { return moveTerrainIcon; }
        }
        public HUDIcon CreateTurretIcon
        {
            get { return createTurretIcon; }
        }
        #endregion
    }
}
