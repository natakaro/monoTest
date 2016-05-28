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
//	File:		pp_HDR.fx
//
//	Desc:		A few routines used for HDR tone mapping
//
//======================================================================

#include "Common.fxh"
#include "Tonemap.fxh"

float g_fDT;
float g_fBloomMultiplier;

float4 LuminancePS(VertexShaderOutput input) : COLOR0
{
    float4 vSample = tex2D(LinearSampler0, input.TexCoord);
    float3 vColor = vSample.rgb;
   
    // calculate the luminance using a weighted average
    float fLuminance = CalcLuminance(vColor);
                
    float fLogLuminace = log(1e-5 + fLuminance);
        
    // Output the luminance to the render target
    return float4(fLogLuminace, 1.0f, 0.0f, 0.0f);
}

float4 CalcAdaptedLumPS(VertexShaderOutput input) : COLOR0
{
    float fLastLum = tex2D(PointSampler1, float2(0.5f, 0.5f)).r;
    float fCurrentLum = tex2D(PointSampler0, float2(0.5f, 0.5f)).r;
    
    // Adapt the luminance using Pattanaik's technique
    const float fTau = 0.5f;
    float fAdaptedLum = fLastLum + (fCurrentLum - fLastLum) * (1 - exp(-g_fDT * fTau));
    
    return float4(fAdaptedLum, 1.0f, 1.0f, 1.0f);
}

float4 ToneMapPS(VertexShaderOutput input) : COLOR0
{
	// Sample the original HDR image
    float4 vSample = tex2D(PointSampler0, input.TexCoord);
    float3 vHDRColor = vSample.rgb;
		
	// Do the tone-mapping
    float3 vToneMapped = fToneMap(vHDRColor);
	
	// Add in the bloom component
    float3 vColor = vToneMapped + tex2D(LinearSampler2, input.TexCoord).rgb * g_fBloomMultiplier;
	
    return float4(vColor, 1.0f);
}

technique Luminance
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL LuminancePS();
    }
}

technique CalcAdaptedLuminance
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL CalcAdaptedLumPS();
    }
}

technique ToneMap
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL ToneMapPS();
    }
}

