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
    class Tile : DrawableObject
    {
        Texture2D texture;

        public void Initialize(ContentManager contentManager)
        {
            texture = contentManager.Load<Texture2D>("textchampfer");
        }

        public override void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.CurrentTechnique = effect.Techniques["Technique1"];
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * Matrix.CreateScale(Map.scale) * Matrix.CreateTranslation(position));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        public Tile(Game game, Matrix inWorldMatrix, Model inModel) : base(game, inWorldMatrix, inModel)
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
