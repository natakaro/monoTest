using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Spells
{
    public class SpellMoveTerrain
    {
        Stopwatch stopwatch = new Stopwatch();
        DrawableObject target;

        BoundingSphere areaSphere;
        Octree octree;
        List<IntersectionRecord> sphere_list;
        float average_y;

        public void Start(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (dObj != null)
            {
                if (target != null && target.Type == DrawableObject.ObjectType.Terrain) //fix jesli uzywajac na jednym klikniemy na drugim
                {
                    target.Acceleration = Vector3.Zero;
                    target.Velocity = Vector3.Zero;
                }

                if (leftButton && rightButton)  //wersja obszarowa, wyrownywanie terenu
                {
                    if (dObj.Type == DrawableObject.ObjectType.Terrain)
                    {
                        if (target != null)
                        {
                            target.Acceleration = Vector3.Zero;
                            target.Velocity = Vector3.Zero;
                        }

                        //tworzymy sphere duza wokol celu, zbieramy kolizje wszystkich wokol i wyliczyamy srednia pozycje na y
                        areaSphere = new BoundingSphere(dObj.Position, Map.scale * 2);
                        sphere_list = octree.AllIntersections(areaSphere, DrawableObject.ObjectType.Terrain);
                        average_y = 0;

                        foreach (IntersectionRecord ir in sphere_list)
                        {
                            ir.DrawableObjectObject.IsStatic = false;
                            average_y += ir.DrawableObjectObject.Position.Y;
                        }
                        average_y = average_y / sphere_list.Count;
                    }
                }

                else if (leftButton || rightButton)
                {
                    if (dObj.Type == DrawableObject.ObjectType.Terrain)
                    {
                        target = dObj;
                        target.IsStatic = false;
                        stopwatch.Start();
                    }
                }
            }
        }

        public void Continue(bool leftButton, bool rightButton)
        {
            if (leftButton == true && rightButton == true && sphere_list != null) //w wersji obszarowej poruszamy kazdym tilem w odpowiednim kierunku z usredniona predkoscia
            {
                foreach (IntersectionRecord ir in sphere_list)
                {
                    if (ir.DrawableObjectObject.Position.Y == average_y)
                    {
                        ir.DrawableObjectObject.Velocity = Vector3.Zero;  //jesli osiagnelismy cel to stop
                    }
                    else if (ir.DrawableObjectObject.Position.Y > average_y)
                        ir.DrawableObjectObject.Velocity = new Vector3(0, average_y - ir.DrawableObjectObject.Position.Y - 0.1f, 0); //dzieki temu kazdy tile trafia na miejsce w dokladnie tym samym momencie
                    else if (ir.DrawableObjectObject.Position.Y < average_y)
                        ir.DrawableObjectObject.Velocity = new Vector3(0, average_y - ir.DrawableObjectObject.Position.Y + 0.1f, 0); //dodanie wyzej lub tutaj odjecie 0.1f przyspiesza zblizenie sie do celu, bo predkosc maleje wraz z odlegloscia
                }
            }
            else if (target != null)
            {  
                if (stopwatch.ElapsedMilliseconds > 100) //zwiekszanie predkosci + zabawa timerem
                {
                    if (leftButton == true)
                        target.Acceleration += new Vector3(0, 1, 0);
                    else if (rightButton == true)
                        target.Acceleration += new Vector3(0, -1, 0);
                    stopwatch.Restart();
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
            }
        }

        public SpellMoveTerrain(Octree octree)
        {
            this.octree = octree;
        }
    }
}
