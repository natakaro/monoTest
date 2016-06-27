using Game1.Helpers;
using Game1.Screens;
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
            //boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1, Matrix.CreateFromQuaternion(Orientation));
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
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["Clipping"].SetValue(false);
                    //effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    //effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    //effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                }
                mesh.Draw();
            }
        }

        public override void Draw(Camera camera, Matrix viewMatrix, Vector4 clipPlane)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(viewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["Clipping"].SetValue(true);
                    effect.Parameters["ClipPlane"].SetValue(clipPlane);
                    //effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    //effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    //effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                }
                mesh.Draw();
            }
        }
    }
}

