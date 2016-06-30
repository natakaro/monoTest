using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Game1.Screens
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class Credits : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;
        List<string> messages;
        int i = 0;
        private Stopwatch stopwatch = new Stopwatch();
        bool done = false;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public Credits()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            messages = new List<string>();
            messages.Add("Dimensional" + "    " + "Clash");
            messages.Add("GRO-POL");
            messages.Add("Maciej" + "    " + "Michalski\nBartłomiej" + "    " +"Paszkowski\nMarcin" + "    " + "Gniewisz");
            messages.Add("Sounds:" + "\nFireball - Large Fireball" + "\nIcebolt - Bottle Rocket Sound" + "\nTileMove - Earthquake" + "\nby   Mike   Koenig\n"
                         + "\nIceWind - Wind Sound" + "\nBossPunch - Explosion Ultra Bass Sound" + "\nby   Mark   DiAngelo"
                );
            messages.Add("Thank you!");
            messages.Add(" ");
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void Activate()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            backgroundTexture = content.Load<Texture2D>("Interface/background");
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void Unload()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            
            SpriteFont font = ScreenManager.Font;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 textSize = font.MeasureString(messages[i]);
            Vector2 textPosition = (viewportSize - textSize) / 2;

            Color color = Color.White * TransitionAlpha;
            if (!done)
            {
                stopwatch.Restart();
                done = true;
            }
            // Draw the text.
            if(stopwatch.ElapsedMilliseconds > 6000 && done)
            {
                i++;
                done = false;
            }

            if(i >= messages.Count-1)
            {
                ScreenManager.Game.Exit();
            }
            spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, fullscreen,
                 new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));
            spriteBatch.DrawString(font, messages[i], textPosition, color);

            spriteBatch.End();

        }


        #endregion
    }
}
