using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Game1
{
    static class Map
    {
        public static float scale = 25f;
        public static bool efekt = false;

        public static List<DrawableObject> CreateMap(Game game, int size, Matrix inWorldMatrix)
        {
            float height = 2 * scale;
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
                    tileList.Add(new Tile(game, Matrix.CreateScale(scale) * Matrix.CreateTranslation(position) * inWorldMatrix));                   
                }
            }
            return tileList;
        }
    }
}
