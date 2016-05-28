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
    public class HDRProcessor
    {
        /// <summary>
        /// Used for textures that store intermediate results of
        /// passes during post-processing
        /// </summary>
		public class IntermediateTexture
        {
            public RenderTarget2D RenderTarget;
            public bool InUse;
        }

        protected GraphicsDevice graphicsDevice;
        protected ContentManager contentManager;
        protected QuadRenderComponent quadRenderer;
        protected List<IntermediateTexture> intermediateTextures = new List<IntermediateTexture>();

        protected Effect blurEffect;
        protected Effect thresholdEffect;
        protected Effect scalingEffect;
        protected Effect HDREffect;

        protected RenderTarget2D currentFrameLuminance;
        protected RenderTarget2D currentFrameAdaptedLuminance;
        protected RenderTarget2D lastFrameAdaptedLuminance;
        protected RenderTarget2D[] luminanceChain;

        protected float toneMapKey = 0.8f;
        protected float maxLuminance = 512.0f;
        protected float bloomThreshold = 0.85f;
        protected float bloomMultiplier = 1.0f;
        protected float blurSigma = 2.5f;

        public float ToneMapKey
        {
            get { return toneMapKey; }
            set { toneMapKey = value; }
        }

        public float MaxLuminance
        {
            get { return maxLuminance; }
            set { maxLuminance = value; }
        }

        public HDRProcessor(GraphicsDevice graphicsDevice, ContentManager contentManager, QuadRenderComponent quadRenderer)
        {
            this.contentManager = contentManager;
            this.graphicsDevice = graphicsDevice;
            this.quadRenderer = quadRenderer;

            // Load the effects
            blurEffect = contentManager.Load<Effect>("Effects/HDR/Blur");
            thresholdEffect = contentManager.Load<Effect>("Effects/HDR/Threshold");
            scalingEffect = contentManager.Load<Effect>("Effects/HDR/Scale");
            HDREffect = contentManager.Load<Effect>("Effects/HDR/HDR");

            // Initialize our buffers
            int width = graphicsDevice.PresentationParameters.BackBufferWidth;
            int height = graphicsDevice.PresentationParameters.BackBufferHeight;

            // Two buffers we'll swap between, so we can adapt the luminance     
            currentFrameLuminance = new RenderTarget2D(graphicsDevice, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
            currentFrameAdaptedLuminance = new RenderTarget2D(graphicsDevice, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
            lastFrameAdaptedLuminance = new RenderTarget2D(graphicsDevice, 1, 1, false, SurfaceFormat.Single, DepthFormat.None);
            graphicsDevice.SetRenderTarget(lastFrameAdaptedLuminance);
            graphicsDevice.Clear(Color.White);
            graphicsDevice.SetRenderTarget(null);

            // We need a luminance chain
            int chainLength = 1;
            int startSize = (int)MathHelper.Min(width / 16, height / 16);
            int size = 16;
            for (size = 16; size < startSize; size *= 4)
                chainLength++;

            luminanceChain = new RenderTarget2D[chainLength];
            size /= 4;
            for (int i = 0; i < chainLength; i++)
            {
                luminanceChain[i] = new RenderTarget2D(graphicsDevice, size, size, false, SurfaceFormat.Single, DepthFormat.None);
                size /= 4;
            }
        }

        /// <summary>
		/// Applies a blur to the specified render target, writes the result
		/// to the specified render target.
		/// </summary>
		/// <param name="source">The render target to use as the source and result</param>
		/// <param name="sigma">The standard deviation used for gaussian weights</param>
        /// <param name="encoded">If true, blurs using LogLuv encoding/decoding</param>
		public void Blur(RenderTarget2D source,
                            RenderTarget2D result,
                            float sigma)
        {
            IntermediateTexture blurH = GetIntermediateTexture(source.Width,
                                                                source.Height,
                                                                source.Format);

            string baseTechniqueName = "GaussianBlur";

            // Do horizontal pass first
            blurEffect.CurrentTechnique = blurEffect.Techniques[baseTechniqueName + "H"];
            blurEffect.Parameters["g_fSigma"].SetValue(sigma);

            PostProcess(source, blurH.RenderTarget, blurEffect);

            // Now the vertical pass 
            blurEffect.CurrentTechnique = blurEffect.Techniques[baseTechniqueName + "V"];

            PostProcess(blurH.RenderTarget, result, blurEffect);

            blurH.InUse = false;
        }

        /// <summary>
        /// Downscales the source to 1/16th size, using software(shader) filtering
        /// </summary>
        /// <param name="source">The source to be downscaled</param>
        /// <param name="result">The RT in which to store the result</param>
        /// <param name="encoded">If true, the source is encoded in LogLuv format</param>
        protected void GenerateDownscaleTargetSW(RenderTarget2D source, RenderTarget2D result)
        {
            string techniqueName = "Downscale4";

            IntermediateTexture downscale1 = GetIntermediateTexture(source.Width / 4, source.Height / 4, source.Format);
            scalingEffect.CurrentTechnique = scalingEffect.Techniques[techniqueName];
            PostProcess(source, downscale1.RenderTarget, scalingEffect);

            scalingEffect.CurrentTechnique = scalingEffect.Techniques[techniqueName];
            PostProcess(downscale1.RenderTarget, result, scalingEffect);
            downscale1.InUse = false;
        }

        /// <summary>
        /// Downscales the source to 1/16th size, using hardware filtering
        /// </summary>
        /// <param name="source">The source to be downscaled</param>
        /// <param name="result">The RT in which to store the result</param>
        protected void GenerateDownscaleTargetHW(RenderTarget2D source, RenderTarget2D result)
        {
            IntermediateTexture downscale1 = GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            scalingEffect.CurrentTechnique = scalingEffect.Techniques["ScaleHW"];
            PostProcess(source, downscale1.RenderTarget, scalingEffect);

            IntermediateTexture downscale2 = GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            scalingEffect.CurrentTechnique = scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale1.RenderTarget, downscale2.RenderTarget, scalingEffect);
            downscale1.InUse = false;

            IntermediateTexture downscale3 = GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            scalingEffect.CurrentTechnique = scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale2.RenderTarget, downscale3.RenderTarget, scalingEffect);
            downscale2.InUse = false;

            scalingEffect.CurrentTechnique = scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale3.RenderTarget, result, scalingEffect);
            downscale3.InUse = false;
        }

        /// <summary>
        /// Calculates the average luminance of the scene
        /// </summary>
        /// <param name="downscaleBuffer">The scene texure, downscaled to 1/16th size</param>
        /// <param name="dt">The time delta</param>
        /// <param name="encoded">If true, the image is encoded in LogLuv format</param>
        protected void CalculateAverageLuminance(RenderTarget2D downscaleBuffer, float dt)
        {
            // Calculate the initial luminance
            HDREffect.CurrentTechnique = HDREffect.Techniques["Luminance"];
            PostProcess(downscaleBuffer, luminanceChain[0], HDREffect);

            // Repeatedly downscale            
            scalingEffect.CurrentTechnique = scalingEffect.Techniques["Downscale4"];
            for (int i = 1; i < luminanceChain.Length; i++)
                PostProcess(luminanceChain[i - 1], luminanceChain[i], scalingEffect);

            // Final downscale            
            scalingEffect.CurrentTechnique = scalingEffect.Techniques["Downscale4Luminance"];
            PostProcess(luminanceChain[luminanceChain.Length - 1], currentFrameLuminance, scalingEffect);

            // Adapt the luminance, to simulate slowly adjust exposure
            HDREffect.Parameters["g_fDT"].SetValue(dt);
            HDREffect.CurrentTechnique = HDREffect.Techniques["CalcAdaptedLuminance"];
            RenderTarget2D[] sources = new RenderTarget2D[2];
            sources[0] = currentFrameLuminance;
            sources[1] = lastFrameAdaptedLuminance;
            PostProcess(sources, currentFrameAdaptedLuminance, HDREffect);

        }

        /// <summary>
        /// Performs tone mapping on the specified render target
        /// </summary>
        /// <param name="source">The source render target</param>
        /// <param name="result">The render target to which the result will be output</param>
        /// <param name="dt">The time elapsed since the last frame</param>
        /// <param name="encoded">If true, use LogLuv encoding</param>
        /// <param name="preferHWScaling">If true, will attempt to use hardware filtering</param>
        public void ToneMap(RenderTarget2D source, RenderTarget2D result, float dt, bool preferHWScaling)
        {
            // Downscale to 1/16 size
            IntermediateTexture downscaleTarget = GetIntermediateTexture(source.Width / 16, source.Height / 16, source.Format);
            //if (preferHWScaling && encoded)
                //GenerateDownscaleTargetHW(source, downscaleTarget.RenderTarget);
            //else
            GenerateDownscaleTargetSW(source, downscaleTarget.RenderTarget);

            // Get the luminance
            CalculateAverageLuminance(downscaleTarget.RenderTarget, dt);

            // Do the bloom first
            IntermediateTexture threshold = GetIntermediateTexture(downscaleTarget.RenderTarget.Width, downscaleTarget.RenderTarget.Height, source.Format);
            thresholdEffect.Parameters["g_fThreshold"].SetValue(bloomThreshold);
            thresholdEffect.Parameters["g_fMiddleGrey"].SetValue(toneMapKey);
            thresholdEffect.Parameters["g_fMaxLuminance"].SetValue(maxLuminance);
            thresholdEffect.CurrentTechnique = thresholdEffect.Techniques["Threshold"];
            RenderTarget2D[] sources2 = new RenderTarget2D[2];
            sources2[0] = downscaleTarget.RenderTarget;
            sources2[1] = currentFrameAdaptedLuminance;
            PostProcess(sources2, threshold.RenderTarget, thresholdEffect);

            IntermediateTexture postBlur = GetIntermediateTexture(downscaleTarget.RenderTarget.Width, downscaleTarget.RenderTarget.Height, SurfaceFormat.Color);
            Blur(threshold.RenderTarget, postBlur.RenderTarget, blurSigma);
            threshold.InUse = false;

            // Scale it back to half of full size (will do the final scaling step when sampling
            // the bloom texture during tone mapping).
            IntermediateTexture upscale1 = GetIntermediateTexture(source.Width / 8, source.Height / 8, SurfaceFormat.Color);
            scalingEffect.CurrentTechnique = scalingEffect.Techniques["ScaleHW"];
            PostProcess(postBlur.RenderTarget, upscale1.RenderTarget, scalingEffect);
            postBlur.InUse = false;

            IntermediateTexture upscale2 = GetIntermediateTexture(source.Width / 4, source.Height / 4, SurfaceFormat.Color);
            PostProcess(upscale1.RenderTarget, upscale2.RenderTarget, scalingEffect);
            upscale1.InUse = false;

            IntermediateTexture bloom = GetIntermediateTexture(source.Width / 2, source.Height / 2, SurfaceFormat.Color);
            PostProcess(upscale2.RenderTarget, bloom.RenderTarget, scalingEffect);
            upscale2.InUse = false;

            // Now do tone mapping on the main source image, and add in the bloom
            HDREffect.Parameters["g_fMiddleGrey"].SetValue(toneMapKey);
            HDREffect.Parameters["g_fMaxLuminance"].SetValue(maxLuminance);
            HDREffect.Parameters["g_fBloomMultiplier"].SetValue(bloomMultiplier);
            RenderTarget2D[] sources3 = new RenderTarget2D[3];
            sources3[0] = source;
            sources3[1] = currentFrameAdaptedLuminance;
            sources3[2] = bloom.RenderTarget;
            HDREffect.CurrentTechnique = HDREffect.Techniques["ToneMap"];
            PostProcess(sources3, result, HDREffect);

            // Flip the luminance textures
            Swap(ref currentFrameAdaptedLuminance, ref lastFrameAdaptedLuminance);

            bloom.InUse = false;
            downscaleTarget.InUse = false;
        }

        /// <summary>
        /// Disposes all intermediate textures in the cache
        /// </summary>
        public void FlushCache()
        {
            foreach (IntermediateTexture intermediateTexture in intermediateTextures)
                intermediateTexture.RenderTarget.Dispose();
            intermediateTextures.Clear();
        }

        /// <summary>
        /// Performs a post-processing step using a single source texture
        /// </summary>
        /// <param name="source">The source texture</param>
        /// <param name="result">The output render target</param>
        /// <param name="effect">The effect to use</param>
		protected void PostProcess(RenderTarget2D source, RenderTarget2D result, Effect effect)
        {
            RenderTarget2D[] sources = new RenderTarget2D[1];
            sources[0] = source;
            PostProcess(sources, result, effect);
        }

        /// <summary>
        /// Performs a post-processing step using multiple source textures
        /// </summary>
        /// <param name="sources">The source textures</param>
        /// <param name="result">The output render target</param>
        /// <param name="effect">The effect to use</param>
		protected void PostProcess(RenderTarget2D[] sources, RenderTarget2D result, Effect effect)
        {
            graphicsDevice.SetRenderTarget(result);
            graphicsDevice.Clear(Color.Black);

            for (int i = 0; i < sources.Length; i++)
                effect.Parameters["SourceTexture" + Convert.ToString(i)].SetValue(sources[i]);
            effect.Parameters["g_vSourceDimensions"].SetValue(new Vector2(sources[0].Width, sources[0].Height));
            if (result == null)
                effect.Parameters["g_vDestinationDimensions"].SetValue(new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight));
            else
                effect.Parameters["g_vDestinationDimensions"].SetValue(new Vector2(result.Width, result.Height));

            // Begin effect
            effect.CurrentTechnique.Passes[0].Apply();

            // Draw primitives
            quadRenderer.Render();
        }


        /// <summary>
        /// Checks the cache to see if a suitable rendertarget has already been created
        /// and isn't in use.  Otherwise, creates one according to the parameters
        /// </summary>
        /// <param name="width">Width of the RT</param>
        /// <param name="height">Height of the RT</param>
        /// <param name="format">Format of the RT</param>
        /// <returns>The suitable RT</returns>
        protected IntermediateTexture GetIntermediateTexture(int width,
                                                                int height,
                                                                SurfaceFormat format)
        {
            // Look for a matching rendertarget in the cache
            for (int i = 0; i < intermediateTextures.Count; i++)
            {
                if (intermediateTextures[i].InUse == false
                    && height == intermediateTextures[i].RenderTarget.Height
                    && format == intermediateTextures[i].RenderTarget.Format
                    && width == intermediateTextures[i].RenderTarget.Width)
                {
                    intermediateTextures[i].InUse = true;
                    return intermediateTextures[i];
                }
            }

            // We didn't find one, let's make one
            IntermediateTexture newTexture = new IntermediateTexture();
            newTexture.RenderTarget = new RenderTarget2D(graphicsDevice,
                                                            width,
                                                            height,
                                                            false,
                                                            format,
                                                            DepthFormat.None);
            intermediateTextures.Add(newTexture);
            newTexture.InUse = true;
            return newTexture;
        }

        /// <summary>
        /// Swaps two RenderTarget's
        /// </summary>
        /// <param name="rt1">The first RT</param>
        /// <param name="rt2">The second RT</param>
        protected void Swap(ref RenderTarget2D rt1, ref RenderTarget2D rt2)
        {
            RenderTarget2D temp = rt1;
            rt1 = rt2;
            rt2 = temp;
        }
    }
}
