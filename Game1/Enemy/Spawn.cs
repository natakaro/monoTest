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

        const int portalParticlesPerFrame = 3;

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

            if (phaseManager.Phase == Phase.Night)
            {
                for (int i = 0; i < portalParticlesPerFrame; i++)
                {
                    GameplayScreen.particleManager.portalParticlesEnemy.AddParticle(position + Core.RandomPointOnCircle(30, 60), Vector3.Zero);
                }

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
            Enemy enemy = new EnemyFly(Game, Matrix.CreateTranslation(0, 30, 0) * worldMatrix, GameplayScreen.assetContentContainer.enemyFly, octree, itemManager, Content, pathMiddle);
            enemies.Add(enemy);
            Octree.AddObject(enemy);
            return true;
        }

        public void UpdatePath()
        {
            Tile start = HexCoordinates.tileFromPosition(position, GameplayScreen.map);
            Tile end = HexCoordinates.tileFromPosition(corePosition, GameplayScreen.map);
            path = pathfinder.Pathfind(start, end, GameplayScreen.map, true);
        }
    }
}
