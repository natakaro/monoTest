using Game1.Lights;
using Game1.Particles;
using Game1.Screens;
using Game1.Turrets;
using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Game1.Helpers.HexCoordinates;

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

        public Model turretLatticeModel;
        public Texture2D turretLatticeTexture;

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

        private TurretLattice lattice;
        private PointLight latticeLight;
        private Vector3 latticeLightOffset = new Vector3(0, 1, 0);

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

                if (lattice == null && dObj != null)
                {
                    lattice = new TurretLattice(game, Matrix.CreateTranslation(dObj.Position), turretLatticeModel, turretLatticeTexture, octree);
                    latticeLight = new PointLight(dObj.Position + latticeLightOffset, Color.White, 5, 1);
                    objectManager.AddAlwaysDraw(lattice);
                    lightManager.AddLight(latticeLight);
                }
            }

            if (dObj != null)
            {
                if (leftButton == false && rightButton == true && stats.currentMana >= rightManaCost && dObj.Type == DrawableObject.ObjectType.Turret)
                {
                    targetedTurret = (Turret)dObj;

                    startingMana = stats.currentMana;
                    manaDeducted = 0;

                    stopwatch.Start();
                    spellReady = false;
                    spellCharging = SpellCharging.Right;
                    stats.SpellStatus(spellCharging, rightCastSpeed, stopwatch);
                    lastMode = LastMode.Right;

                    if (lattice != null)
                    {
                        objectManager.RemoveAlwaysDraw(lattice);
                        lightManager.RemoveLight(latticeLight);

                        lattice = null;
                        latticeLight = null;
                    }
                }
            }

            //if (leftButton == false && rightButton == true && stats.currentMana >= rightManaCost && stats.currentEssence >= rightEssenceCost)
            //{
            //    startingMana = stats.currentMana;
            //    manaDeducted = 0;
            //    startingEssence = stats.currentEssence;
            //    essenceDeducted = 0;

            //    stopwatch.Start();
            //    spellReady = false;
            //    spellCharging = SpellCharging.Right;
            //    stats.SpellStatus(spellCharging, rightCastSpeed, stopwatch);
            //    lastMode = LastMode.Right;

            //    if (lattice == null && dObj != null)
            //    {
            //        lattice = new TurretLattice(game, Matrix.CreateTranslation(dObj.Position), turretLatticeModel, turretLatticeTexture, octree);
            //        latticeLight = new PointLight(dObj.Position + latticeLightOffset, Color.White, 5, 1);
            //        objectManager.AddAlwaysDraw(lattice);
            //        lightManager.AddLight(latticeLight);
            //    }
            //}
            //if (leftButton == true && rightButton == true && stats.currentMana >= dualManaCost && dObj.Type == DrawableObject.ObjectType.Turret)
            //{
            //    targetedTurret = (Turret)dObj;

            //    startingMana = stats.currentMana;
            //    manaDeducted = 0;

            //    stopwatch.Start();
            //    spellReady = false;
            //    spellCharging = SpellCharging.Dual;
            //    stats.SpellStatus(spellCharging, dualCastSpeed, stopwatch);
            //    lastMode = LastMode.Dual;

            //    if (lattice != null)
            //    {
            //        objectManager.RemoveAlwaysDraw(lattice);
            //        lightManager.RemoveLight(latticeLight);

            //        lattice = null;
            //        latticeLight = null;
            //    }
            //}
        }

        public void Continue(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (spellCharging > 0)
            {
                if (leftButton == true && rightButton == false)
                {
                    if (lattice == null && dObj != null)
                    {
                        lattice = new TurretLattice(game, Matrix.CreateTranslation(dObj.Position), turretLatticeModel, turretLatticeTexture, octree);
                        latticeLight = new PointLight(dObj.Position + latticeLightOffset, Color.White, 5, 1);
                        objectManager.AddAlwaysDraw(lattice);
                        lightManager.AddLight(latticeLight);
                    }
                    else if (lattice != null && dObj != null)
                    {
                        lattice.Position = dObj.Position;
                        latticeLight.Position = dObj.Position + latticeLightOffset;
                    }
                    else if (lattice != null && dObj == null)
                    {
                        objectManager.RemoveAlwaysDraw(lattice);
                        lightManager.RemoveLight(latticeLight);

                        lattice = null;
                        latticeLight = null;
                    }

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

                else if (leftButton == false && rightButton == true) //usuwanie turreta
                {
                    if (lattice != null)
                    {
                        objectManager.RemoveAlwaysDraw(lattice);
                        lightManager.RemoveLight(latticeLight);

                        lattice = null;
                        latticeLight = null;
                    }

                    if (dObj != null && dObj.Type == DrawableObject.ObjectType.Turret)
                    {
                        if (spellReady == false)
                        {
                            manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / rightCastSpeed) * rightManaCost, rightManaCost);
                            if (stopwatch.ElapsedMilliseconds >= rightCastSpeed)
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
                        if (dObj.Type == DrawableObject.ObjectType.Tile && (dObj as Tile).ObjectOn == null)
                        {
                            if (GameplayScreen.phaseManager.Phase == Phase.Day || (GameplayScreen.phaseManager.Phase == Phase.Night && (dObj as Tile).IsPath == false))
                            {
                                Turret turret = new Turret(game, Matrix.CreateTranslation(dObj.Position), smallTurretModel, camera, octree, objectManager, smallTurretTexture, lightManager, particleManager);
                                spellReady = false;
                                (dObj as Tile).ObjectOn = turret;
                                turret.Tile = dObj as Tile;

                                if (UpdatePaths() == false)
                                {
                                    turret.Tile.ObjectOn = null;
                                    turret.Destroy(false);
                                    stats.currentEssence += leftEssenceCost;
                                }
                                else
                                {
                                    GameplayScreen.turretList.Add(turret);
                                    Octree.AddObject(turret);
                                }
                            }
                            else
                            {
                                stats.currentMana = startingMana;
                                manaDeducted = 0;
                                stats.currentEssence = startingEssence;
                                essenceDeducted = 0;
                            }
                        }
                        else
                        {
                            stats.currentMana = startingMana;
                            manaDeducted = 0;
                            stats.currentEssence = startingEssence;
                            essenceDeducted = 0;
                        }
                    }
                    else
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

                if (lastMode == LastMode.Right) //usuwanie turreta
                {
                    if (dObj != null && dObj.Type == DrawableObject.ObjectType.Turret)
                    {
                        if (targetedTurret != null)
                        {
                            if (spellReady == true)
                            {
                                (dObj as Turret).Tile.ObjectOn = null;
                                GameplayScreen.turretList.Remove(targetedTurret);
                                targetedTurret.Destroy(true);
                                stats.currentEssence = Math.Min((stats.currentEssence + leftEssenceCost / 2), stats.maxEssence);

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

                    UpdatePaths();
                }
            }
            if (lattice != null)
            {
                objectManager.RemoveAlwaysDraw(lattice);
                lightManager.RemoveLight(latticeLight);

                lattice = null;
                latticeLight = null;
            }
        }

        private bool UpdatePaths()
        {
            foreach (Spawn spawn in GameplayScreen.spawns)
            {
                if (spawn.UpdatePath() == false)
                {
                    return false;
                }
            }

            return true;
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
            rightManaCost = 10;
            dualManaCost = 10;

            leftEssenceCost = 100;
            rightEssenceCost = 0;

            //in milliseconds
            leftCastSpeed = 1000;
            rightCastSpeed = 2000;
            dualCastSpeed = 2000;

            smallTurretModel = game.Content.Load<Model>("Models/turret");
            smallTurretTexture = game.Content.Load<Texture2D>("Textures/turret");
            bigTurretModel = game.Content.Load<Model>("Models/big_turret/big_turret");
            bigTurretTexture = game.Content.Load<Texture2D>("Textures/big_turret/big_turret_FIRE");

            turretLatticeModel = game.Content.Load<Model>("Models/turretLattice");
            turretLatticeTexture = game.Content.Load<Texture2D>("Textures/turretLattice");
        }
    }
}
