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
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        #region Fields

        List<MenuEntry> menuEntries = new List<MenuEntry>();
        int? selectedEntry = 0;
        int previousSelectedEntry = 0;
        string menuTitle;

        InputAction menuUp;
        InputAction menuDown;
        InputAction menuLeft;
        InputAction menuRight;
        InputAction menuSelect;
        InputAction menuCancel;

        ContentManager content;
        Texture2D arrowTexture;

        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            menuUp = new InputAction(
                new Keys[] { Keys.Up, Keys.W },
                null,
                true);
            menuDown = new InputAction(
                new Keys[] { Keys.Down, Keys.S },
                null,
                true);
            menuLeft = new InputAction(
                new Keys[] { Keys.Left, Keys.A },
                null,
                true);
            menuRight = new InputAction(
                new Keys[] { Keys.Right, Keys.D },
                null,
                true);
            menuSelect = new InputAction(
                new Keys[] { Keys.Enter, Keys.Space },
                null,
                true);
            menuCancel = new InputAction(
                new Keys[] { Keys.Escape },
                null,
                true);
        }

        public override void Activate()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            arrowTexture = content.Load<Texture2D>("Interface/arrow");
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            //ScreenManager.Game.IsMouseVisible = true;
            // Move to the previous menu entry?
            if (menuUp.Evaluate(input))
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

            // Move to the next menu entry?
            if (menuDown.Evaluate(input))
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

            if (menuLeft.Evaluate(input))
            {
                ScreenManager.Game.IsMouseVisible = false;
                OnSelectEntryLeft(selectedEntry);
            }

            if (menuRight.Evaluate(input))
            {
                ScreenManager.Game.IsMouseVisible = false;
                OnSelectEntryRight(selectedEntry);
            }

            if (menuSelect.Evaluate(input))
            {
                ScreenManager.Game.IsMouseVisible = false;
                OnSelectEntry(selectedEntry);
            }

            if (menuCancel.Evaluate(input))
            {
                OnCancel();
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
                if(selectedEntry != null)
                    previousSelectedEntry = (int)selectedEntry;

                selectedEntry = null;

                ScreenManager.Game.IsMouseVisible = true;

                for (int i = 0; i < menuEntries.Count; i++)
                {
                    if (input.LastMouseState.Y > menuEntries[i].Position.Y - menuEntries[i].GetHeight(this) / 2 &&
                        input.LastMouseState.Y < menuEntries[i].Position.Y + menuEntries[i].GetHeight(this) / 2 &&
                        input.LastMouseState.X > menuEntries[i].Position.X &&
                        input.LastMouseState.X < menuEntries[i].Position.X + menuEntries[i].GetWidth(this))
                    {
                        selectedEntry = i;
                    }

                    if (menuEntries[i].DrawArrows)
                    {
                        if (input.LastMouseState.Y > menuEntries[i].Position.Y - menuEntries[i].GetHeight(this) / 2 &&
                            input.LastMouseState.Y < menuEntries[i].Position.Y + menuEntries[i].GetHeight(this) / 2 &&
                            ((input.LastMouseState.X > menuEntries[i].Position.X - 30 &&
                            input.LastMouseState.X < menuEntries[i].Position.X - 10) ||
                            (input.LastMouseState.X > menuEntries[i].Position.X + menuEntries[i].GetWidth(this) + 8 &&
                            input.LastMouseState.X < menuEntries[i].Position.X + menuEntries[i].GetWidth(this) + 28)))
                        {
                            selectedEntry = i;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int? entryIndex)
        {
            if(entryIndex != null)
                menuEntries[(int)entryIndex].OnSelectEntry();
        }

        protected virtual void OnSelectEntryLeft(int? entryIndex)
        {
            if (entryIndex != null)
                menuEntries[(int)entryIndex].OnSelectEntryLeft();
        }

        protected virtual void OnSelectEntryRight(int? entryIndex)
        {
            if (entryIndex != null)
                menuEntries[(int)entryIndex].OnSelectEntryRight();
        }

        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel()
        {
            ExitScreen();
        }

        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, EventArgs e)
        {
            OnCancel();
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the screen the chance to position the menu entries. By default
        /// all menu entries are lined up in a vertical list, centered on the screen.
        /// </summary>
        protected virtual void UpdateMenuEntryLocations()
        {
            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // start at Y = 175; each X value is generated per entry
            Vector2 position = new Vector2(0f, 175f);

            // update each menu entry's location in turn
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];

                // each entry is to be centered horizontally
                position.X = ScreenManager.GraphicsDevice.Viewport.Width / 2 - menuEntry.GetWidth(this) / 2;

                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256;
                else
                    position.X += transitionOffset * 512;

                // set the entry's position
                menuEntry.Position = position;

                // move down for the next entry the size of this entry
                position.Y += menuEntry.GetHeight(this);
            }
        }


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);

                menuEntries[i].Update(this, isSelected, gameTime);
            }
        }


        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // make sure our entries are in the right place before we draw them
            UpdateMenuEntryLocations();

            GraphicsDevice graphics = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];

                bool isSelected = IsActive && (i == selectedEntry);

                menuEntry.Draw(this, isSelected, gameTime);

                if (menuEntry.DrawArrows)
                {
                    spriteBatch.Draw(arrowTexture, new Vector2(menuEntry.Position.X - 25, menuEntry.Position.Y - 8), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
                    spriteBatch.Draw(arrowTexture, new Vector2(menuEntry.Position.X + menuEntry.GetWidth(this) + 14, menuEntry.Position.Y - 8), null, Color.White);
                }
            }

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            // Draw the menu title centered on the screen
            Vector2 titlePosition = new Vector2(graphics.Viewport.Width / 2, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = new Color(192, 192, 192) * TransitionAlpha;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        #endregion
    }
}
