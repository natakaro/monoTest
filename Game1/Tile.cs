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
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.World = Matrix.CreateWorld(position, Vector3.Forward, Vector3.Up);
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }

                mesh.Draw();
            }
        }

        public Tile(Game1 game, Vector3 xyz) : base(game)
        {
            position = xyz;
        }
    }
}
