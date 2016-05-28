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
//	File:		pp_Threshold.fx
//
//	Desc:		Applies a threshold, typically used to apply bloom.
//
//======================================================================

#include "Common.fxh"
#include "Tonemap.fxh"

float g_fThreshold = 0.7f;

float4 ThresholdPS(VertexShaderOutput input) : COLOR0
{
    float4 vSample = tex2D(PointSampler0, input.TexCoord);

    vSample = float4(fToneMap(vSample.rgb), 1.0f);
		
    vSample -= g_fThreshold;
    vSample = max(vSample, 0.0f);

    return vSample;
}


technique Threshold
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL ThresholdPS();
    }
}