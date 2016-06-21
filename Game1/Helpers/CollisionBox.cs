using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.Helpers
{
    class CollisionBox
    {
        public static Vector3[] GetVertexElement(ModelMeshPart meshPart, VertexElementUsage usage)
        {
            VertexDeclaration vd = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] elements = vd.GetVertexElements();

            Func<VertexElement, bool> elementPredicate = ve => ve.VertexElementUsage == usage && ve.VertexElementFormat == VertexElementFormat.Vector3;
            if (!elements.Any(elementPredicate))
                return null;

            VertexElement element = elements.First(elementPredicate);

            Vector3[] vertexData = new Vector3[meshPart.NumVertices];
            meshPart.VertexBuffer.GetData((meshPart.VertexOffset * vd.VertexStride) + element.Offset,
                vertexData, 0, vertexData.Length, vd.VertexStride);

            return vertexData;
        }

        public static BoundingBox? GetBoundingBox(ModelMeshPart meshPart, Matrix transform)
        {
            if (meshPart.VertexBuffer == null)
                return null;

            Vector3[] positions = GetVertexElement(meshPart, VertexElementUsage.Position);
            if (positions == null)
                return null;

            Vector3[] transformedPositions = new Vector3[positions.Length];
            Vector3.Transform(positions, ref transform, transformedPositions);

            return BoundingBox.CreateFromPoints(transformedPositions);
        }

        public static BoundingBox CreateBoundingBox(Model model, Vector3 position, float scale)
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            BoundingBox result = new BoundingBox();
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BoundingBox? meshPartBoundingBox = GetBoundingBox(meshPart, boneTransforms[mesh.ParentBone.Index]);
                    if (meshPartBoundingBox != null)
                        result = BoundingBox.CreateMerged(result, meshPartBoundingBox.Value);
                }
            result.Max *= scale;
            result.Min *= scale;
            result.Max += position;
            result.Min += position;
            return result;
        }

        public static BoundingBox CreateBoundingBox(Model model, Vector3 position, float scale, Matrix rotation)
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            BoundingBox result = new BoundingBox();
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BoundingBox? meshPartBoundingBox = GetBoundingBox(meshPart, boneTransforms[mesh.ParentBone.Index]*rotation);
                    if (meshPartBoundingBox != null)
                        result = BoundingBox.CreateMerged(result, meshPartBoundingBox.Value);
                }
            
            result.Max *= scale;
            result.Min *= scale;
            result.Max += position;
            result.Min += position;
            return result;
        }

        public static BoundingBox CreateBoundingBox(AnimatedModel model, Vector3 position, float scale, Matrix rotation)
        {
            Matrix[] boneTransforms = model.Skeleton;
           // model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            BoundingBox result = new BoundingBox();
            foreach (ModelMesh mesh in model.Model.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BoundingBox? meshPartBoundingBox = GetBoundingBox(meshPart, boneTransforms[mesh.ParentBone.Index] * rotation);
                    if (meshPartBoundingBox != null)
                        result = BoundingBox.CreateMerged(result, meshPartBoundingBox.Value);
                }

            result.Max *= scale;
            result.Min *= scale;
            result.Max += position;
            result.Min += position;
            return result;
        }

    }
}