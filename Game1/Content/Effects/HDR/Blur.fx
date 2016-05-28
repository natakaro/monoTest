#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

//======================================================================
//
//	HDRSample
//
//		by MJP
//		09/20/08
//
//======================================================================
//
//	File:		pp_Blur.fx
//
//	Desc:		Implements several variants of post-processing blur
//				techiques.
//
//======================================================================

#include "Common.fxh"

float g_fSigma = 0.5f;

float CalcGaussianWeight(int iSamplePoint)
{
    float g = 1.0f / sqrt(2.0f * 3.14159 * g_fSigma * g_fSigma);
    return (g * exp(-(iSamplePoint * iSamplePoint) / (2 * g_fSigma * g_fSigma)));
}

float4 fGaussianBlurH(VertexShaderOutput input) : COLOR0
{
    int iRadius = 6;

    float4 vColor = 0;
    float2 vTexCoord = input.TexCoord;

    for (int i = -iRadius; i < iRadius; i++)
    {
        float fWeight = CalcGaussianWeight(i);
        vTexCoord.x = input.TexCoord.x + (i / g_vSourceDimensions.x);
        float4 vSample = tex2D(PointSampler0, vTexCoord);
        vColor += vSample * fWeight;
    }
	
    return vColor;
}

float4 fGaussianBlurV(VertexShaderOutput input) : COLOR0
{
    int iRadius = 6;

    float4 vColor = 0;
    float2 vTexCoord = input.TexCoord;

    for (int i = -iRadius; i < iRadius; i++)
    {
        float fWeight = CalcGaussianWeight(i);
        vTexCoord.y = input.TexCoord.y + (i / g_vSourceDimensions.y);
        float4 vSample = tex2D(PointSampler0, vTexCoord);
        vColor += vSample * fWeight;
    }

    return vColor;
}

technique GaussianBlurH
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL fGaussianBlurH();
    }
}

technique GaussianBlurV
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL fGaussianBlurV();
    }
}