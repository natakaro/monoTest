using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Game1.HUD
{
    public class HUDIcon : HUDElement
    {
        Texture2D icon;
        Vector2 origin;
        private const float alpha = 100f / 255f;
        float scale;
        Stopwatch stopwatch;

        bool prevSelected = false;
        bool currentlySelected = false;

        float animationSpeed = 250f;

        public HUDIcon(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension, Texture2D iconTexture, float scale = 1) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.enabled = true;
            this.icon = iconTexture;
            this.scale = scale;
            //origin = new Vector2(icon.Width / 2, icon.Height / 2);
            origin = Vector2.Zero;
            stopwatch = new Stopwatch();
        }

        public void Draw(bool selected)
        {
            if (enabled)
            {
                if (selected)
                {
                    if (currentlySelected == false)
                        stopwatch.Restart();
                    currentlySelected = true;

                    Vector2 position2 = position;
                    position2.Y -= 5;
                    Color color2 = Color.White * ALPHA;

                    if (stopwatch.ElapsedMilliseconds < animationSpeed)
                    {
                        position2.Y = MathHelper.Lerp(position.Y, position2.Y, stopwatch.ElapsedMilliseconds / animationSpeed);
                        color2 = Color.Lerp(Color.White * alpha, Color.White * ALPHA, stopwatch.ElapsedMilliseconds / animationSpeed);
                    }
                    else if (stopwatch.ElapsedMilliseconds >= animationSpeed)
                        stopwatch.Stop();

                    spriteBatch.Draw(icon, position2, null, color2, 0, origin, scale, SpriteEffects.None, 0);
                }
                else
                {
                    if (currentlySelected == true)
                    {
                        stopwatch.Restart();
                        prevSelected = true;
                    }
                    currentlySelected = false;

                    Vector2 position2 = position;
                    Color color2 = Color.White * alpha;

                    if (prevSelected)
                    {
                        if (stopwatch.ElapsedMilliseconds < animationSpeed)
                        {
                            position2.Y = MathHelper.Lerp(position.Y - 5, position.Y, stopwatch.ElapsedMilliseconds / animationSpeed);
                            color2 = Color.Lerp(Color.White * ALPHA, Color.White * alpha, stopwatch.ElapsedMilliseconds / animationSpeed);
                        }
                        else if (stopwatch.ElapsedMilliseconds >= animationSpeed)
                        {
                            stopwatch.Stop();
                            prevSelected = false;
                        }
                    }

                    spriteBatch.Draw(icon, position2, null, color2, 0, origin, scale, SpriteEffects.None, 0);
                }
            }
        }

        public override void LoadContent(ContentManager Content)
        {

        }

        public override void Draw()
        {
            if (enabled)
            {
                spriteBatch.Draw(icon, position, null, Color.White * alpha, 0, origin, scale, SpriteEffects.None, 0);
            }
        }
    }
}
