using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Game1.Spells
{
    class SpellFireballProjectile : DrawableObject
    {
        Texture2D texture;
        Model model;

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>("fireball");
            effect = contentManager.Load<Effect>("Effects/test");
            texture = contentManager.Load<Texture2D>("firedot");
        }

        public override void Draw(Camera camera)
        {
            model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
        }

        public override void DrawDeferred(Camera camera)
        {
            model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
        }

        public SpellFireballProjectile(Game game, Matrix inWorldMatrix) : base(game, inWorldMatrix)
        {
            m_static = false;
            boundingSphere = new BoundingSphere(position, 1f);
            boundingBox = new BoundingBox(position - new Vector3(1, 1, 1), position + new Vector3(1, 1, 1)); // na oko wartosci, koniecznie wprowadzic poprawne!!
            type = ObjectType.Projectile;

            Initialize(game.Content);
        }

        public override void HandleIntersection(IntersectionRecord ir)
        {
            if (ir.OtherDrawableObjectObject.Type == ObjectType.Terrain)
                Alive = false;
        }
    }
}
