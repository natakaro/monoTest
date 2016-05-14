using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Game1.Helpers;
using Game1.Lights;

namespace Game1.Spells
{
    class SpellFireballProjectile : DrawableObject
    {
        Texture2D texture;
        private PointLight pointLight;
        LightManager lightManager;

        public override void Draw(Camera camera)
        {
            //model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);
            pointLight.Position = position;
            return ret;
        }

        public SpellFireballProjectile(Game game, Matrix inWorldMatrix, Model inModel, Texture2D inTexture, LightManager lightManager) : base(game, inWorldMatrix, inModel)
        {
            this.lightManager = lightManager;
            texture = inTexture;
            m_static = false;
            boundingSphere = new BoundingSphere(position, 1f);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            type = ObjectType.Projectile;

            pointLight = new PointLight(position, Color.OrangeRed, 25, 1);
            lightManager.AddLight(pointLight);
        }

        public override void HandleIntersection(IntersectionRecord ir)
        {
            if (ir.OtherDrawableObjectObject.Type == ObjectType.Terrain)
                Destroy();
        }

        private void Destroy()
        {
            Alive = false;
            lightManager.RemoveLight(pointLight);
        }
    }
}
