using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class Sky : DrawableObject
    {

        Texture2D texture;

        public void Initialize(ContentManager contentManager)
        {
            texture = contentManager.Load<Texture2D>("MonoCubeTexture");
        }


        public override void Draw(Camera camera)
        {
            //worldMatrix = Matrix.CreateScale(Map.scale) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(camera.worldMatrix * Matrix.CreateTranslation(position));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        public Sky(Game game, Matrix inWorldMatrix, Model inModel) : base(game, inWorldMatrix, inModel)
        {
            position = new Vector3(0, 0, 0);
            boundingSphere = new BoundingSphere(position, 25);
            Initialize(game.Content);
            type = ObjectType.Ethereal;
        }
    }
}
