using Game1.Lights;
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
        public Model turretModel;
        public Texture2D turretTexture;
        private LightManager lightManager;
        private Stats stats;

        private Stopwatch stopwatch = new Stopwatch();

        private float leftManaCost;
        private float rightManaCost;
        private float dualManaCost;

        private float leftCastSpeed;
        private float rightCastSpeed;
        private float dualCastSpeed;

        private bool spellStarted;
        private bool spellReady;
        private float startingMana;
        private float manaDeducted;

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
            if (leftButton == true && rightButton == false && stats.currentMana >= leftManaCost)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                stopwatch.Start();
                spellReady = false;
                spellStarted = true;
                stats.SpellStatus(spellStarted, leftCastSpeed, stopwatch);
                lastMode = LastMode.Left;
            }
            if (leftButton == false && rightButton == true && stats.currentMana >= rightManaCost)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                stopwatch.Start();
                spellReady = false;
                spellStarted = true;
                stats.SpellStatus(spellStarted, rightCastSpeed, stopwatch);
                lastMode = LastMode.Right;
            }
            if (leftButton == true && rightButton == true && stats.currentMana >= dualManaCost && dObj.Type == DrawableObject.ObjectType.Turret)
            {
                targetedTurret = (Turret)dObj;

                startingMana = stats.currentMana;
                manaDeducted = 0;
                stopwatch.Start();
                spellReady = false;
                spellStarted = true;
                stats.SpellStatus(spellStarted, dualCastSpeed, stopwatch);
                lastMode = LastMode.Dual;
            }
        }

        public void Continue(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (spellStarted == true)
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

                else if (leftButton == false && rightButton == true)  //?
                {
                    if (spellReady == false)
                    {
                        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / rightCastSpeed) * rightManaCost, rightManaCost);
                        if (stopwatch.ElapsedMilliseconds >= rightCastSpeed)
                            spellReady = true;
                    }
                    stats.currentMana = startingMana - manaDeducted;
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
            if (spellStarted == true)
            {
                if (lastMode == LastMode.Left)
                {
                    if (spellReady == true && dObj != null)
                    {
                        if (dObj.Type == DrawableObject.ObjectType.Terrain)
                        {
                            Turret turret = new Turret(game, Matrix.CreateTranslation(dObj.Position), turretModel, octree, turretTexture, lightManager);
                            octree.m_objects.Add(turret);
                            spellReady = false;
                        }
                    }
                    else if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                        manaDeducted = 0;
                    }
                    spellStarted = false;
                    stats.SpellStatus(false);
                    stopwatch.Reset();
                }
                if (lastMode == LastMode.Right)
                {
                    if (spellReady == true && dObj != null)
                    {
                        if (dObj.Type == DrawableObject.ObjectType.Terrain)
                        {
                            Turret turret = new Turret(game, Matrix.CreateTranslation(dObj.Position), turretModel, octree, turretTexture, lightManager);
                            octree.m_objects.Add(turret);
                            spellReady = false;
                        }
                    }
                    else if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                        manaDeducted = 0;
                    }
                    spellStarted = false;
                    stats.SpellStatus(false);
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
                                //TODO: add resources back

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
                    spellStarted = false;
                    stats.SpellStatus(false);
                    stopwatch.Reset();
                }
            }
        }

        public SpellCreateTurret(Game game, Camera camera, Octree octree, LightManager lightManager, Stats stats)
        {
            this.game = game;
            this.camera = camera;
            this.octree = octree;
            this.lightManager = lightManager;
            this.stats = stats;

            leftManaCost = 150;
            rightManaCost = 150;
            dualManaCost = 10;

            //in milliseconds
            leftCastSpeed = 1000;
            rightCastSpeed = 2000;
            dualCastSpeed = 2000;

            turretModel = game.Content.Load<Model>("Models/turret");
            turretTexture = game.Content.Load<Texture2D>("Textures/turret");
        }
    }
}
