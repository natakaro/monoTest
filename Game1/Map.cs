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
using Game1.Turrets;

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
            int i = 0;
            foreach (HexOffsetH coord in map)
            {
                //if (coord.height != 255) { throw new Exception(coord.height.ToString() + " " + coord.x.ToString() + " " + coord.y.ToString()); }
                var axial = coord.oddQ_toCube().ToAxial();
                var tile = tileFromAxial(axial, GameplayScreen.map);
                var position = tile.Position;
                
                float temp = Game1.random.Next(0, 359);
                Matrix worldm = Matrix.CreateRotationY(MathHelper.ToRadians(temp)) * Matrix.CreateTranslation(position);
                int id = (int)coord.height;

                if (id == 10)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.pinetreeModel, octree, assetContainer.pinetreeTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 20)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.tree1Model, octree, assetContainer.tree1Texture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 30)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.tree2Model, octree, assetContainer.tree2Texture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 40)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.treetrunkModel, octree, assetContainer.treetrunkTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 50)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rockModel, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 60)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rock1Model, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 70)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rock2Model, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 80)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rock3Model, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 90)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rock4Model, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 100)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rock5Model, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 110)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rock6Model, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 120)
                {
                    Asset asset = new Asset(game, worldm, assetContainer.rock7Model, octree, assetContainer.rockTexture, id);
                    assetDictionary.Add(axial, asset);
                    tile.ObjectOn = asset;
                }
                else if (id == 130)
                {
                    Spawn spawn = new Spawn(game, worldm, assetContainer.spawnModel, octree, itemManager, Content, corePosition, phaseManager, GameplayScreen.wavesList[i]);
                    assetDictionary.Add(axial, spawn);
                    tile.ObjectOn = spawn;
                    GameplayScreen.spawns.Add(spawn);
                    i++;
                }
                else if (id == 140)
                {
                    Spawn spawn = new Spawn(game, worldm, assetContainer.spawnModel, octree, itemManager, Content, corePosition, phaseManager, GameplayScreen.wavesList[i]);
                    assetDictionary.Add(axial, spawn);
                    tile.ObjectOn = spawn;
                    GameplayScreen.spawns.Add(spawn);
                    i++;
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

        //flat top
        public static void Draw(SpriteBatch spriteBatch, Texture2D tileTex, Texture2D playerTex, Vector2 startingPos, Vector2 mapTileCount, float scale = 1f, float alpha = 1f)
        {
            Dictionary<AxialCoordinate, Tile> map = GameplayScreen.map;
            Vector2 tileSize = new Vector2(tileTex.Width, tileTex.Height) * scale;
            Vector2 origin = tileSize / 2;

            for (int y = 0; y < mapTileCount.Y; y++)
            {
                for (int x = 0; x < mapTileCount.X; x++)
                {
                    float offset = 0;
                    if (x % 2 != 0)
                        offset = tileSize.Y / 2;
                    HexOffset coord = new HexOffset(x, y);
                    var axial = coord.oddQ_toCube().ToAxial();

                    var tile = tileFromAxial(axial, map);
                    var position = tile.Position.Y;
                    var isPath = tile.IsPath;

                    Color color = Color.White;
                    if (position < 128)
                    {
                        color = Color.Lerp(Color.Green, Color.Yellow, position / 128f) * alpha;
                        if (position < 5)
                            color = Color.SkyBlue * alpha;
                    }
                    else
                        color = Color.Lerp(Color.Yellow, Color.Red, (position - 128f) / 128f) * alpha;

                    spriteBatch.Draw(tileTex, new Vector2(startingPos.X + x * tileSize.X * 0.75f, startingPos.Y + y * tileSize.Y + offset), null, color, 0, origin, 1, SpriteEffects.None, 0);

                    Texture2D pathIcon = GameplayScreen.assetContentContainer.pathIcon;
                    if (isPath)
                        spriteBatch.Draw(pathIcon, new Vector2(startingPos.X + x * tileSize.X * 0.75f, startingPos.Y + y * tileSize.Y + offset), null, Color.White * alpha, 0, origin, 1, SpriteEffects.None, 0);
                }
            }
            HexOffset playerCoord = pixelToAxialH(GameplayScreen.camera.Position, Map.size).ToCube().to_oddQ_Offset();

            Vector2 playerIconOrigin = new Vector2(playerTex.Width / 2, playerTex.Height / 2);
            float rotation = -MathHelper.ToRadians(GameplayScreen.camera.HeadingDegrees);

            spriteBatch.Draw(playerTex, new Vector2(startingPos.X + playerCoord.x * tileSize.X * 0.75f, startingPos.Y + playerCoord.y * tileSize.Y), null, Color.White * alpha, rotation, playerIconOrigin, scale, SpriteEffects.None, 0);
        }

        public static void Draw(SpriteBatch spriteBatch, Texture2D tileTex, Texture2D playerTex, Vector2 startingPos, Vector2 mapTileCount, CubeCoordinate center, int radius, float scale = 1f, float alpha = 1f)
        {
            Dictionary<AxialCoordinate, Tile> map = GameplayScreen.map;
            Vector2 tileSize = new Vector2(tileTex.Width, tileTex.Height) * scale;
            Vector2 origin = tileSize / 2;

            var list = CubeSpiral(center, radius);
            HexOffset centerCoord = center.to_oddQ_Offset();

            foreach (CubeCoordinate coordinate in list)
            {
                HexOffset coord = coordinate.to_oddQ_Offset();
                
                float offset = 0;
                if ((int)coord.x % 2 != 0)
                    offset = tileSize.Y / 2;

                var axial = coordinate.ToAxial();

                var tile = tileFromAxial(axial, map);
                if (tile != null)
                {
                    var position = tile.Position.Y;
                    var isPath = tile.IsPath;

                    Color color = Color.White;
                    if (position < 128)
                    {
                        color = Color.Lerp(Color.Green, Color.Yellow, position / 128f) * alpha;
                        if (position < 5) //waterheight
                            color = Color.SkyBlue * alpha;
                    }
                    else
                        color = Color.Lerp(Color.Yellow, Color.Red, (position - 128f) / 128f) * alpha;

                    spriteBatch.Draw(tileTex, new Vector2(startingPos.X + (coord.x - centerCoord.x) * tileSize.X * 0.75f, startingPos.Y + (coord.y - centerCoord.y) * tileSize.Y + offset), null, color, 0, origin, scale, SpriteEffects.None, 0);

                    Texture2D pathIcon = GameplayScreen.assetContentContainer.pathIcon;
                    if (isPath)
                        spriteBatch.Draw(pathIcon, new Vector2(startingPos.X + (coord.x - centerCoord.x) * tileSize.X * 0.75f, startingPos.Y + (coord.y - centerCoord.y) * tileSize.Y + offset), null, Color.White * alpha, 0, origin, scale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(tileTex, new Vector2(startingPos.X + (coord.x - centerCoord.x) * tileSize.X * 0.75f, startingPos.Y + (coord.y - centerCoord.y) * tileSize.Y + offset), null, Color.SkyBlue, 0, origin, scale, SpriteEffects.None, 0);
                }
            }

            Vector2 playerIconOrigin = new Vector2(playerTex.Width / 2, playerTex.Height / 2);
            float rotation = -MathHelper.ToRadians(GameplayScreen.camera.HeadingDegrees);

            spriteBatch.Draw(playerTex, startingPos, null, Color.White * alpha, rotation, playerIconOrigin, 1, SpriteEffects.None, 0);
        }


        //pointy top
        //public void Draw(SpriteBatch spriteBatch, Texture2D tileTex, Vector2 mapSize, Dictionary<AxialCoordinate, Tile> map)
        //{
        //    Vector2 tileSize = new Vector2(tileTex.Width, tileTex.Height);
        //    for (int y = 0; y < mapSize.Y; y++)
        //    {
        //        float offset = 0;
        //        if (y % 2 != 0)
        //            offset = tileSize.X / 2;
        //        for (int x = 0; x < mapSize.X; x++)
        //        {
        //            HexOffset coord = new HexOffset(x, y);
        //            var axial = coord.oddQ_toCube().ToAxial();

        //            var position = tileFromAxial(axial, map).Position.Y;

        //            spriteBatch.Draw(tileTex, new Vector2(x * tileTex.Width + offset, y * tileTex.Height * 0.75f), Color.Lerp(Color.White, Color.Red, position / 255f));
        //        }
        //    }
        //}

        //wszystko
        //public static void DrawAssets(SpriteBatch spriteBatch, Texture2D tileTex, Vector2 startingPos, Vector2 mapTileCount, float alpha = 1f)
        //{
        //    Dictionary<AxialCoordinate, DrawableObject> assets = GameplayScreen.mapAsset;

        //    Vector2 tileSize = new Vector2(tileTex.Width, tileTex.Height);
        //    foreach(var item in assets)
        //    {
        //        AxialCoordinate axial = pixelToAxialH(item.Value.Position, Map.size);
        //        HexOffset coord = axial.ToCube().to_oddQ_Offset();

        //        float offset = 0;
        //        if (coord.x % 2 != 0)
        //            offset = tileSize.Y / 2;

        //        Texture2D icon = tileTex;

        //        if (item.Value is Asset)
        //        {
        //            if ((item.Value as Asset).ModelID >= 50 && (item.Value as Asset).ModelID <= 120)
        //                icon = GameplayScreen.assetContentContainer.rockIcon;
        //            else if ((item.Value as Asset).ModelID == 10)
        //                icon = GameplayScreen.assetContentContainer.pinetreeIcon;
        //            else if ((item.Value as Asset).ModelID == 20)
        //                icon = GameplayScreen.assetContentContainer.tree1Icon;
        //            else if ((item.Value as Asset).ModelID == 30)
        //                icon = GameplayScreen.assetContentContainer.tree2Icon;
        //            else if ((item.Value as Asset).ModelID == 40)
        //                icon = GameplayScreen.assetContentContainer.treetrunkIcon;
        //        }
        //        else if (item.Value is Spawn)
        //            icon = GameplayScreen.assetContentContainer.spawnIcon;

        //        Vector2 origin = new Vector2(icon.Width / 2, icon.Height / 2);
        //        spriteBatch.Draw(icon, new Vector2(startingPos.X + coord.x * tileTex.Width * 0.75f, startingPos.Y + coord.y * tileTex.Height + offset), null, Color.DarkGray * alpha, 0, origin, 1, SpriteEffects.None, 0);
        //    }

        //    AxialCoordinate axialCore = pixelToAxialH(GameplayScreen.core.Position, Map.size);
        //    HexOffset coordCore = axialCore.ToCube().to_oddQ_Offset();
        //    float offsetCore = 0;
        //    if (coordCore.x % 2 != 0)
        //        offsetCore = tileSize.Y / 2;

        //    Vector2 coreOrigin = new Vector2(GameplayScreen.assetContentContainer.coreIcon.Width / 2, GameplayScreen.assetContentContainer.coreIcon.Height / 2);
        //    spriteBatch.Draw(GameplayScreen.assetContentContainer.coreIcon, new Vector2(startingPos.X + coordCore.x * tileTex.Width * 0.75f, startingPos.Y + coordCore.y * tileTex.Height + offsetCore), null, Color.DarkGray * alpha, 0, coreOrigin, 1, SpriteEffects.None, 0);
        //}

        //tylko core i spawny
        public static void DrawAssets(SpriteBatch spriteBatch, Texture2D tileTex, Vector2 startingPos, Vector2 mapTileCount, float scale = 1f, float alpha = 1f)
        {
            Dictionary<AxialCoordinate, DrawableObject> assets = GameplayScreen.mapAsset;
            Vector2 tileSize = new Vector2(tileTex.Width, tileTex.Height) * scale;

            Color color = Color.White * alpha;

            foreach (var item in assets)
            {
                if (item.Value is Spawn)
                {
                    Spawn spawn = item.Value as Spawn;

                    AxialCoordinate axial = pixelToAxialH(item.Value.Position, Map.size);
                    HexOffset coord = axial.ToCube().to_oddQ_Offset();

                    float offset = 0;
                    if (coord.x % 2 != 0)
                        offset = tileSize.Y / 2;

                    Texture2D icon = GameplayScreen.assetContentContainer.spawnIcon;
                    Vector2 origin = new Vector2(icon.Width / 2, icon.Height / 2);

                    spriteBatch.Draw(icon, new Vector2(startingPos.X + coord.x * tileSize.X * 0.75f, startingPos.Y + coord.y * tileSize.Y + offset), null, color, 0, origin, scale, SpriteEffects.None, 0);

                    List<DrawableObject> enemyList = spawn.enemies;

                    foreach (DrawableObject enemy in enemyList)
                    {
                        AxialCoordinate axialEnemy = pixelToAxialH(enemy.Position, Map.size);
                        HexOffset enemyCoord = axialEnemy.ToCube().to_oddQ_Offset();

                        float enemyOffset = 0;
                        if ((int)enemyCoord.x % 2 != 0)
                            enemyOffset = tileSize.Y / 2;

                        Texture2D enemyIcon = GameplayScreen.assetContentContainer.enemyIcon;
                        Vector2 enemyOrigin = new Vector2(enemyIcon.Width / 2, enemyIcon.Height / 2);

                        spriteBatch.Draw(enemyIcon, new Vector2(startingPos.X + enemyCoord.x * tileSize.X * 0.75f, startingPos.Y + enemyCoord.y * tileSize.Y + enemyOffset), null, color, 0, enemyOrigin, scale, SpriteEffects.None, 0);
                    }
                }
            }

            AxialCoordinate axialCore = pixelToAxialH(GameplayScreen.core.Position, Map.size);
            HexOffset coordCore = axialCore.ToCube().to_oddQ_Offset();

            float offsetCore = 0;
            if (coordCore.x % 2 != 0)
                offsetCore = tileSize.Y / 2;

            Texture2D coreIcon = GameplayScreen.assetContentContainer.coreIcon;
            Vector2 coreOrigin = new Vector2(coreIcon.Width / 2, coreIcon.Height / 2);

            spriteBatch.Draw(GameplayScreen.assetContentContainer.coreIcon, new Vector2(startingPos.X + coordCore.x * tileSize.X * 0.75f, startingPos.Y + coordCore.y * tileSize.Y + offsetCore), null, Color.DarkGray * alpha, 0, coreOrigin, scale, SpriteEffects.None, 0);

            foreach (Turret turret in GameplayScreen.turretList)
            {
                AxialCoordinate axialTurret = pixelToAxialH(turret.Position, Map.size);
                HexOffset turretCoord = axialTurret.ToCube().to_oddQ_Offset();

                float turretOffset = 0;
                if ((int)turretCoord.x % 2 != 0)
                    turretOffset = tileSize.Y / 2;

                Texture2D turretIcon = GameplayScreen.assetContentContainer.turretIcon;
                Vector2 turretOrigin = new Vector2(turretIcon.Width / 2, turretIcon.Height / 2);

                spriteBatch.Draw(turretIcon, new Vector2(startingPos.X + turretCoord.x * tileSize.X * 0.75f, startingPos.Y + turretCoord.y * tileSize.Y + turretOffset), null, color, 0, turretOrigin, scale, SpriteEffects.None, 0);
            }
        }

        public static void DrawAssets(SpriteBatch spriteBatch, Texture2D tileTex, Vector2 startingPos, Vector2 mapTileCount, CubeCoordinate center, int radius, float scale = 1f, float alpha = 1f)
        {
            Dictionary<AxialCoordinate, DrawableObject> assets = GameplayScreen.mapAsset;
            Vector2 tileSize = new Vector2(tileTex.Width, tileTex.Height) * scale;

            var list = CubeSpiral(center, radius);
            HexOffset centerCoord = center.to_oddQ_Offset();

            Color color = Color.White * alpha;

            AxialCoordinate axialCore = pixelToAxialH(GameplayScreen.core.Position, Map.size);
            CubeCoordinate cubeCore = CubeRound(axialCore.ToCube());
            bool coreVisible = false;

            foreach (CubeCoordinate coordinate in list)
            {
                if (coordinate == cubeCore)
                    coreVisible = true;

                var axial = coordinate.ToAxial();

                var asset = assetFromAxial(axial, assets);
                if (asset is Spawn)
                {
                    HexOffset coord = coordinate.to_oddQ_Offset();

                    float offset = 0;
                    if ((int)coord.x % 2 != 0)
                        offset = tileSize.Y / 2;

                    Texture2D icon = GameplayScreen.assetContentContainer.spawnIcon;
                    Vector2 origin = new Vector2(icon.Width / 2, icon.Height / 2);

                    spriteBatch.Draw(icon, new Vector2(startingPos.X + (coord.x - centerCoord.x) * tileSize.X * 0.75f, startingPos.Y + (coord.y - centerCoord.y) * tileSize.Y + offset), null, color, 0, origin, scale, SpriteEffects.None, 0);
                }
            }

            foreach (var item in assets)
            {
                if (item.Value is Spawn)
                {
                    Spawn spawn = item.Value as Spawn;

                    List<DrawableObject> enemyList = spawn.enemies;

                    Texture2D icon = GameplayScreen.assetContentContainer.enemyIcon;
                    Vector2 origin = new Vector2(icon.Width / 2, icon.Height / 2);

                    foreach (DrawableObject enemy in enemyList)
                    {
                        AxialCoordinate axialEnemy = pixelToAxialH(enemy.Position, Map.size);
                        HexOffset enemyCoord = axialEnemy.ToCube().to_oddQ_Offset();

                        if (Math.Abs(enemyCoord.x - centerCoord.x) < 26 && Math.Abs(enemyCoord.y - centerCoord.y) < 26)
                        {
                            float enemyOffset = 0;
                            if ((int)enemyCoord.x % 2 != 0)
                                enemyOffset = tileSize.Y / 2;

                            spriteBatch.Draw(icon, new Vector2(startingPos.X + (enemyCoord.x - centerCoord.x) * tileSize.X * 0.75f, startingPos.Y + (enemyCoord.y - centerCoord.y) * tileSize.Y + enemyOffset), null, color, 0, origin, scale, SpriteEffects.None, 0);
                        }
                    }
                }
            }

            if (coreVisible)
            {
                HexOffset coordCore = axialCore.ToCube().to_oddQ_Offset();

                float offsetCore = 0;
                if ((int)coordCore.x % 2 != 0)
                    offsetCore = tileSize.Y / 2;

                Texture2D coreIcon = GameplayScreen.assetContentContainer.coreIcon;
                Vector2 coreOrigin = new Vector2(coreIcon.Width / 2, coreIcon.Height / 2);

                spriteBatch.Draw(coreIcon, new Vector2(startingPos.X + (coordCore.x - centerCoord.x) * tileSize.X * 0.75f, startingPos.Y + (coordCore.y - centerCoord.y) * tileSize.Y + offsetCore), null, color, 0, coreOrigin, scale, SpriteEffects.None, 0);
            }

            Texture2D turretIcon = GameplayScreen.assetContentContainer.turretIcon;
            Vector2 turretOrigin = new Vector2(turretIcon.Width / 2, turretIcon.Height / 2);

            foreach (Turret turret in GameplayScreen.turretList)
            {
                AxialCoordinate axialTurret = pixelToAxialH(turret.Position, Map.size);
                HexOffset turretCoord = axialTurret.ToCube().to_oddQ_Offset();

                if (Math.Abs(turretCoord.x - centerCoord.x) < 26 && Math.Abs(turretCoord.y - centerCoord.y) < 26)
                {
                    float turretOffset = 0;
                    if ((int)turretCoord.x % 2 != 0)
                        turretOffset = tileSize.Y / 2;

                    spriteBatch.Draw(turretIcon, new Vector2(startingPos.X + (turretCoord.x - centerCoord.x) * tileSize.X * 0.75f, startingPos.Y + (turretCoord.y - centerCoord.y) * tileSize.Y + turretOffset), null, color, 0, turretOrigin, scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}
