using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Game1.Helpers;
using Game1.Lights;

namespace Game1.Items
{
    public class Item : DrawableObject
    {
        private Texture2D texture;
        private PointLight pointLight;
        private LightManager lightManager;
        private ItemManager itemManager;
        private Stats stats;

        public Item(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ItemManager itemManager, Texture2D inTexture, LightManager lightManager, Stats stats) : base(game, inWorldMatrix, inModel, octree)
        {
            this.lightManager = lightManager;
            this.itemManager = itemManager;
            this.stats = stats;
            texture = inTexture;

            type = ObjectType.Item;

            boundingSphere = new BoundingSphere(position, 5f);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);

            pointLight = new PointLight(position + new Vector3(0, 3, 0), Color.DarkViolet, 5, 5);
            lightManager.AddLight(pointLight);
        }

        public override void Draw(Camera camera)
        {
            //model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(0.1f) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["Clipping"].SetValue(false);
                }
                mesh.Draw();
            }
        }

        public void PickUp()
        {
            if (stats.currentEssence < stats.maxEssence)
            {
                stats.currentEssence = Math.Min(stats.currentEssence + 10, stats.maxEssence);
                Destroy();
            }
        }

        private void Destroy()
        {
            lightManager.RemoveLight(pointLight);
            Alive = false;
            itemManager.Remove(this);
        }
    }
}
