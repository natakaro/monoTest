using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public struct VertexPosition : IVertexType
    {
        public Vector3 Position;
        public VertexPosition(Vector3 position)
        {
            this.Position = position;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
        );
        
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }

    class Sky
    {
        private TextureCube skyBoxTexture;
        private Effect skyBoxEffect;
        private VertexBuffer skyBoxVertexBuffer;

        public Sky(string skyboxTexture, GraphicsDevice Device, ContentManager Content)
        {
            skyBoxTexture = Content.Load<TextureCube>(skyboxTexture);
            skyBoxEffect = Content.Load<Effect>("Effects/sky");
            CreateSkyboxVertexBuffer(Device);
        }

        private void CreateSkyboxVertexBuffer(GraphicsDevice device)
        {
            Vector3 forwardBottomLeft = new Vector3(-1, -1, -1);
            Vector3 forwardBottomRight = new Vector3(1, -1, -1);
            Vector3 forwardUpperLeft = new Vector3(-1, 1, -1);
            Vector3 forwardUpperRight = new Vector3(1, 1, -1);

            Vector3 backBottomLeft = new Vector3(-1, -1, 1);
            Vector3 backBottomRight = new Vector3(1, -1, 1);
            Vector3 backUpperLeft = new Vector3(-1, 1, 1);
            Vector3 backUpperRight = new Vector3(1, 1, 1);

            VertexPosition[] vertices = new VertexPosition[36];
            int i = 0;

            //face in front of the camera
            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(forwardUpperRight);

            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperRight);
            vertices[i++] = new VertexPosition(forwardBottomRight);

            //face to the right of the camera
            vertices[i++] = new VertexPosition(forwardBottomRight);
            vertices[i++] = new VertexPosition(forwardUpperRight);
            vertices[i++] = new VertexPosition(backUpperRight);

            vertices[i++] = new VertexPosition(forwardBottomRight);
            vertices[i++] = new VertexPosition(backUpperRight);
            vertices[i++] = new VertexPosition(backBottomRight);

            //face behind the camera
            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(backUpperRight);
            vertices[i++] = new VertexPosition(backUpperLeft);

            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(backBottomRight);
            vertices[i++] = new VertexPosition(backUpperRight);

            //face to the left of the camera
            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(backUpperLeft);
            vertices[i++] = new VertexPosition(forwardUpperLeft);

            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(forwardBottomLeft);

            //face above the camera
            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(backUpperLeft);
            vertices[i++] = new VertexPosition(backUpperRight);

            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(backUpperRight);
            vertices[i++] = new VertexPosition(forwardUpperRight);

            //face under the camera
            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(backBottomRight);
            vertices[i++] = new VertexPosition(backBottomLeft);

            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardBottomRight);
            vertices[i++] = new VertexPosition(backBottomRight);

            skyBoxVertexBuffer = new VertexBuffer(device, typeof(VertexPosition), vertices.Length, BufferUsage.WriteOnly);//new VertexBuffer(device, vertices.Length * VertexPosition.SizeInBytes, BufferUsage.WriteOnly);
            skyBoxVertexBuffer.SetData(vertices);
        }

        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            skyBoxEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(camera.Position));
            skyBoxEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            skyBoxEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            skyBoxEffect.Parameters["SkyBoxTexture"].SetValue(skyBoxTexture);

            skyBoxEffect.Techniques[0].Passes[0].Apply();
            graphicsDevice.SetVertexBuffer(skyBoxVertexBuffer);
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
