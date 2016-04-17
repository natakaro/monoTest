﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class TestBox : DrawableObject
    {

        Texture2D texture;
        Model model;

        public void Initialize(ContentManager contentManager)
        {
            model = contentManager.Load<Model>("Monocube");
            texture = contentManager.Load<Texture2D>("MonoCubeTexture");
        }


        public override void Draw(Camera camera)
        {
            model.Draw(camera.worldMatrix*Matrix.CreateScale(25)*Matrix.CreateTranslation(position), camera.viewMatrix, camera.projMatrix);
            //worldMatrix = Matrix.CreateScale(Map.scale) * Matrix.CreateTranslation(position) * camera.worldMatrix;
            /*foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * worldMatrix);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    effect.Parameters["WorldInverseTranspose"].SetValue(
                                            Matrix.Transpose(camera.worldMatrix * mesh.ParentBone.Transform));
                    effect.Parameters["Texture"].SetValue(texture);
                }
                mesh.Draw();
            }*/
        }

        public TestBox(Game game, Matrix inWorldMatrix) : base(game, inWorldMatrix)
        {
            position = new Vector3(-40, 0, -40);
            Initialize(game.Content);
            boundingSphere = new BoundingSphere(position, model.Meshes[0].BoundingSphere.Radius * 25);
            boundingBox = new BoundingBox(position - new Vector3(35, 35, 35), position + new Vector3(35, 35, 35)); // na oko wartosci, koniecznie wprowadzic poprawne!!
            type = ObjectType.Item;
            m_static = false;
            //acceleration = new Vector3(3);
        }
    }
}
