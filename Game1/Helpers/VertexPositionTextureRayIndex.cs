using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionTextureRayIndex : IVertexType
    {
        public Vector3 Position;
        public Vector3 TextureCoordinateRayIndex;
        public static readonly VertexDeclaration VertexDeclaration;
        public VertexPositionTextureRayIndex(Vector3 position, Vector3 texcoordRayindex)
        {
            this.Position = position;
            this.TextureCoordinateRayIndex = texcoordRayindex;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
        public override int GetHashCode()
        {
            // TODO: Fix get hashcode
            return 0;
        }

        public override string ToString()
        {
            return "{{Position:" + this.Position + " TextureCoordinateRayIndex:" + this.TextureCoordinateRayIndex + "}}";
        }

        public static bool operator ==(VertexPositionTextureRayIndex left, VertexPositionTextureRayIndex right)
        {
            return ((left.Position == right.Position) && (left.TextureCoordinateRayIndex == right.TextureCoordinateRayIndex));
        }

        public static bool operator !=(VertexPositionTextureRayIndex left, VertexPositionTextureRayIndex right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((VertexPositionTextureRayIndex)obj));
        }

        static VertexPositionTextureRayIndex()
        {
            VertexElement[] elements = new VertexElement[] { new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0) };
            VertexDeclaration declaration = new VertexDeclaration(elements);
            VertexDeclaration = declaration;
        }
    }
}
