using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Particles.ParticleSystems
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ProjectileTrailHeadParticleSystem : ParticleSystem
    {
        public ProjectileTrailHeadParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/particles/trailhead";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(0.25f);

            settings.DurationRandomness = 0.25f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1;

            settings.MinColor = new Color(255, 255, 255, 255)*100;
            settings.MaxColor = new Color(255, 255, 255, 255)*100;

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 3;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 3;

            settings.BlendState = BlendState.Additive;
        }
    }
}
