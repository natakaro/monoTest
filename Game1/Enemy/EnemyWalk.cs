using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AnimationAux;
using static Game1.Helpers.HexCoordinates;
using Game1.Items;

namespace Game1
{
    public class EnemyWalk : Enemy
    {

        private AnimatedModel animatedModel = null;
        private AnimatedModel dance = null;


        public EnemyWalk(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, List<Vector3> path) : base(game, inWorldMatrix, inModel, octree, itemManager, Content, path)
        {
            animatedModel = new AnimatedModel("Models/h");
            animatedModel.LoadContent(Content);


            dance = new AnimatedModel("Models/h_walk");
            dance.LoadContent(Content);
            AnimationClip clip = dance.Clips[0];

            AnimationPlayer player = animatedModel.PlayClip(clip);
            player.Looping = true;

            type = ObjectType.Enemy;
            boundingBox = CollisionBox.CreateBoundingBox(animatedModel.Model, position, 1);
            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
            //positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);

            targetRotation = 0;
            speed = 45;
            maxHealth = 100;
            currentHealth = 100;
        }



        //ten ble stary
        public new bool Update(GameTime gameTime, Octree octree, List<Tile> path)
        {
            Dictionary<AxialCoordinate, Tile> map = Game1.map;

            if (path.Count != 0)
            {
                try
                {
                    targetTile = path[tileNumber];
                    Vector3 direction = Vector3.Normalize(targetTile.Position - position);
                    Vector3 directionXZ = Vector3.Normalize(new Vector3(targetTile.Position.X, 0, targetTile.Position.Z) - new Vector3(position.X, 0, position.Z));
                    velocity = speed * directionXZ;

                    Tile tile = tileFromPosition(position, map);

                    if (tile != null)
                    {
                        distance = boundingBox.Min.Y - tile.BoundingBox.Max.Y;
                        boundingBox.Min.Y -= distance * gravity / 10 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    }
                    if (tile == null)
                        boundingBox.Min.Y -= gravity * (float)(gameTime.ElapsedGameTime.TotalSeconds);

                    boundingBox.Max.Y = boundingBox.Min.Y + a;
                    position.Y = boundingBox.Min.Y + b;

                    targetRotation = (float)Math.Atan2((double)(targetTile.Position.X - position.X), (double)(targetTile.Position.Z - position.Z));

                    worldMatrix = Matrix.CreateRotationY(targetRotation) * Matrix.CreateTranslation(position);
                    animatedModel.Update(gameTime);

                    while(Vector3.Distance(position, targetTile.Position) < 25 && tileNumber < path.Count)
                    {
                        tileNumber++;
                        targetTile = path[tileNumber];
                    }

                    //if (Vector3.Distance(position, targetTile.Position) < 25 && tileNumber < path.Count)
                    //{
                    //    tileNumber++;
                    //}
                }
                catch
                {
                    alive = false;
                }

            }
            bool ret = base.Update(gameTime);

            return ret;
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                Dictionary<AxialCoordinate, Tile> map = Game1.map;

                if (path.Count - tileNumber > 0)
                {
                    Vector3 targetPosition = path[tileNumber];
                    Vector3 direction = Vector3.Normalize(targetPosition - position);
                    Vector3 directionXZ = Vector3.Normalize(new Vector3(targetPosition.X, 0, targetPosition.Z) - new Vector3(position.X, 0, position.Z));
                    velocity = speed * direction;

                    //Tile tile = tileFromPosition(position, map);

                    //if (tile != null)
                    //{
                    //    distance = boundingBox.Min.Y - tile.BoundingBox.Max.Y;
                    //    boundingBox.Min.Y -= distance * gravity / 10 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                    //}
                    //if (tile == null)
                    //    boundingBox.Min.Y -= gravity * (float)(gameTime.ElapsedGameTime.TotalSeconds);

                    //boundingBox.Max.Y = boundingBox.Min.Y + a;
                    //position.Y = boundingBox.Min.Y + b;

                    targetRotation = (float)Math.Atan2((double)(targetPosition.X - position.X), (double)(targetPosition.Z - position.Z));
                    worldMatrix = Matrix.CreateRotationY(targetRotation) * Matrix.CreateTranslation(position);

                    animatedModel.Update(gameTime);

                    //while (Vector3.Distance(position, targetPosition) < 25 && tileNumber < path.Count - 1)
                    //{
                        //tileNumber++;
                        //targetPosition = path[tileNumber];
                    //}

                    if (Vector3.Distance(position, targetPosition) < 25 && tileNumber < path.Count)
                    {
                        tileNumber++;
                    }
                }
                else
                    alive = false;
            }
            
            return ret;
        }

        public override void Draw(Camera camera)
        {
            /*
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Technique1"];
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    //effect.Parameters["FarClip"].SetValue(camera.FarZ);
                }
                mesh.Draw();
            }
            */
            animatedModel.Draw(GraphicsDevice, camera, worldMatrix, Content);
        }


        public AnimatedModel AnimatedModel
        {
            get { return animatedModel; }
        }

    }
}
