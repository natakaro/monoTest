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
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/particles/explosion";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 5;
            settings.MaxHorizontalVelocity = 7.5f;

            settings.MinVerticalVelocity = -5;
            settings.MaxVerticalVelocity = 5;

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 3;
            settings.MaxStartSize = 3;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 30;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
