using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Game1.GameSettings;

namespace Game1.Postprocess
{
    /// <summary>
	/// Handles rendering of various post-processing techniques,
    /// including blurring, downscaling, and DOF blending
	/// </summary>
    public class DepthOfFieldProcessor
    {
        protected GraphicsDevice graphicsDevice;
        protected ContentManager contentManager;
        protected List<IntermediateTexture> intermediateTextures = new List<IntermediateTexture>();

        protected QuadRenderComponent quadRenderer;

        protected Effect blurEffect;
        protected Effect scalingEffect;
        protected Effect dofEffect;

        protected float blurSigma = 2.5f;

        protected Vector3[] frustumCornersWS = new Vector3[8];
        protected Vector3[] frustumCornersVS = new Vector3[8];
        protected Vector3[] farFrustumCornersVS = new Vector3[4];
        protected RenderTarget2D[] singleSourceArray = new RenderTarget2D[1];
        protected RenderTarget2D[] doubleSourceArray = new RenderTarget2D[2];
        protected RenderTarget2D[] tripleSourceArray = new RenderTarget2D[3];

        /// <summary>
        /// The class constructor
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for rendering</param>
        /// <param name="contentManager">The ContentManager from which to load Effects</param>
        /// <param name="quadRenderer">The QuadRenderComponent to use for drawing fullscreen quads</param>
        public DepthOfFieldProcessor(GraphicsDevice graphicsDevice, ContentManager contentManager, QuadRenderComponent quadRenderer)
        {
            this.contentManager = contentManager;
            this.graphicsDevice = graphicsDevice;
            this.quadRenderer = quadRenderer;

            // Load the effects
            blurEffect = contentManager.Load<Effect>("Effects/HDR/Blur");
            scalingEffect = contentManager.Load<Effect>("Effects/HDR/Scale");
            dofEffect = contentManager.Load<Effect>("Effects/HDR/DepthOfField");
        }

        /// <summary>
		/// Applies a blur to the specified render target, writes the result
		/// to the specified render target.
		/// </summary>
		/// <param name="source">The render target to use as the source</param>
        /// <param name="result">The render target to use as the result</param>
		/// <param name="sigma">The standard deviation used for gaussian weights</param>
		public void Blur(RenderTarget2D source,
                            RenderTarget2D result,
                            float sigma)
        {
            IntermediateTexture blurH = GetIntermediateTexture(source.Width,
                                                                source.Height,
                                                                source.Format);

            String baseTechniqueName = "GaussianBlur";

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
        /// Applies a blur to the specified render target, using a depth texture
        /// to prevent pixels from blurring with pixels that are "in front"
        /// </summary>
        /// <param name="source">The render target to use as the source</param>
        /// <param name="result">The render target to use as the result</param>
        /// <param name="depthTexture">The depth texture to use</param>
        /// <param name="sigma">The standard deviation used for gaussian weights</param>
        public void DepthBlur(RenderTarget2D source,
                            RenderTarget2D result,
                            RenderTarget2D depthTexture,
                            float sigma)
        {
            IntermediateTexture blurH = GetIntermediateTexture(source.Width,
                                                                source.Height,
                                                                source.Format);

            String baseTechniqueName = "GaussianDepthBlur";

            // Do horizontal pass first
            blurEffect.CurrentTechnique = blurEffect.Techniques[baseTechniqueName + "H"];
            blurEffect.Parameters["g_fSigma"].SetValue(sigma);

            RenderTarget2D[] sources = doubleSourceArray;
            sources[0] = source;
            sources[1] = depthTexture;
            PostProcess(sources, blurH.RenderTarget, blurEffect);

            // Now the vertical pass 
            blurEffect.CurrentTechnique = blurEffect.Techniques[baseTechniqueName + "V"];

            sources[0] = blurH.RenderTarget;
            sources[1] = depthTexture;
            PostProcess(blurH.RenderTarget, result, blurEffect);

            blurH.InUse = false;
        }

        /// <summary>
        /// Downscales the source to 1/16th size, using software(shader) filtering
        /// </summary>
        /// <param name="source">The source to be downscaled</param>
        /// <param name="result">The RT in which to store the result</param>
        protected void GenerateDownscaleTargetSW(RenderTarget2D source, RenderTarget2D result)
        {
            String techniqueName = "Downscale4";

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
        /// Performs tone mapping on the specified render target
        /// </summary>
        /// <param name="source">The source render target</param>
        /// <param name="result">The render target to which the result will be output</param>
        /// <param name="depthTexture">The render target containing scene depth</param>
        /// <param name="camera">The camera used to render the scene</param>
        /// <param name="dofType">The type of DOF effect to apply</param>
        /// <param name="focalDistance">The distance to the camera focal point</param>
        /// <param name="focalWidth">The width of the camera focal point</param>
        public void DOF(RenderTarget2D source,
                            RenderTarget2D result,
                            RenderTarget2D depthTexture,
                            Camera camera,
                            DOFType dofType,
                            float focalDistance,
                            float focalWidth)
        {
            if (dofType == DOFType.DiscBlur)
            {
                // Scale tap offsets based on render target size
                float dx = 0.5f / (float)source.Width;
                float dy = 0.5f / (float)source.Height;

                // Generate the texture coordinate offsets for our disc
                Vector2[] discOffsets = new Vector2[12];
                discOffsets[0] = new Vector2(-0.326212f * dx, -0.40581f * dy);
                discOffsets[1] = new Vector2(-0.840144f * dx, -0.07358f * dy);
                discOffsets[2] = new Vector2(-0.840144f * dx, 0.457137f * dy);
                discOffsets[3] = new Vector2(-0.203345f * dx, 0.620716f * dy);
                discOffsets[4] = new Vector2(0.96234f * dx, -0.194983f * dy);
                discOffsets[5] = new Vector2(0.473434f * dx, -0.480026f * dy);
                discOffsets[6] = new Vector2(0.519456f * dx, 0.767022f * dy);
                discOffsets[7] = new Vector2(0.185461f * dx, -0.893124f * dy);
                discOffsets[8] = new Vector2(0.507431f * dx, 0.064425f * dy);
                discOffsets[9] = new Vector2(0.89642f * dx, 0.412458f * dy);
                discOffsets[10] = new Vector2(-0.32194f * dx, -0.932615f * dy);
                discOffsets[11] = new Vector2(-0.791559f * dx, -0.59771f * dy);

                // Set array of offsets
                dofEffect.Parameters["g_vFilterTaps"].SetValue(discOffsets);

                dofEffect.CurrentTechnique = dofEffect.Techniques["DOFDiscBlur"];

                dofEffect.Parameters["g_fFocalDistance"].SetValue(focalDistance);
                dofEffect.Parameters["g_fFocalWidth"].SetValue(focalWidth / 2.0f);
                dofEffect.Parameters["g_fFarClip"].SetValue(camera.FarZ);
                dofEffect.Parameters["g_fAttenuation"].SetValue(1.0f);

                RenderTarget2D[] sources = doubleSourceArray;
                sources[0] = source;
                sources[1] = depthTexture;

                PostProcess(sources, result, dofEffect);
            }
            else
            {
                // Downscale to 1/16th size and blur
                IntermediateTexture downscaleTexture = GetIntermediateTexture(source.Width / 4, source.Height / 4, SurfaceFormat.Color);
                GenerateDownscaleTargetSW(source, downscaleTexture.RenderTarget);

                // For the "dumb" DOF type just do a blur, otherwise use a special blur
                // that takes depth into account
                if (dofType == DOFType.Downscale)
                    Blur(downscaleTexture.RenderTarget, downscaleTexture.RenderTarget, 2.5f);
                else if (dofType == DOFType.DownscaleWithDepth)
                    DepthBlur(downscaleTexture.RenderTarget, downscaleTexture.RenderTarget, depthTexture, 2.5f);


                dofEffect.CurrentTechnique = dofEffect.Techniques["DOFBlurBuffer"];

                dofEffect.Parameters["g_fFocalDistance"].SetValue(focalDistance);
                dofEffect.Parameters["g_fFocalWidth"].SetValue(focalWidth / 2.0f);
                dofEffect.Parameters["g_fFarClip"].SetValue(camera.FarZ);
                dofEffect.Parameters["g_fAttenuation"].SetValue(1.0f);

                RenderTarget2D[] sources = tripleSourceArray;
                sources[0] = source;
                sources[1] = downscaleTexture.RenderTarget;
                sources[2] = depthTexture;

                PostProcess(sources, result, dofEffect);

                downscaleTexture.InUse = false;
            }
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
            RenderTarget2D[] sources = singleSourceArray;
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

            // Draw the quad
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
        protected static void Swap(ref RenderTarget2D rt1, ref RenderTarget2D rt2)
        {
            RenderTarget2D temp = rt1;
            rt1 = rt2;
            rt2 = temp;
        }
    }

}
