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
using System.Diagnostics;
using Game1.HUD;

namespace Game1.Spells
{
    class SpellFireballProjectile : DrawableObject
    {
        Texture2D texture;
        private PointLight pointLight;
        LightManager lightManager;
        HUDManager hudManager;
        Stopwatch stopwatch;

        public event EventHandler hitEvent;

        public new void Draw(Camera camera)
        {
            //model.Draw(camera.worldMatrix * Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["FarClip"].SetValue(camera.FarZ);
                    effect.Parameters["Texture"].SetValue(texture);
                    effect.Parameters["Clipping"].SetValue(false);
                }
                mesh.Draw();
            }
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            pointLight.Position = position;
            BoundingSphere pointLightSphere = pointLight.BoundingSphere;
            pointLightSphere.Center = position;

            if (stopwatch.ElapsedMilliseconds > 5000)
                Destroy();

            return ret;
        }

        public SpellFireballProjectile(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, Texture2D inTexture, LightManager lightManager, HUDManager hudManager) : base(game, inWorldMatrix, inModel, octree)
        {
            this.lightManager = lightManager;
            this.hudManager = hudManager;
            texture = inTexture;
            m_static = false;
            boundingSphere = new BoundingSphere(position, 1f);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            type = ObjectType.Projectile;

            pointLight = new PointLight(position, Color.OrangeRed, 25, 5);
            lightManager.AddLight(pointLight);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            hitEvent += hudManager.Crosshair.HandleHitEvent;
        }

        public override void HandleIntersection(IntersectionRecord ir)
        {
            if (ir.DrawableObjectObject != null)
            {
                if (ir.DrawableObjectObject.Type == ObjectType.Terrain)
                {
                    OnHitEvent();
                    Destroy();
                }
            }
        }

        private void Destroy()
        {
            hitEvent -= hudManager.Crosshair.HandleHitEvent;
            lightManager.RemoveLight(pointLight);
            Alive = false;
        }

        public void OnHitEvent()
        {
            hitEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
