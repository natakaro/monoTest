using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public static class HexCoordinates
    {
        public struct HexOffsetH
        {
            public float x;
            public float y;
            public float height;

            public HexOffsetH(float x, float y, float height)
            {
                this.x = x;
                this.y = y;
                this.height = height;
            }

            public CubeCoordinateH evenQ_toCube()
            {
                CubeCoordinateH ret;

                ret.x = x;
                ret.z = y - (x + (x % 2)) / 2;
                ret.y = -ret.x - ret.z;

                ret.height = height;

                return ret;
            }
            public CubeCoordinateH oddQ_toCube()
            {
                CubeCoordinateH ret;

                ret.x = x;
                ret.z = y - (x - (x % 2)) / 2;
                ret.y = -ret.x - ret.z;

                ret.height = height;

                return ret;
            }
            public CubeCoordinateH evenR_toCube()
            {
                CubeCoordinateH ret;

                ret.x = x - (y + (y % 2)) / 2;
                ret.z = y;
                ret.y = -ret.x - ret.z;

                ret.height = height;

                return ret;
            }
            public CubeCoordinateH oddR_toCube()
            {
                CubeCoordinateH ret;

                ret.x = x - (y - (y % 2)) / 2;
                ret.z = y;
                ret.y = -ret.x - ret.z;

                ret.height = height;

                return ret;
            }
        }
        public struct AxialCoordinate
        {
            public float q;
            public float r;

            public AxialCoordinate(float q, float r)
            {
                this.q = q;
                this.r = r;
            }

            public CubeCoordinate ToCube()
            {
                CubeCoordinate ret;

                ret.x = q;
                ret.z = r;
                ret.y = -ret.x - ret.z;

                return ret;
            }

            public Vector2 ToVector2()
            {
                return new Vector2(q, r);
            }
        };

        public struct CubeCoordinate
        {
            public float x;
            public float y;
            public float z;

            public CubeCoordinate(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public AxialCoordinate ToAxial()
            {
                AxialCoordinate ret;

                ret.q = x;
                ret.r = z;

                return ret;
            }

            public Vector2 to_evenQ_Offset()
            {
                Vector2 ret;

                ret.X = x;
                ret.Y = z + (x + (x % 2)) / 2;

                return ret;
            }

            public Vector2 to_oddQ_Offset()
            {
                Vector2 ret;

                ret.X = x;
                ret.Y = z + (x - (x % 2)) / 2;

                return ret;
            }

            public Vector2 to_evenR_Offset()
            {
                Vector2 ret;

                ret.X = x + (z + (z % 2)) / 2;
                ret.Y = z;

                return ret;
            }

            public Vector2 to_oddR_Offset()
            {
                Vector2 ret;

                ret.X = x + (z - (z % 2)) / 2;
                ret.Y = z;

                return ret;
            }

            public static CubeCoordinate operator +(CubeCoordinate a, CubeCoordinate b)
            {
                return new CubeCoordinate(a.x + b.x, a.y + b.y, a.z + b.z);
            }
        }

        public struct AxialCoordinateH
        {
            public float q;
            public float r;
            public float height;

            public AxialCoordinateH(float q, float r, float height)
            {
                this.q = q;
                this.r = r;

                this.height = height;
            }

            public CubeCoordinateH ToCube()
            {
                CubeCoordinateH ret;

                ret.x = q;
                ret.z = r;
                ret.y = -ret.x - ret.z;

                ret.height = height;

                return ret;
            }

            public Vector2 ToVector2()
            {
                return new Vector2(q, r);
            }

            public Vector3 ToVector3()
            {
                return new Vector3(q, height, r);
            }

            public static implicit operator AxialCoordinate(AxialCoordinateH value)
            {
                return new AxialCoordinate(value.q, value.r);
            }
        };

        public struct CubeCoordinateH
        {
            public float x;
            public float y;
            public float z;
            public float height;

            public CubeCoordinateH(float x, float y, float z, float height)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.height = height;
            }

            public AxialCoordinateH ToAxial()
            {
                AxialCoordinateH ret;

                ret.q = x;
                ret.r = z;

                ret.height = height;

                return ret;
            }

            public HexOffsetH to_evenQ_Offset()
            {
                HexOffsetH ret;

                ret.x = x;
                ret.y = z + (x + (x % 2)) / 2;

                ret.height = height;

                return ret;
            }

            public HexOffsetH to_oddQ_Offset()
            {
                HexOffsetH ret;

                ret.x = x;
                ret.y = z + (x - (x % 2)) / 2;

                ret.height = height;

                return ret;
            }

            public HexOffsetH to_evenR_Offset()
            {
                HexOffsetH ret;

                ret.x = x + (z + (z % 2)) / 2;
                ret.y = z;

                ret.height = height;

                return ret;
            }

            public HexOffsetH to_oddR_Offset()
            {
                HexOffsetH ret;

                ret.x = x + (z - (z % 2)) / 2;
                ret.y = z;

                ret.height = height;

                return ret;
            }

            public static implicit operator CubeCoordinate(CubeCoordinateH value)
            {
                return new CubeCoordinate(value.x, value.y, value.z);
            }
        }

        public static Tile tileFromAxial(AxialCoordinate axial, Dictionary<AxialCoordinate, Tile> dictionary)
        {
            Tile tile;
            dictionary.TryGetValue(axial, out tile);

            return tile;
        }

        public static Vector3 WorldToCube(Tile tile)
        {
            Vector3 tileWorldPosition = tile.Position;
            float x = tileWorldPosition.X;


            return tile.Position;
        }

        public static Vector3 evenRToCube(Vector2 value)
        {
            float col = value.X;
            float row = value.Y;

            float x = col - (row + (row % 2)) / 2;
            float z = row;
            float y = -x - z;

            return new Vector3(x, y, z);
        }


        public static Vector3 oddRToCube(Vector2 value)
        {
            float col = value.X;
            float row = value.Y;

            float x = col - (row - (row % 2)) / 2;
            float z = row;
            float y = -x - z;

            return new Vector3(x, y, z);
        }

        public static Vector3 axialHToPixel(AxialCoordinateH axial, float size)
        {
            Vector3 ret;

            //ret.X = size * (float)Math.Sqrt(3) * (axial.q + axial.r / 2);
            //ret.Z = size * 3 / 2 * axial.r;

            ret.X = size * 3 / 2 * axial.q;
            ret.Z = size * (float)Math.Sqrt(3) * (axial.r + axial.q / 2);

            ret.Y = axial.height;

            return ret;
        }

        public static AxialCoordinateH pixelToAxialH(Vector3 pixel, float size)
        {
            AxialCoordinateH ret;

            //ret.q = (pixel.X * (float)Math.Sqrt(3) / 3 - pixel.Z / 3) / size;
            //ret.r = pixel.Z * 2 / 3 / size;

            ret.q = pixel.X * 2 / 3 / size;
            ret.r = (-pixel.X / 3 + (float)Math.Sqrt(3) / 3 * pixel.Z) / size;

            ret.height = pixel.Y;

            return ret;
        }



        public static float CubeDistance(CubeCoordinateH a, CubeCoordinateH b)
        {
            //return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;
            return Math.Max(Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y)), Math.Abs(a.z - b.z));
        }

        public static List<CubeCoordinateH> CubeLerp(CubeCoordinateH a, CubeCoordinateH b)
        {
            var ret = new List<CubeCoordinateH>();
            var distance = CubeDistance(a, b);

            int steps = (int)distance;

            for (int i = 1; i <= steps; i++)
            {
                float amount = 1.0f / steps * i;
                ret.Add(CubeRound(new CubeCoordinateH(MathHelper.Lerp(a.x, b.x, amount), MathHelper.Lerp(a.y, b.y, amount), MathHelper.Lerp(a.z, b.z, amount), MathHelper.Lerp(a.height, b.height, amount))));
            }

            return ret;
        }

        public static CubeCoordinateH CubeRound(CubeCoordinateH cube)
        {
            CubeCoordinateH ret;

            float rx = (float)Math.Round(cube.x);
            float ry = (float)Math.Round(cube.y);
            float rz = (float)Math.Round(cube.z);

            float x_diff = Math.Abs(rx - cube.x);
            float y_diff = Math.Abs(ry - cube.y);
            float z_diff = Math.Abs(rz - cube.z);

            if (x_diff > y_diff && x_diff > z_diff)
                rx = -ry - rz;
            else if (y_diff > z_diff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            ret.x = rx;
            ret.y = ry;
            ret.z = rz;

            ret.height = cube.height;

            return ret;
        }

        public static CubeCoordinate CubeRound(CubeCoordinate cube)
        {
            CubeCoordinate ret;

            float rx = (float)Math.Round(cube.x);
            float ry = (float)Math.Round(cube.y);
            float rz = (float)Math.Round(cube.z);

            float x_diff = Math.Abs(rx - cube.x);
            float y_diff = Math.Abs(ry - cube.y);
            float z_diff = Math.Abs(rz - cube.z);

            if (x_diff > y_diff && x_diff > z_diff)
                rx = -ry - rz;
            else if (y_diff > z_diff)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            ret.x = rx;
            ret.y = ry;
            ret.z = rz;

            return ret;
        }

        public static Tile tileFromPosition(Vector3 position, Dictionary<AxialCoordinate, Tile> map)
        {
            return tileFromAxial(CubeRound(pixelToAxialH(position, Map.size).ToCube()).ToAxial(), map);
        }
    }
}
