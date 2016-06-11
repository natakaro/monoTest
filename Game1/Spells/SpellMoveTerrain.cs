using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Helpers;

namespace Game1.Spells
{
    public class SpellMoveTerrain
    {
        
        private DrawableObject target; 
        private BoundingSphere areaSphere;
        private Octree octree;
        private List<IntersectionRecord> sphere_list;
        private float average_y;
        private Stats stats;

        private Stopwatch stopwatch = new Stopwatch();

        private float manaCost;
        private float dualManaCost;

        private float castSpeed;
        private float dualCastSpeed;

        private bool spellStarted;
        private bool spellReady;
        private float startingMana;
        private float manaDeducted;

        public void Start(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (dObj != null)
            {
                startingMana = stats.currentMana;
                manaDeducted = 0;

                if (target != null && target.Type == DrawableObject.ObjectType.Terrain) //fix jesli uzywajac na jednym klikniemy na drugim
                {
                    target.Acceleration = Vector3.Zero;
                    target.Velocity = Vector3.Zero;
                }

                if (leftButton && rightButton)  //wersja obszarowa, wyrownywanie terenu
                {
                    if (startingMana >= dualManaCost)
                    {
                        if (dObj.Type == DrawableObject.ObjectType.Terrain)
                        {
                            if (target != null)
                            {
                                target.Acceleration = Vector3.Zero;
                                target.Velocity = Vector3.Zero;
                            }

                            spellStarted = true;
                            //tworzymy sphere duza wokol celu, zbieramy kolizje wszystkich wokol i wyliczyamy srednia pozycje na y
                            areaSphere = new BoundingSphere(dObj.Position, 40f);
                            sphere_list = octree.AllIntersections(areaSphere, DrawableObject.ObjectType.Terrain);
                            average_y = 0;

                            foreach (IntersectionRecord ir in sphere_list)
                            {
                                ir.DrawableObjectObject.IsStatic = false;
                                average_y += ir.DrawableObjectObject.Position.Y;
                            }
                            average_y = average_y / sphere_list.Count;
                            stopwatch.Start();
                        }
                    }
                }

                else if ((leftButton || rightButton) && startingMana >= manaCost)
                {
                    if (dObj.Type == DrawableObject.ObjectType.Terrain)
                    {
                        spellStarted = true;

                        target = dObj;
                        target.IsStatic = false;
                        if(leftButton)
                            target.Acceleration = new Vector3(0, 10, 0);
                        else
                            target.Acceleration = new Vector3(0, -10, 0);
                        stopwatch.Start();
                    }
                }
            }
        }

        public void Continue(bool leftButton, bool rightButton)
        {
            if (spellStarted == true)
            {
                if (leftButton == true && rightButton == true && sphere_list != null) //w wersji obszarowej poruszamy kazdym tilem w odpowiednim kierunku z usredniona predkoscia
                {
                    //DebugShapeRenderer.AddBoundingSphere(areaSphere, Color.White);
                    foreach (IntersectionRecord ir in sphere_list)
                    {
                        //if (ir.DrawableObjectObject.Position.Y == average_y)
                        //{
                        //    ir.DrawableObjectObject.Velocity = Vector3.Zero;  //jesli osiagnelismy cel to stop
                        //}
                        //else if (ir.DrawableObjectObject.Position.Y > average_y)
                        //    ir.DrawableObjectObject.Velocity = new Vector3(0, average_y - ir.DrawableObjectObject.Position.Y - 0.1f, 0); //dzieki temu kazdy tile trafia na miejsce w dokladnie tym samym momencie
                        //else if (ir.DrawableObjectObject.Position.Y < average_y)
                        //    ir.DrawableObjectObject.Velocity = new Vector3(0, average_y - ir.DrawableObjectObject.Position.Y + 0.1f, 0); //dodanie wyzej lub tutaj odjecie 0.1f przyspiesza zblizenie sie do celu, bo predkosc maleje wraz z odlegloscia

                        //if (stopwatch.ElapsedMilliseconds < dualCastSpeed)
                        if (ir.DrawableObjectObject.Position.Y != average_y)
                        {
                            manaDeducted = Math.Min((stopwatch.ElapsedMilliseconds / dualCastSpeed) * dualManaCost, dualManaCost);
                            stats.currentMana = startingMana - manaDeducted;
                            ir.DrawableObjectObject.Velocity = new Vector3(0, MathHelper.Lerp(0, average_y - ir.DrawableObjectObject.Position.Y, stopwatch.ElapsedMilliseconds / dualCastSpeed), 0);
                        }
                        else
                        {
                            ir.DrawableObjectObject.Velocity = Vector3.Zero;
                        }
                    }
                }
                else if (target != null)
                {
                    if (stats.currentMana > 0)
                    {
                        manaDeducted = (stopwatch.ElapsedMilliseconds / castSpeed) * manaCost;
                        stats.currentMana = startingMana - manaDeducted;

                        if (stopwatch.ElapsedMilliseconds > castSpeed) //zwiekszanie predkosci + zabawa timerem
                        {
                            if (leftButton == true)
                                target.Acceleration += new Vector3(0, 1, 0);
                            else if (rightButton == true)
                                target.Acceleration += new Vector3(0, -1, 0);
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

            if (sphere_list != null && sphere_list.Count != 0)
            {
                foreach (IntersectionRecord ir in sphere_list)
                {
                    ir.DrawableObjectObject.Acceleration = Vector3.Zero;
                    ir.DrawableObjectObject.Velocity = Vector3.Zero;
                    ir.DrawableObjectObject.IsStatic = true;
                }
                sphere_list.Clear();
                stopwatch.Reset();
            }
            spellStarted = false;
        }

        public SpellMoveTerrain(Octree octree, Stats stats)
        {
            this.octree = octree;
            this.stats = stats;

            manaCost = 10;
            dualManaCost = 150;

            castSpeed = 1000;
            dualCastSpeed = 1000;
        }
    }
}
