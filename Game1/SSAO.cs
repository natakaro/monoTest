using Game1.Helpers;
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
    public class SSAO
    {
        const int kernelSize = 16;
        const int noiseSize = 4;

        Texture2D noiseTex;
        Effect ssaoEffect;
        Effect ssao2Effect;
        Effect ssaoBlur;
        RenderTarget2D normalTarget;
        RenderTarget2D depthTarget;
        RenderTarget2D ssaoTarget;
        RenderTarget2D blurTarget;
        ContentManager Content;
        GraphicsDevice GraphicsDevice;
        GameSettings Settings;
        QuadRenderComponent quadRenderer;
        Camera Camera;
        Random random;
        Vector3[] kernel;

        public SSAO(GraphicsDevice GraphicsDevice, ContentManager Content, GameSettings Settings, QuadRenderComponent quadRenderer, Camera Camera, RenderTarget2D normalTarget, RenderTarget2D depthTarget)
        {
            this.GraphicsDevice = GraphicsDevice;
            this.Content = Content;
            this.Settings = Settings;
            this.quadRenderer = quadRenderer;
            this.Camera = Camera;
            this.normalTarget = normalTarget;
            this.depthTarget = depthTarget;

            random = new Random();

            int backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            ssaoTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            blurTarget = new RenderTarget2D(GraphicsDevice, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            kernel = GenerateKernel(kernelSize);
            noiseTex = GenerateNoise(noiseSize);

            ssaoEffect = Content.Load<Effect>("Effects/SSAO");
            ssaoEffect.Parameters["randomMap"].SetValue(noiseTex);
            ssaoEffect.Parameters["normalMap"].SetValue(normalTarget);
            ssaoEffect.Parameters["depthMap"].SetValue(depthTarget);
            ssaoEffect.Parameters["random_size"].SetValue(16);
            ssaoEffect.Parameters["g_sample_rad"].SetValue((float)1);
            ssaoEffect.Parameters["g_intensity"].SetValue((float)1);
            ssaoEffect.Parameters["g_scale"].SetValue((float)1);
            ssaoEffect.Parameters["g_bias"].SetValue((float)0.001);
            ssaoEffect.Parameters["g_screen_size"].SetValue(backbufferWidth * backbufferHeight);

            ssao2Effect = Content.Load<Effect>("Effects/SSAO2");
            ssao2Effect.Parameters["randomMap"].SetValue(noiseTex);
            ssao2Effect.Parameters["normalMap"].SetValue(normalTarget);
            ssao2Effect.Parameters["depthMap"].SetValue(depthTarget);
            ssao2Effect.Parameters["Radius"].SetValue((float)5);
            ssao2Effect.Parameters["Power"].SetValue((float)2);
            ssao2Effect.Parameters["NoiseScale"].SetValue(new Vector2(backbufferWidth / noiseSize, backbufferHeight / noiseSize));
            ssao2Effect.Parameters["SampleKernelSize"].SetValue(kernelSize);
            ssao2Effect.Parameters["SampleKernel"].SetValue(kernel);

            ssaoBlur = Content.Load<Effect>("Effects/SSAOBlur");
            ssaoBlur.Parameters["texelSize"].SetValue(new Vector2(1.0f / backbufferWidth, 1.0f / backbufferHeight));
            ssaoBlur.Parameters["SSAO"].SetValue(SSAOTarget);
        }

        public void DrawSSAO()
        {
            GraphicsDevice.SetRenderTarget(ssaoTarget);
            GraphicsDevice.Clear(Color.White);
            if (Settings.DrawSSAO)
            {
                if (Settings.SSAO2 == true)
                {
                    ssao2Effect.Parameters["InvertProjection"].SetValue(Matrix.Invert(Camera.ProjectionMatrix));
                    ssao2Effect.Parameters["View"].SetValue(Camera.ViewMatrix);
                    ssao2Effect.Parameters["Projection"].SetValue(Camera.ProjectionMatrix);
                    ssao2Effect.Parameters["Radius"].SetValue(Settings.SSAORadius);
                    ssao2Effect.Parameters["Power"].SetValue(Settings.SSAOPower);
                    ssao2Effect.CurrentTechnique.Passes[0].Apply();
                }
                else
                {
                    ssaoEffect.Parameters["InvertProjection"].SetValue(Matrix.Invert(Camera.ProjectionMatrix));
                    ssaoEffect.Parameters["View"].SetValue(Camera.ViewMatrix);
                    ssaoEffect.CurrentTechnique.Passes[0].Apply();
                }
                quadRenderer.Render(Vector2.One * -1, Vector2.One);
            }

            GraphicsDevice.SetRenderTarget(blurTarget);
            GraphicsDevice.Clear(Color.White);
            if (Settings.DrawSSAO && Settings.BlurSSAO)
            {
                ssaoBlur.CurrentTechnique.Passes[0].Apply();
                quadRenderer.Render(Vector2.One * -1, Vector2.One);
            }
        }

        private Vector3[] GenerateKernel(int kernelSize)
        {
            Vector3[] kernel = new Vector3[kernelSize];
            for (int i = 0; i < kernelSize; ++i)
            {
                kernel[i] = new Vector3(
                    (float)random.NextDouble() * 2 - 1,
                    (float)random.NextDouble() * 2 - 1,
                    (float)random.NextDouble());

                kernel[i].Normalize();

                //kernel[i] *= (float)random.NextDouble();

                float scale = i / kernelSize;
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                kernel[i] *= scale;
            }

            return kernel;
        }

        private Texture2D GenerateNoise(int noiseSize)
        {
            Texture2D noiseTex = new Texture2D(GraphicsDevice, noiseSize, noiseSize, false, SurfaceFormat.Color);
            Vector3[] noise = new Vector3[noiseSize * noiseSize];
            for (int i = 0; i < noiseSize * noiseSize; ++i)
            {
                noise[i] = new Vector3(
                    (float)random.NextDouble() * 2 - 1,
                    (float)random.NextDouble() * 2 - 1,
                    0.0f);

                noise[i].Normalize();
            }

            noiseTex.SetData<Vector3>(noise);
            //noiseTex.SaveAsPng(new System.IO.FileStream("../noise.png", System.IO.FileMode.Create), 4, 4);
            return noiseTex;
        }

        public RenderTarget2D SSAOTarget
        {
            get { return ssaoTarget; }
        }
        public RenderTarget2D BlurTarget
        {
            get { return blurTarget; }
        }
    }
}
