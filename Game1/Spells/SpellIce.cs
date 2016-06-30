using Game1.HUD;
using Game1.Lights;
using Game1.Particles;
using Game1.Turrets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Spells
{
    public class SpellIce
    {
        private Game game;
        private Camera camera;
        private Octree octree;
        private ObjectManager objectManager;
        private Model iceboltModel;
        private Texture2D iceboltTexture;
        private LightManager lightManager;
        private ParticleManager particleManager;
        private HUDManager hudManager;
        private Stats stats;
        private Viewport viewport;

        private Stopwatch stopwatch = new Stopwatch();

        private float leftManaCost;
        private float rightManaCost;
        private float dualManaCost;

        private float leftCastSpeed;
        private float rightCastSpeed;
        private float dualCastSpeed;

        private SpellCharging spellCharging;
        private bool spellReady;
        private float startingMana;
        private float manaDeducted;

        private float leftDamage;
        private float rightDamage;
        private float projectileSpeed;
        private float particlesPerSecond;
        private float timeBetweenParticles;
        private float timeLeftOver;
        private float coneRange;

        private float timer;

        private BoundingFrustum frustum;
        private Matrix projectionMatrix;

        public event EventHandler hitEvent;

        private enum LastMode
        {
            Left = 0,
            Right = 1,
            Dual = 2
        };

        private LastMode lastMode;

        public void Start(bool leftButton, bool rightButton)
        {
            if (leftButton == true && rightButton == false && stats.currentMana >= leftManaCost)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                stopwatch.Start();
                spellReady = false;
                spellCharging = SpellCharging.Left;
                stats.SpellStatus(spellCharging, leftCastSpeed, stopwatch);
                lastMode = LastMode.Left;
            }
            if (leftButton == false && rightButton == true && stats.currentMana >= rightManaCost && stats.rightModeEnabled)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                stopwatch.Start();
                spellReady = false;
                spellCharging = SpellCharging.Right;
                stats.SpellStatus(spellCharging, rightCastSpeed, stopwatch);
                lastMode = LastMode.Right;
            }
            //if (leftButton == true && rightButton == true && stats.currentMana >= dualManaCost)
            //{
            //    startingMana = stats.currentMana;
            //    manaDeducted = 0;
            //    stopwatch.Start();
            //    spellReady = false;
            //    spellCharging = SpellCharging.Dual;
            //    stats.SpellStatus(spellCharging, dualCastSpeed, stopwatch);
            //    lastMode = LastMode.Dual;
            //}
        }

        public void Continue(bool leftButton, bool rightButton, GameTime gameTime)
        {
            if (spellCharging > 0)
            {
                if (leftButton == true && rightButton == false)
                {
                    if (spellReady == false)
                    {
                        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / leftCastSpeed) * leftManaCost, leftManaCost);
                        if (stopwatch.ElapsedMilliseconds >= leftCastSpeed)
                            spellReady = true;
                    }
                    stats.currentMana = startingMana - manaDeducted;
                }

                else if (leftButton == false && rightButton == true)  //cone
                {
                    if (spellReady == false)
                    {
                        if (startingMana >= rightManaCost)
                        {
                            manaDeducted = (stopwatch.ElapsedMilliseconds / rightCastSpeed) * rightManaCost;
                            stats.currentMana = startingMana - manaDeducted;
                            if (stopwatch.ElapsedMilliseconds >= rightCastSpeed)
                                spellReady = true;
                        }
                        else
                        {
                            spellCharging = SpellCharging.None;
                        }
                    }
                    else if (spellReady == true)
                    {
                        if (stats.currentMana > 0)
                        {
                            frustum.Matrix = camera.ViewMatrix * projectionMatrix;
                            Vector3 position = camera.Position + camera.ViewDirection * 15 + new Vector3(0, -3, 0);
                            Vector3 velocity = camera.ViewDirection * 25;

                            float elapsedMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                            timer += elapsedMs;

                            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                            float timeToSpend = timeLeftOver + elapsedTime;
                            float currentTime = -timeLeftOver;

                            while (timeToSpend > timeBetweenParticles)
                            {
                                currentTime += timeBetweenParticles;
                                timeToSpend -= timeBetweenParticles;
                                particleManager.iceParticles.AddParticle(position, velocity);
                            }

                            manaDeducted = (stopwatch.ElapsedMilliseconds / rightCastSpeed) * rightManaCost;
                            stats.currentMana = startingMana - manaDeducted;

                            if (timer > 50)
                            {
                                List<IntersectionRecord> hitList = octree.AllIntersections(frustum);
                                foreach (IntersectionRecord ir in hitList)
                                {
                                    if (ir.DrawableObjectObject.Type == DrawableObject.ObjectType.Enemy)
                                    {
                                        OnHitEvent();
                                        Enemy enemy = ir.DrawableObjectObject as Enemy;
                                        enemy.Damage(rightDamage, DamageType.Ice);
                                    }
                                    else if (ir.DrawableObjectObject.Type == DrawableObject.ObjectType.Turret)
                                    {
                                        Turret turret = ir.DrawableObjectObject as Turret;
                                        if (turret.mode != Mode.IceRight)
                                        {
                                            OnHitEvent();
                                            turret.SwitchMode(Mode.IceRight);
                                        }
                                    }
                                }
                                timer = 0;
                            }

                            timeLeftOver = timeToSpend;
                            //DebugShapeRenderer.AddBoundingFrustum(frustum, Color.White);
                        }
                        else
                            Stop(false, false);
                    }
                }

                //else if (leftButton == true && rightButton == true)
                //{
                //    if (spellReady == false)
                //    {
                //        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / dualCastSpeed) * dualManaCost, dualManaCost);
                //        if (stopwatch.ElapsedMilliseconds >= dualCastSpeed)
                //            spellReady = true;
                //    }
                //    stats.currentMana = startingMana - manaDeducted;
                //}
            }
        }

        public void Stop(bool leftButton, bool rightButton)
        {
            if (spellCharging > 0)
            {
                if (lastMode == LastMode.Left)
                {
                    if (spellReady == true)
                    {
                        IceProjectile iceBolt = new IceProjectile(game, camera.WeaponWorldMatrix(2, -2, 2, 1) /*Matrix.CreateTranslation(camera.Position)*/, iceboltModel, iceboltTexture, octree, objectManager, hudManager, leftDamage, particleManager.iceExplosionParticles, particleManager.iceExplosionSnowParticles, particleManager.iceProjectileTrailParticles);
                        iceBolt.Velocity = camera.ViewDirection * projectileSpeed;
                        objectManager.Add(iceBolt);
                        spellReady = false;
                    }
                    else if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                        manaDeducted = 0;
                    }
                    spellCharging = SpellCharging.None;
                    stats.SpellStatus(spellCharging);
                    stopwatch.Reset();
                }
                if (lastMode == LastMode.Right)
                {
                    if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                    }
                    spellCharging = SpellCharging.None;
                    stats.SpellStatus(spellCharging);
                    stopwatch.Reset();
                }
                //if (lastMode == LastMode.Dual) //placeholder chwilowo, trzeba zrobic jakis okrag obszarowy a nie kulki wokol
                //{
                //    if (spellReady == true)
                //    {
                //        for (int i = 0; i < 16; i++)
                //        {
                //            Vector3 direction = Vector3.Transform(new Vector3(camera.ViewDirection.X, 0, camera.ViewDirection.Z), Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians((360 / 16) * i)));
                //            IceProjectile iceBolt = new IceProjectile(game, Matrix.CreateTranslation(camera.Position), iceboltModel, iceboltTexture, octree, objectManager, hudManager, leftDamage, particleManager.iceExplosionParticles, particleManager.iceExplosionSnowParticles, particleManager.iceProjectileTrailParticles);
                //            iceBolt.Velocity = direction * 100;
                //            objectManager.Add(iceBolt);
                //            iceBolt.hitEvent += hudManager.Crosshair.HandleHitEvent;
                //            spellReady = false;
                //        }
                //    }
                //    else if (spellReady == false)
                //    {
                //        stats.currentMana = startingMana;
                //        manaDeducted = 0;
                //    }
                //    spellCharging = SpellCharging.None;
                //    stats.SpellStatus(spellCharging);
                //    stopwatch.Reset();
                //}
            }
        }

        public SpellIce(Game game, Camera camera, Octree octree, ObjectManager objectManager, LightManager lightManager, ParticleManager particleManager, HUDManager hudManager, Stats stats)
        {
            this.game = game;
            this.camera = camera;
            this.octree = octree;
            this.objectManager = objectManager;
            this.lightManager = lightManager;
            this.particleManager = particleManager;
            this.hudManager = hudManager;
            this.stats = stats;
            this.viewport = game.GraphicsDevice.Viewport;

            leftManaCost = 25;
            rightManaCost = 10;
            dualManaCost = 150;

            //in milliseconds
            leftCastSpeed = 1000;
            rightCastSpeed = 250;
            dualCastSpeed = 1000;

            leftDamage = 50;
            rightDamage = 1;
            projectileSpeed = 250;
            coneRange = 150f;

            particlesPerSecond = 250;
            timeBetweenParticles = 1.0f / particlesPerSecond;

            iceboltModel = game.Content.Load<Model>("Models/icebolt");
            iceboltTexture = game.Content.Load<Texture2D>("Textures/icebolt");

            frustum = new BoundingFrustum(camera.ViewProjectionMatrix);

            Vector2 size = new Vector2(viewport.Bounds.Width, viewport.Bounds.Height);
            size.X /= 256f;
            size.Y /= 256f;
            projectionMatrix = Matrix.CreatePerspective(size.X, size.Y, 15f, coneRange);

            hitEvent += hudManager.Crosshair.HandleHitEvent;
        }

        public void OnHitEvent()
        {
            hitEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
