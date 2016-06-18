using Game1.Helpers;
using Game1.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class Spawn : DrawableObject
    {
        List <DrawableObject> enemies;
        List <Tile> path;
        List<Vector3> pathMiddle;
        ContentManager Content;
        Vector3 corePosition;
        PathFinder pathfinder;
        PhaseManager phaseManager;
        ItemManager itemManager;
        int enemyType = 0;

        //do testów
        private Stopwatch stopwatch = new Stopwatch();

        public Spawn(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, Vector3 corePosition, PhaseManager phaseManager, int enemyType) : base(game, inWorldMatrix, inModel, octree)
        {
            type = ObjectType.Spawn;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            enemies = new List<DrawableObject>();
            pathfinder = new PathFinder();

            this.octree = octree;
            this.Content = Content;
            this.corePosition = corePosition;
            this.phaseManager = phaseManager;
            this.itemManager = itemManager;

            this.enemyType = enemyType;

            Tile start = HexCoordinates.tileFromPosition(position, Game1.map);
            Tile end = HexCoordinates.tileFromPosition(corePosition, Game1.map);
            path = pathfinder.Pathfind(start, end, Game1.map, true);
            pathMiddle = pathfinder.PathfindMiddle(path);
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            List<DrawableObject> toRemove = new List<DrawableObject>();

            foreach(DrawableObject enemy in enemies)
            {
                //enemy.Update(gameTime, octree, pathMiddle);
                if(enemy.Alive == false)
                {
                    toRemove.Add(enemy);
                }
            }

            foreach (DrawableObject enemy in toRemove)
            {
                enemies.Remove(enemy);
            }

            if (phaseManager.Phase == Phase.Day)
            {
                stopwatch.Reset();
            }

            if (phaseManager.Phase == Phase.Night)
            {
                stopwatch.Start();
                if (stopwatch.ElapsedMilliseconds > 6000)
                {
                    if(enemyType == 1)
                    {
                        SpawnEnemy();
                    }
                    else
                    {
                        SpawnFlyEnemy();
                    }
                    stopwatch.Restart();
                }
            }

            return ret;
        }

        public bool SpawnEnemy()
        {
            Enemy enemy = new EnemyWalk(Game, worldMatrix, model, octree, itemManager, Content, pathMiddle);
            enemies.Add(enemy);
            Octree.AddObject(enemy);
            return true;
        }
        public bool SpawnFlyEnemy()
        {
            Enemy enemy = new EnemyFly(Game, Matrix.CreateTranslation(0, 30, 0)*worldMatrix, Game1.assetContentContainer.enemyFly, octree, itemManager, Content, pathMiddle);
            enemies.Add(enemy);
            Octree.AddObject(enemy);
            return true;
        }

        public void UpdatePath()
        {
            Tile start = HexCoordinates.tileFromPosition(position, Game1.map);
            Tile end = HexCoordinates.tileFromPosition(corePosition, Game1.map);
            path = pathfinder.Pathfind(start, end, Game1.map, true);
        }
    }
}
