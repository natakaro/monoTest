using Game1.Helpers;
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
        QuadRenderComponent quadRenderer;
        const float waterHeight = 5;
        RenderTarget2D reflectionTarget;
        Matrix reflectionViewMatrix;
        Effect waterEffect;
        Texture2D normalMap;
        Texture2D heightMap;
        Texture2D foamMap;
        Vector3 windDirection = new Vector3(1, 0, 0);

        public Water(GraphicsDevice graphicsDevice, ContentManager content, QuadRenderComponent quadRenderer)
        {
            this.graphicsDevice = graphicsDevice;
            this.content = content;
            this.quadRenderer = quadRenderer;
            reflectionTarget = new RenderTarget2D(graphicsDevice, graphicsDevice.PresentationParameters.BackBufferWidth / 2, graphicsDevice.PresentationParameters.BackBufferHeight / 2, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
            waterEffect = content.Load<Effect>("Effects/Water");
            normalMap = content.Load<Texture2D>("Textures/waternormal");
            heightMap = content.Load<Texture2D>("Textures/waterheight");
            foamMap = content.Load<Texture2D>("Textures/waterfoam2");
        }

        public void RenderReflectionMap(GameTime gameTime, Camera camera, SkyDome sky)
        {
            Vector3 reflectionCameraPosition = camera.Position;
            reflectionCameraPosition.Y = -camera.Position.Y + waterHeight * 2;

            Plane reflectionPlane = new Plane(new Vector3(0, 1, 0), -waterHeight);
            reflectionViewMatrix = Matrix.CreateReflection(reflectionPlane) * camera.ViewMatrix;

            RenderTargetBinding[] prevTargets = graphicsDevice.GetRenderTargets();

            graphicsDevice.SetRenderTarget(reflectionTarget);
            graphicsDevice.Clear(Color.Transparent);
            sky.Draw(gameTime, reflectionViewMatrix, reflectionCameraPosition);

            graphicsDevice.SetRenderTargets(prevTargets);

            //reflectionTarget.SaveAsPng(new System.IO.FileStream("../Tex.png", System.IO.FileMode.Create), 1280, 720);
        }

        public void DrawWater(Camera camera, float time, RenderTarget2D colorTarget, RenderTarget2D depthTarget, Vector3 lightDirection, Vector3 lightColor)
        {
            Matrix worldMatrix = Matrix.Identity;
            //waterEffect.Parameters["World"].SetValue(Matrix.Identity));
            //waterEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            //waterEffect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            waterEffect.Parameters["ViewProjection"].SetValue(camera.ViewProjectionMatrix);
            waterEffect.Parameters["InvertView"].SetValue(Matrix.Invert(camera.ViewMatrix));
            waterEffect.Parameters["FrustumCornersVS"].SetValue(camera.FrustumCorners);
            waterEffect.Parameters["cameraPos"].SetValue(camera.Position);

            waterEffect.Parameters["colorMap"].SetValue(colorTarget);
            waterEffect.Parameters["depthMap"].SetValue(depthTarget);
            waterEffect.Parameters["reflectionMap"].SetValue(reflectionTarget);

            waterEffect.Parameters["normalMap"].SetValue(normalMap);
            waterEffect.Parameters["heightMap"].SetValue(heightMap);
            waterEffect.Parameters["foamMap"].SetValue(foamMap);

            waterEffect.Parameters["waterLevel"].SetValue(waterHeight);
            waterEffect.Parameters["timer"].SetValue(time);
            waterEffect.Parameters["lightDir"].SetValue(lightDirection);
            waterEffect.Parameters["sunColor"].SetValue(lightColor);

            waterEffect.CurrentTechnique.Passes[0].Apply();
            quadRenderer.Render();
        }
    }
}
