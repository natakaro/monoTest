#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3
	#define PS_SHADERMODEL ps_4_0_level_9_3
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

float4 fGaussianDepthBlurH(VertexShaderOutput input) : COLOR0
{
    int iRadius = 6;

    float4 vColor = 0;
    float2 vTexCoord = input.TexCoord;
    float4 vCenterColor = tex2D(PointSampler0, input.TexCoord);
    float fCenterDepth = tex2D(PointSampler1, input.TexCoord).x;
    if(fCenterDepth == 0)
        fCenterDepth = 1;

    for (int i = -iRadius; i < 0; i++)
    {
        vTexCoord.x = input.TexCoord.x + (i / g_vSourceDimensions.x);
        float fDepth = tex2D(PointSampler1, vTexCoord).x;
        if (fDepth == 0)
            fDepth = 1;
        float fWeight = CalcGaussianWeight(i);
    
        if (fDepth >= fCenterDepth)
        {
            float4 vSample = tex2D(PointSampler0, vTexCoord);
            vColor += vSample * fWeight;
        }
        else
            vColor += vCenterColor * fWeight;
    }
    
    for (int i = 1; i < iRadius; i++)
    {
        vTexCoord.x = input.TexCoord.x + (i / g_vSourceDimensions.x);
        float fDepth = tex2D(PointSampler1, vTexCoord).x;
        if (fDepth == 0)
            fDepth = 1;
        float fWeight = CalcGaussianWeight(i);
    
        if (fDepth >= fCenterDepth)
        {
            float4 vSample = tex2D(PointSampler0, vTexCoord);
            vColor += vSample * fWeight;
        }
        else
            vColor += vCenterColor * fWeight;
    }
    
    vColor += vCenterColor * CalcGaussianWeight(0);
	
    return vColor;
}

float4 fGaussianDepthBlurV(VertexShaderOutput input) : COLOR0
{
    int iRadius = 6;

    float4 vColor = 0;
    float2 vTexCoord = input.TexCoord;
    float4 vCenterColor = tex2D(PointSampler0, input.TexCoord);
    float fCenterDepth = tex2D(PointSampler1, input.TexCoord).x;
    if (fCenterDepth == 0)
        fCenterDepth = 1;

    for (int i = -iRadius; i < 0; i++)
    {
        vTexCoord.y = input.TexCoord.y + (i / g_vSourceDimensions.y);
        float fDepth = tex2D(PointSampler1, vTexCoord).x;
        if (fDepth == 0)
            fDepth = 1;
        float fWeight = CalcGaussianWeight(i);
		
        if (fDepth >= fCenterDepth)
        {
            float4 vSample = tex2D(PointSampler0, vTexCoord);
            vColor += vSample * fWeight;
        }
        else
            vColor += vCenterColor * fWeight;
    }
    
    for (int i = 1; i < iRadius; i++)
    {
        vTexCoord.y = input.TexCoord.y + (i / g_vSourceDimensions.y);
        float fDepth = tex2D(PointSampler1, vTexCoord).x;
        if (fDepth == 0)
            fDepth = 1;
        float fWeight = CalcGaussianWeight(i);
    
        if (fDepth >= fCenterDepth)
        {
            float4 vSample = tex2D(PointSampler0, vTexCoord);
            vColor += vSample * fWeight;
        }
        else
            vColor += vCenterColor * fWeight;
    }
	
    vColor += vCenterColor * CalcGaussianWeight(0);
	
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

technique GaussianDepthBlurH
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL fGaussianDepthBlurH();
    }
}

technique GaussianDepthBlurV
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL fGaussianDepthBlurV();
    }
}