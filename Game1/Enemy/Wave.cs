using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public class Wave
    {
        public int enemyType;
        public float time;
        public int number;
        public float stopwatch;

        public Wave(int enemyType, float time, int number, float stopwatch)
        {
            this.enemyType = enemyType;
            this.time = time;
            this.number = number;
            this.stopwatch = stopwatch;
        }


    }

}
