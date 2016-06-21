using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class Asset : DrawableObject
    {
        Texture2D texture;
        private int modelId;

        public int ModelID
        {
            get { return modelId; }
        }
        public Asset(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, Texture2D inTexture, int modelId) : base(game, inWorldMatrix, inModel, octree)
        {
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            texture = inTexture;
            this.modelId = modelId;

            type = ObjectType.Asset;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
        }

        public override void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["Clipping"].SetValue(false);
                }
                mesh.Draw();
            }
        }
    }
}

