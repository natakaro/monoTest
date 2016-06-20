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
    public class Enemy : DrawableObject
    {
        //Ray positionray;
        protected float gravity = 100f;
        protected float distance;

        protected float speed = 35;
        protected float a;
        protected float b;

        protected ContentManager Content;
        protected ItemManager itemManager;

        public List<Vector3> path;

        protected float targetRotation;

        protected Tile targetTile;
        protected int tileNumber;

        protected float maxHealth;
        protected float currentHealth;

        public float MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }
        public float CurrentHealth
        {
            get { return currentHealth; }
            set { currentHealth = value; }
        }

        public Enemy(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, ContentManager Content, List<Vector3> path) : base(game, inWorldMatrix, inModel, octree)
        {
            this.Content = Content;
            this.itemManager = itemManager;
            this.path = path;
            m_static = false;
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);



            type = ObjectType.Enemy;
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
            //positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);

            tileNumber = 0;

            targetRotation = 0;

            maxHealth = 100;
            currentHealth = 100;
        }



        //ten ble stary
        public bool Update(GameTime gameTime, Octree octree, List<Tile> path)
        {
            Dictionary<AxialCoordinate, Tile> map = GameplayScreen.map;

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

                    while (Vector3.Distance(position, targetTile.Position) < 25 && tileNumber < path.Count)
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
                Dictionary<AxialCoordinate, Tile> map = GameplayScreen.map;

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

        /*public override void Draw(Camera camera)
        {
            
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
            
        }*/

        public void Damage(float value)
        {
            CurrentHealth -= value;
        }

        public void Die()
        {
            itemManager.Spawn(position);
            Alive = false;
        }


        public float Rotation
        {
            get { return targetRotation; }
        }
    }
}
