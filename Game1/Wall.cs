using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Helpers;

namespace Game1
{
    class Wall : DrawableObject
    {

        Texture2D texture;
        Model model;

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>("Monocube");
            texture = contentManager.Load<Texture2D>("MonoCubeTexture");
        }


        public override void Draw(Camera camera)
        {
            //model.Draw(camera.worldMatrix*Matrix.CreateScale(25)*Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            //worldMatrix = Matrix.CreateScale(Map.scale) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(camera.worldMatrix * Matrix.CreateScale(25) * Matrix.CreateTranslation(position));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        public Wall(Game game, Matrix inWorldMatrix) : base(game, inWorldMatrix)
        {
            position = new Vector3(100, 40, -200);
            Initialize(game.Content);
            boundingSphere = new BoundingSphere(position, model.Meshes[0].BoundingSphere.Radius * 25);
            //boundingBox = new BoundingBox(position - new Vector3(50, 35, 50), position + new Vector3(50, 35, 50));
            boundingBox = CollisionBox.CreateBoundingBox(model, position, Map.scale);
            type = ObjectType.Item;
        }
    }
}
