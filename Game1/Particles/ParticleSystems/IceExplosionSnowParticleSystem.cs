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
    class IceExplosionSnowParticleSystem : ParticleSystem
    {
        public IceExplosionSnowParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures/particles/iceSmall";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(3);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -5;
            settings.MaxVerticalVelocity = 15;

            settings.Gravity = new Vector3(0, -20, 0);

            settings.EndVelocity = 0;

            //settings.MinColor = Color.LightGray;
            //settings.MaxColor = Color.White;
            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 0.5f;
            settings.MaxStartSize = 2;

            settings.MinEndSize = 1;
            settings.MaxEndSize = 4;

            // Use additive blending.
            //settings.BlendState = BlendState.Additive;
        }
    }
}
