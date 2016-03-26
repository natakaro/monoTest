using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class DrawableModel
    {
        protected Matrix worldMatrix;
        protected Model model;
        protected Vector3 position;
        protected int modelID;
        protected Texture2D texture;

        public DrawableModel(Model inModel, Matrix inWorldMatrix, int inModelID)
        {
            model = inModel;
            worldMatrix = inWorldMatrix;
            modelID = inModelID;
            position = new Vector3(inWorldMatrix.M41, inWorldMatrix.M42, inWorldMatrix.M43);
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = mesh.ParentBone.Transform * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }
        }


        #region Accessors
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                //boundingSphere.Center = value;
            }
        }

        //public BoundingBox BoundingBox
        //{
        //    get
        //    {
        //        return boundingBox;
        //    }
        //    set
        //    {
        //        boundingBox = value;
        //    }
        //}

        //public BoundingSphere BoundingSphere
        //{
        //    get
        //    {
        //        return boundingSphere;
        //    }
        //    set
        //    {
        //        boundingSphere = value;
        //    }
        //}

        public Model Model { get; }
        public int ModelID { get; }
        public Matrix WorldMatrix
        {
            get
            {
                return worldMatrix;
            }
            set
            {
                worldMatrix = value;
                position = new Vector3(value.M41, value.M42, value.M43);
            }
        }
        #endregion
    }
}
