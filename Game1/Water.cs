using Game1.Sky;
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
    public class Water
    {
        GraphicsDevice graphicsDevice;
        ContentManager content;
        const float waterHeight = -225.0f;
        const float waterWidth = 10000.0f;
        const float waterLength = 10000.0f;
        RenderTarget2D reflectionTarget;
        Matrix reflectionViewMatrix;
        Effect waterEffect;
        VertexBuffer waterVertexBuffer;

        public Water(GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.graphicsDevice = graphicsDevice;
            this.content = content;
            reflectionTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
            waterEffect = content.Load<Effect>("Effects/Water");
            SetUpWaterVertices();
        }

        private Plane CreatePlane(float height, Vector3 planeNormalDirection, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
            if (clipSide)
                planeCoeffs *= -1;
            return new Plane(planeCoeffs);
        }

        private void SetUpWaterVertices()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            waterVertices[0] = new VertexPositionTexture(new Vector3(-waterWidth, waterHeight, -waterLength), new Vector2(0, 1));
            waterVertices[1] = new VertexPositionTexture(new Vector3(waterWidth, waterHeight, waterLength), new Vector2(1, 0));
            waterVertices[2] = new VertexPositionTexture(new Vector3(-waterWidth, waterHeight, waterLength), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(-waterWidth, waterHeight, -waterLength), new Vector2(0, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(waterWidth, waterHeight, -waterLength), new Vector2(1, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(waterWidth, waterHeight, waterLength), new Vector2(1, 0));

            waterVertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionTexture.VertexDeclaration, waterVertices.Count(), BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(waterVertices);
        }

        public void RenderReflectionMap(GameTime gameTime, Camera camera, SkyDome sky)
        {
            Vector3 reflectionCameraPosition = camera.Position;
            reflectionCameraPosition.Y = -camera.Position.Y + waterHeight * 2;
            //Vector3 reflectionCameraTargetPosition = camera.ViewDirection;
            //reflectionCameraTargetPosition.Y = -camera.ViewDirection.Y + waterHeight * 2;
            //Vector3 cameraRight = Vector3.Transform(new Vector3(1, 1, 0), Matrix.CreateFromQuaternion(camera.Orientation));
            //Vector3 invUpVector = Vector3.Cross(cameraRight, reflectionCameraTargetPosition - reflectionCameraPosition);

            //reflectionViewMatrix = Matrix.CreateLookAt(reflectionCameraPosition, reflectionCameraTargetPosition, invUpVector);
            

            Plane reflectionPlane = CreatePlane(waterHeight -1.25f, new Vector3(0, -1, 0), true);
            reflectionViewMatrix = Matrix.CreateReflection(reflectionPlane) * camera.ViewMatrix;
            //reflectionEffect.Parameters["ClipPlane"].SetValue(new Vector4(reflectionPlane.Normal, reflectionPlane.D));
            //reflectionEffect.Parameters["Clipping"].SetValue(true);

            RenderTargetBinding[] prevTargets = graphicsDevice.GetRenderTargets();

            graphicsDevice.SetRenderTarget(reflectionTarget);
            graphicsDevice.Clear(Color.Transparent);
            sky.Draw(gameTime, reflectionViewMatrix, reflectionCameraPosition);

            graphicsDevice.SetRenderTargets(prevTargets);

            //reflectionTarget.SaveAsPng(new System.IO.FileStream("../Tex.png", System.IO.FileMode.Create), 1280, 720);
        }

        public void DrawWater(Camera camera, float time)
        {
            Matrix worldMatrix = Matrix.Identity;
            waterEffect.Parameters["World"].SetValue(worldMatrix);
            waterEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            waterEffect.Parameters["ReflectionView"].SetValue(reflectionViewMatrix);
            waterEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            waterEffect.Parameters["ReflectionMap"].SetValue(reflectionTarget);
            //waterEffect.Parameters["xRefractionMap"].SetValue(refractionMap);

            waterEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.SetVertexBuffer(waterVertexBuffer);
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, waterVertexBuffer.VertexCount / 3);
        }
    }
}
