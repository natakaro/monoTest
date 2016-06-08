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
        public new void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Technique1"];
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Clipping"].SetValue(false);
                }
                mesh.Draw();
            }
        }

        public Asset(Game game, Matrix inWorldMatrix, Model inModel) : base(game, inWorldMatrix, inModel)
        {
            m_instanced = true;
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            
            type = ObjectType.Unit;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
        }
    }
}
