using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Shadows
{
    class ShadowEffect
    {
        private readonly Effect _innerEffect;

        private readonly EffectParameter _cameraPosWSParameter;
        private readonly EffectParameter _shadowMatrixParameter;
        private readonly EffectParameter _cascadeSplitsParameter;
        private readonly EffectParameter _cascadeOffsetsParameter;
        private readonly EffectParameter _cascadeScalesParameter;
        private readonly EffectParameter _biasParameter;
        private readonly EffectParameter _offsetScaleParameter;
        private readonly EffectParameter _lightDirectionParameter;
        private readonly EffectParameter _lightColorParameter;
        private readonly EffectParameter _diffuseColorParameter;
        private readonly EffectParameter _worldParameter;
        private readonly EffectParameter _viewProjectionParameter;
        private readonly EffectParameter _shadowMapParameter;

        private readonly EffectParameter _colorMap;
        private readonly EffectParameter _normalMap;
        private readonly EffectParameter _depthMap;
        private readonly EffectParameter _ssaoMap;

        private readonly EffectParameter _invertViewParameter;
        private readonly EffectParameter _invertProjectionParameter;
        private readonly EffectParameter _invertViewProjectionParameter;

        private readonly EffectParameter _nearClipParameter;
        private readonly EffectParameter _farClipParameter;
        private readonly EffectParameter _frustumCorners;

        private readonly EffectParameter _skyIntensity;

        public bool VisualizeCascades { get; set; }
        public bool FilterAcrossCascades { get; set; }
        public FixedFilterSize FilterSize { get; set; }

        public Vector3 CameraPosWS { get; set; }
        public Matrix ShadowMatrix { get; set; }
        public float[] CascadeSplits { get; private set; }
        public Vector4[] CascadeOffsets { get; private set; }
        public Vector4[] CascadeScales { get; private set; }
        public float Bias { get; set; }
        public float OffsetScale { get; set; }
        public Vector3 LightDirection { get; set; }
        public Vector3 LightColor { get; set; }
        public Vector3 DiffuseColor { get; set; }
        public Matrix World { get; set; }
        public Matrix ViewProjection { get; set; }
        public Texture2D ShadowMap { get; set; }
        public Texture2D ColorMap { get; set; }
        public Texture2D NormalMap { get; set; }
        public Texture2D DepthMap { get; set; }
        public Texture2D SSAOMap { get; set; }
        public Matrix InvertView { get; set; }
        public Matrix InvertProjection { get; set; }
        public Matrix InvertViewProjection { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public Vector3[] FrustumCorners { get; set; }
        public float SkyIntensity { get; set; }

        public ShadowEffect(GraphicsDevice graphicsDevice, Effect innerEffect)
        {
            _innerEffect = innerEffect;

            _cameraPosWSParameter = _innerEffect.Parameters["CameraPosWS"];
            _shadowMatrixParameter = _innerEffect.Parameters["ShadowMatrix"];
            _cascadeSplitsParameter = _innerEffect.Parameters["CascadeSplits"];
            _cascadeOffsetsParameter = _innerEffect.Parameters["CascadeOffsets"];
            _cascadeScalesParameter = _innerEffect.Parameters["CascadeScales"];
            _biasParameter = _innerEffect.Parameters["Bias"];
            _offsetScaleParameter = _innerEffect.Parameters["OffsetScale"];
            _lightDirectionParameter = _innerEffect.Parameters["LightDirection"];
            _lightColorParameter = _innerEffect.Parameters["LightColor"];
            _diffuseColorParameter = _innerEffect.Parameters["DiffuseColor"];
            _worldParameter = _innerEffect.Parameters["World"];
            _viewProjectionParameter = _innerEffect.Parameters["ViewProjection"];
            _shadowMapParameter = _innerEffect.Parameters["ShadowMap"];

            _colorMap = _innerEffect.Parameters["colorMap"];
            _normalMap = _innerEffect.Parameters["normalMap"];
            _depthMap = _innerEffect.Parameters["depthMap"];
            _ssaoMap = _innerEffect.Parameters["ssaoMap"];

            _invertViewParameter = _innerEffect.Parameters["InvertView"];
            _invertProjectionParameter = _innerEffect.Parameters["InvertProjection"];
            _invertViewProjectionParameter = _innerEffect.Parameters["InvertViewProjection"];

            _nearClipParameter = _innerEffect.Parameters["NearClip"];
            _farClipParameter = _innerEffect.Parameters["FarClip"];

            _frustumCorners = _innerEffect.Parameters["FrustumCornersVS"];

            _skyIntensity = _innerEffect.Parameters["SkyIntensity"];

            CascadeSplits = new float[ShadowRenderer.NumCascades];
            CascadeOffsets = new Vector4[ShadowRenderer.NumCascades];
            CascadeScales = new Vector4[ShadowRenderer.NumCascades];
        }

        public void Apply()
        {
            var techniqueName = "Visualize" + VisualizeCascades + "Filter" + FilterAcrossCascades + "FilterSize" + FilterSize;
            _innerEffect.CurrentTechnique = _innerEffect.Techniques[techniqueName];

            _cameraPosWSParameter.SetValue(CameraPosWS);
            _shadowMatrixParameter.SetValue(ShadowMatrix);
            _cascadeSplitsParameter.SetValue(new Vector4(CascadeSplits[0], CascadeSplits[1], CascadeSplits[2], CascadeSplits[3]));
            _cascadeOffsetsParameter.SetValue(CascadeOffsets);
            _cascadeScalesParameter.SetValue(CascadeScales);
            _biasParameter.SetValue(Bias);
            _offsetScaleParameter.SetValue(OffsetScale);
            _lightDirectionParameter.SetValue(LightDirection);
            _lightColorParameter.SetValue(LightColor);
            //_diffuseColorParameter.SetValue(DiffuseColor);
            _worldParameter.SetValue(World);
            _viewProjectionParameter.SetValue(ViewProjection);
            _shadowMapParameter.SetValue(ShadowMap);

            _colorMap.SetValue(ColorMap);
            _normalMap.SetValue(NormalMap);
            _depthMap.SetValue(DepthMap);
            _ssaoMap.SetValue(SSAOMap);

            _invertViewParameter.SetValue(InvertView);
            _invertProjectionParameter.SetValue(InvertProjection);
            _invertViewProjectionParameter.SetValue(InvertViewProjection);

            _nearClipParameter.SetValue(NearClip);
            _farClipParameter.SetValue(FarClip);

            _frustumCorners.SetValue(FrustumCorners);

            _skyIntensity.SetValue(SkyIntensity);

            _innerEffect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
