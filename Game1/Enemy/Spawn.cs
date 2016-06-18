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
        List <Enemy> enemies;
        List <Tile> path;
        List<Vector3> pathMiddle;
        ContentManager Content;
        Vector3 corePosition;
        PathFinder pathfinder;
        PhaseManager phaseManager;
        ItemManager itemManager;

        //do testów
        private Stopwatch stopwatch = new Stopwatch();

        public Spawn(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, Vector3 corePosition, PhaseManager phaseManager) : base(game, inWorldMatrix, inModel, octree)
        {
            type = ObjectType.Spawn;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            enemies = new List<Enemy>();
            pathfinder = new PathFinder();

            this.octree = octree;
            this.Content = Content;
            this.corePosition = corePosition;
            this.phaseManager = phaseManager;
            this.itemManager = itemManager;

            Tile start = HexCoordinates.tileFromPosition(position, Game1.map);
            Tile end = HexCoordinates.tileFromPosition(corePosition, Game1.map);
            path = pathfinder.Pathfind(start, end, Game1.map, true);
            pathMiddle = pathfinder.PathfindMiddle(path);
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            List<Enemy> toRemove = new List<Enemy>();

            foreach(Enemy enemy in enemies)
            {
                //enemy.Update(gameTime, octree, pathMiddle);
                if(enemy.Alive == false)
                {
                    toRemove.Add(enemy);
                }
            }

            foreach (Enemy enemy in toRemove)
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
                if (stopwatch.ElapsedMilliseconds > 3000)
                {
                    SpawnEnemy();
                    stopwatch.Restart();
                }
            }

            return ret;
        }

        public bool SpawnEnemy()
        {
            Enemy enemy = new Enemy(Game, worldMatrix, model, octree, itemManager, Content, pathMiddle);
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
