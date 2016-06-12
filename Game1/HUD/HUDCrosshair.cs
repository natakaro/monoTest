using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Game1.HUD
{
    public class HUDCrosshair : HUDElement
    {
        Texture2D crosshairDotTexture;
        Texture2D crosshairChargeTexture;
        Texture2D crosshairChargeFullTexture;
        Texture2D crosshairCastingTexture;
        Texture2D crosshairHitMarkerTexture;
        Stats stats;
        Stopwatch hitMarkerStopwatch;
        bool hitMarker;

        public HUDCrosshair(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension, Stats stats) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.stats = stats;
            this.enabled = true;
            this.hitMarker = false;
        }
        public override void LoadContent(ContentManager Content)
        {
            crosshairDotTexture = Content.Load<Texture2D>("Hud/crosshair/dot");
            crosshairChargeTexture = Content.Load<Texture2D>("Hud/crosshair/charge");
            crosshairChargeFullTexture = Content.Load<Texture2D>("Hud/crosshair/chargefull");
            crosshairCastingTexture = Content.Load<Texture2D>("Hud/crosshair/casting");
            crosshairHitMarkerTexture = Content.Load<Texture2D>("Hud/crosshair/hitmarker");

            hitMarkerStopwatch = new Stopwatch();
        }

        public override void Draw()
        {
            if(enabled)
            {
                if (stats.spellCharging)
                {
                    if (stats.castSpeed == 0) //moveterrain
                    {                      
                        spriteBatch.Draw(crosshairCastingTexture, position + new Vector2(32, 32), null, new Color(200, 200, 200, 200), MathHelper.ToRadians((float)stats.castTimer.ElapsedMilliseconds/(100f/36f)), dimension / 2, 1f, SpriteEffects.None, 0);
                    }
                    else
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

                }

                spriteBatch.Draw(crosshairDotTexture, position, new Color(200, 200, 200, 200));

                //hitmarker
                if(hitMarker == true)
                {
                    spriteBatch.Draw(crosshairHitMarkerTexture, position, new Color(200, 200, 200, 200));
                }
            }
        }

        public void Update()
        {
            if(hitMarker == true)
            {
                if(hitMarkerStopwatch.ElapsedMilliseconds >= 250)
                {
                    hitMarkerStopwatch.Reset();
                    hitMarker = false;
                }
            }
        }

        public void HandleHitEvent(object sender, EventArgs eventArgs)
        {
            hitMarker = true;
            hitMarkerStopwatch.Start();
        }
    }
}
