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
using static Game1.Helpers.HexCoordinates;

namespace Game1
{
    public class Tile : DrawableObject
    {
        private bool isPath;
        private DrawableObject objectOn;
        public bool IsPath
        {
            get { return isPath; }
            set { isPath = value; }
        }
        public DrawableObject ObjectOn
        {
            get { return objectOn; }
            set { objectOn = value; }
        }

        public Tile(Game game, Matrix inWorldMatrix, Model inModel, Octree octree) : base(game, inWorldMatrix, inModel, octree)
        {
            m_instanced = true;
            IsPath = false;
            objectOn = null;
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            
            type = ObjectType.Tile;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
        }

        public override void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Technique1Color"];
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Clipping"].SetValue(false);
                    effect.Parameters["DissolveMap"].SetValue(GameplayScreen.assetContentContainer.dissolveTexture);
                    effect.Parameters["DissolveThreshold"].SetValue(dissolveAmount);
                    effect.Parameters["EdgeMap"].SetValue(GameplayScreen.assetContentContainer.edgeTexture);
                    if (IsPath)
                        effect.Parameters["Emissive"].SetValue(0.1f);
                    else
                        effect.Parameters["Emissive"].SetValue(0.0f);
                }
                mesh.Draw();
            }
        }

        //public override void Draw(Camera camera, Matrix viewMatrix, Vector4 clipPlane)
        //{
        //    foreach (ModelMesh mesh in model.Meshes)
        //    {
        //        foreach (Effect effect in mesh.Effects)
        //        {
        //            effect.CurrentTechnique = effect.Techniques["Technique1Color"];
        //            effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * modelBones[mesh.ParentBone.Index] * worldMatrix);
        //            effect.Parameters["View"].SetValue(viewMatrix);
        //            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
        //            effect.Parameters["FarClip"].SetValue(camera.FarZ);
        //            effect.Parameters["Clipping"].SetValue(true);
        //            effect.Parameters["ClipPlane"].SetValue(clipPlane);
        //            if (IsPath)
        //                effect.Parameters["Emissive"].SetValue(0.1f);
        //            else
        //                effect.Parameters["Emissive"].SetValue(0.0f);
        //        }
        //        mesh.Draw();
        //    }
        //}

    }
}
