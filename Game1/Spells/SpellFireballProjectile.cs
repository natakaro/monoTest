using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Game1.Helpers;
using Game1.Lights;
using System.Diagnostics;
using Game1.HUD;
using Game1.Particles;

namespace Game1.Spells
{
    class SpellFireballProjectile : DrawableObject
    {
        private PointLight pointLight;
        private LightManager lightManager;
        private HUDManager hudManager;
        private ObjectManager objectManager;

        private ParticleSystem explosionParticles;
        private ParticleSystem explosionSmokeParticles;
        private ParticleEmitter trailEmitter;
        private ParticleEmitter trailHeadEmitter;

        private float damage;
        private float age;

        private const float lifespan = 5f;
        private const float trailParticlesPerSecond = 200;
        private const float trailHeadParticlesPerSecond = 50;
        private const int numExplosionParticles = 10;
        private const int numExplosionSmokeParticles = 40;

        public event EventHandler hitEvent;

        public override void Draw(Camera camera)
        {
            //model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            //foreach (ModelMesh mesh in model.Meshes)
            //{
                //foreach (Effect effect in mesh.Effects)
                //{
                    //effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    //effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    //effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    //effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    //effect.Parameters["Texture"].SetValue(texture);
                    //effect.Parameters["Clipping"].SetValue(false);
                //}
                //mesh.Draw();
            //}
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            trailEmitter.Update(gameTime, position);
            trailHeadEmitter.Update(gameTime, position);

            pointLight.Position = position;
            BoundingSphere pointLightSphere = pointLight.BoundingSphere;
            pointLightSphere.Center = position;

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            age += elapsedTime;

            if (age > lifespan)
                Destroy();

            return ret;
        }

        public SpellFireballProjectile(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ObjectManager objectManager, 
                                       LightManager lightManager, HUDManager hudManager, float damage,
                                       ParticleSystem explosionParticles,
                                       ParticleSystem explosionSmokeParticles,
                                       ParticleSystem fireProjectileTrailParticles,
                                       ParticleSystem projectileTrailHeadParticles) : base(game, inWorldMatrix, inModel, octree)
        {
            this.lightManager = lightManager;
            this.hudManager = hudManager;
            this.objectManager = objectManager;

            m_static = false;
            boundingSphere = new BoundingSphere(position, 2f);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 2);
            type = ObjectType.Projectile;

            pointLight = new PointLight(position, Color.OrangeRed, 15, 5);
            lightManager.AddLight(pointLight);

            this.damage = damage;

            hitEvent += hudManager.Crosshair.HandleHitEvent;

            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
            trailEmitter = new ParticleEmitter(fireProjectileTrailParticles,
                                               trailParticlesPerSecond, position);
            trailHeadEmitter = new ParticleEmitter(projectileTrailHeadParticles,
                                                   trailHeadParticlesPerSecond, position);
        }

        public override void HandleIntersection(IntersectionRecord ir)
        {
            if (ir.DrawableObjectObject != null)
            {
                if (ir.DrawableObjectObject.Type == ObjectType.Enemy)
                {
                    OnHitEvent();
                    Enemy hitEnemy = (Enemy)ir.DrawableObjectObject;
                    hitEnemy.Damage(damage);
                    Destroy();
                }

                else if (ir.DrawableObjectObject.Type == ObjectType.Tile)
                {
                    Destroy();
                }                
            }
        }

        private void Destroy()
        {
            for (int i = 0; i < numExplosionParticles; i++)
                explosionParticles.AddParticle(position, -velocity * 0.05f);

            for (int i = 0; i < numExplosionSmokeParticles; i++)
                explosionSmokeParticles.AddParticle(position, -velocity * 0.05f);

            hitEvent -= hudManager.Crosshair.HandleHitEvent;
            lightManager.RemoveLight(pointLight);
            Alive = false;
            objectManager.Remove(this);
        }

        public void OnHitEvent()
        {
            hitEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
