using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    class Enemy : DrawableObject
    {
        Ray positionray;
        float gravity = 100f;
        float distance;

        float speed = 70;
        float a;
        float b;
        public float feetheight;
        public Enemy(Game game, Matrix inWorldMatrix, Model inModel) : base(game, inWorldMatrix, inModel)
        {
            m_instanced = true;
            //boundingSphere = new BoundingSphere(position, Map.scale * 0.75f);

            type = ObjectType.Unit;
            boundingBox = CollisionBox.CreateBoundingBox(model, position, 1);
            positionray = new Ray(new Vector3(position.X, boundingBox.Min.Y, position.Z), Vector3.Down);

            a = boundingBox.Max.Y - boundingBox.Min.Y;
            b = position.Y - boundingBox.Min.Y;
        }

        public bool Update(GameTime gameTime, Camera camera, Octree octree)
        {
            
            Vector3 temp;
            temp = Vector3.Normalize(camera.Position - position);
            Vector3 temp2;
            temp2 = new Vector3(temp.X, 0, temp.Z);
            position += speed * temp2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            boundingBox.Max += speed * temp2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            boundingBox.Min += speed * temp2 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            IntersectionRecord ir = octree.HighestIntersection(this, ObjectType.Terrain);
            
            if (ir != null && ir.DrawableObjectObject != null)//..ujowy if ale działa
            {
                distance = boundingBox.Min.Y - ir.DrawableObjectObject.BoundingBox.Max.Y;
                boundingBox.Min.Y -= distance * gravity/10 * (float)(gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (ir.DrawableObjectObject == null)
                boundingBox.Min.Y -= gravity*(float)(gameTime.ElapsedGameTime.TotalSeconds);

            

            boundingBox.Max.Y = boundingBox.Min.Y + a;
            position.Y = boundingBox.Min.Y + b;
            worldMatrix = Matrix.CreateTranslation(position);

            return true;

        }



        public override void Draw(Camera camera)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(modelBones[mesh.ParentBone.Index] * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                }
                mesh.Draw();
            }
        }
    }
}
