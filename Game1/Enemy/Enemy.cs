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

namespace Game1
{
    class Enemy : DrawableObject
    {
        //Ray positionray;
        float gravity = 100f;
        float distance;

        float speed = 35;
        float a;
        float b;

        ContentManager Content;
        private AnimatedModel modell = null;
        private AnimatedModel dance = null;

        private Tile targetTile;
        private int tileNumber;

        public Enemy(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ContentManager Content) : base(game, inWorldMatrix, inModel, octree)
        {
            this.Content = Content;
            m_static = false;
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
           

            modell = new AnimatedModel("Models/dude");
            modell.LoadContent(Content);


            dance = new AnimatedModel("Models/dude");
            dance.LoadContent(Content);
            AnimationClip clip = dance.Clips[0];

            AnimationPlayer player = modell.PlayClip(clip);
            player.Looping = true;

            type = ObjectType.Enemy;
            boundingBox = CollisionBox.CreateBoundingBox(modell.Model, position, 1);
            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
            //positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);

            tileNumber = 0;
        }

        public bool Update(GameTime gameTime, Octree octree, List<Tile> path)
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

                    orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY((float)Math.Atan2(targetTile.Position.Z - position.Z, targetTile.Position.X - position.X)));
                    worldMatrix = Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position);
                    modell.Update(gameTime);

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

        public bool Update(GameTime gameTime, Octree octree, List<Vector3> path)
        {
            bool ret = base.Update(gameTime);

            Dictionary<AxialCoordinate, Tile> map = Game1.map;

            if (path.Count != 0)
            {
                try
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

                    orientation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY((float)Math.Atan2(targetPosition.Z - position.Z, targetPosition.X - position.X)));
                    worldMatrix = Matrix.CreateFromQuaternion(orientation) * Matrix.CreateTranslation(position);
                    modell.Update(gameTime);

                    while (Vector3.Distance(position, targetPosition) < 25 && tileNumber < path.Count)
                    {
                        tileNumber++;
                        targetPosition = path[tileNumber];
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
            modell.Draw(GraphicsDevice, camera, worldMatrix, Content);
        }
    }
}
