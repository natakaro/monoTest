using Game1.Particles.ParticleSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Particles
{
    public class ParticleManager
    {
        Game game;
        ContentManager Content;

        public ParticleSystem fireParticles;
        public ParticleSystem explosionParticles;
        public ParticleSystem explosionSmokeParticles;
        public ParticleSystem fireProjectileTrailParticles;
        public ParticleSystem projectileTrailHeadParticles;

        public ParticleSystem iceParticles;
        public ParticleSystem iceExplosionParticles;
        public ParticleSystem iceExplosionSnowParticles;
        public ParticleSystem iceProjectileTrailParticles;

        public ParticleSystem smokeProjectileTrailParticles;
        public ParticleSystem smokePlumeParticles;
        
        public ParticleSystem portalParticlesFriendly;
        public ParticleSystem portalParticlesEnemy;

        public ParticleSystem lootParticles;

        List<ParticleSystem> particleSystems;

        public ParticleManager(Game game, ContentManager Content)
        {
            this.game = game;
            this.Content = Content;

            particleSystems = new List<ParticleSystem>();

            // Construct our particle system components.
            fireParticles = new FireParticleSystem(game, Content);
            explosionParticles = new ExplosionParticleSystem(game, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(game, Content);
            fireProjectileTrailParticles = new FireProjectileTrailParticleSystem(game, Content);
            projectileTrailHeadParticles = new ProjectileTrailHeadParticleSystem(game, Content);

            iceParticles = new IceParticleSystem(game, Content);
            iceExplosionParticles = new IceExplosionParticleSystem(game, Content);
            iceExplosionSnowParticles = new IceExplosionSnowParticleSystem(game, Content);
            iceProjectileTrailParticles = new IceProjectileTrailParticleSystem(game, Content);

            smokeProjectileTrailParticles = new SmokeProjectileTrailParticleSystem(game, Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(game, Content);

            portalParticlesFriendly = new PortalParticleSystemFriendly(game, Content);
            portalParticlesEnemy = new PortalParticleSystemEnemy(game, Content);

            lootParticles = new LootParticleSystem(game, Content);

            //// Set the draw order so the explosions and fire
            //// will appear over the top of the smoke.
            //smokePlumeParticles.DrawOrder = 100;
            //explosionSmokeParticles.DrawOrder = 200;
            //projectileTrailHeadParticles.DrawOrder = 250;
            //smokeProjectileTrailParticles.DrawOrder = 300;
            //fireProjectileTrailParticles.DrawOrder = 300;
            //explosionParticles.DrawOrder = 400;
            //fireParticles.DrawOrder = 500;

            // Register the particle system components.
            particleSystems.Add(fireParticles);
            particleSystems.Add(explosionParticles);
            particleSystems.Add(explosionSmokeParticles);
            particleSystems.Add(fireProjectileTrailParticles);
            particleSystems.Add(projectileTrailHeadParticles);

            particleSystems.Add(iceParticles);
            particleSystems.Add(iceExplosionParticles);
            particleSystems.Add(iceExplosionSnowParticles);
            particleSystems.Add(iceProjectileTrailParticles);

            particleSystems.Add(smokeProjectileTrailParticles);
            particleSystems.Add(smokePlumeParticles);
            
            particleSystems.Add(portalParticlesFriendly);
            particleSystems.Add(portalParticlesEnemy);

            particleSystems.Add(lootParticles);

            foreach (ParticleSystem system in particleSystems)
            {
                system.Initialize();
                system.LoadContent();
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach(ParticleSystem system in particleSystems)
            {
                system.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, Camera camera, float farClip, RenderTarget2D depthTarget)
        {
            foreach (ParticleSystem system in particleSystems)
            {
                system.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
                system.Draw(gameTime);
            }
        }
    }
}
