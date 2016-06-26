using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.Particles.ParticleSystems
{
    class LootParticleSystem : ParticleSystem
    {
        public LootParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/particles/loot";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(2f);

            settings.DurationRandomness = 1f;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 2;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 5;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 5, 0);

            settings.MinColor = new Color(255, 255, 255, 20);
            settings.MaxColor = new Color(255, 255, 255, 80);

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 0.5f;
            settings.MaxStartSize = 1;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 2;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
