using Game1.Helpers;
using Game1.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class Turret : DrawableObject
    {
        Texture2D texture;
        private PointLight pointLight;
        LightManager lightManager;

        public new void Draw(Camera camera)
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
        public Turret(Game game, Matrix inWorldMatrix, Model inModel, Octree octree, Texture2D inTexture, LightManager lightManager) : base(game, inWorldMatrix, inModel, octree)
        {
            this.lightManager = lightManager;
            texture = inTexture;

            type = ObjectType.Turret;

            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);

            pointLight = new PointLight(position + new Vector3(0, 30, 0), Color.Violet, 50, 10);
            lightManager.AddLight(pointLight);
        }

        public void Destroy()
        {
            lightManager.RemoveLight(pointLight);
            Alive = false;
        }
    }
}
