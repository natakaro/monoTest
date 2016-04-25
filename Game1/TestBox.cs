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
    class TestBox : DrawableObject
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

        public override bool Update(GameTime gameTime)
        {
            if (!m_static)
            {
                lastPosition = position;
                velocity += acceleration * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                position += velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds);
                boundingSphere.Center = position;
                boundingBox = new BoundingBox(position - new Vector3(35, 35, 35), position + new Vector3(35, 35, 35));
                return lastPosition != position;    //lets you know if the object actually moved relative to its last position
            }

            return false;
        }

        public TestBox(Game game, Matrix inWorldMatrix) : base(game, inWorldMatrix)
        {
            position = new Vector3(-40, 0, -40);
            Initialize(game.Content);
            boundingSphere = new BoundingSphere(position, model.Meshes[0].BoundingSphere.Radius * 25);
            boundingBox = new BoundingBox(position - new Vector3(45, 45, 45), position + new Vector3(45, 45, 45)); // na oko wartosci, koniecznie wprowadzic poprawne!!
            type = ObjectType.Item;
            m_static = false;
            acceleration = new Vector3(0, 1f, 0);
        }
    }
}
