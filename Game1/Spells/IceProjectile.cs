﻿using Game1.Helpers;
using Game1.HUD;
using Game1.Particles;
using Game1.Turrets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Screens;

namespace Game1.Spells
{
    class IceProjectile : DrawableObject
    {
        private HUDManager hudManager;
        private ObjectManager objectManager;

        private Texture2D texture;

        private ParticleSystem iceExplosionParticles;
        private ParticleSystem iceExplosionSnowParticles;
        private ParticleEmitter trailEmitter;        

        private float damage;
        private float age;

        private const float lifespan = 8f;
        private const float trailParticlesPerSecond = 100;
        private const float trailHeadParticlesPerSecond = 50;
        private const int numExplosionParticles = 5;
        private const int numExplosionSnowParticles = 80;

        public event EventHandler hitEvent;

        Sound sound;

        public override void Draw(Camera camera)
        {
            //model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["Emissive"].SetValue(1f);
                    effect.Parameters["Clipping"].SetValue(false);
                }
                mesh.Draw();
            }
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            trailEmitter.Update(gameTime, position);

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            age += elapsedTime;

            if (age > lifespan)
                Destroy();

            sound.Update(position);

            return ret;
        }

        public IceProjectile(Game game, Matrix inWorldMatrix, Model inModel, Texture2D texture, Octree octree, ObjectManager objectManager,
                                       HUDManager hudManager, float damage,
                                       ParticleSystem iceExplosionParticles,
                                       ParticleSystem iceExplosionSnowParticles,
                                       ParticleSystem iceProjectileTrailParticles) : base(game, inWorldMatrix, inModel, octree)
        {
            this.texture = texture;
            this.hudManager = hudManager;
            this.objectManager = objectManager;

            sound = new Sound(GameplayScreen.assetContentContainer.icebolt);
            sound.soundInstance.Volume = 0.5f;
            sound.Play(position);

            m_static = false;
            boundingSphere = new BoundingSphere(position, 2f);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            type = ObjectType.Projectile;

            this.damage = damage;

            this.iceExplosionParticles = iceExplosionParticles;
            this.iceExplosionSnowParticles = iceExplosionSnowParticles;
            trailEmitter = new ParticleEmitter(iceProjectileTrailParticles,
                                               trailParticlesPerSecond, position);
        }

        public override void HandleIntersection(IntersectionRecord ir)
        {
            if (ir.DrawableObjectObject != null)
            {
                if (ir.DrawableObjectObject.Type == ObjectType.Enemy)
                {
                    OnHitEvent();
                    Enemy hitEnemy = ir.DrawableObjectObject as Enemy;
                    hitEnemy.Damage(damage, DamageType.Ice);
                    Destroy();
                }

                else if (ir.DrawableObjectObject.Type == ObjectType.Turret)
                {
                    Turret turret = ir.DrawableObjectObject as Turret;

                    if (turret.mode != Mode.IceLeft)
                    {
                        OnHitEvent();
                        turret.SwitchMode(Mode.IceLeft);
                        Destroy(false);
                    }
                }

                else if ((ir.DrawableObjectObject.Type == ObjectType.Tile) || (ir.DrawableObjectObject.Type == ObjectType.Asset))
                {
                    Destroy();
                }
            }
        }

        private void Destroy(bool explosion = true)
        {
            if (explosion)
            {
                for (int i = 0; i < numExplosionParticles; i++)
                    iceExplosionParticles.AddParticle(position, -velocity * 0.02f);

                for (int i = 0; i < numExplosionSnowParticles; i++)
                    iceExplosionSnowParticles.AddParticle(position, -velocity * 0.02f);
            }

            hitEvent -= hudManager.Crosshair.HandleHitEvent;
            Alive = false;
            objectManager.Remove(this);
        }

        public void OnHitEvent()
        {
            hitEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
