using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Helpers
{
    public partial class QuadRenderComponent
    {
        #region Private Members        
        VertexPositionTextureRayIndex[] verts = null;
        short[] ib = null;
        IGraphicsDeviceService graphicsService;
        GraphicsDevice device;

        #endregion

        #region Constructor
        public QuadRenderComponent(Game game, IGraphicsDeviceService graphicsService)
        {
            this.graphicsService = graphicsService;
            this.device = graphicsService.GraphicsDevice;
        }
        #endregion

        #region LoadGraphicsContent

        public void LoadContent()
        {
            verts = new VertexPositionTextureRayIndex[]
                    {
                            new VertexPositionTextureRayIndex(
                                new Vector3(0,0,0),
                                new Vector3(1,1,2)),
                            new VertexPositionTextureRayIndex(
                                new Vector3(0,0,0),
                                new Vector3(0,1,3)),
                            new VertexPositionTextureRayIndex(
                                new Vector3(0,0,0),
                                new Vector3(0,0,0)),
                            new VertexPositionTextureRayIndex(
                                new Vector3(0,0,0),
                                new Vector3(1,0,1))
                    };

            ib = new short[] { 0, 1, 2, 2, 3, 0 };

        }
        #endregion

        #region void Render(Vector2 v1, Vector2 v2)
        public void Render(Vector2 v1, Vector2 v2)
        {
            verts[0].Position.X = v2.X;
            verts[0].Position.Y = v1.Y;

            verts[1].Position.X = v1.X;
            verts[1].Position.Y = v1.Y;

            verts[2].Position.X = v1.X;
            verts[2].Position.Y = v2.Y;

            verts[3].Position.X = v2.X;
            verts[3].Position.Y = v2.Y;

            device.DrawUserIndexedPrimitives<VertexPositionTextureRayIndex>
                (PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }

        public void Render()
        {
            verts[0].Position.X = 1;
            verts[0].Position.Y = -1;

            verts[1].Position.X = -1;
            verts[1].Position.Y = -1;

            verts[2].Position.X = -1;
            verts[2].Position.Y = 1;

            verts[3].Position.X = 1;
            verts[3].Position.Y = 1;

            device.DrawUserIndexedPrimitives<VertexPositionTextureRayIndex>
                (PrimitiveType.TriangleList, verts, 0, 4, ib, 0, 2);
        }
        #endregion
    }
}
