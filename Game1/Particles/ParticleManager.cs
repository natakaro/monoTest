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

        public ParticleManager(Game game, ContentManager Content)
        {
            this.game = game;
            this.Content = Content;

            // Construct our particle system components.
            explosionParticles = new ExplosionParticleSystem(game, Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(game, Content);
            smokeProjectileTrailParticles = new SmokeProjectileTrailParticleSystem(game, Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(game, Content);
            fireParticles = new FireParticleSystem(game, Content);
            fireProjectileTrailParticles = new FireProjectileTrailParticleSystem(game, Content);

            // Set the draw order so the explosions and fire
            // will appear over the top of the smoke.
            smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            smokeProjectileTrailParticles.DrawOrder = 300;
            fireProjectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            // Register the particle system components.
            game.Components.Add(explosionParticles);
            game.Components.Add(explosionSmokeParticles);
            game.Components.Add(smokeProjectileTrailParticles);
            game.Components.Add(smokePlumeParticles);
            game.Components.Add(fireParticles);
            game.Components.Add(fireProjectileTrailParticles);
        }

        public void Draw(Camera camera, float farClip, RenderTarget2D depthTarget)
        {
            // Pass camera matrices through to the particle system components.
            explosionParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            explosionSmokeParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            smokeProjectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            smokePlumeParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            fireParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
            fireProjectileTrailParticles.SetCamera(camera.ViewMatrix, camera.ProjectionMatrix, farClip, depthTarget);
        }
    }
}
