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
        Model model;
        Vector3 position;

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>("1");
        }

        public void Draw(Camera camera)
        {
            model.Draw(Matrix.CreateTranslation(position) * camera.worldMatrix, camera.ViewMatrix, camera.ProjectionMatrix);
        }

        public Tile(Game1 game, Vector3 xyz) : base(game)
        {
            position = xyz;
        }
    }
}
