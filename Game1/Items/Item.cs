using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Game1.Helpers;
using Game1.Lights;
using Game1.Screens;

namespace Game1.Items
{
    public abstract class Item : DrawableObject
    {
        protected Texture2D texture;
        protected LightManager lightManager;
        protected ItemManager itemManager;
        protected Stats stats;
        protected PointLight pointLight;

        protected float targetScale;

        protected float age;
        protected const float lifespan = 30f;
        protected const float spawnLength = 0.5f;

        protected const float lootParticlesPerSecond = 5;
        protected float timeBetweenParticles;
        protected float timeLeftOver;

        public Item(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, Texture2D inTexture, LightManager lightManager, Stats stats) : base(game, inWorldMatrix, inModel, octree)
        {
            this.lightManager = lightManager;
            this.itemManager = itemManager;
            this.stats = stats;
            texture = inTexture;

            type = ObjectType.Item;

            boundingSphere = new BoundingSphere(position, 5f);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);

            targetScale = 1;

            dissolveAmount = 1;
            scale = 0;

            timeBetweenParticles = 1.0f / lootParticlesPerSecond;
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            age += elapsedTime;

            float timeToSpend = timeLeftOver + elapsedTime;
            float currentTime = -timeLeftOver;

            while (timeToSpend > timeBetweenParticles)
            {
                currentTime += timeBetweenParticles;
                timeToSpend -= timeBetweenParticles;

                GameplayScreen.particleManager.lootParticles.AddParticle(position, velocity);
            }

            if (age < spawnLength)
            {
                dissolveAmount = MathHelper.Lerp(1, 0, age / spawnLength);
                scale = MathHelper.Lerp(0, targetScale, age / spawnLength);
            }
            else
            {
                dissolveAmount = 0;
                scale = targetScale;
            }

            if (age > lifespan)
                Destroy();

            timeLeftOver = timeToSpend;

            return ret;
        }

        public override void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Emissive"].SetValue(0.1f);
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["Clipping"].SetValue(false);
                    effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                }
                mesh.Draw();
            }
        }

        public abstract void PickUp();

        public virtual void Destroy()
        {
            Alive = false;
            itemManager.Remove(this);
        }
    }
}
