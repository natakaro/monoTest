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
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class FireParticleSystem : ParticleSystem
    {
        public FireParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/particles/fireSmall";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(1f);

            settings.DurationRandomness = 1f;

            settings.MinHorizontalVelocity = -2;
            settings.MaxHorizontalVelocity = 20;

            settings.MinVerticalVelocity = -2;
            settings.MaxVerticalVelocity = 10;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 5, 0);

            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 0.5f;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 4;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
