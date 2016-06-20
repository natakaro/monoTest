using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Particles.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class ExplosionSmokeParticleSystem : ParticleSystem
    {
        public ExplosionSmokeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/particles/smoke";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 25;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 25;

            settings.Gravity = new Vector3(0, -20, 0);

            settings.EndVelocity = 0;

            //settings.MinColor = Color.LightGray;
            //settings.MaxColor = Color.White;
            settings.MinColor = new Color(64, 96, 128, 16);
            settings.MaxColor = new Color(255, 255, 255, 8);

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 3;
            settings.MaxStartSize = 3;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 10;
        }
    }
}
