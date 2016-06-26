using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class TimeOfDay
    {
        public int Hours
        { get; set; }
        public int Minutes
        { get; set; }
        public float Seconds
        { get; set; }
        public int TotalMinutes
        { get { return Hours * 60 + Minutes; } }
        public float TotalSeconds
        { get { return (float)TotalMinutes * 60 + Seconds; } }
        public float HoursFloat
        { get { return Hours + (float)Minutes / 60; } }
        public float MinutesFloat
        { get { return Minutes + Seconds / 60; } }
        public float TimeFloat
        { get { return Hours + MinutesFloat / 60; } }
        public float TimeFloatCut
        {
            get {
                double time = Hours + MinutesFloat / 60;
                return (float)Math.Round(time, 2);
                }
        }

        public bool IsDay
        {
            get { return (TimeFloat >= 6 && TimeFloat < 18); }
        }
        public TimeOfDay(int hours, int minutes, float seconds)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }
        public void Update(float dt, int timeScale)
        {
            Seconds += dt * timeScale;
            if (Seconds >= 60)
            {
                int deltaMinutes = (int)Seconds / 60;
                Seconds -= deltaMinutes * 60;
                Minutes += deltaMinutes;
            }
            if (Seconds < 0)
            {
                int deltaMinutes = (int)Seconds / 60;
                Seconds += deltaMinutes * 60;
                Minutes -= deltaMinutes;
            }
            if (Minutes >= 60)
            {
                Minutes -= 60;
                Hours++;
            }
            if (Minutes < 0)
            {
                Minutes += 60;
                Hours--;
            }
            if (Hours >= 24)
            {
                Hours -= 24;
            }
            if (Hours < 0)
            {
                Hours += 24;
            }
        }

        private float LogisticFunction(float max, float x, float steepness)
        {
            return max / (1 + (float)Math.Exp(-steepness * x));
        }

        /// <summary>
        /// Returns a logistic function of time, useful for varying light intensity during the day/night cycle and such
        /// </summary>
        /// <returns>A float larger than 0.0 and smaller than 1.0</returns>
        public float LogisticTime(float min, float max, float steepness)
        {
            float sunrise = 6;
            float sunset = 18;
            float output;
            if (HoursFloat >= 0 && HoursFloat < 12)
                output = LogisticFunction(1, HoursFloat - sunrise, steepness);
            else
                output = 1.0f - LogisticFunction(1, HoursFloat - sunset, steepness);
            return min + output * (max - min);
        }
    }
}
