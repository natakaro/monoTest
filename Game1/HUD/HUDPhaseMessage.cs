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
    public class HUDPhaseMessage : HUDElement
    {
        Texture2D dayMessage;
        Texture2D nightMessage;
        Stopwatch messageStopwatch;
        Phase phase;

        public HUDPhaseMessage(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Vector2 position, Vector2 dimension) : base(spriteBatch, graphicsDevice, position, dimension)
        {
            this.enabled = false;
        }

        public override void Draw()
        {
            if (enabled)
            {
                float alpha = 1;
                if (messageStopwatch.ElapsedMilliseconds <= 1000)
                    alpha = MathHelper.Lerp(0, 1, (float)messageStopwatch.ElapsedMilliseconds / 1000f);
                else if (messageStopwatch.ElapsedMilliseconds > 7000)
                    alpha = MathHelper.Lerp(1, 0, ((float)messageStopwatch.ElapsedMilliseconds - 7000f) / 3000f);

                if (phase == Phase.Day)
                {
                    spriteBatch.Draw(dayMessage, position, Color.White * ALPHA * alpha);
                }
                else if (phase == Phase.Night)
                {
                    spriteBatch.Draw(nightMessage, position, Color.White * ALPHA * alpha);
                }
            }
        }

        public override void LoadContent(ContentManager Content)
        {
            dayMessage = Content.Load<Texture2D>("Hud/day_message");
            nightMessage = Content.Load<Texture2D>("Hud/night_message");

            messageStopwatch = new Stopwatch();
        }

        public void Update()
        {
            if (enabled == true)
            {
                if (messageStopwatch.ElapsedMilliseconds >= 10000f)
                {
                    messageStopwatch.Reset();
                    enabled = false;
                }
            }
        }

        public void HandleMessageEvent(object sender, PhaseEventArgs eventArgs)
        {
            messageStopwatch.Restart();
            phase = eventArgs.Phase;
            enabled = true;
        }
    }
}
