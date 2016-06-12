﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.HUD
{
    class HUDCrosshair : HUDElement
    {
        Texture2D crosshairDotTexture;
        Texture2D crosshairChargeTexture;
        Texture2D crosshairChargeFullTexture;
        Stats stats;

        public HUDCrosshair(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension, Stats stats) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.stats = stats;
            this.enabled = true;
        }
        public override void LoadContent(ContentManager Content)
        {
            crosshairDotTexture = Content.Load<Texture2D>("Hud/crosshair/dot");
            crosshairChargeTexture = Content.Load<Texture2D>("Hud/crosshair/charge");
            crosshairChargeFullTexture = Content.Load<Texture2D>("Hud/crosshair/chargefull");
        }

        public override void Draw()
        {
            if(enabled)
            {
                Rectangle rectangle = new Rectangle();
                rectangle.Width = (int)dimension.X;
                rectangle.Height = (int)dimension.Y;
                rectangle.X = (int)position.X;
                rectangle.Y = (int)position.Y;


                if (stats.spellCharging)
                {
                    float step = stats.castSpeed / 30;

                    if (stats.castTimer.ElapsedMilliseconds < stats.castSpeed)
                    {
                        for (int i = 0; i < (int)(Math.Min(stats.castTimer.ElapsedMilliseconds, stats.castSpeed) / step); i++)
                        {
                            spriteBatch.Draw(crosshairChargeTexture, position + new Vector2(32, 32), null, new Color(100, 100, 100, 100), MathHelper.ToRadians(180 / 30 * i), dimension / 2, 1f, SpriteEffects.None, 0);
                        }
                    }
                    else
                    {
                        spriteBatch.Draw(crosshairChargeFullTexture, position, new Color(200, 200, 200, 200));
                    }

                }

                spriteBatch.Draw(crosshairDotTexture, position, new Color(200, 200, 200, 200));
            }
        }

        public void Update()
        {

        }
    }
}
