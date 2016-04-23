using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Spells
{
    public class SpellMoveTerrain
    {
        DrawableObject target;
        public void Cast(bool leftButton, bool rightButton, DrawableObject dObj)
        {
            if (dObj != null)
            {
                if (target != null && target.Type == DrawableObject.ObjectType.Terrain)
                {
                    target.Velocity = Vector3.Zero;
                }

                if (dObj.Type == DrawableObject.ObjectType.Terrain)
                {
                    target = dObj;
                    target.IsStatic = false;

                    if (leftButton == true)
                        target.Velocity = new Vector3(0, 10, 0);
                    else if (rightButton == true)
                        target.Velocity = new Vector3(0, -10, 0);
                    else
                        StopCasting();
                }
            }
        }

        private void StopCasting()
        {
            if (target != null && target.Type == DrawableObject.ObjectType.Terrain)
            {
                target.Velocity = Vector3.Zero;
                target.IsStatic = true;
            }
        }
    }
}
