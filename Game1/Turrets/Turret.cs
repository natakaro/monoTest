﻿using Game1.Helpers;
using Game1.Lights;
using Game1.Particles;
using Game1.Screens;
using Game1.Spells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Turrets
{
    public enum Mode
    {
        Off = 0,
        FireLeft = 1,
        FireRight = 2,
        IceLeft = 3,
        IceRight = 4
    }

    class Turret : DrawableObject
    {
        Game game;
        Camera camera;
        Texture2D texture;
        private PointLight pointLight;
        LightManager lightManager;
        ObjectManager objectManager;
        ParticleManager particleManager;

        public Model fireballModel;
        public Texture2D fireballTexture;

        public Model iceboltModel;
        public Texture2D iceboltTexture;

        Vector3 shootStartPosition;

        float range;
        float projectileSpeed;
        float rateOfFire;
        float damage;
        float particlesPerSecond;
        float timeBetweenParticles;
        float timeLeftOver;

        BoundingSphere rangeSphere;
        List<DrawableObject> enemiesInRange;
        Enemy currentTarget;

        float timer = 0;

        protected float spawnAge;
        protected const float spawnLength = 2f;

        public Mode mode;

        Vector3 direction;
        private Matrix viewMatrix;
        private Matrix projectionMatrix;
        private BoundingFrustum frustum;

        public Turret(Game game, Matrix inWorldMatrix, Model inModel, Camera camera, Octree octree, ObjectManager objectManager, Texture2D inTexture, LightManager lightManager, ParticleManager particleManager) : base(game, inWorldMatrix, inModel, octree)
        {
            this.game = game;
            this.camera = camera;
            this.lightManager = lightManager;
            this.objectManager = objectManager;
            this.particleManager = particleManager;
            texture = inTexture;

            type = ObjectType.Turret;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);

            shootStartPosition = position + new Vector3(0, 30, 0);

            pointLight = new PointLight(shootStartPosition, Color.Violet, 50, 10);
            lightManager.AddLight(pointLight);

            fireballModel = game.Content.Load<Model>("Models/fireball");
            fireballTexture = game.Content.Load<Texture2D>("Textures/firedot");

            iceboltModel = game.Content.Load<Model>("Models/icebolt");
            iceboltTexture = game.Content.Load<Texture2D>("Textures/icebolt");

            range = 0;
            projectileSpeed = 0;
            rateOfFire = 0;
            damage = 0;
            particlesPerSecond = 0;
            rangeSphere = new BoundingSphere(shootStartPosition, range);
            frustum = new BoundingFrustum(camera.ViewProjectionMatrix);

            enemiesInRange = new List<DrawableObject>();
            currentTarget = null;

            dissolveAmount = 1;
            scale = 1;

            mode = Mode.Off;
        }

        public override void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Clipping"].SetValue(false);
                    effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                }
                mesh.Draw();
            }
        }

        public void SwitchMode(Mode argument)
        {
            this.mode = argument;
            switch (mode)
            {
                case Mode.Off:
                    range = 0;
                    projectileSpeed = 0;
                    rateOfFire = 0;
                    damage = 0;
                    rangeSphere = new BoundingSphere(shootStartPosition, range);
                    break;
                case Mode.FireLeft:
                    range = 250;
                    projectileSpeed = 400;
                    rateOfFire = 1000;
                    damage = 10;
                    rangeSphere = new BoundingSphere(shootStartPosition, range);
                    break;
                case Mode.FireRight:
                    range = 100;
                    projectileSpeed = 0;
                    rateOfFire = 50;
                    damage = 1;
                    particlesPerSecond = 250;
                    timeBetweenParticles = 1.0f / particlesPerSecond;
                    rangeSphere = new BoundingSphere(shootStartPosition, range);
                    projectionMatrix = Matrix.CreatePerspective(7.5f, 4.2f, 15f, range);
                    break;
                case Mode.IceLeft:
                    range = 250;
                    projectileSpeed = 250;
                    rateOfFire = 1000;
                    damage = 10;
                    rangeSphere = new BoundingSphere(shootStartPosition, range);
                    break;
                case Mode.IceRight:
                    range = 100;
                    projectileSpeed = 0;
                    rateOfFire = 50;
                    damage = 1;
                    particlesPerSecond = 250;
                    timeBetweenParticles = 1.0f / particlesPerSecond;
                    rangeSphere = new BoundingSphere(shootStartPosition, range);
                    projectionMatrix = Matrix.CreatePerspective(7.5f, 4.2f, 15f, range);
                    break;
            }
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spawnAge += elapsedTime;

            if (spawnAge < spawnLength)
            {
                dissolveAmount = MathHelper.Lerp(1, 0, spawnAge / spawnLength);
                //scale = MathHelper.Lerp(0, 1, spawnAge / spawnLength);
            }
            else
            {
                dissolveAmount = 0;
                //scale = 1;
            }

            if (mode != Mode.Off)
            {
                SearchForEnemies();
                TargetClosest();
                Shoot(gameTime);
            }

            return ret;
        }

        private void SearchForEnemies()
        {
            enemiesInRange.Clear();
            foreach(IntersectionRecord ir in octree.AllIntersections(rangeSphere, ObjectType.Enemy))
            {
                enemiesInRange.Add(ir.DrawableObjectObject);
            }
        }

        private void TargetClosest()
        {
            currentTarget = null;
            float distance = float.MaxValue;

            foreach(Enemy enemy in enemiesInRange)
            {
                float distanceTest = Vector3.Distance(enemy.Position, Position);
                if (distance > distanceTest)
                {
                    distance = distanceTest;
                    currentTarget = enemy;
                }
            }
        }

        private void Shoot(GameTime gameTime)
        {
            if (currentTarget != null && currentTarget.Alive)
            {
                float elapsedMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                timer += elapsedMs;

                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                Vector3 interception = currentTarget.Position + new Vector3(0, 20, 0);
                direction = Vector3.Normalize(interception - shootStartPosition);

                switch (mode)
                {
                    case Mode.FireLeft:
                        if (timer > rateOfFire)
                        {
                            //interception = currentTarget.Position + new Vector3(0, 20, 0);
                            interception = iterative_approximation(currentTarget.Position + new Vector3(0, 20, 0), currentTarget.Velocity, projectileSpeed);
                            //interception = direct_solution(currentTarget.Position + new Vector3(0, 20, 0), currentTarget.Velocity, projectileSpeed);
                            direction = Vector3.Normalize(interception - shootStartPosition);

                            FireProjectile projectile = new FireProjectile(game, Matrix.CreateTranslation(shootStartPosition), fireballModel, octree, objectManager, lightManager, GameplayScreen.hudManager, damage, particleManager.explosionParticles, particleManager.explosionSmokeParticles, particleManager.fireProjectileTrailParticles, particleManager.projectileTrailHeadParticles);

                            projectile.Position = shootStartPosition;
                            projectile.Velocity = direction * projectileSpeed;
                            objectManager.Add(projectile);

                            timer = 0;
                        }
                        break;

                    case Mode.IceLeft:
                        if (timer > rateOfFire)
                        {
                            //Vector3 interception = currentTarget.Position + new Vector3(0, 20, 0);
                            interception = iterative_approximation(currentTarget.Position + new Vector3(0, 20, 0), currentTarget.Velocity, projectileSpeed);
                            //Vector3 interception = direct_solution(currentTarget.Position + new Vector3(0, 20, 0), currentTarget.Velocity, projectileSpeed);
                            direction = Vector3.Normalize(interception - shootStartPosition);

                            IceProjectile projectile = new IceProjectile(game, Matrix.CreateTranslation(shootStartPosition), iceboltModel, iceboltTexture, octree, objectManager, GameplayScreen.hudManager, damage, particleManager.iceExplosionParticles, particleManager.iceExplosionSnowParticles, particleManager.iceProjectileTrailParticles);

                            projectile.Position = shootStartPosition;
                            projectile.Velocity = direction * projectileSpeed;
                            objectManager.Add(projectile);

                            timer = 0;
                        }
                        break;

                    case Mode.FireRight:
                        viewMatrix = Matrix.CreateLookAt(shootStartPosition, interception, Vector3.Up);
                        frustum.Matrix = viewMatrix * projectionMatrix;
                        Vector3 firePosition = shootStartPosition + direction * 15;
                        Vector3 fireVelocity = direction * 50;

                        float timeToSpendFire = timeLeftOver + elapsedTime;
                        float currentTimeFire = -timeLeftOver;

                        while (timeToSpendFire > timeBetweenParticles)
                        {
                            currentTimeFire += timeBetweenParticles;
                            timeToSpendFire -= timeBetweenParticles;
                            particleManager.fireParticles.AddParticle(firePosition, fireVelocity);
                        }

                        if (timer > rateOfFire)
                        {
                            List<IntersectionRecord> hitList = octree.AllIntersections(frustum, ObjectType.Enemy);

                            foreach (IntersectionRecord ir in hitList)
                            {
                                Enemy enemy = ir.DrawableObjectObject as Enemy;
                                enemy.Damage(damage, DamageType.Fire);
                            }

                            timer = 0;
                        }
                        timeLeftOver = timeToSpendFire;
                        //DebugShapeRenderer.AddBoundingFrustum(frustum, Color.White);
                        break;

                    case Mode.IceRight:
                        viewMatrix = Matrix.CreateLookAt(shootStartPosition, interception, Vector3.Up);
                        frustum.Matrix = viewMatrix * projectionMatrix;
                        Vector3 icePosition = shootStartPosition + direction * 15;
                        Vector3 iceVelocity = direction * 25;

                        float timeToSpendIce = timeLeftOver + elapsedTime;
                        float currentTimeIce = -timeLeftOver;

                        while (timeToSpendIce > timeBetweenParticles)
                        {
                            currentTimeIce += timeBetweenParticles;
                            timeToSpendIce -= timeBetweenParticles;
                            particleManager.iceParticles.AddParticle(icePosition, iceVelocity);
                        }

                        if (timer > rateOfFire)
                        {
                            List<IntersectionRecord> hitList = octree.AllIntersections(frustum, ObjectType.Enemy);

                            foreach (IntersectionRecord ir in hitList)
                            {
                                Enemy enemy = ir.DrawableObjectObject as Enemy;
                                enemy.Damage(damage, DamageType.Ice);
                            }

                            timer = 0;
                        }
                        timeLeftOver = timeToSpendIce;
                        //DebugShapeRenderer.AddBoundingFrustum(frustum, Color.White);
                        break;
                }
            }
        }

        private Vector3 iterative_approximation(Vector3 target_position, Vector3 target_velocity, float projectile_speed)
        {
            int MAX_ITERATIONS = 5;
            float EPSILON = 0.1f;

            float t = 0.0f;
            for (int iteration = 0; iteration < MAX_ITERATIONS; ++iteration)
            {
                float old_t = t;
                t = Vector3.Distance(shootStartPosition, target_position + t * target_velocity) / projectile_speed;
                if (t - old_t < EPSILON)
                    break;
            }

            return target_position + t * target_velocity;
        }

        float first_positive_solution_of_quadratic_equation(float a, float b, float c)
        {
            float discriminant = b * b - 4.0f * a * c;
            if (discriminant < 0.0f)
                return -1.0f; // Indicate there is no solution                                                                      
            float s = (float)Math.Sqrt(discriminant);
            float x1 = (-b - s) / (2.0f * a);
            if (x1 > 0.0f)
                return x1;
            float x2 = (-b + s) / (2.0f * a);
            if (x2 > 0.0f)
                return x2;
            return -1.0f; // Indicate there is no positive solution                                                               
        }

        Vector3 direct_solution(Vector3 target_position, Vector3 target_velocity, float projectile_speed)
        {
            float a = Vector3.Dot(target_velocity, target_velocity) - projectile_speed * projectile_speed;
            float b = 2.0f * Vector3.Dot(target_position, target_velocity);
            float c = Vector3.Dot(target_position, target_position);

            float t = first_positive_solution_of_quadratic_equation(a, b, c);
            if (t <= 0.0f)
                return shootStartPosition; // Indicate we failed to find a solution

            return target_position + t * target_velocity;
        }

        public void Destroy()
        {
            lightManager.RemoveLight(pointLight);
            Alive = false;
        }
    }
}
