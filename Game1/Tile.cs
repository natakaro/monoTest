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
    class Tile : GameComponent
    {
        public Model model;
        public Vector3 position;
        int id;
        public Matrix temp;
        public BoundingSphere mini;
        Effect effect;
        Effect old;
        Texture2D texture;

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>(id.ToString());
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

        public void Draw(Camera camera)
        {
            //temp = Matrix.CreateScale(Map.skala) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            model.Draw(Matrix.CreateScale(Map.skala) * Matrix.CreateTranslation(position) * camera.worldMatrix, camera.ViewMatrix, camera.ProjectionMatrix);
        }


        public void DrawEffect(Camera camera)
        {
            temp = Matrix.CreateScale(Map.skala) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * temp);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["WorldInverseTranspose"].SetValue(
                                            Matrix.Transpose(camera.worldMatrix * mesh.ParentBone.Transform));
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        public Tile(Game1 game, Vector3 xyz, int id) : base(game)
        {
            position = xyz;
            this.id = id;
            mini = new BoundingSphere(position, Map.skala * 0.75f);
        }
    }
}
