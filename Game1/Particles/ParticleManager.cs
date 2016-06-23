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

        public ParticleSystem explosionParticles;
        public ParticleSystem explosionSmokeParticles;
        public ParticleSystem smokeProjectileTrailParticles;
        public ParticleSystem smokePlumeParticles;
        public ParticleSystem fireParticles;
        public ParticleSystem fireProjectileTrailParticles;
        public ParticleSystem projectileTrailHeadParticles;
        public ParticleSystem portalParticlesFriendly;
        public ParticleSystem portalParticlesEnemy;

        List<ParticleSystem> particleSystems;

        public ParticleManager(Game game, ContentManager Content)
        {
            this.game = game;
            this.Content = Content;

            particleSystems = new List<ParticleSystem>();

            // Construct our particle system components.
            explosionParticles = new ExplosionParticleSystem(game, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(game, Content);
            smokeProjectileTrailParticles = new SmokeProjectileTrailParticleSystem(game, Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(game, Content);
            fireParticles = new FireParticleSystem(game, Content);
            fireProjectileTrailParticles = new FireProjectileTrailParticleSystem(game, Content);
            projectileTrailHeadParticles = new ProjectileTrailHeadParticleSystem(game, Content);
            portalParticlesFriendly = new PortalParticleSystemFriendly(game, Content);
            portalParticlesEnemy = new PortalParticleSystemEnemy(game, Content);

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
            particleSystems.Add(explosionParticles);
            particleSystems.Add(explosionSmokeParticles);
            particleSystems.Add(smokeProjectileTrailParticles);
            particleSystems.Add(smokePlumeParticles);
            particleSystems.Add(fireParticles);
            particleSystems.Add(fireProjectileTrailParticles);
            particleSystems.Add(projectileTrailHeadParticles);
            particleSystems.Add(portalParticlesFriendly);
            particleSystems.Add(portalParticlesEnemy);

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
            // Pass camera matrices through to the particle system components.
            //explosionParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            //explosionSmokeParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            //smokeProjectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            //smokePlumeParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            //fireParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            //fireProjectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            //projectileTrailHeadParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            //portalParticlesFriendly.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);

            foreach (ParticleSystem system in particleSystems)
            {
                system.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
                system.Draw(gameTime);
            }
        }
    }
}
