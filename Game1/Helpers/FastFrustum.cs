using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public struct FastFrustum
    {

        private readonly Vector3 NearNormal, LeftNormal, RightNormal, BottomNormal, TopNormal;
        private readonly float NearD, LeftD, RightD, BottomD, TopD;

        public FastFrustum(BoundingFrustum source)
        {

            NearNormal = source.Near.Normal; NearD = source.Near.D;
            LeftNormal = source.Left.Normal; LeftD = source.Left.D;
            RightNormal = source.Right.Normal; RightD = source.Right.D;
            BottomNormal = source.Bottom.Normal; BottomD = source.Bottom.D;
            TopNormal = source.Top.Normal; TopD = source.Top.D;
            /* FarNormal    = source.Far.Normal;    FarD    = source.Far.D; */

        }

        // Camera matrix is view * projection 
        public FastFrustum(ref Matrix cameraMatrix)
        {

            float x = -cameraMatrix.M14 - cameraMatrix.M11;
            float y = -cameraMatrix.M24 - cameraMatrix.M21;
            float z = -cameraMatrix.M34 - cameraMatrix.M31;
            float scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
            LeftNormal = new Vector3(x * scale, y * scale, z * scale);
            LeftD = (-cameraMatrix.M44 - cameraMatrix.M41) * scale;

            x = -cameraMatrix.M14 + cameraMatrix.M11;
            y = -cameraMatrix.M24 + cameraMatrix.M21;
            z = -cameraMatrix.M34 + cameraMatrix.M31;
            scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
            RightNormal = new Vector3(x * scale, y * scale, z * scale);
            RightD = (-cameraMatrix.M44 + cameraMatrix.M41) * scale;

            x = -cameraMatrix.M14 + cameraMatrix.M12;
            y = -cameraMatrix.M24 + cameraMatrix.M22;
            z = -cameraMatrix.M34 + cameraMatrix.M32;
            scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
            TopNormal = new Vector3(x * scale, y * scale, z * scale);
            TopD = (-cameraMatrix.M44 + cameraMatrix.M42) * scale;

            x = -cameraMatrix.M14 - cameraMatrix.M12;
            y = -cameraMatrix.M24 - cameraMatrix.M22;
            z = -cameraMatrix.M34 - cameraMatrix.M32;
            scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
            BottomNormal = new Vector3(x * scale, y * scale, z * scale);
            BottomD = (-cameraMatrix.M44 - cameraMatrix.M42) * scale;

            x = -cameraMatrix.M13;
            y = -cameraMatrix.M23;
            z = -cameraMatrix.M33;
            scale = 1.0f / ((float)Math.Sqrt((x * x) + (y * y) + (z * z)));
            NearNormal = new Vector3(x * scale, y * scale, z * scale);
            NearD = (-cameraMatrix.M43) * scale;

            /*z = -cameraMatrix.M14 + cameraMatrix.M13;
              y = -cameraMatrix.M24 + cameraMatrix.M23;
              z = -cameraMatrix.M34 + cameraMatrix.M33;
              scale = 1.0f / ((float) Math.Sqrt((x * x) + (y * y) + (z * z)));
              FarNormal = new Vector3(x * scale, y * scale, z * scale);
              FarD      = (-cameraMatrix.M44 + cameraMatrix.M43) * scale;*/

        }

        public bool Intersects(ref BoundingSphere sphere)
        {

            Vector3 p = sphere.Center; float radius = sphere.Radius;

            if (NearD + (NearNormal.X * p.X) + (NearNormal.Y * p.Y) + (NearNormal.Z * p.Z) > radius) return false;
            if (LeftD + (LeftNormal.X * p.X) + (LeftNormal.Y * p.Y) + (LeftNormal.Z * p.Z) > radius) return false;
            if (RightD + (RightNormal.X * p.X) + (RightNormal.Y * p.Y) + (RightNormal.Z * p.Z) > radius) return false;
            if (BottomD + (BottomNormal.X * p.X) + (BottomNormal.Y * p.Y) + (BottomNormal.Z * p.Z) > radius) return false;
            if (TopD + (TopNormal.X * p.X) + (TopNormal.Y * p.Y) + (TopNormal.Z * p.Z) > radius) return false;

            /* Can ignore far plane when distant object culling is handled by another mechanism
            if(FarD + (FarNormal.X * p.X) + (FarNormal.Y * p.Y) + (FarNormal.Z * p.Z) > radius)             return false;
            */
            return true;

        }

        public bool Intersects(ref BoundingBox box)
        {

            Vector3 p;

            p.X = (NearNormal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (NearNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (NearNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (NearD + (NearNormal.X * p.X) + (NearNormal.Y * p.Y) + (NearNormal.Z * p.Z) > 0) return false;

            p.X = (LeftNormal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (LeftNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (LeftNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (LeftD + (LeftNormal.X * p.X) + (LeftNormal.Y * p.Y) + (LeftNormal.Z * p.Z) > 0) return false;

            p.X = (RightNormal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (RightNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (RightNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (RightD + (RightNormal.X * p.X) + (RightNormal.Y * p.Y) + (RightNormal.Z * p.Z) > 0) return false;

            p.X = (BottomNormal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (BottomNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (BottomNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (BottomD + (BottomNormal.X * p.X) + (BottomNormal.Y * p.Y) + (BottomNormal.Z * p.Z) > 0) return false;

            p.X = (TopNormal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (TopNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (TopNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if (TopD + (TopNormal.X * p.X) + (TopNormal.Y * p.Y) + (TopNormal.Z * p.Z) > 0) return false;

            /* Can ignore far plane when distant object culling is handled by another mechanism
            p.X = (FarNormal.X >= 0 ? box.Min.X : box.Max.X);
            p.Y = (FarNormal.Y >= 0 ? box.Min.Y : box.Max.Y);
            p.Z = (FarNormal.Z >= 0 ? box.Min.Z : box.Max.Z);
            if(FarD + (FarNormal.X * p.X) + (FarNormal.Y * p.Y) + (FarNormal.Z * p.Z) > 0) return false;
             */

            return true;

        }

    }
}
