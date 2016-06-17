using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class Core : DrawableObject
    {
        Texture2D texture;
        public Core(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, Texture2D inTexture) : base(game, inWorldMatrix, inModel, octree)
        {
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            texture = inTexture;

            type = ObjectType.Core;

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
