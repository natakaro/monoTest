using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    class OctreeNode
    {
        private const int maxObjectsInNode = 1; //max models in a node before it splits up into eight children
        private const float minSize = 5.0f; //min size for splitting up

        private Vector3 center;
        private float size;
        List<DrawableModel> modelList; //list of the 5 models
        private BoundingBox nodeBoundingBox;

        OctreeNode nodeUFL;
        OctreeNode nodeUFR;
        OctreeNode nodeUBL;
        OctreeNode nodeUBR;
        OctreeNode nodeDFL;
        OctreeNode nodeDFR;
        OctreeNode nodeDBL;
        OctreeNode nodeDBR;
        List<OctreeNode> childList;

        private static int modelsDrawn;
        private static int modelsStoredInQuadTree;

        public OctreeNode(Vector3 center, float size)
        {
            this.center = center;
            this.size = size;
            modelList = new List<DrawableModel>();
            childList = new List<OctreeNode>(8);

            Vector3 diagonalVector = new Vector3(size / 2.0f, size / 2.0f, size / 2.0f);
            nodeBoundingBox = new BoundingBox(center - diagonalVector, center + diagonalVector);
        }

        #region Public Methods
        public int Add(Model model, Matrix worldMatrix)
        {
            DrawableModel newDModel = new DrawableModel(model, worldMatrix, modelsStoredInQuadTree++);
            AddDrawableModel(newDModel);
            return newDModel.ModelID;
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix, BoundingFrustum cameraFrustum)
        {
            ContainmentType cameraNodeContainment = cameraFrustum.Contains(nodeBoundingBox);
            if (cameraNodeContainment != ContainmentType.Disjoint)
            {
                foreach (DrawableModel dModel in modelList)
                {
                    dModel.Draw(viewMatrix, projectionMatrix);
                    modelsDrawn++;
                }

                foreach (OctreeNode childNode in childList)
                    childNode.Draw(viewMatrix, projectionMatrix, cameraFrustum);
            }

        }

        public void DrawBounds(Matrix viewMatrix, Matrix projectionMatrix)
        {
            DebugShapeRenderer.AddBoundingBox(nodeBoundingBox, Color.Yellow);

            foreach (DrawableModel dModel in modelList)
                DebugShapeRenderer.AddBoundingBox(dModel.BoundingBox, Color.Blue);

            foreach (OctreeNode childNode in childList)
                childNode.DrawBounds(viewMatrix, projectionMatrix);
        }

        public void DrawInstancing(List<DrawableModel> insta, BoundingFrustum cameraFrustum)
        {
            ContainmentType cameraNodeContainment = cameraFrustum.Contains(nodeBoundingBox);
            if (cameraNodeContainment != ContainmentType.Disjoint)
            {
                foreach (DrawableModel dModel in modelList)
                {
                    insta.Add(dModel);
                    modelsDrawn++;
                }

                foreach (OctreeNode childNode in childList)
                    childNode.DrawInstancing(insta, cameraFrustum);
            }
        }

        public void UpdateModelWorldMatrix(int modelID, Matrix newWorldMatrix)
        {
            DrawableModel deletedModel = RemoveDrawableModel(modelID);
            deletedModel.WorldMatrix = newWorldMatrix;
            AddDrawableModel(deletedModel);
        }

        /// <summary>
        /// Returns a list of models in the node that collide with the bounding sphere
        /// </summary>
        /// <param name="bSphere">The bounding sphere you want to check collisions on</param>
        /// <param name="collidingList">The list of models</param>
        /// <returns></returns>
        public List<DrawableModel> GetIntersection(BoundingSphere bSphere, List<DrawableModel> collidingList)
        {
            ContainmentType sphereContainment = bSphere.Contains(nodeBoundingBox);
            if (sphereContainment != ContainmentType.Disjoint)
            {
                foreach (DrawableModel dModel in modelList)
                {
                    if (dModel.BoundingBox.Intersects(bSphere))
                        collidingList.Add(dModel);
                }

                foreach (OctreeNode childNode in childList)
                    childNode.GetIntersection(bSphere, collidingList);

                return collidingList;
            }
            return collidingList;
        }

        public DrawableModel GetIntersection(BoundingSphere bSphere)
        {
            ContainmentType sphereContainment = bSphere.Contains(nodeBoundingBox);
            if (sphereContainment != ContainmentType.Disjoint)
            {
                foreach (DrawableModel dModel in modelList)
                {
                    return dModel;
                }

                foreach (OctreeNode childNode in childList)
                    childNode.GetIntersection(bSphere);

            }
            return null;
        }
        #endregion

        #region Private Methods
        private void CreateChildNodes()
        {
            float sizeOver2 = size / 2.0f;
            float sizeOver4 = size / 4.0f;

            nodeUFR = new OctreeNode(center + new Vector3(sizeOver4, sizeOver4, -sizeOver4), sizeOver2);
            nodeUFL = new OctreeNode(center + new Vector3(-sizeOver4, sizeOver4, -sizeOver4), sizeOver2);
            nodeUBR = new OctreeNode(center + new Vector3(sizeOver4, sizeOver4, sizeOver4), sizeOver2);
            nodeUBL = new OctreeNode(center + new Vector3(-sizeOver4, sizeOver4, sizeOver4), sizeOver2);
            nodeDFR = new OctreeNode(center + new Vector3(sizeOver4, -sizeOver4, -sizeOver4), sizeOver2);
            nodeDFL = new OctreeNode(center + new Vector3(-sizeOver4, -sizeOver4, -sizeOver4), sizeOver2);
            nodeDBR = new OctreeNode(center + new Vector3(sizeOver4, -sizeOver4, sizeOver4), sizeOver2);
            nodeDBL = new OctreeNode(center + new Vector3(-sizeOver4, -sizeOver4, sizeOver4), sizeOver2);

            childList.Add(nodeUFR);
            childList.Add(nodeUFL);
            childList.Add(nodeUBR);
            childList.Add(nodeUBL);
            childList.Add(nodeDFR);
            childList.Add(nodeDFL);
            childList.Add(nodeDBR);
            childList.Add(nodeDBL);
        }

        private void AddDrawableModel(DrawableModel dModel)
        {
            if(childList.Count == 0)
            {
                modelList.Add(dModel);

                bool maxObjectsReached = (modelList.Count > maxObjectsInNode);
                bool minSizeNotReached = (size > minSize);
                if(maxObjectsReached && minSizeNotReached)
                {
                    CreateChildNodes();
                    foreach(DrawableModel currentDModel in modelList)
                    {
                        Distribute(currentDModel);
                    }
                    modelList.Clear();
                }
            }
            else
            {
                Distribute(dModel);
            }
        }

        private DrawableModel RemoveDrawableModel(int modelID)
        {
            DrawableModel dModel = null;

            for (int i = 0; i < modelList.Count; i++)
            {
                if (modelList[i].ModelID == modelID)
                {
                    dModel = modelList[i];
                    modelList.Remove(dModel);
                }
            }

            int child = 0;
            while ((dModel == null) && ( child < childList.Count))
            {
                dModel = childList[child++].RemoveDrawableModel(modelID);
            }

            return dModel;
        }

        private void Distribute(DrawableModel dModel)
        {
            Vector3 position = dModel.Position;
            if (position.Y > center.Y)                      //Up
                if (position.Z < center.Z)                   //Forward
                    if (position.X < center.X)                //Left
                        nodeUFL.AddDrawableModel(dModel);
                    else                                      //Right
                        nodeUFR.AddDrawableModel(dModel);
                else                                         //Back
                    if (position.X < center.X)                //Left
                        nodeUBL.AddDrawableModel(dModel);
                    else                                      //Right
                        nodeUBR.AddDrawableModel(dModel);
            else                                            //Down
                if (position.Z < center.Z)                   //Forward
                    if (position.X < center.X)                //Left
                        nodeDFL.AddDrawableModel(dModel);
                    else                                      //Right
                        nodeDFR.AddDrawableModel(dModel);
                else                                         //Back
                    if (position.X < center.X)                //Left
                        nodeDBL.AddDrawableModel(dModel);
                    else                                      //Right
                        nodeDBR.AddDrawableModel(dModel);
        }
        #endregion

        #region Properties
        public int ModelsDrawn
        {
            get { return modelsDrawn; }
            set { modelsDrawn = value; }
        }
        #endregion
    }
}
