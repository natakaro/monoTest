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

        public void Initialize(ContentManager contentManager)
        {
            texture = contentManager.Load<Texture2D>("firedot");
        }

        public override void Draw(Camera camera)
        {
            //model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * Matrix.CreateTranslation(position));
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        public SpellFireballProjectile(Game game, Matrix inWorldMatrix, Model inModel) : base(game, inWorldMatrix, inModel)
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
