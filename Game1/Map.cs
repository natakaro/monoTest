using System;
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
    static class Map
    {
        public static bool efekt = false;


        public static List<DrawableObject> CreateMapFromTex(Game game, Texture2D tex, Model inModel, Octree octree)
        {
            float height = 50f;
            float vert = 0.75f * height;
            float width = (float)Math.Sqrt(3) / 2 * height;
            float horiz = width;
            List<DrawableObject> tileList = new List<DrawableObject>();
            Color[] color = Texture2dHelper.GetPixels(tex);

            //Color[] color2 = Texture2dHelper.GetPixels(tex2); //drzewa i inne

            for (int i = 0; i < tex.Width; i++)
            {
                for (int j = 0; j < tex.Height; j++)
                {
                    float temp = Texture2dHelper.GetPixel(color, i, j, tex.Width).R;
                    Vector3 position = new Vector3(i * vert, temp, (j * horiz) + (i % 2) * horiz / 2);


                    tileList.Add(new Tile(game, Matrix.CreateTranslation(position), inModel, octree));

                    //position.Y += 50; // nie wiem czy będzie potrzebne zależy od pivotów

                    /*
                    Color temp2 = Texture2dHelper.GetPixel(color, i, j, tex.Width);
                    if(temp2.R == 255)
                    {
                        tileList.Add(new Asset(game, Matrix.CreateTranslation(position), inModel1));
                    }
                    else if(temp2.B == 255)
                    {
                        tileList.Add(new Asset(game, Matrix.CreateTranslation(position), inModel2));
                    }
                    else if (temp2.G == 255)
                    {
                        tileList.Add(new Asset(game, Matrix.CreateTranslation(position), inModel3));
                    }*/
                }
            }



            return tileList;
        }

        public static List<DrawableObject> CreateMap(Game game, int size, Model inModel, Octree octree)
        {
            float height = 50f;
            float vert = 0.75f * height;
            float width = (float)Math.Sqrt(3) / 2 * height;
            float horiz = width;
            List<DrawableObject> tileList = new List<DrawableObject>();

            Random a = new Random();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Vector3 position = new Vector3(i * vert, a.Next(5), (j * horiz) + (i % 2) * horiz / 2);
                    //mapa[i, j] = new Tile(game, new Vector3(i * vert, a.Next(5), (j * width) + (i % 2) * width / 2) , a.Next(1, 2));
                    //mapa[i, j] = new Tile(game, Matrix.CreateScale(scale) * Matrix.CreateTranslation(position) * worldMatrix, a.Next(1, 2));
                    tileList.Add(new Tile(game, Matrix.CreateTranslation(position), inModel, octree));
                }
            }
            return tileList;
        }
    }
}
