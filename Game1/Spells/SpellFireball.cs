using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Spells
{
    public class SpellFireball
    {
        float elapsedTime = 0f;
        public void Cast(bool leftButton, bool rightButton, Game game, GameTime gameTime, Camera camera, Octree octree)
        {
            if (leftButton)
            {
                elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (elapsedTime > 1f) //strzela co sekunde gdy trzymasz przycisk
                {
                    SpellFireballProjectile fireball = new SpellFireballProjectile(game, Matrix.CreateTranslation(camera.Position) * camera.worldMatrix);
                    fireball.Position = camera.Position;
                    fireball.Velocity = camera.GetMouseRay(game.GraphicsDevice.Viewport).Direction * 100;
                    octree.m_objects.Add(fireball);
                    elapsedTime = 0f;
                }
            }
            else
                StopCasting();
        }

        public void StopCasting()
        {
            elapsedTime = 0;
        }
    }
}
