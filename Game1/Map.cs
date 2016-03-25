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
    class Map : GameComponent
    {
        private Matrix worldMatrix;
        private Tile[,] mapa;
        private List<DrawableObject> tileList;

        public static float scale = 25f;
        float height;
        float width;
        float vert;
        float horiz;
        public static bool efekt = false;

        public void Initialize(ContentManager contentManager)
        {
            foreach (Tile tile in mapa)
            {
                tile.Initialize(contentManager);
            }
        }
        public Map(Game1 game, int size, Matrix inWorldMatrix) : base(game)
        {
            worldMatrix = inWorldMatrix;
            height = 2 * scale;
            vert = 0.75f * height;
            width = (float)Math.Sqrt(3) / 2 * height;
            horiz = width;

            mapa = new Tile[size, size];
            tileList = new List<DrawableObject>();

            Random a = new Random();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Vector3 position = new Vector3(i * vert, a.Next(5), (j * horiz) + (i % 2) * horiz / 2);
                    //mapa[i, j] = new Tile(game, new Vector3(i * vert, a.Next(5), (j * width) + (i % 2) * width / 2) , a.Next(1, 2));
                    mapa[i, j] = new Tile(game, Matrix.CreateScale(scale) * Matrix.CreateTranslation(position) * worldMatrix, a.Next(1, 2));
                    tileList.Add(mapa[i, j]);
                }
            }
        }

        public void reload()
        {
            foreach (Tile tile in mapa)
            {
                tile.reload();
            }
        }

        public void Draw(Camera camera)
        {
            if (efekt)
            {
                foreach (Tile tile in mapa)
                {
                    tile.DrawEffect(camera);
                }
            }
            else
            {
                foreach (Tile tile in mapa)
                {
                    tile.Draw(camera);
                }
            }
        }

        #region Properties
        public Tile[,] Mapa
        {
            get { return mapa; }
        }
        public List<DrawableObject> TileList
        {
            get { return tileList; }
        }
        #endregion
    }
}
