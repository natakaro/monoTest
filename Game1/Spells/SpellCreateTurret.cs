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
    public class SpellCreateTurret
    {
        private Game game;
        private Camera camera;
        private Octree octree;
        private ObjectManager objectManager;
        private ParticleManager particleManager;
        public Model smallTurretModel;
        public Texture2D smallTurretTexture;
        public Model bigTurretModel;
        public Texture2D bigTurretTexture;

        private LightManager lightManager;
        private Stats stats;

        private Stopwatch stopwatch = new Stopwatch();

        private float leftManaCost;
        private float rightManaCost;
        private float dualManaCost;

        private float leftEssenceCost;
        private float rightEssenceCost;

        private float leftCastSpeed;
        private float rightCastSpeed;
        private float dualCastSpeed;

        private SpellCharging spellCharging;
        private bool spellReady;

        private float startingMana;
        private float manaDeducted;

        private float startingEssence;
        private float essenceDeducted;

        private Turret targetedTurret;

        private enum LastMode
        {
            Left = 0,
            Right = 1,
            Dual = 2
        };

        private LastMode lastMode;

        public void Start(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (leftButton == true && rightButton == false && stats.currentMana >= leftManaCost && stats.currentEssence >= leftEssenceCost)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                startingEssence = stats.currentEssence;
                essenceDeducted = 0;

                stopwatch.Start();
                spellReady = false;
                spellCharging = SpellCharging.Left;
                stats.SpellStatus(spellCharging, leftCastSpeed, stopwatch);
                lastMode = LastMode.Left;
            }
            if (leftButton == false && rightButton == true && stats.currentMana >= rightManaCost && stats.currentEssence >= rightEssenceCost)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                startingEssence = stats.currentEssence;
                essenceDeducted = 0;

                stopwatch.Start();
                spellReady = false;
                spellCharging = SpellCharging.Right;
                stats.SpellStatus(spellCharging, rightCastSpeed, stopwatch);
                lastMode = LastMode.Right;
            }
            if (leftButton == true && rightButton == true && stats.currentMana >= dualManaCost && dObj.Type == DrawableObject.ObjectType.Turret)
            {
                targetedTurret = (Turret)dObj;

                startingMana = stats.currentMana;
                manaDeducted = 0;

                stopwatch.Start();
                spellReady = false;
                spellCharging = SpellCharging.Dual;
                stats.SpellStatus(spellCharging, dualCastSpeed, stopwatch);
                lastMode = LastMode.Dual;
            }
        }

        public void Continue(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (spellCharging > 0)
            {
                if (leftButton == true && rightButton == false)
                {
                    if (spellReady == false)
                    {
                        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / leftCastSpeed) * leftManaCost, leftManaCost);
                        essenceDeducted = Math.Min((stopwatch.ElapsedMilliseconds / leftCastSpeed) * leftEssenceCost, leftEssenceCost);
                        if (stopwatch.ElapsedMilliseconds >= leftCastSpeed)
                            spellReady = true;
                    }
                    stats.currentMana = startingMana - manaDeducted;
                    stats.currentEssence = startingEssence - essenceDeducted;
                }

                else if (leftButton == false && rightButton == true)  //?
                {
                    if (spellReady == false)
                    {
                        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / rightCastSpeed) * rightManaCost, rightManaCost);
                        essenceDeducted = Math.Min((stopwatch.ElapsedMilliseconds / rightCastSpeed) * rightEssenceCost, rightEssenceCost);
                        if (stopwatch.ElapsedMilliseconds >= rightCastSpeed)
                            spellReady = true;
                    }
                    stats.currentMana = startingMana - manaDeducted;
                    stats.currentEssence = startingEssence - essenceDeducted;
                }

                else if (leftButton == true && rightButton == true) //usuwanie turreta
                {
                    if (dObj != null && dObj.Type == DrawableObject.ObjectType.Turret)
                    {
                        if (spellReady == false)
                        {
                            manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / dualCastSpeed) * dualManaCost, dualManaCost);
                            if (stopwatch.ElapsedMilliseconds >= dualCastSpeed)
                                spellReady = true;
                        }
                        stats.currentMana = startingMana - manaDeducted;
                    }
                    else
                    {
                        targetedTurret = null;
                        Stop(false, false, dObj);
                    }
                }
            }
        }

        public void Stop(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (spellCharging > 0)
            {
                if (lastMode == LastMode.Left)
                {
                    if (spellReady == true && dObj != null)
                    {
                        if (dObj.Type == DrawableObject.ObjectType.Tile)
                        {
                            Turret turret = new Turret(game, Matrix.CreateTranslation(dObj.Position), smallTurretModel, octree, objectManager, smallTurretTexture, lightManager, particleManager);
                            Octree.AddObject(turret);
                            spellReady = false;
                        }
                    }
                    else if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                        manaDeducted = 0;
                        stats.currentEssence = startingEssence;
                        essenceDeducted = 0;
                    }
                    spellCharging = SpellCharging.None;
                    stats.SpellStatus(spellCharging);
                    stopwatch.Reset();
                }
                if (lastMode == LastMode.Right)
                {
                    if (spellReady == true && dObj != null)
                    {
                        if (dObj.Type == DrawableObject.ObjectType.Tile)
                        {
                            Turret turret = new Turret(game, Matrix.CreateTranslation(dObj.Position), bigTurretModel, octree, objectManager, bigTurretTexture, lightManager, particleManager);
                            Octree.AddObject(turret);
                            spellReady = false;
                        }
                    }
                    else if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                        manaDeducted = 0;
                        stats.currentEssence = startingEssence;
                        essenceDeducted = 0;
                    }
                    spellCharging = SpellCharging.None;
                    stats.SpellStatus(spellCharging);
                    stopwatch.Reset();
                }
                if (lastMode == LastMode.Dual) //usuwanie turreta
                {
                    if (dObj != null && dObj.Type == DrawableObject.ObjectType.Turret)
                    {
                        if (targetedTurret != null)
                        {
                            if (spellReady == true)
                            {
                                targetedTurret.Destroy();
                                stats.currentEssence += leftEssenceCost / 2;

                                spellReady = false;
                            }
                            else if (spellReady == false)
                            {
                                stats.currentMana = startingMana;
                                manaDeducted = 0;
                            }
                        }
                    }

                    targetedTurret = null;
                    spellCharging = SpellCharging.None;
                    stats.SpellStatus(spellCharging);
                    stopwatch.Reset();
                }
            }
        }

        public SpellCreateTurret(Game game, Camera camera, Octree octree, ObjectManager objectManager, LightManager lightManager, ParticleManager particleManager, Stats stats)
        {
            this.game = game;
            this.camera = camera;
            this.octree = octree;
            this.objectManager = objectManager;
            this.lightManager = lightManager;
            this.particleManager = particleManager;
            this.stats = stats;

            leftManaCost = 150;
            rightManaCost = 150;
            dualManaCost = 10;

            leftEssenceCost = 100;
            rightEssenceCost = 100;

            //in milliseconds
            leftCastSpeed = 1000;
            rightCastSpeed = 2000;
            dualCastSpeed = 2000;

            smallTurretModel = game.Content.Load<Model>("Models/turret");
            smallTurretTexture = game.Content.Load<Texture2D>("Textures/turret");
            bigTurretModel = game.Content.Load<Model>("Models/big_turret/big_turret");
            bigTurretTexture = game.Content.Load<Texture2D>("Textures/big_turret/big_turret_FIRE");
        }
    }
}
