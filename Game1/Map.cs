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
using static Game1.Helpers.HexCoordinates;
using Game1.Items;
using Game1.Screens;

namespace Game1
{
    static class Map
    {
        public static bool efekt = false;

        public static int MapWidth;
        public static int MapHeight;
        public static float size = 25f;

        public static Dictionary<AxialCoordinate, Tile> CreateMapFromTex(Game game, Texture2D tex, Model inModel, Octree octree)
        {
            float height = 2 * size;
            float vert = 0.75f * height;
            float width = (float)Math.Sqrt(3) / 2 * height;
            float horiz = width;
            Dictionary<AxialCoordinate, Tile> tileDictionary = new Dictionary<AxialCoordinate, Tile>();

            //List<AxialCoordinateH> map = readMapAxial(tex);
            //foreach(AxialCoordinateH coord in map)
            //{
            //
            //    //var position = axialHToPixel(coord, size);
            //    //var position = new Vector3(coord.q * vert, coord.height, (coord.r * horiz) + (coord.q % 2) * horiz / 2);
            //    tileDictionary.Add(coord, new Tile(game, Matrix.CreateTranslation(position), inModel, octree));
            //}

            List<HexOffsetH> map = readMapOffset(tex);
            foreach (HexOffsetH coord in map)
            {
                var axial = coord.oddQ_toCube().ToAxial();
                var position = axialHToPixel(axial, size);
                //var position = axialHToPixel(coord, size);
                //var position = new Vector3(coord.q * vert, coord.height, (coord.r * horiz) + (coord.q % 2) * horiz / 2);
                tileDictionary.Add(axial, new Tile(game, Matrix.CreateTranslation(position), inModel, octree));
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



            return tileDictionary;
        }


        public static Dictionary<AxialCoordinate, DrawableObject> CreateAssetMapFromTex(Game game, Texture2D tex, ContentManager Content, Octree octree, ItemManager itemManager, Vector3 corePosition, PhaseManager phaseManager, AssetContentContainer assetContainer)
        {
            float height = 2 * size;
            float vert = 0.75f * height;
            float width = (float)Math.Sqrt(3) / 2 * height;
            float horiz = width;
            Dictionary<AxialCoordinate, DrawableObject> assetDictionary = new Dictionary<AxialCoordinate, DrawableObject>();
            List<HexOffsetH> map = readMapOffset(tex);
            Random rand = new Random();
            foreach (HexOffsetH coord in map)
            {
                //if (coord.height != 255) { throw new Exception(coord.height.ToString() + " " + coord.x.ToString() + " " + coord.y.ToString()); }
                var axial = coord.oddQ_toCube().ToAxial();
                var position = tileFromAxial(axial, GameplayScreen.map).Position;
                
                float temp = rand.Next(0, 359);
                Matrix worldm = Matrix.CreateRotationY((float)temp) * Matrix.CreateTranslation(position);
                if (coord.height == 10)
                {
                   assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.pinetreeModel, octree, assetContainer.pinetreeTexture));
                }
                else if (coord.height == 20)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.tree1Model, octree, assetContainer.tree1Texture));
                }
                else if (coord.height == 30)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.tree2Model, octree, assetContainer.tree2Texture));
                }
                else if (coord.height == 40)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.treetrunkModel, octree, assetContainer.treetrunkTexture));
                }
                else if (coord.height == 50)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rockModel, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 60)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rock1Model, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 70)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rock2Model, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 80)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rock3Model, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 90)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rock4Model, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 100)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rock5Model, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 110)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rock6Model, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 120)
                {
                    assetDictionary.Add(axial, new Asset(game, worldm, assetContainer.rock7Model, octree, assetContainer.rockTexture));
                }
                else if (coord.height == 130)
                {
                    assetDictionary.Add(axial, new Spawn(game, worldm, assetContainer.spawnModel, octree, itemManager, Content, corePosition, phaseManager, 1));
                }
                else if (coord.height == 140)
                {
                    assetDictionary.Add(axial, new Spawn(game, worldm, assetContainer.spawnModel, octree, itemManager, Content, corePosition, phaseManager, 2));
                }
            }

            return assetDictionary;
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


        public static List<AxialCoordinateH> readMapAxial(Texture2D tex)
        {
            Color[] color = Texture2dHelper.GetPixels(tex);
            MapWidth = tex.Width;
            MapHeight = tex.Height;

            var ret = new List<AxialCoordinateH>();

            for (int i = 0; i < tex.Width; i++)
            {
                for (int j = 0; j < tex.Height; j++)
                {
                    float height = Texture2dHelper.GetPixel(color, i, j, tex.Width).R;
                    AxialCoordinateH coord;
                    coord.q = i;
                    coord.r = j;
                    coord.height = height;
                    ret.Add(coord);
                }
            }
            return ret;
        }

        public static List<HexOffsetH> readMapOffset(Texture2D tex)
        {
            Color[] color = Texture2dHelper.GetPixels(tex);
            MapWidth = tex.Width;
            MapHeight = tex.Height;

            var ret = new List<HexOffsetH>();

            for (int i = 0; i < tex.Width; i++)
            {
                for (int j = 0; j < tex.Height; j++)
                {
                    float height = Texture2dHelper.GetPixel(color, i, j, tex.Width).R;
                    HexOffsetH coord;
                    coord.x = i;
                    coord.y = j;
                    coord.height = height;
                    ret.Add(coord);
                }
            }
            return ret;
        }



        //public static Vector3 axialToPixel(AxialCoordinate axial, float size)
        //{
        //Vector3 ret;

        ////ret.X = size * (float)Math.Sqrt(3) * (axial.q + axial.r / 2);
        ////ret.Z = size * 3 / 2 * axial.r;

        //ret.X = size * 3 / 2 * axial.q;
        //ret.Z = size * (float)Math.Sqrt(3) * (axial.r + axial.q / 2);

        //ret.Y = axial.height;

        //return ret;
        //}

        //public static AxialCoordinate pixelToAxial(Vector3 pixel, float size)
        //{
        //    AxialCoordinate ret;

        //    //ret.q = (pixel.X * (float)Math.Sqrt(3) / 3 - pixel.Z / 3) / size;
        //    //ret.r = pixel.Z * 2 / 3 / size;

        //    ret.q = pixel.X * 2 / 3 / size;
        //    ret.r = (-pixel.X / 3 + (float)Math.Sqrt(3) / 3 * pixel.Z) / size;

        //    return ret;
        //}
    }
}
