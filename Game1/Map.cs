﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Game1.Helpers;

namespace Game1
{
    public struct AxialCoordinate
    {
        public float q;
        public float r;
        public float height;

        public CubeCoordinate ToCube()
        {
            CubeCoordinate ret;

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

        public Vector3 ToOddROffset()
        {
            Vector3 ret;

            ret.X = q + (r - (r % 2)) / 2;
            ret.Z = r;
            ret.Y = height;

            return ret;
        }
    };

    public struct CubeCoordinate
    {
        public float x;
        public float y;
        public float z;
        public float height;

        public CubeCoordinate(float x, float y, float z, float height)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.height = height;
        }

        public AxialCoordinate ToAxial()
        {
            AxialCoordinate ret;

            ret.q = x;
            ret.r = z;

            ret.height = height;

            return ret;
        }

        public Vector3 ToOddROffset()
        {
            Vector3 ret;

            ret.X = x + (z - (z % 2)) / 2;
            ret.Z = z;
            ret.Y = height;

            return ret;
        }
    }

    static class Map
    {
        public static bool efekt = false;

        public static int MapWidth;
        public static int MapHeight;
        public static float size = 25f;

        public static List<DrawableObject> CreateMapFromTex(Game game, Texture2D tex, Model inModel, Octree octree)
        {
            float height = 2 * size;
            float vert = 0.75f * height;
            float width = (float)Math.Sqrt(3) / 2 * height;
            float horiz = width;
            List<DrawableObject> tileList = new List<DrawableObject>();

            List<AxialCoordinate> map = readMapAxial(tex);

            foreach(AxialCoordinate coord in map)
            {
                var offset = coord.ToOddROffset();
                //var position = hexToPixel(coord, size);
                var position = new Vector3(coord.q * vert, coord.height, (coord.r * horiz) + (coord.q % 2) * horiz / 2);
                tileList.Add(new Tile(game, Matrix.CreateTranslation(position), inModel, octree));
            }

            //Color[] color = Texture2dHelper.GetPixels(tex);

            ////Color[] color2 = Texture2dHelper.GetPixels(tex2); //drzewa i inne
            //MapWidth = tex.Width;
            //MapHeight = tex.Height;

            //for (int i = 0; i < tex.Width; i++)
            //{
            //    for (int j = 0; j < tex.Height; j++)
            //    {
            //        float temp = Texture2dHelper.GetPixel(color, i, j, tex.Width).R;
            //        Vector3 position = new Vector3(i * vert, temp, (j * horiz) + (i % 2) * horiz / 2);


            //        tileList.Add(new Tile(game, Matrix.CreateTranslation(position), inModel, octree));

            //        //position.Y += 50; // nie wiem czy będzie potrzebne zależy od pivotów

            //        /*
            //        Color temp2 = Texture2dHelper.GetPixel(color, i, j, tex.Width);
            //        if(temp2.R == 255)
            //        {
            //            tileList.Add(new Asset(game, Matrix.CreateTranslation(position), inModel1));
            //        }
            //        else if(temp2.B == 255)
            //        {
            //            tileList.Add(new Asset(game, Matrix.CreateTranslation(position), inModel2));
            //        }
            //        else if (temp2.G == 255)
            //        {
            //            tileList.Add(new Asset(game, Matrix.CreateTranslation(position), inModel3));
            //        }*/
            //    }
            //}



            return tileList;
        }

        //public static List<DrawableObject> CreateMap(Game game, int size, Model inModel, Octree octree)
        //{
        //    float height = 50f;
        //    float vert = 0.75f * height;
        //    float width = (float)Math.Sqrt(3) / 2 * height;
        //    float horiz = width;
        //    List<DrawableObject> tileList = new List<DrawableObject>();

        //    Random a = new Random();

        //    for (int i = 0; i < size; i++)
        //    {
        //        for (int j = 0; j < size; j++)
        //        {
        //            Vector3 position = new Vector3(i * vert, a.Next(5), (j * horiz) + (i % 2) * horiz / 2);
        //            //mapa[i, j] = new Tile(game, new Vector3(i * vert, a.Next(5), (j * width) + (i % 2) * width / 2) , a.Next(1, 2));
        //            //mapa[i, j] = new Tile(game, Matrix.CreateScale(scale) * Matrix.CreateTranslation(position) * worldMatrix, a.Next(1, 2));
        //            tileList.Add(new Tile(game, Matrix.CreateTranslation(position), inModel, octree));
        //        }
        //    }
        //    return tileList;
        //}

        
        public static List<AxialCoordinate> readMapAxial(Texture2D tex)
        {
            Color[] color = Texture2dHelper.GetPixels(tex);
            MapWidth = tex.Width;
            MapHeight = tex.Height;

            var ret = new List<AxialCoordinate>();

            for (int i = 0; i < tex.Width; i++)
            {
                for (int j = 0; j < tex.Height; j++)
                {
                    float height = Texture2dHelper.GetPixel(color, i, j, tex.Width).R;
                    AxialCoordinate coord;
                    coord.q = i;
                    coord.r = j;
                    coord.height = height;
                    ret.Add(coord);
                }
            }
            return ret;
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

            float x = col - (row - (row%2)) / 2;
            float z = row;
            float y = -x - z;

            return new Vector3(x, y, z);
        }

        public static Vector3 hexToPixel(AxialCoordinate axial, float size)
        {
            Vector3 ret;

            //ret.X = size * (float)Math.Sqrt(3) * (axial.q + axial.r / 2);
            //ret.Z = size * 3 / 2 * axial.r;

            ret.X = size * 3 / 2 * axial.q;
            ret.Z = size * (float)Math.Sqrt(3) * (axial.r + axial.q / 2);
            
            ret.Y = axial.height;

            return ret;
        }

        public static AxialCoordinate pixelToHex(Vector3 pixel, float size)
        {
            AxialCoordinate ret;

            //ret.q = (pixel.X * (float)Math.Sqrt(3) / 3 - pixel.Z / 3) / size;
            //ret.r = pixel.Z * 2 / 3 / size;

            ret.q = pixel.X * 2 / 3 / size;
            ret.r = (-pixel.X / 3 + (float)Math.Sqrt(3) / 3 * pixel.Z) / size;
            
            ret.height = pixel.Y;
            
            return ret;
        }
    }
}
