using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static Game1.Helpers.HexCoordinates;

namespace Game1.HUD
{
    class HUDMinimap : HUDElement
    {
        Texture2D minimapTexture;
        Texture2D dayIcon;
        Texture2D nightIcon;

        TimeOfDay timeOfDay;
        Dictionary<AxialCoordinate, Tile> map;
        
        private const float alpha = 100f / 255f;

        public HUDMinimap(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension, TimeOfDay timeOfDay, Dictionary<AxialCoordinate, Tile> map) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.timeOfDay = timeOfDay;
            this.map = map;
            this.enabled = true;
        }

        public override void Draw()
        {
            if (enabled)
            {
                spriteBatch.Draw(minimapTexture, position, Color.White * alpha);

                Texture2D icon;

                if (timeOfDay.IsDay)
                    icon = dayIcon;
                else
                    icon = nightIcon;

                spriteBatch.Draw(icon, Rotate(MathHelper.ToRadians(timeOfDay.TimeFloat * 15) + MathHelper.PiOver2, minimapTexture.Width / 2 - icon.Width / 2 - 2, position + new Vector2(minimapTexture.Width / 2, minimapTexture.Height / 2)) - new Vector2(icon.Width, icon.Height) / 2, Color.White * ALPHA);
            }
        }

        public Vector2 Rotate(float angle, float distance, Vector2 center)
        {
            return new Vector2((float)(distance * Math.Cos(angle)), (float)(distance * Math.Sin(angle))) + center;
        }

        public override void LoadContent(ContentManager Content)
        {
            minimapTexture = Content.Load<Texture2D>("Interface/HUD/minimapRound");
            dayIcon = Content.Load<Texture2D>("Interface/HUD/Icons/sunrise");
            nightIcon = Content.Load<Texture2D>("Interface/HUD/Icons/night-sky");
        }
    }
}
