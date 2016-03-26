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
        // public BoundingSphere mini;
        Effect old;
        

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>("1");
            effect = contentManager.Load<Effect>("Effects/Toon");
            texture = contentManager.Load<Texture2D>("textchampfer");
            old = model.Meshes[0].Effects[0];
        }

        public void reload()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = old;
                }
            }
        }

        //public void Draw(Camera camera)
        //{
        //    //temp = Matrix.CreateScale(Map.skala) * Matrix.CreateTranslation(position) * camera.worldMatrix;
        //    model.Draw(Matrix.CreateScale(Map.skala) * Matrix.CreateTranslation(position) * camera.worldMatrix, camera.ViewMatrix, camera.ProjectionMatrix);
        //}


        public void DrawEffect(Camera camera)
        {
            //worldMatrix = Matrix.CreateScale(Map.scale) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["WorldInverseTranspose"].SetValue(
                                            Matrix.Transpose(camera.worldMatrix * mesh.ParentBone.Transform));
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        //public Tile(Game1 game, Vector3 xyz, int id) : base(game)
        //{
        //    position = xyz;
        //    modelID = id;
        //    mini = new BoundingSphere(position, Map.skala * 0.75f);
        //}

        public Tile(Game game, Matrix inWorldMatrix, int inModelID) : base(game, inWorldMatrix, inModelID)
        {
            boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            boundingBox = new BoundingBox(position - new Vector3 (25, 10, 25), position + new Vector3(25,10,25)); // na oko wartosci, koniecznie wprowadzic poprawne!!
            type = ObjectType.Terrain;
        }
    }
}
