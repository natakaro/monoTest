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
        GameSettings settings;
        QuadRenderComponent quadRenderer;
        const float waterHeight = 5;
        RenderTarget2D reflectionTarget;
        RenderTarget2D colorTarget;
        RenderTarget2D normalTarget;
        RenderTarget2D depthTarget;
        RenderTarget2D lightTarget;
        Matrix reflectionViewMatrix;
        Effect waterEffect;
        Effect clearBufferEffect;
        Effect lightEffect;
        Effect finalCombineEffect;
        Texture2D normalMap;
        Texture2D heightMap;
        Texture2D foamMap;
        Vector3 windDirection = new Vector3(1, 0, 0);
        List<IntersectionRecord> frustumIntersections;
        List<IntersectionRecord> frustumInstancedIntersections;

        public Water(GraphicsDevice graphicsDevice, ContentManager content, GameSettings settings, QuadRenderComponent quadRenderer)
        {
            this.graphicsDevice = graphicsDevice;
            this.content = content;
            this.quadRenderer = quadRenderer;
            this.settings = settings;
            int backbufferWidth = graphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = graphicsDevice.PresentationParameters.BackBufferHeight;
            int targetScale = 4;
            reflectionTarget = new RenderTarget2D(graphicsDevice, backbufferWidth / targetScale, backbufferHeight / targetScale, false, SurfaceFormat.HdrBlendable, graphicsDevice.PresentationParameters.DepthStencilFormat);
            colorTarget = new RenderTarget2D(graphicsDevice, backbufferWidth / targetScale, backbufferHeight / targetScale, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
            normalTarget = new RenderTarget2D(graphicsDevice, backbufferWidth / targetScale, backbufferHeight / targetScale, false, SurfaceFormat.Color, DepthFormat.None);
            depthTarget = new RenderTarget2D(graphicsDevice, backbufferWidth / targetScale, backbufferHeight / targetScale, false, SurfaceFormat.Single, DepthFormat.None);
            lightTarget = new RenderTarget2D(graphicsDevice, backbufferWidth / targetScale, backbufferHeight / targetScale, false, SurfaceFormat.HdrBlendable, DepthFormat.None);

            waterEffect = content.Load<Effect>("Effects/Water");
            normalMap = content.Load<Texture2D>("Textures/waternormal");
            //heightMap = content.Load<Texture2D>("Textures/waterheight");
            heightMap = new Texture2D(graphicsDevice, 512, 512, false, SurfaceFormat.Color, 100);
            for(int i = 0; i < 100; i++)
            {
                Texture2D layer = content.Load<Texture2D>("Textures/Waves/" + i.ToString("D2"));
                heightMap.SetData(0, i, null, layer.GetPixels(), 0, 262144);
            }
            foamMap = content.Load<Texture2D>("Textures/waterfoam2");

            lightEffect = content.Load<Effect>("Effects/DirectionalLight");
            finalCombineEffect = content.Load<Effect>("Effects/CombineFinal");
        }

        public void RenderReflectionMap(GameTime gameTime, Camera camera, SkyDome sky, Vector3 lightDirection, Vector3 lightColor, float skyIntensity, Octree octree, InstancingManager instancingManager)
        {
            Vector3 reflectionCameraPosition = camera.Position;
            reflectionCameraPosition.Y = -camera.Position.Y + waterHeight * 2;

            Plane reflectionPlane = new Plane(new Vector3(0, 1, 0), -waterHeight);
            reflectionViewMatrix = Matrix.CreateReflection(reflectionPlane) * camera.ViewMatrix;

            RenderTargetBinding[] prevTargets = graphicsDevice.GetRenderTargets();

            graphicsDevice.SetRenderTargets(colorTarget, normalTarget, depthTarget);

            sky.Draw(gameTime, reflectionViewMatrix, reflectionCameraPosition);

            if (settings.Reflect)
            {
                graphicsDevice.BlendState = BlendState.Opaque;
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;

                Vector4 clipPlane = new Vector4(reflectionPlane.Normal, reflectionPlane.D);

                //Renders all visible objects by iterating through the oct tree recursively and testing for intersection 
                //with the current camera view frustum
                frustumIntersections = octree.AllIntersections(new BoundingFrustum(reflectionViewMatrix * camera.ProjectionMatrix));
                frustumInstancedIntersections = frustumIntersections.FindAll(ir => ir.DrawableObjectObject.IsInstanced == true);
                frustumIntersections.RemoveAll(ir => ir.DrawableObjectObject.IsInstanced == true);
                instancingManager.DrawModelHardwareInstancing(frustumInstancedIntersections, reflectionViewMatrix, clipPlane);
                foreach (IntersectionRecord ir in frustumIntersections)
                {
                    // ir.DrawableObjectObject.UpdateLOD(camera);
                    ir.DrawableObjectObject.Draw(camera, reflectionViewMatrix, clipPlane);
                }

                //lighting pass
                graphicsDevice.SetRenderTarget(lightTarget);
                graphicsDevice.Clear(Color.Transparent);

                //get camera frustum corners
                Vector3[] farFrustumCornersVS = new Vector3[4];
                BoundingFrustum Frustum = new BoundingFrustum(reflectionViewMatrix * camera.ProjectionMatrix);
                Vector3[] frustumCornersWS = Frustum.GetCorners();
                for (int i = 0; i < 4; i++)
                {
                    farFrustumCornersVS[i] = Vector3.Transform(frustumCornersWS[i + 4], reflectionViewMatrix);
                }

                lightEffect.Parameters["colorMap"].SetValue(colorTarget);
                lightEffect.Parameters["normalMap"].SetValue(normalTarget);
                lightEffect.Parameters["depthMap"].SetValue(depthTarget);

                lightEffect.Parameters["LightDirection"].SetValue(lightDirection);
                lightEffect.Parameters["LightColor"].SetValue(lightColor);
                lightEffect.Parameters["SkyIntensity"].SetValue(skyIntensity);

                lightEffect.Parameters["CameraPosWS"].SetValue(reflectionCameraPosition);
                lightEffect.Parameters["InvertView"].SetValue(Matrix.Invert(reflectionViewMatrix));

                lightEffect.Parameters["FrustumCornersVS"].SetValue(farFrustumCornersVS);

                lightEffect.Techniques[0].Passes[0].Apply();
                quadRenderer.Render();

                //Combine everything
                graphicsDevice.SetRenderTarget(reflectionTarget);
                graphicsDevice.Clear(Color.Transparent);

                finalCombineEffect.Parameters["colorMap"].SetValue(colorTarget);
                finalCombineEffect.Parameters["lightMap"].SetValue(lightTarget);

                finalCombineEffect.Techniques[0].Passes[0].Apply();
                //render a full-screen quad
                quadRenderer.Render();
            }

            graphicsDevice.SetRenderTargets(prevTargets);

            //reflectionTarget.SaveAsPng(new System.IO.FileStream("../Tex.png", System.IO.FileMode.Create), 1280, 720);
        }

        public void DrawWater(Camera camera, float time, RenderTarget2D colorTarget, RenderTarget2D depthTarget, Vector3 lightDirection, Vector3 lightColor)
        {
            graphicsDevice.SamplerStates[0] = SamplerStateUtility.ColorMap;
            graphicsDevice.SamplerStates[1] = SamplerStateUtility.WaterReflectionNormalMap;
            graphicsDevice.SamplerStates[2] = SamplerStateUtility.DepthMap;
            graphicsDevice.SamplerStates[3] = SamplerStateUtility.WaterHeightMap;
            graphicsDevice.SamplerStates[4] = SamplerStateUtility.WaterReflectionMap;
            graphicsDevice.SamplerStates[5] = SamplerStateUtility.WaterFoamMap;

            graphicsDevice.Textures[0] = colorTarget;
            graphicsDevice.Textures[1] = normalTarget;
            graphicsDevice.Textures[2] = depthTarget;
            graphicsDevice.Textures[3] = heightMap;
            graphicsDevice.Textures[4] = reflectionTarget;
            graphicsDevice.Textures[5] = foamMap;

            waterEffect.Parameters["ViewProjection"].SetValue(camera.ViewProjectionMatrix);
            waterEffect.Parameters["InvertView"].SetValue(Matrix.Invert(camera.ViewMatrix));
            waterEffect.Parameters["FrustumCornersVS"].SetValue(camera.FrustumCorners);
            waterEffect.Parameters["cameraPos"].SetValue(camera.Position);

            waterEffect.Parameters["colorMap"].SetValue(colorTarget);
            waterEffect.Parameters["normalMap"].SetValue(normalMap);
            waterEffect.Parameters["depthMap"].SetValue(depthTarget);
            waterEffect.Parameters["heightMap"].SetValue(heightMap);
            waterEffect.Parameters["reflectionMap"].SetValue(reflectionTarget);
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
