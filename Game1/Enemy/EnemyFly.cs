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
using Game1.Screens;

namespace Game1
{
    public class EnemyFly : Enemy
    {

        Texture2D tex;
        public EnemyFly(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, List<Vector3> path) : base(game, inWorldMatrix, inModel, octree, itemManager, Content, path)
        {
            maxHealth = 50;
            currentHealth = 50;
            //tex = Game1.assetContentContainer.enemyFlyTexture;
            tex = Content.Load<Texture2D>("Textures/ship/enemy_ship");
        }

        public override bool Update(GameTime gameTime)
        {
            if (CurrentHealth <= 0)
            {
                Die(gameTime);
            }
            else
            {
                Dictionary<AxialCoordinate, Tile> map = GameplayScreen.map;

                if (path.Count - tileNumber > 0)
                {
                    Vector3 targetPosition = path[tileNumber];
                    Vector3 direction = Vector3.Normalize(targetPosition - position);
                    Vector3 distance = new Vector3(targetPosition.X, 0, targetPosition.Z) - new Vector3(position.X, 0, position.Z);
                    Vector3 directionXZ = Vector3.Normalize(distance);
                    velocity = speed * direction;

                    Vector3 dist2 = path[tileNumber - 1] - position;
                    position.Y += 5 * dist2.Y * (float)(gameTime.ElapsedGameTime.TotalSeconds);

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
                    Orientation = worldMatrix.Rotation;

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

            bool ret = base.Update(gameTime);

            return ret;
        }

        public override void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Technique1"];
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Texture"].SetValue(tex);
                    effect.Parameters["Clipping"].SetValue(false);
                    effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                }
                mesh.Draw();
            }
        }
    }
}
