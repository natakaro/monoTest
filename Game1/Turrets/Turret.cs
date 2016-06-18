using Game1.Helpers;
using Game1.Lights;
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
    class Turret : DrawableObject
    {
        Game game;
        Texture2D texture;
        private PointLight pointLight;
        LightManager lightManager;
        ObjectManager objectManager;

        public Model fireballModel;
        public Texture2D fireballTexture;

        Vector3 shootStartPosition;

        float range;
        float projectileSpeed;
        float rateOfFire;
        float damage;

        BoundingSphere rangeSphere;
        List<DrawableObject> enemiesInRange;
        Enemy currentTarget;
        Stopwatch shootStopwatch;

        Stopwatch scaleStopwatch;
        bool fullSize;

        public Turret(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ObjectManager objectManager, Texture2D inTexture, LightManager lightManager) : base(game, inWorldMatrix, inModel, octree)
        {
            this.game = game;
            this.lightManager = lightManager;
            this.objectManager = objectManager;
            texture = inTexture;

            type = ObjectType.Turret;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);

            shootStartPosition = position + new Vector3(0, 30, 0);

            pointLight = new PointLight(shootStartPosition, Color.Violet, 50, 10);
            lightManager.AddLight(pointLight);

            fireballModel = game.Content.Load<Model>("Models/fireball");
            fireballTexture = game.Content.Load<Texture2D>("Textures/firedot");

            range = 250;
            projectileSpeed = 100;
            rateOfFire = 500;
            damage = 10;
            rangeSphere = new BoundingSphere(shootStartPosition, range);

            enemiesInRange = new List<DrawableObject>();
            currentTarget = null;

            shootStopwatch = new Stopwatch();

            scaleStopwatch = new Stopwatch();
            scaleStopwatch.Start();
            fullSize = false;
        }

        public override void Draw(Camera camera)
        {
            if (fullSize == false)
            {
                scale = 0;
                if (scaleStopwatch.ElapsedMilliseconds < 750)
                {
                    scale = MathHelper.Lerp(0, 1, scaleStopwatch.ElapsedMilliseconds / 750f);
                }
                else
                {
                    scale = 1;
                    fullSize = true;
                    scaleStopwatch.Stop();
                }
                Matrix scaleMatrix = Matrix.CreateScale(scale);

                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.Parameters["World"].SetValue(scaleMatrix * modelBones[mesh.ParentBone.Index] * worldMatrix);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                        effect.Parameters["FarClip"].SetValue(camera.FarZ);
                        effect.Parameters["Texture"].SetValue(texture);
                        effect.Parameters["Clipping"].SetValue(false);
                    }
                    mesh.Draw();
                }
            }
            else
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                        effect.Parameters["FarClip"].SetValue(camera.FarZ);
                        effect.Parameters["Texture"].SetValue(texture);
                        effect.Parameters["Clipping"].SetValue(false);
                    }
                    mesh.Draw();
                }
            }

            //DebugShapeRenderer.AddBoundingSphere(rangeSphere, Color.Red);
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            SearchForEnemies();
            TargetClosest();
            Shoot(gameTime);

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
                shootStopwatch.Start();

                if (shootStopwatch.ElapsedMilliseconds > rateOfFire)
                {
                    //Vector3 interception = currentTarget.Position + new Vector3(0, 20, 30);
                    Vector3 interception = iterative_approximation(currentTarget.Position + new Vector3(0, 20, 0), currentTarget.Velocity, projectileSpeed);
                    //Vector3 interception = direct_solution(currentTarget.Position + new Vector3(0, 20, 0), currentTarget.Velocity, projectileSpeed);
                    Vector3 direction = Vector3.Normalize(interception - shootStartPosition);

                    TurretProjectile projectile = new TurretProjectile(game, Matrix.CreateTranslation(shootStartPosition), fireballModel, octree, objectManager, fireballTexture, lightManager, damage);
                    
                    projectile.Position = shootStartPosition;
                    projectile.Velocity = direction * projectileSpeed;
                    //projectile.Acceleration = direction * 10;
                    objectManager.Add(projectile);
                    //octree.AddObject(projectile);

                    shootStopwatch.Reset();
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
