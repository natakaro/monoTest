using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Helpers;
using static Game1.Helpers.HexCoordinates;

namespace Game1.Spells
{
    public class SpellMoveTerrain
    {
        private PhaseManager phaseManager;
        
        private DrawableObject target; 
        private Octree octree;
        private float average_y;
        private Stats stats;
        private Dictionary<AxialCoordinate, Tile> map;

        private Stopwatch stopwatch = new Stopwatch();

        private float manaCost;
        private float dualManaCost;

        private float castSpeed;
        private float dualCastSpeed;

        private SpellCharging spellCharging;
        private bool spellReady;
        private float startingMana;
        private float manaDeducted;

        bool tooHigh = false;
        bool tooLow = false;
        List<Tile> neighbors;

        public void Start(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (dObj != null && phaseManager.Phase == Phase.Day)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;

                if (target != null && target.Type == DrawableObject.ObjectType.Tile) //fix jesli uzywajac na jednym klikniemy na drugim
                {
                    target.Acceleration = Vector3.Zero;
                    target.Velocity = Vector3.Zero;
                }

                if (leftButton && rightButton)  //wersja obszarowa, wyrownywanie terenu
                {
                    if (startingMana >= dualManaCost)
                    {
                        if (dObj.Type == DrawableObject.ObjectType.Tile)
                        {
                            if (target != null)
                            {
                                target.Acceleration = Vector3.Zero;
                                target.Velocity = Vector3.Zero;
                            }

                            spellCharging = SpellCharging.Dual;
                            ////tworzymy sphere duza wokol celu, zbieramy kolizje wszystkich wokol i wyliczyamy srednia pozycje na y
                            //areaSphere = new BoundingSphere(dObj.Position, 40f);
                            //sphere_list = octree.AllIntersections(areaSphere, DrawableObject.ObjectType.Tile);

                            //average_y = 0;

                            //foreach (IntersectionRecord ir in sphere_list)
                            //{
                            //    ir.DrawableObjectObject.IsStatic = false;
                            //    average_y += ir.DrawableObjectObject.Position.Y;
                            //}
                            //average_y = average_y / sphere_list.Count;

                            target = dObj;
                            target.IsStatic = false;

                            neighbors = GetNeighborTiles((Tile)target, map);

                            average_y = dObj.Position.Y;

                            foreach (Tile neighbor in neighbors)
                            {
                                neighbor.IsStatic = false;
                                average_y += neighbor.Position.Y;
                            }
                            average_y = average_y / neighbors.Count + 1;

                            stopwatch.Start();
                            stats.SpellStatus(spellCharging, dualCastSpeed, stopwatch);
                        }
                    }
                }

                else if ((leftButton || rightButton) && startingMana >= manaCost)
                {
                    if (dObj.Type == DrawableObject.ObjectType.Tile)
                    {
                        target = dObj;

                        neighbors = GetNeighborTiles((Tile)target, map);

                        foreach (Tile neighbor in neighbors)
                        {
                            if (target.Position.Y - neighbor.Position.Y > 100)
                                tooHigh = true;
                            else if (target.Position.Y - neighbor.Position.Y < -100)
                                tooLow = true;
                        }

                        target.IsStatic = false;
                        if (leftButton)
                        {
                            if (tooHigh == false)
                            {
                                spellCharging = SpellCharging.Left;
                                target.Acceleration = new Vector3(0, 10, 0);
                            }
                        }
                        else
                        {
                            if (tooLow == false)
                            {
                                spellCharging = SpellCharging.Right;
                                target.Acceleration = new Vector3(0, -10, 0);
                            }
                        }
                        stopwatch.Start();
                        stats.SpellStatus(spellCharging, 0, stopwatch);
                    }
                }
            }
        }

        public void Continue(bool leftButton, bool rightButton)
        {
            if (spellCharging > 0)
            {
                if (leftButton == true && rightButton == true && neighbors != null) //w wersji obszarowej poruszamy kazdym tilem w odpowiednim kierunku z usredniona predkoscia
                {
                    //DebugShapeRenderer.AddBoundingSphere(areaSphere, Color.White);
                    //foreach (IntersectionRecord ir in sphere_list)
                    //{
                    //    //if (ir.DrawableObjectObject.Position.Y == average_y)
                    //    //{
                    //    //    ir.DrawableObjectObject.Velocity = Vector3.Zero;  //jesli osiagnelismy cel to stop
                    //    //}
                    //    //else if (ir.DrawableObjectObject.Position.Y > average_y)
                    //    //    ir.DrawableObjectObject.Velocity = new Vector3(0, average_y - ir.DrawableObjectObject.Position.Y - 0.1f, 0); //dzieki temu kazdy tile trafia na miejsce w dokladnie tym samym momencie
                    //    //else if (ir.DrawableObjectObject.Position.Y < average_y)
                    //    //    ir.DrawableObjectObject.Velocity = new Vector3(0, average_y - ir.DrawableObjectObject.Position.Y + 0.1f, 0); //dodanie wyzej lub tutaj odjecie 0.1f przyspiesza zblizenie sie do celu, bo predkosc maleje wraz z odlegloscia

                    //    //if (stopwatch.ElapsedMilliseconds < dualCastSpeed)
                    //    if (ir.DrawableObjectObject.Position.Y != average_y)
                    //    {
                    //        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / dualCastSpeed) * dualManaCost, dualManaCost);
                    //        stats.currentMana = startingMana - manaDeducted;
                    //        ir.DrawableObjectObject.Velocity = new Vector3(0, MathHelper.Lerp(0, average_y - ir.DrawableObjectObject.Position.Y, stopwatch.ElapsedMilliseconds / dualCastSpeed), 0);
                    //    }
                    //    else
                    //    {
                    //        ir.DrawableObjectObject.Velocity = Vector3.Zero;
                    //    }
                    //}
                    foreach (Tile neighbor in neighbors)
                    {
                        if (neighbor.Position.Y != average_y)
                        {
                            manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / dualCastSpeed) * dualManaCost, dualManaCost);
                            stats.currentMana = startingMana - manaDeducted;
                            neighbor.Velocity = new Vector3(0, MathHelper.Lerp(0, average_y - neighbor.Position.Y, stopwatch.ElapsedMilliseconds / dualCastSpeed), 0);
                        }
                        else
                        {
                            neighbor.Velocity = Vector3.Zero;
                        }
                    }
                    if (target.Position.Y != average_y)
                    {
                        manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / dualCastSpeed) * dualManaCost, dualManaCost);
                        stats.currentMana = startingMana - manaDeducted;
                        target.Velocity = new Vector3(0, MathHelper.Lerp(0, average_y - target.Position.Y, stopwatch.ElapsedMilliseconds / dualCastSpeed), 0);
                    }
                    else
                    {
                        target.Velocity = Vector3.Zero;
                    }
                }
                else if (target != null)
                {
                    if (stats.currentMana > 0 && phaseManager.Phase == Phase.Day)
                    {
                        manaDeducted = (stopwatch.ElapsedMilliseconds / castSpeed) * manaCost;
                        stats.currentMana = startingMana - manaDeducted;

                        if (stopwatch.ElapsedMilliseconds > castSpeed) //zwiekszanie predkosci + zabawa timerem
                        {
                            if (leftButton == true)
                            {
                                if (tooHigh == false)
                                    target.Acceleration += new Vector3(0, 1, 0);
                                else
                                {
                                    Stop(false, false);
                                }
                            }
                            else if (rightButton == true)
                            {
                                if (tooLow == false)
                                    target.Acceleration += new Vector3(0, -1, 0);
                                else
                                {
                                    Stop(false, false);
                                }
                            }
                            startingMana -= manaDeducted;
                            stopwatch.Restart();
                        }
                    }
                    else
                        Stop(false, false);
                }
            }
        }

        public void Stop(bool leftButton, bool rightButton)
        {
            if (target != null)
            {
                target.Acceleration = Vector3.Zero;
                target.Velocity = Vector3.Zero;
                target.IsStatic = true;
                target = null;
                stopwatch.Reset();
            }

            if (neighbors != null && neighbors.Count != 0)
            {
                foreach (Tile neighbor in neighbors)
                {
                    neighbor.Acceleration = Vector3.Zero;
                    neighbor.Velocity = Vector3.Zero;
                    neighbor.IsStatic = true;
                }
                neighbors.Clear();
                stopwatch.Reset();
            }
            spellCharging = SpellCharging.None;
            stats.SpellStatus(spellCharging);
        }

        public SpellMoveTerrain(Octree octree, Stats stats, PhaseManager phaseManager, Dictionary<AxialCoordinate, Tile> map)
        {
            this.octree = octree;
            this.stats = stats;
            this.phaseManager = phaseManager;
            this.map = map;

            manaCost = 10;
            dualManaCost = 150;

            castSpeed = 1000;
            dualCastSpeed = 1000;
        }
    }
}
