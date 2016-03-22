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

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>(id.ToString());
        }

        public void Draw(Camera camera)
        {
            temp = Matrix.CreateScale(Map.skala) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            model.Draw(Matrix.CreateScale(Map.skala) * Matrix.CreateTranslation(position) * camera.worldMatrix, camera.ViewMatrix, camera.ProjectionMatrix);
        }

        public Tile(Game1 game, Vector3 xyz, int id) : base(game)
        {
            position = xyz;
            this.id = id;
        }
    }
}
