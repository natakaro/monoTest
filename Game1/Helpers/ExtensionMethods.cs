using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public static class ExtensionMethods
    {
        public static bool FastIntersectTest(this BoundingFrustum frustum, ref BoundingSphere sphere)
        {

            Vector3 normal; Vector3 p = sphere.Center;
            float radius = sphere.Radius;

            normal = frustum.Near.Normal;
            if (frustum.Near.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > radius)
                return false;
            normal = frustum.Left.Normal;
            if (frustum.Left.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > radius)
                return false;
            normal = frustum.Right.Normal;
            if (frustum.Right.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > radius)
                return false;
            normal = frustum.Bottom.Normal;
            if (frustum.Bottom.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > radius)
                return false;
            normal = frustum.Top.Normal;
            if (frustum.Top.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > radius)
                return false;

            /* Can ignore far plane when distant object culling is handled by another mechanism
            normal = frustum.Far.Normal;
            if(frustum.Far.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > radius)
                return false;
            */

            return true;

        }

        public static bool FastIntersectTest(this BoundingFrustum frustum, ref BoundingBox box)
        {

            Vector3 normal, p;

            normal = frustum.Near.Normal;
            p.X = (normal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (normal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (normal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (frustum.Near.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > 0)
                return false;

            normal = frustum.Left.Normal;
            p.X = (normal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (normal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (normal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (frustum.Left.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > 0)
                return false;

            normal = frustum.Right.Normal;
            p.X = (normal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (normal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (normal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (frustum.Right.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > 0)
                return false;

            normal = frustum.Bottom.Normal;
            p.X = (normal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (normal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (normal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (frustum.Bottom.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > 0)
                return false;

            normal = frustum.Top.Normal;
            p.X = (normal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (normal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (normal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (frustum.Top.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > 0)
                return false;

            /* Can ignore far plane when distant object culling is handled by another mechanism
            normal = frustum.Far.Normal;
            p.X = (normal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (normal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (normal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if(frustum.Far.D + (normal.X * p.X) + (normal.Y * p.Y) + (normal.Z * p.Z) > 0)
                return false;
             */

            return true;

        }
    }
}
