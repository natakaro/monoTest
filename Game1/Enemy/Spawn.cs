using Game1.Helpers;
using Game1.Items;
using Game1.Screens;
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
        public List <DrawableObject> enemies;
        List <Tile> path;
        List<Vector3> pathMiddle;
        ContentManager Content;
        Vector3 corePosition;
        PathFinder pathfinder;
        PhaseManager phaseManager;
        ItemManager itemManager;
        List<Wave> waves;
        int waveNumber = 0;
        int enemyNumber = 0;

        //do testów
        private Stopwatch stopwatch = new Stopwatch();

        const int portalParticlesPerFrame = 3;

        public Spawn(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, Vector3 corePosition, PhaseManager phaseManager, List<Wave> waves) : base(game, inWorldMatrix, inModel, octree)
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

            this.waves = waves;

            Tile start = HexCoordinates.tileFromPosition(position, GameplayScreen.map);
            Tile end = HexCoordinates.tileFromPosition(corePosition, GameplayScreen.map);
            path = pathfinder.Pathfind(start, end, GameplayScreen.map, true);
            pathMiddle = pathfinder.PathfindMiddle(path);
        }

        public override void Draw(Camera camera)
        {
            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    foreach (Effect effect in mesh.Effects)
            //    {
            //        effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
            //        effect.Parameters["View"].SetValue(camera.ViewMatrix);
            //        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            //        effect.Parameters["FarClip"].SetValue(camera.FarZ);
            //        effect.Parameters["Texture"].SetValue(texture);
            //        effect.Parameters["Clipping"].SetValue(false);
            //    }
            //    mesh.Draw();
            //}
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

            if (phaseManager.Phase == Phase.Night && waveNumber < waves.Count)
            {
                for (int i = 0; i < portalParticlesPerFrame; i++)
                {
                    GameplayScreen.particleManager.portalParticlesEnemy.AddParticle(position + Core.RandomPointOnCircle(30, 60), Vector3.Zero);
                }

                
                if(GameplayScreen.timeOfDay.TimeFloatCut == waves[waveNumber].time) // &&  //waves[waveNumber].time ==
                {
                    stopwatch.Start();
                }

                if (stopwatch.ElapsedMilliseconds > waves[waveNumber].stopwatch)
                {
                    if (enemyNumber < waves[waveNumber].number)
                    {
                        SpawnEnemy(waves[waveNumber].enemyType);
                        enemyNumber++;
                        stopwatch.Restart();
                    }
                    else
                    {
                        enemyNumber=0;
                        waveNumber++;
                        stopwatch.Reset();
                    }
                }
            }

            return ret;
        }

        public bool SpawnEnemy(int type)
        {
            Enemy enemy = null;
            switch (type)
            {
                case 0:
                    enemy = new EnemyFly(Game, worldMatrix, model, octree, itemManager, Content, pathMiddle);
                    break;
                case 1:
                    enemy = new EnemyWalk(Game, worldMatrix, model, octree, itemManager, Content, pathMiddle);
                    break;
                case 2:
                    enemy = new EnemyGremlin(Game, worldMatrix, model, octree, itemManager, Content, pathMiddle);
                    break;
                case 3:
                    enemy = new EnemyWeird(Game, worldMatrix, model, octree, itemManager, Content, pathMiddle);
                    break;
                case 7:
                    enemy = new Boss(Game, Matrix.CreateWorld(new Vector3(3000, 240, 1700),Vector3.Forward, Vector3.Up), model, octree, itemManager, Content, pathMiddle);
                    break;
            }
            enemies.Add(enemy);
            Octree.AddObject(enemy);
            return true;
        }
        
        /*
        public bool SpawnFlyEnemy()
        {
            Enemy enemy = new EnemyFly(Game, Matrix.CreateTranslation(0, 30, 0) * worldMatrix, GameplayScreen.assetContentContainer.enemyFly, octree, itemManager, Content, pathMiddle);
            enemies.Add(enemy);
            Octree.AddObject(enemy);
            return true;
        }
        */

        public bool UpdatePath()
        {
            Tile start = HexCoordinates.tileFromPosition(position, GameplayScreen.map);
            Tile end = HexCoordinates.tileFromPosition(corePosition, GameplayScreen.map);
            foreach (Tile tile in path)
            {
                tile.IsPath = false;
            }
            var newPath = pathfinder.Pathfind(start, end, GameplayScreen.map, true);
            if (newPath != null)
            {
                path = newPath;
                return true;
            }
            else
            {
                foreach (Tile tile in path)
                {
                    tile.IsPath = true;
                }
                return false;
            }
        }
    }
}
