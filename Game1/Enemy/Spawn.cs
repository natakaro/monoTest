﻿using Game1.Helpers;
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
        ContentManager Content;
        Vector3 corePosition;
        PathFinder pathfinder;

        //do testów
        private Stopwatch stopwatch = new Stopwatch();

        public Spawn(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ContentManager Content, Vector3 corePosition) : base(game, inWorldMatrix, inModel, octree)
        {
            type = ObjectType.Enemy;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            enemies = new List<Enemy>();
            pathfinder = new PathFinder();

            this.octree = octree;
            this.Content = Content;
            this.corePosition = corePosition;

            Tile start = HexCoordinates.tileFromPosition(position, Game1.tileDictionary);
            Tile end = HexCoordinates.tileFromPosition(corePosition, Game1.tileDictionary);
            path = pathfinder.Pathfind(start, end, Game1.tileDictionary, true);
            octree.AddObject(this); //dodanie siebie do octree

            //test
            stopwatch.Start();
        }

        public override bool Update(GameTime gameTime)
        {
            List<Enemy> removes = new List<Enemy>();
            foreach(Enemy enemy in enemies)
            {
                enemy.Update(gameTime, octree, path);
                if(enemy.Alive == false)
                {
                    removes.Add(enemy);
                }
            }

            foreach (Enemy enemy in removes)
            {
                enemies.Remove(enemy);
            }

            if(stopwatch.ElapsedMilliseconds > 3000)
            {
                SpawnEnemy();
                stopwatch.Restart();
            }

                return true;
        }

        public bool SpawnEnemy()
        {
            Enemy temp = new Enemy(Game, worldMatrix, model, octree, Content);
            enemies.Add(temp);
            octree.AddObject(temp);
            return true;
        }

        public void UpdatePath()
        {
            Tile start = HexCoordinates.tileFromPosition(position, Game1.tileDictionary);
            Tile end = HexCoordinates.tileFromPosition(corePosition, Game1.tileDictionary);
            path = pathfinder.Pathfind(start, end, Game1.tileDictionary, true);
        }
    }
}