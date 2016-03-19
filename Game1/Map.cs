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
        public Tile[,] mapa;
        //float skala = 1f;
        float szerokosc;
        float wysokosc;
        float odleglosc;

        public void Initialize(ContentManager contentManager)
        {
            foreach (Tile tile in mapa)
            {
                tile.Initialize(contentManager);
            }
        }
        public Map(Game1 game, int size) : base(game)
        {
            szerokosc = 2;   // * skala;
            wysokosc = (float)Math.Sqrt(3) / 2 * szerokosc;
            odleglosc = 0.75f * szerokosc;
            mapa = new Tile[size, size];
            Random a = new Random();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    mapa[i, j] = new Tile(game, new Vector3(i * odleglosc, 0, (j * wysokosc) + (i % 2) * wysokosc / 2) , a.Next(1, 4));
                }
            }

            
        }

        public void Draw(Camera camera)
        {
            foreach (Tile tile in mapa)
            {
                tile.Draw(camera);
            }
        }
    }
}
