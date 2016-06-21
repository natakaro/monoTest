using Game1.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Game1.Helpers.HexCoordinates;

namespace Game1.Screens
{
    class MapScreen : GameScreen
    {
        InputAction exit;
        Texture2D tileTex;
        Texture2D backgroundTexture;
        Texture2D horizontalBorderTexture;
        Texture2D verticalBorderTexture;
        Texture2D cornerTexture;

        #region Initialization
        public MapScreen()
        {
            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            exit = new InputAction(
                new Keys[] { Keys.Tab, Keys.Escape, Keys.M },
                null,
                true);
        }

        public override void Activate()
        {
            ContentManager content = ScreenManager.Game.Content;
            tileTex = content.Load<Texture2D>("Interface/Map/tile");
            horizontalBorderTexture = content.Load<Texture2D>("Interface/messagebox_horizontalborder");
            verticalBorderTexture = content.Load<Texture2D>("Interface/messagebox_verticalborder");
            cornerTexture = content.Load<Texture2D>("Interface/messagebox_corner");
            backgroundTexture = new Texture2D(ScreenManager.Game.GraphicsDevice, 1, 1);
            backgroundTexture.SetData(new Color[] { Color.Black });
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            ScreenManager.Game.IsMouseVisible = true;
            if (exit.Evaluate(input))
            {
                Rectangle clientBounds = ScreenManager.Game.Window.ClientBounds;
                Mouse.SetPosition(clientBounds.Width / 2, clientBounds.Height / 2);
                ExitScreen();
            }
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            //// Center the message text in the viewport.
            //Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            //Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            //Vector2 mapDimensions = font.MeasureString(message) + new Vector2(0, ScreenManager.Font.LineSpacing);
            //Vector2 mapPosition = (viewportSize - mapDimensions) / 2;

            //// make sure our entries are in the right place before we draw them
            //UpdateMenuEntryLocations(mapDimensions, mapPosition);

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 0;
            const int vPad = 0;

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);

            Texture2D mapTex = GameplayScreen.mapTex;
            Vector2 mapTileCount = new Vector2(mapTex.Width, mapTex.Height);
            Vector2 mapDimensions = new Vector2(tileTex.Width * mapTileCount.X * 0.75f, tileTex.Height * mapTileCount.Y + tileTex.Height * 0.5f);

            Vector2 mapPosition = viewportSize / 2 - mapDimensions / 2;
            Vector2 borderOffset = new Vector2(tileTex.Width / 2, tileTex.Height / 2);

            //Rectangle mapRectangle = new Rectangle((int)mapPosition.X,
            //                                       (int)mapPosition.Y,
            //                                       (int)mapDimensions.X,
            //                                       (int)mapDimensions.Y);

            mapPosition -= borderOffset;

            Rectangle backgroundRectangle = new Rectangle((int)mapPosition.X - hPad,
                                                          (int)mapPosition.Y - vPad,
                                                          (int)mapDimensions.X + hPad * 2,
                                                          (int)mapDimensions.Y + vPad * 2);

            Rectangle topBorder = new Rectangle((int)mapPosition.X - hPad + cornerTexture.Width / 2,
                                                (int)mapPosition.Y - vPad,
                                                (int)mapDimensions.X + hPad * 2 - cornerTexture.Width / 2,
                                                horizontalBorderTexture.Height);

            Rectangle bottomBorder = new Rectangle((int)mapPosition.X - hPad + cornerTexture.Width / 2,
                                                  (int)mapPosition.Y + (int)mapDimensions.Y + vPad,
                                                  (int)mapDimensions.X + hPad * 2 - cornerTexture.Width / 2,
                                                  horizontalBorderTexture.Height);

            Rectangle leftBorder = new Rectangle((int)mapPosition.X - hPad,
                                                 (int)mapPosition.Y - vPad + cornerTexture.Height / 2,
                                                 verticalBorderTexture.Width,
                                                 (int)mapDimensions.Y + vPad * 2 - cornerTexture.Height / 2);

            Rectangle rightBorder = new Rectangle((int)mapPosition.X + (int)mapDimensions.X + hPad,
                                                  (int)mapPosition.Y - vPad + cornerTexture.Height / 2,
                                                  verticalBorderTexture.Width,
                                                  (int)mapDimensions.Y + vPad * 2 - cornerTexture.Height / 2);

            Rectangle ULcorner = new Rectangle((int)mapPosition.X - hPad,
                                                (int)mapPosition.Y - vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            Rectangle URcorner = new Rectangle((int)mapPosition.X + (int)mapDimensions.X + hPad,
                                                (int)mapPosition.Y - vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            Rectangle DLcorner = new Rectangle((int)mapPosition.X - hPad,
                                                (int)mapPosition.Y + (int)mapDimensions.Y + vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            Rectangle DRcorner = new Rectangle((int)mapPosition.X + (int)mapDimensions.X + hPad,
                                                (int)mapPosition.Y + (int)mapDimensions.Y + vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            mapPosition += borderOffset;

            // Fade the popup alpha during transitions.
            Color color = Color.White * TransitionAlpha;

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(backgroundTexture, backgroundRectangle, color);

            Map.Draw(spriteBatch, tileTex, mapPosition, mapTileCount, TransitionAlpha);
            Map.DrawAssets(spriteBatch, tileTex, mapPosition, mapTileCount, TransitionAlpha);

            Vector2 horizontalBorderOrigin = new Vector2(horizontalBorderTexture.Width / 2, horizontalBorderTexture.Height / 2);
            Vector2 verticalBorderOrigin = new Vector2(verticalBorderTexture.Width / 2, verticalBorderTexture.Height / 2);

            spriteBatch.Draw(horizontalBorderTexture, topBorder, null, color, 0, horizontalBorderOrigin, SpriteEffects.None, 0);
            spriteBatch.Draw(horizontalBorderTexture, bottomBorder, null, color, 0, horizontalBorderOrigin, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(verticalBorderTexture, leftBorder, null, color, 0, verticalBorderOrigin, SpriteEffects.None, 0);
            spriteBatch.Draw(verticalBorderTexture, rightBorder, null, color, 0, verticalBorderOrigin, SpriteEffects.FlipHorizontally, 0);

            Vector2 cornerOrigin = new Vector2(cornerTexture.Width / 2, cornerTexture.Height / 2);

            spriteBatch.Draw(cornerTexture, ULcorner, null, color, 0, cornerOrigin, SpriteEffects.None, 0);
            spriteBatch.Draw(cornerTexture, URcorner, null, color, 0, cornerOrigin, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(cornerTexture, DLcorner, null, color, 0, cornerOrigin, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(cornerTexture, DRcorner, null, color, 0, cornerOrigin, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);

            spriteBatch.End();
        }
        #endregion
    }
}
