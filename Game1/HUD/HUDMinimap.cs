using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static Game1.Helpers.HexCoordinates;
using Game1.Screens;

namespace Game1.HUD
{
    class HUDMinimap : HUDElement
    {
        Texture2D minimapTexture;
        Texture2D minimapMaskTexture;
        Texture2D dayIcon;
        Texture2D nightIcon;

        Texture2D tileTex;
        Texture2D playerTex;

        TimeOfDay timeOfDay;
        Dictionary<AxialCoordinate, Tile> map;

        Texture2D mapTex;
        Vector2 mapTileCount;

        private const float alpha = 100f / 255f;

        Vector2 minimapCenter;

        public HUDMinimap(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension, TimeOfDay timeOfDay, Dictionary<AxialCoordinate, Tile> map) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.timeOfDay = timeOfDay;
            this.map = map;
            this.enabled = true;

            mapTex = GameplayScreen.mapTex;
            mapTileCount = new Vector2(mapTex.Width, mapTex.Height);
        }

        public override void Draw()
        {
            if (enabled)
            {
                var cube = pixelToAxialH(GameplayScreen.camera.Position, Map.size).ToCube();
                cube = CubeRound(cube);

                var offset = cube.to_oddQ_Offset();
                //offset.x += 0.5f;
                //offset.y += 0.5f;

                Map.Draw(spriteBatch, tileTex, playerTex, position + minimapCenter, mapTileCount, cube, 26, 0.5f, 1);
                //Map.Draw(spriteBatch, tileTex, playerTex, position + minimapCenter - new Vector2((offset.x - (float)Math.Truncate(offset.x)) * tileTex.Width, (offset.y - (float)Math.Truncate(offset.y)) * tileTex.Height), mapTileCount, cube, 26, 0.5f, 1);
                Map.DrawAssets(spriteBatch, tileTex, position + minimapCenter, mapTileCount, cube, 26, 0.5f, 1);
                //Map.Draw(spriteBatch, tileTex, position + minimapCenter - new Vector2(offset.x * tileTex.Width, offset.y * tileTex.Height), mapTileCount, alpha);
            }
        }

        public void DrawBackground()
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

        public void DrawMask()
        {
            if (enabled)
            {
                spriteBatch.Draw(minimapMaskTexture, position, Color.White * alpha);
            }
        }

        public Vector2 Rotate(float angle, float distance, Vector2 center)
        {
            return new Vector2((float)(distance * Math.Cos(angle)), (float)(distance * Math.Sin(angle))) + center;
        }

        public override void LoadContent(ContentManager Content)
        {
            minimapTexture = Content.Load<Texture2D>("Interface/HUD/minimapRound");
            minimapMaskTexture = Content.Load<Texture2D>("Interface/HUD/minimapRoundMask");
            dayIcon = Content.Load<Texture2D>("Interface/HUD/Icons/sunrise");
            nightIcon = Content.Load<Texture2D>("Interface/HUD/Icons/night-sky");
            tileTex = Content.Load<Texture2D>("Interface/Map/tile");
            playerTex = Content.Load<Texture2D>("Interface/Map/tile");

            minimapCenter = new Vector2(minimapTexture.Width / 2, minimapTexture.Height / 2);
        }
    }
}
