using Game1.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Shadows
{
    class ShadowRenderer
    {
        public const int NumCascades = 4;
        private const int ShadowMapSize = 2048;
        private static readonly int[] kernelSizes = { 2, 3, 5, 7 };
        public FixedFilterSize fixedFilterSize;

        private bool stabilizeCascades = false;
        private bool visualizeCascades = false;
        private bool filterAcrossCascades = false;
        private Vector3 lightDirection;

        private ContentManager contentManager;
        private GraphicsDevice graphicsDevice;

        private ShadowEffect shadowEffect;
        private ShadowMapEffect shadowMapEffect;
        private RenderTarget2D shadowMap;

        private float[] cascadeSplits;
        private Vector3[] frustumCorners;

        public ShadowRenderer(GraphicsDevice graphicsDevice,
                        ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.graphicsDevice = graphicsDevice;

            cascadeSplits = new float[4];
            frustumCorners = new Vector3[8];

            lightDirection = Vector3.Normalize(new Vector3(1, 1, -1));
            fixedFilterSize = FixedFilterSize.Filter2x2;

            shadowEffect = new ShadowEffect(graphicsDevice, contentManager.Load<Effect>("Effects/Shadow"));
            shadowMapEffect = new ShadowMapEffect(graphicsDevice, contentManager.Load<Effect>("Effects/ShadowMap"));

            CreateShadowMaps();
        }

        private void CreateShadowMaps()
        {
            if (shadowMap != null)
                shadowMap.Dispose();

            shadowMap = new RenderTarget2D(graphicsDevice,
                ShadowMapSize, ShadowMapSize,
                false, SurfaceFormat.Single,
                DepthFormat.Depth24, 1,
                RenderTargetUsage.DiscardContents,
                false, NumCascades);
        }

        public void RenderShadowMap(GraphicsDevice graphicsDevice, Camera camera, Matrix worldMatrix, List<DrawableObject> list)
        {
            // Set cascade split ratios.
            cascadeSplits[0] = 0.05f;
            cascadeSplits[1] = 0.15f;
            cascadeSplits[2] = 0.50f;
            cascadeSplits[3] = 1.0f;

            var globalShadowMatrix = MakeGlobalShadowMatrix(camera);
            shadowEffect.ShadowMatrix = globalShadowMatrix;

            // Render the meshes to each cascade.
            for (var cascadeIdx = 0; cascadeIdx < NumCascades; ++cascadeIdx)
            {
                // Set the shadow map as the render target
                graphicsDevice.SetRenderTarget(shadowMap, cascadeIdx);
                graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

                // Get the 8 points of the view frustum in world space
                ResetViewFrustumCorners();

                var prevSplitDist = cascadeIdx == 0 ? 0.0f : cascadeSplits[cascadeIdx - 1];
                var splitDist = cascadeSplits[cascadeIdx];

                var invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);
                for (var i = 0; i < 8; ++i)
                    frustumCorners[i] = Vector4.Transform(frustumCorners[i], invViewProj).ToVector3();

                // Get the corners of the current cascade slice of the view frustum
                for (var i = 0; i < 4; ++i)
                {
                    var cornerRay = frustumCorners[i + 4] - frustumCorners[i];
                    var nearCornerRay = cornerRay * prevSplitDist;
                    var farCornerRay = cornerRay * splitDist;
                    frustumCorners[i + 4] = frustumCorners[i] + farCornerRay;
                    frustumCorners[i] = frustumCorners[i] + nearCornerRay;
                }

                // Calculate the centroid of the view frustum slice
                var frustumCenter = Vector3.Zero;
                for (var i = 0; i < 8; ++i)
                    frustumCenter = frustumCenter + frustumCorners[i];
                frustumCenter /= 8.0f;

                // Pick the up vector to use for the light camera
                var upDir = camera.worldMatrix.Right;

                Vector3 minExtents;
                Vector3 maxExtents;

                if (stabilizeCascades)
                {
                    // This needs to be constant for it to be stable
                    upDir = Vector3.Up;

                    // Calculate the radius of a bounding sphere surrounding the frustum corners
                    var sphereRadius = 0.0f;
                    for (var i = 0; i < 8; ++i)
                    {
                        var dist = (frustumCorners[i] - frustumCenter).Length();
                        sphereRadius = Math.Max(sphereRadius, dist);
                    }

                    sphereRadius = (float)Math.Ceiling(sphereRadius * 16.0f) / 16.0f;

                    maxExtents = new Vector3(sphereRadius);
                    minExtents = -maxExtents;
                }
                else
                {
                    // Create a temporary view matrix for the light
                    var lightCameraPos = frustumCenter;
                    var lookAt = frustumCenter - lightDirection;
                    var lightView = Matrix.CreateLookAt(lightCameraPos, lookAt, upDir);

                    // Calculate an AABB around the frustum corners
                    var mins = new Vector3(float.MaxValue);
                    var maxes = new Vector3(float.MinValue);
                    for (var i = 0; i < 8; ++i)
                    {
                        var corner = Vector4.Transform(frustumCorners[i], lightView).ToVector3();
                        mins = Vector3.Min(mins, corner);
                        maxes = Vector3.Max(maxes, corner);
                    }

                    minExtents = mins;
                    maxExtents = maxes;

                    // Adjust the min/max to accommodate the filtering size
                    var scale = (ShadowMapSize + kernelSizes[(int) fixedFilterSize]) / (float)ShadowMapSize;
                    minExtents.X *= scale;
                    minExtents.Y *= scale;
                    maxExtents.X *= scale;
                    maxExtents.Y *= scale;
                }

                var cascadeExtents = maxExtents - minExtents;

                // Get position of the shadow camera
                var shadowCameraPos = frustumCenter + lightDirection * -minExtents.Z;

                // Come up with a new orthographic camera for the shadow caster
                var shadowCamera = new ShadowOrthographicCamera(
                    minExtents.X, minExtents.Y, maxExtents.X, maxExtents.Y,
                    0.0f, cascadeExtents.Z);
                shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, upDir);

                if (stabilizeCascades)
                {
                    // Create the rounding matrix, by projecting the world-space origin and determining
                    // the fractional offset in texel space
                    var shadowMatrixTemp = shadowCamera.ViewProjection;
                    var shadowOrigin = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
                    shadowOrigin = Vector4.Transform(shadowOrigin, shadowMatrixTemp);
                    shadowOrigin = shadowOrigin * (ShadowMapSize / 2.0f);

                    var roundedOrigin = Vector4Utility.Round(shadowOrigin);
                    var roundOffset = roundedOrigin - shadowOrigin;
                    roundOffset = roundOffset * (2.0f / ShadowMapSize);
                    roundOffset.Z = 0.0f;
                    roundOffset.W = 0.0f;

                    var shadowProj = shadowCamera.Projection;
                    //shadowProj.r[3] = shadowProj.r[3] + roundOffset;
                    shadowProj.M41 += roundOffset.X;
                    shadowProj.M42 += roundOffset.Y;
                    shadowProj.M43 += roundOffset.Z;
                    shadowProj.M44 += roundOffset.W;
                    shadowCamera.Projection = shadowProj;
                }

                // Draw the mesh with depth only, using the new shadow camera
                RenderDepth(graphicsDevice, shadowCamera, worldMatrix, true, list);

                // Apply the scale/offset matrix, which transforms from [-1,1]
                // post-projection space to [0,1] UV space
                var texScaleBias = Matrix.CreateScale(0.5f, -0.5f, 1.0f)
                    * Matrix.CreateTranslation(0.5f, 0.5f, 0.0f);
                var shadowMatrix = shadowCamera.ViewProjection;
                shadowMatrix = shadowMatrix * texScaleBias;

                // Store the split distance in terms of view space depth
                var clipDist = camera.FarZ - camera.NearZ;

                shadowEffect.CascadeSplits[cascadeIdx] = camera.NearZ + splitDist * clipDist;

                // Calculate the position of the lower corner of the cascade partition, in the UV space
                // of the first cascade partition
                var invCascadeMat = Matrix.Invert(shadowMatrix);
                var cascadeCorner = Vector4.Transform(Vector3.Zero, invCascadeMat).ToVector3();
                cascadeCorner = Vector4.Transform(cascadeCorner, globalShadowMatrix).ToVector3();

                // Do the same for the upper corner
                var otherCorner = Vector4.Transform(Vector3.One, invCascadeMat).ToVector3();
                otherCorner = Vector4.Transform(otherCorner, globalShadowMatrix).ToVector3();

                // Calculate the scale and offset
                var cascadeScale = Vector3.One / (otherCorner - cascadeCorner);
                shadowEffect.CascadeOffsets[cascadeIdx] = new Vector4(-cascadeCorner, 0.0f);
                shadowEffect.CascadeScales[cascadeIdx] = new Vector4(cascadeScale, 1.0f);
            }
        }

        private void ResetViewFrustumCorners()
        {
            frustumCorners[0] = new Vector3(-1.0f, 1.0f, 0.0f);
            frustumCorners[1] = new Vector3(1.0f, 1.0f, 0.0f);
            frustumCorners[2] = new Vector3(1.0f, -1.0f, 0.0f);
            frustumCorners[3] = new Vector3(-1.0f, -1.0f, 0.0f);
            frustumCorners[4] = new Vector3(-1.0f, 1.0f, 1.0f);
            frustumCorners[5] = new Vector3(1.0f, 1.0f, 1.0f);
            frustumCorners[6] = new Vector3(1.0f, -1.0f, 1.0f);
            frustumCorners[7] = new Vector3(-1.0f, -1.0f, 1.0f);
        }

        /// <summary>
        /// Makes the "global" shadow matrix used as the reference point for the cascades.
        /// </summary>
        private Matrix MakeGlobalShadowMatrix(Camera camera)
        {
            // Get the 8 points of the view frustum in world space
            ResetViewFrustumCorners();

            var invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);
            var frustumCenter = Vector3.Zero;
            for (var i = 0; i < 8; i++)
            {
                frustumCorners[i] = Vector4.Transform(frustumCorners[i], invViewProj).ToVector3();
                frustumCenter += frustumCorners[i];
            }

            frustumCenter /= 8.0f;

            // Pick the up vector to use for the light camera
            var upDir = camera.worldMatrix.Right;

            // This needs to be constant for it to be stable
            if (stabilizeCascades)
                upDir = Vector3.Up;

            // Get position of the shadow camera
            var shadowCameraPos = frustumCenter + lightDirection * -0.5f;

            // Come up with a new orthographic camera for the shadow caster
            var shadowCamera = new ShadowOrthographicCamera(-0.5f, -0.5f, 0.5f, 0.5f, 0.0f, 1.0f);
            shadowCamera.SetLookAt(shadowCameraPos, frustumCenter, upDir);

            var texScaleBias = Matrix.CreateScale(0.5f, -0.5f, 1.0f);
            texScaleBias.Translation = new Vector3(0.5f, 0.5f, 0.0f);
            return shadowCamera.ViewProjection * texScaleBias;
        }

        private void RenderDepth(GraphicsDevice graphicsDevice, ShadowCamera camera, Matrix worldMatrix, bool shadowRendering, List<DrawableObject> list)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            graphicsDevice.RasterizerState = shadowRendering
                ? RasterizerStateUtility.CreateShadowMap
                : RasterizerState.CullCounterClockwise;

            var worldViewProjection = worldMatrix * camera.ViewProjection;

            foreach (DrawableObject dObject in list)
            {
                foreach (var mesh in dObject.Model.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                        if (meshPart.PrimitiveCount > 0)
                        {
                            shadowMapEffect.WorldViewProjection = dObject.ModelBones[mesh.ParentBone.Index] * Matrix.CreateScale(Map.scale)  * Matrix.CreateTranslation(dObject.Position) * worldViewProjection;
                            shadowMapEffect.Apply();

                            graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                            graphicsDevice.Indices = meshPart.IndexBuffer;

                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex, meshPart.PrimitiveCount);
                        }
                }
            }
        }

        public void Render(GraphicsDevice graphicsDevice, Camera camera, Matrix worldMatrix,
            RenderTarget2D colorMap, RenderTarget2D normalMap, RenderTarget2D depthMap)
        {
            // Render scene.

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            graphicsDevice.SamplerStates[0] = SamplerStateUtility.ShadowMap;

            shadowEffect.VisualizeCascades = visualizeCascades;
            shadowEffect.FilterAcrossCascades = filterAcrossCascades;
            shadowEffect.FilterSize = fixedFilterSize;
            shadowEffect.Bias = 0.002f;
            shadowEffect.OffsetScale = 0.0f;

            shadowEffect.ViewProjection = camera.ViewProjectionMatrix;
            shadowEffect.CameraPosWS = camera.Position;
            
            shadowEffect.ShadowMap = shadowMap;
            
            shadowEffect.LightDirection = lightDirection;
            shadowEffect.LightColor = new Vector3(3, 3, 3);

            shadowEffect.ColorMap = colorMap;
            shadowEffect.NormalMap = normalMap;
            shadowEffect.DepthMap = depthMap;
            shadowEffect.InvertViewProjection = Matrix.Invert(camera.ViewProjectionMatrix);

            //shadowEffect.DiffuseColor = basicEffect.DiffuseColor;
            shadowEffect.World = worldMatrix;

            shadowEffect.Apply();
        }
    }
}
