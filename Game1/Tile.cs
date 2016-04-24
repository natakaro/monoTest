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
    class Tile : DrawableObject
    {
        Texture2D texture;
        Model model;
        Matrix[] modelBones;

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>("1");
            effect = contentManager.Load<Effect>("Effects/test");
            texture = contentManager.Load<Texture2D>("textchampfer");
            modelBones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelBones);
        }

        public override void Draw(Camera camera)
        {
            //worldMatrix = Matrix.CreateScale(Map.scale) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    Vector3 temp = Game1.slonce - position;
                    //slonce
                    effect.Parameters["DiffuseLightDirection"].SetValue(temp);
                    if (selected == true)
                        effect.Parameters["DiffuseIntensity"].SetValue(10 - (temp.Length() / 1000));
                    else
                        effect.Parameters["DiffuseIntensity"].SetValue(5 - (temp.Length() / 1000));
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * Matrix.CreateScale(Map.scale) * Matrix.CreateTranslation(position));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["WorldInverseTranspose"].SetValue(
                                            Matrix.Transpose(camera.worldMatrix * mesh.ParentBone.Transform));
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        public Tile(Game game, Matrix inWorldMatrix) : base(game, inWorldMatrix)
        {
            m_instanced = true;
            boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            //boundingBox = new BoundingBox(position - new Vector3 (25, 4, 25), position + new Vector3(25, 4, 25)); // na oko wartosci, koniecznie wprowadzic poprawne!!
            
            type = ObjectType.Terrain;

            Initialize(game.Content);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, Map.scale);
        }
    }
}
