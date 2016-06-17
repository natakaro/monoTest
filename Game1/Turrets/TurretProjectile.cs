using Game1.Helpers;
using Game1.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Turrets
{
    class TurretProjectile : DrawableObject
    {
        private Texture2D texture;
        private PointLight pointLight;
        private LightManager lightManager;
        private ObjectManager objectManager;
        private Stopwatch stopwatch;
        private float damage;

        public override void Draw(Camera camera)
        {
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

        public TurretProjectile(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, ObjectManager objectManager, Texture2D inTexture, LightManager lightManager, float damage) : base(game, inWorldMatrix, inModel, octree)
        {
            this.lightManager = lightManager;
            this.objectManager = objectManager;
            texture = inTexture;
            m_static = false;
            boundingSphere = new BoundingSphere(position, 1f);
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            type = ObjectType.Projectile;

            pointLight = new PointLight(position, Color.OrangeRed, 25, 5);
            lightManager.AddLight(pointLight);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            this.damage = damage;
        }

        public override void HandleIntersection(IntersectionRecord ir)
        {
            if (ir.DrawableObjectObject != null)
            {
                if (ir.DrawableObjectObject.Type == ObjectType.Enemy)
                {
                    Enemy hitEnemy = (Enemy)ir.DrawableObjectObject;
                    hitEnemy.Damage(damage);
                    Destroy();
                }

                else if (ir.DrawableObjectObject.Type == ObjectType.Tile)
                {
                    Destroy();
                }
            }
        }

        private void Destroy()
        {
            lightManager.RemoveLight(pointLight);
            Alive = false;
            objectManager.Remove(this);
        }
    }
}
