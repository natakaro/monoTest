using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Spells
{
    public class SpellFireball
    {
        Game game;
        Camera camera;
        Octree octree;

        Stopwatch stopwatch = new Stopwatch();

        public void Start(bool leftButton, bool rightButton)
        {
            stopwatch.Start();
        }

        public void Continue(bool leftButton, bool rightButton)
        {
            if (leftButton)
            {
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    SpellFireballProjectile fireball = new SpellFireballProjectile(game, Matrix.CreateTranslation(camera.Position) * camera.worldMatrix);
                    fireball.Position = camera.Position;
                    fireball.Velocity = camera.GetMouseRay(game.GraphicsDevice.Viewport).Direction * 100;
                    octree.m_objects.Add(fireball);
                    stopwatch.Restart();
                }
            }
            else if (rightButton)
            {
                if (stopwatch.ElapsedMilliseconds > 250) //strzela 4 razy szybciej, a kulki sie rozpedzaja, tak dla sprawdzenia czy dziala
                {
                    SpellFireballProjectile fireball = new SpellFireballProjectile(game, Matrix.CreateTranslation(camera.Position) * camera.worldMatrix);
                    fireball.Position = camera.Position;
                    fireball.Velocity = camera.GetMouseRay(game.GraphicsDevice.Viewport).Direction * 1;
                    fireball.Acceleration = camera.GetMouseRay(game.GraphicsDevice.Viewport).Direction * 10;
                    octree.m_objects.Add(fireball);
                    stopwatch.Restart();
                }
            }     
        }

        public void Stop(bool leftButton, bool rightButton)
        {
            stopwatch.Reset();
        }

        public SpellFireball(Game game, Camera camera, Octree octree)
        {
            this.game = game;
            this.camera = camera;
            this.octree = octree;
        }
    }
}
