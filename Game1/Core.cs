using Game1.Helpers;
using Game1.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class Core : DrawableObject
    {
        Texture2D texture;
        const int portalParticlesPerFrame = 3;

        public Core(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, Texture2D inTexture) : base(game, inWorldMatrix, inModel, octree)
        {
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);
            texture = inTexture;

            type = ObjectType.Core;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
        }

        public override void Draw(Camera camera)
        {
            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    foreach (Effect effect in mesh.Effects)
            //    {
            //        effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
            //        effect.Parameters["View"].SetValue(camera.ViewMatrix);
            //        effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            //        effect.Parameters["FarClip"].SetValue(camera.FarZ);
            //        effect.Parameters["Texture"].SetValue(texture);
            //        effect.Parameters["Clipping"].SetValue(false);
            //    }
            //    mesh.Draw();
            //}
        }

        public override bool Update(GameTime gameTime)
        {
            bool ret = base.Update(gameTime);

            for (int i = 0; i < portalParticlesPerFrame; i++)
            {
                GameplayScreen.particleManager.portalParticlesFriendly.AddParticle(position + RandomPointOnCircle(40, 80), Vector3.Zero);
            }

            return ret;
        }

        public static Vector3 RandomPointOnCircle(float radius, float height)
        {
            double angle = GameplayScreen.random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle) / 2;
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }
    }
}
