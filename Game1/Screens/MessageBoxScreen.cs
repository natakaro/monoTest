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
    class MessageBoxScreen : MenuScreen
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

        #region Properties

        #endregion

        #region Initialization
        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.
        /// </summary>
        public MessageBoxScreen(string message, bool oneOption = false) : base(message)
        {
            if (oneOption)
            {
                MenuEntry okMenuEntry = new MenuEntry("Ok");
                okMenuEntry.Selected += OkMenuEntry_Selected; ;
                MenuEntries.Add(okMenuEntry);
            }
            else
            {
                MenuEntry yesGameMenuEntry = new MenuEntry("Yes");
                MenuEntry noMenuEntry = new MenuEntry("No");
                yesGameMenuEntry.Selected += YesGameMenuEntry_Selected; ;
                noMenuEntry.Selected += NoMenuEntry_Selected; ;
                MenuEntries.Add(yesGameMenuEntry);
                MenuEntries.Add(noMenuEntry);
            }

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

        private void OkMenuEntry_Selected(object sender, EventArgs e)
        {
            Accepted?.Invoke(this, EventArgs.Empty);

            ExitScreen();
        }

        private void NoMenuEntry_Selected(object sender, EventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);

            ExitScreen();
        }

        private void YesGameMenuEntry_Selected(object sender, EventArgs e)
        {
            Accepted?.Invoke(this, EventArgs.Empty);

            ExitScreen();
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
            if (menuSelect.Evaluate(input))
            {
                //ScreenManager.Game.IsMouseVisible = false;
                OnSelectEntry(selectedEntry);
                
            }
            else if (menuCancel.Evaluate(input))
            {
                // Raise the cancelled event, then exit the message box.
                Cancelled?.Invoke(this, EventArgs.Empty);

                ExitScreen();
            }

            if (menuLeft.Evaluate(input))
            {
                ScreenManager.Game.IsMouseVisible = false;

                if (selectedEntry == null)
                    selectedEntry = previousSelectedEntry;
                else
                {
                    selectedEntry--;

                    if (selectedEntry < 0)
                        selectedEntry = menuEntries.Count - 1;
                }
            }

            if (menuRight.Evaluate(input))
            {
                ScreenManager.Game.IsMouseVisible = false;

                if (selectedEntry == null)
                    selectedEntry = previousSelectedEntry;
                else
                {
                    selectedEntry++;

                    if (selectedEntry >= menuEntries.Count)
                        selectedEntry = 0;
                }
            }

            if (input.IsNewLeftMouseClick)
            {
                if (selectedEntry != null)
                {
                    if (input.LastMouseState.Y > menuEntries[(int)selectedEntry].Position.Y - menuEntries[(int)selectedEntry].GetHeight(this) / 2 &&
                                input.LastMouseState.Y < menuEntries[(int)selectedEntry].Position.Y + menuEntries[(int)selectedEntry].GetHeight(this) / 2 &&
                                input.LastMouseState.X > menuEntries[(int)selectedEntry].Position.X - 30 &&
                                input.LastMouseState.X < menuEntries[(int)selectedEntry].Position.X - 10)
                        OnSelectEntryLeft(selectedEntry);
                    else
                        OnSelectEntry(selectedEntry);
                }
                else
                    OnSelectEntry(selectedEntry);
            }

            if (input.CurrentMouseState.Position != input.LastMouseState.Position)
            {
                if (selectedEntry != null)
                    previousSelectedEntry = (int)selectedEntry;

                selectedEntry = null;

                ScreenManager.Game.IsMouseVisible = true;

                for (int i = 0; i < menuEntries.Count; i++)
                {
                    if (input.LastMouseState.Y > menuEntries[i].Position.Y - menuEntries[i].GetHeight(this) &&
                        input.LastMouseState.Y < menuEntries[i].Position.Y &&
                        input.LastMouseState.X > menuEntries[i].Position.X &&
                        input.LastMouseState.X < menuEntries[i].Position.X + menuEntries[i].GetWidth(this))
                    {
                        selectedEntry = i;
                    }
                }
            }
        }

        #endregion

        #region Update
        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected void UpdateMenuEntryLocations(Vector2 textSize, Vector2 textPosition)
        {
            Vector2 position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, textPosition.Y + textSize.Y);

            if (menuEntries.Count == 1)
            {
                MenuEntry menuEntry = menuEntries[0];

                menuEntry.Position = position;
            }
            else if (menuEntries.Count == 2)
            {
                MenuEntry menuEntry1 = menuEntries[0];
                MenuEntry menuEntry2 = menuEntries[1];

                position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2 - textSize.X / 4 - menuEntry1.GetWidth(this) / 2;
                menuEntry1.Position = position;

                position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2 + textSize.X / 4 - menuEntry2.GetWidth(this) / 2;
                menuEntry2.Position = position;
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
            Vector2 textSize = font.MeasureString(message) + new Vector2(0, ScreenManager.Font.LineSpacing);
            Vector2 textPosition = (viewportSize - textSize) / 2;

            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations(textSize, textPosition);

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Vector2 horizontalBorderOrigin = new Vector2(horizontalBorderTexture.Width / 2, horizontalBorderTexture.Height / 2);
            Vector2 verticalBorderOrigin = new Vector2(verticalBorderTexture.Width / 2, verticalBorderTexture.Height / 2);
            Vector2 cornerOrigin = new Vector2(cornerTexture.Width / 2, cornerTexture.Height / 2);

            Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            Rectangle topBorder = new Rectangle((int)textPosition.X - hPad + cornerTexture.Width / 2,
                                                (int)textPosition.Y - vPad,
                                                (int)textSize.X + hPad * 2 - cornerTexture.Width / 2,
                                                horizontalBorderTexture.Height);

            Rectangle bottomBorder = new Rectangle((int)textPosition.X - hPad + cornerTexture.Width / 2,
                                                  (int)textPosition.Y + (int)textSize.Y + vPad,
                                                  (int)textSize.X + hPad * 2 - cornerTexture.Width / 2,
                                                  horizontalBorderTexture.Height);

            Rectangle leftBorder = new Rectangle((int)textPosition.X - hPad,
                                                 (int)textPosition.Y - vPad + cornerTexture.Height / 2,
                                                 verticalBorderTexture.Width,
                                                 (int)textSize.Y + vPad * 2 - cornerTexture.Height / 2);

            Rectangle rightBorder = new Rectangle((int)textPosition.X + (int)textSize.X + hPad,
                                                  (int)textPosition.Y - vPad + cornerTexture.Height / 2,
                                                  verticalBorderTexture.Width,
                                                  (int)textSize.Y + vPad * 2 - cornerTexture.Height / 2);

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

            spriteBatch.Draw(horizontalBorderTexture, topBorder, null, color, 0, horizontalBorderOrigin, SpriteEffects.None, 0);
            spriteBatch.Draw(horizontalBorderTexture, bottomBorder, null, color, 0, horizontalBorderOrigin, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(verticalBorderTexture, leftBorder, null, color, 0, verticalBorderOrigin, SpriteEffects.None, 0);
            spriteBatch.Draw(verticalBorderTexture, rightBorder, null, color, 0, verticalBorderOrigin, SpriteEffects.FlipHorizontally, 0);

            spriteBatch.Draw(cornerTexture, ULcorner, null, color, 0, cornerOrigin, SpriteEffects.None, 0);
            spriteBatch.Draw(cornerTexture, URcorner, null, color, 0, cornerOrigin, SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(cornerTexture, DLcorner, null, color, 0, cornerOrigin, SpriteEffects.FlipVertically, 0);
            spriteBatch.Draw(cornerTexture, DRcorner, null, color, 0, cornerOrigin, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);

            // Draw the message box text.
            spriteBatch.DrawString(font, message, textPosition, color);

            // Draw each menu entry in turn.
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                MenuEntry menuEntry = MenuEntries[i];

                bool isSelected = IsActive && (i == selectedEntry);

                menuEntry.Draw(this, isSelected, gameTime);

            }

            spriteBatch.End();
        }


        #endregion
    }
}
