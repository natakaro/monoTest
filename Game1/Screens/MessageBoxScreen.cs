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

namespace Game1.Screens
{
    class MessageBoxScreen : GameScreen
    {
        #region Fields

        string message;
        Texture2D backgroundTexture;
        Texture2D horizontalBorderTexture;
        Texture2D verticalBorderTexture;
        Texture2D cornerTexture;

        InputAction menuSelect;
        InputAction menuCancel;

        #endregion

        #region Events

        public event EventHandler Accepted;
        public event EventHandler Cancelled;

        #endregion

        #region Initialization
        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.
        /// </summary>
        public MessageBoxScreen(string message, bool includeUsageText = true)
        {
            const string usageText = "\nSpace, Enter = ok" +
                                     "\nEsc = cancel";

            if (includeUsageText)
                this.message = message + usageText;
            else
                this.message = message;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            menuSelect = new InputAction(
                new Keys[] { Keys.Space, Keys.Enter },
                null,
                true);
            menuCancel = new InputAction(
                new Keys[] { Keys.Escape, Keys.Back },
                null,
                true);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void Activate()
        {
            ContentManager content = ScreenManager.Game.Content;
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

            // We pass in our ControllingPlayer, which may either be null (to
            // accept input from any player) or a specific index. If we pass a null
            // controlling player, the InputState helper returns to us which player
            // actually provided the input. We pass that through to our Accepted and
            // Cancelled events, so they can tell which player triggered them.
            if (menuSelect.Evaluate(input))
            {
                // Raise the accepted event, then exit the message box.
                Accepted?.Invoke(this, EventArgs.Empty);

                ExitScreen();
            }
            else if (menuCancel.Evaluate(input))
            {
                // Raise the cancelled event, then exit the message box.
                Cancelled?.Invoke(this, EventArgs.Empty);

                ExitScreen();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            Rectangle topBorder = new Rectangle((int)textPosition.X - hPad + cornerTexture.Width,
                                                (int)textPosition.Y - vPad,
                                                (int)textSize.X + hPad * 2 - cornerTexture.Width,
                                                horizontalBorderTexture.Height);

            Rectangle bottomBorder = new Rectangle((int)textPosition.X - hPad + cornerTexture.Width,
                                                  (int)textPosition.Y + (int)textSize.Y + vPad,
                                                  (int)textSize.X + hPad * 2 - cornerTexture.Width,
                                                  horizontalBorderTexture.Height);

            Rectangle leftBorder = new Rectangle((int)textPosition.X - hPad,
                                                 (int)textPosition.Y - vPad + cornerTexture.Height,
                                                 verticalBorderTexture.Width,
                                                 (int)textSize.Y + vPad * 2 - cornerTexture.Height);

            Rectangle rightBorder = new Rectangle((int)textPosition.X + (int)textSize.X + hPad,
                                                  (int)textPosition.Y - vPad + cornerTexture.Height,
                                                  verticalBorderTexture.Width,
                                                  (int)textSize.Y + vPad * 2 - cornerTexture.Height);

            Rectangle ULcorner = new Rectangle((int)textPosition.X - hPad,
                                                (int)textPosition.Y - vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            Rectangle URcorner = new Rectangle((int)textPosition.X + (int)textSize.X + hPad,
                                                (int)textPosition.Y - vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            Rectangle DLcorner = new Rectangle((int)textPosition.X - hPad,
                                                (int)textPosition.Y + (int)textSize.Y + vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            Rectangle DRcorner = new Rectangle((int)textPosition.X + (int)textSize.X + hPad,
                                                (int)textPosition.Y + (int)textSize.Y + vPad,
                                                cornerTexture.Width,
                                                cornerTexture.Height);

            // Fade the popup alpha during transitions.
            Color color = Color.White * TransitionAlpha;

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(backgroundTexture, backgroundRectangle, color * (100f/255f));

            spriteBatch.Draw(horizontalBorderTexture, topBorder, color);
            spriteBatch.Draw(horizontalBorderTexture, bottomBorder, null, color, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(verticalBorderTexture, leftBorder, color);
            spriteBatch.Draw(verticalBorderTexture, rightBorder, null, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);

            spriteBatch.Draw(cornerTexture, ULcorner, color);
            spriteBatch.Draw(cornerTexture, URcorner, null, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(cornerTexture, DLcorner, null, color, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(cornerTexture, DRcorner, null, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);

            // Draw the message box text.
            spriteBatch.DrawString(font, message, textPosition, color);

            spriteBatch.End();
        }


        #endregion
    }
}
