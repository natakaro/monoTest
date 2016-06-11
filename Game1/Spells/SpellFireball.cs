﻿using Game1.Lights;
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
    public class SpellFireball
    {
        private Game game;
        private Camera camera;
        private Octree octree;
        public Model fireballModel;
        public Texture2D fireballTexture;
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
                spellStarted = true;
                lastMode = LastMode.Left;
            }
            if (leftButton == false && rightButton == true && stats.currentMana >= rightManaCost)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                stopwatch.Start();
                spellReady = false;
                spellStarted = true;
                lastMode = LastMode.Right;
            }
            if (leftButton == true && rightButton == true && stats.currentMana >= dualManaCost)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;
                stopwatch.Start();
                spellReady = false;
                spellStarted = true;
                lastMode = LastMode.Dual;
            }
        }

        public void Continue(bool leftButton, bool rightButton)
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

                else if (leftButton == false && rightButton == true)  //poki co karabin kulek, ale moze jakis cone - miotacz ognia bliskodystansowy?
                {
                    if (spellReady == false)
                    {
                        if (startingMana >= rightManaCost)
                        {
                            manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / rightCastSpeed) * rightManaCost, rightManaCost);
                            stats.currentMana = startingMana - manaDeducted;
                            if (stopwatch.ElapsedMilliseconds >= rightCastSpeed)
                                spellReady = true;
                        }
                        else
                        {
                            spellStarted = false;
                        }
                    }
                    else if (spellReady == true)
                    {
                        SpellFireballProjectile fireball = new SpellFireballProjectile(game, Matrix.CreateTranslation(camera.Position), fireballModel, octree, fireballTexture, lightManager);
                        fireball.Position = camera.Position;
                        fireball.Velocity = camera.GetMouseRay(game.GraphicsDevice.Viewport).Direction * 1;
                        fireball.Acceleration = camera.GetMouseRay(game.GraphicsDevice.Viewport).Direction * 10;
                        octree.m_objects.Add(fireball);

                        spellReady = false;
                        startingMana -= manaDeducted;
                        manaDeducted = 0;
                        stopwatch.Restart();
                    }
                }

                else if (leftButton == true && rightButton == true)
                {
                    if (spellReady == false)
                    {
                        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / dualCastSpeed) * dualManaCost, dualManaCost);
                        if (stopwatch.ElapsedMilliseconds >= dualCastSpeed)
                            spellReady = true;
                    }
                    stats.currentMana = startingMana - manaDeducted;
                }
            } 
        }

        public void Stop(bool leftButton, bool rightButton)
        {
            if (spellStarted == true)
            {
                if (lastMode == LastMode.Left)
                {
                    if (spellReady == true)
                    {
                        SpellFireballProjectile fireball = new SpellFireballProjectile(game, Matrix.CreateTranslation(camera.Position), fireballModel, octree, fireballTexture, lightManager);
                        fireball.Position = camera.Position;
                        fireball.Velocity = camera.ViewDirection * 100;
                        octree.m_objects.Add(fireball);
                        spellReady = false;
                    }
                    else if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                        manaDeducted = 0;
                    }
                    spellStarted = false;
                    stopwatch.Reset();
                }
                if (lastMode == LastMode.Right)
                {
                    if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                    }
                    spellStarted = false;
                    stopwatch.Reset();
                }
                if (lastMode == LastMode.Dual) //placeholder chwilowo, trzeba zrobic jakis okrag obszarowy a nie kulki wokol
                {
                    if (spellReady == true)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            Vector3 direction = Vector3.Transform(new Vector3(camera.ViewDirection.X, 0, camera.ViewDirection.Z), Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians((360 / 16)*i)));
                            SpellFireballProjectile fireball = new SpellFireballProjectile(game, Matrix.CreateTranslation(camera.Position), fireballModel, octree, fireballTexture, lightManager);
                            fireball.Position = camera.Position;
                            fireball.Velocity = direction * 100;
                            octree.m_objects.Add(fireball);
                            spellReady = false;
                        }
                    }
                    else if (spellReady == false)
                    {
                        stats.currentMana = startingMana;
                        manaDeducted = 0;
                    }
                    spellStarted = false;
                    stopwatch.Reset();
                }
            }
        }

        public SpellFireball(Game game, Camera camera, Octree octree, LightManager lightManager, Stats stats)
        {
            this.game = game;
            this.camera = camera;
            this.octree = octree;
            this.lightManager = lightManager;
            this.stats = stats;

            leftManaCost = 25;
            rightManaCost = 10;
            dualManaCost = 150;

            //in milliseconds
            leftCastSpeed = 1000;
            rightCastSpeed = 250;
            dualCastSpeed = 1000;

            fireballModel = game.Content.Load<Model>("Models/fireball");
            fireballTexture = game.Content.Load<Texture2D>("Textures/firedot");
        }
    }
}
