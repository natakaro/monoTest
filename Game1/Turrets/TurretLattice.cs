using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Game1.Screens;
using Game1.Helpers;

namespace Game1.Turrets
{
    class TurretLattice : DrawableObject
    {
        Texture2D texture;
        public TurretLattice(Game game, Matrix inWorldMatrix, Model inModel, Texture2D texture, Octree octree) : base(game, inWorldMatrix, inModel, octree)
        {
            this.texture = texture;
            type = ObjectType.Turret;
            m_static = false;

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
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Clipping"].SetValue(false);
                    effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                    effect.Parameters["Emissive"].SetValue(1f);
                }
                mesh.Draw();
            }
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            return ret;
        }
    }
}
