using Game1.HUD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public enum Phase
    {
        Day = 0,
        Night = 1
    };

    public class PhaseEventArgs : EventArgs
    {
        public Phase Phase { get; set; }
    }

    public class PhaseManager
    {
        Phase currentPhase;
        Game game;
        TimeOfDay timeOfDay;
        HUDManager hudManager;

        public event EventHandler<PhaseEventArgs> messageEvent;

        public Phase Phase
        {
            get { return currentPhase; }
        }

        public PhaseManager(Game game, TimeOfDay timeOfDay, HUDManager hudManager)
        {
            this.game = game;
            this.timeOfDay = timeOfDay;
            this.hudManager = hudManager;

            messageEvent += hudManager.PhaseMessage.HandleMessageEvent;
        }

        public void Update(GameTime gameTime)
        {
            if ((timeOfDay.Hours >= 6 && timeOfDay.Hours < 18) && currentPhase == Phase.Night)
            {
                currentPhase = Phase.Day;

                PhaseEventArgs args = new PhaseEventArgs();
                args.Phase = Phase.Day;
                OnMessageEvent(args);
            }
            else if ((timeOfDay.Hours >= 18 || timeOfDay.Hours < 6) && currentPhase == Phase.Day)
            {
                currentPhase = Phase.Night;

                PhaseEventArgs args = new PhaseEventArgs();
                args.Phase = Phase.Night;
                OnMessageEvent(args);
            }
        }

        public void OnMessageEvent(PhaseEventArgs e)
        {
            messageEvent?.Invoke(this, e);
        }
    }
}
