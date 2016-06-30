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
    public enum DamageType
    {
        Fire = 0,
        Ice = 1,
        IceSlow = 2
    }
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

        protected float deathAge;
        protected const float deathLength = 1f;

        protected float spawnAge;
        protected const float spawnLength = 1f;

        protected bool chilled = false;
        protected float chilledAge;
        protected float chilledLength = 0;
        protected float chilledLengthMax;

        protected bool died = false;

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

            dissolveAmount = 1;
            chilledLengthMax = 5;
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            if (CurrentHealth <= 0)
            {
                Die(gameTime);
            }
            else
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (dissolveAmount >= 0)
                {
                    spawnAge += elapsedTime;

                    dissolveAmount = MathHelper.Lerp(1, 0, spawnAge / spawnLength);
                }
                else
                    dissolveAmount = 0;

                Dictionary<AxialCoordinate, Tile> map = GameplayScreen.map;

                if (path.Count - tileNumber > 0)
                {
                    Vector3 targetPosition = path[tileNumber];
                    Vector3 direction = Vector3.Normalize(targetPosition - position);
                    Vector3 directionXZ = Vector3.Normalize(new Vector3(targetPosition.X, 0, targetPosition.Z) - new Vector3(position.X, 0, position.Z));
                    velocity = speed * direction;

                    if (chilled)
                    {
                        velocity /= 2;
                        chilledAge += elapsedTime;

                        if (chilledAge > chilledLength)
                        {
                            chilled = false;
                            chilledAge = 0;
                        }
                    }

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
                    EnterCore(gameTime);
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

        public void Damage(float value, DamageType type)
        {
            CurrentHealth -= value;
            if (type == DamageType.Ice)
            {
                chilled = true;
                chilledAge = 0;
                chilledLength = chilledLengthMax;
            }

            if (type == DamageType.IceSlow)
            {
                chilled = true;
                chilledAge = 0;
                if (chilledLength < chilledLengthMax)
                    chilledLength += 0.1f;
            }
        }

        public virtual void Die(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            deathAge += elapsedTime;

            dissolveAmount = MathHelper.Lerp(0, 1, deathAge / deathLength);

            if (deathAge > deathLength)
            {
                if (died == false)
                {
                    GameplayScreen.stats.currentExp += 100;
                    itemManager.SpawnEssence(position);
                    died = true;
                }
                Alive = false;
            }
        }

        public virtual void EnterCore(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            deathAge += elapsedTime;

            dissolveAmount = MathHelper.Lerp(0, 1, deathAge / deathLength);

            if (deathAge > deathLength)
            {
                GameplayScreen.stats.currentHealth -= 20;
                Alive = false;
            }
        }

        public float Rotation
        {
            get { return targetRotation; }
        }
    }
}
