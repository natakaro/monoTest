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
//	File:		pp_Scaling.fx
//
//	Desc:		Used for downscaling in software and scaling in
//				hardware.
//
//======================================================================

#include "Common.fxh"

static const float g_vOffsets[4] = { -1.5f, -0.5f, 0.5f, 1.5f };

// Downscales to 1/16 size, using 16 samples
float4 DownscalePS(VertexShaderOutput input) : COLOR0
{
    float4 vColor = 0;
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float2 vOffset;
            vOffset = float2(g_vOffsets[x], g_vOffsets[y]) / g_vSourceDimensions;
            float4 vSample = tex2D(PointSampler0, input.TexCoord + vOffset);
            vColor += vSample;
        }
    }

    vColor /= 16.0f;
	
    return vColor;
}

float4 DownscalePSDecode(VertexShaderOutput input) : COLOR0
{
    float4 vColor = 0;
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float2 vOffset;
            vOffset = float2(g_vOffsets[x], g_vOffsets[y]) / g_vSourceDimensions;
            float4 vSample = tex2D(PointSampler0, input.TexCoord + vOffset);
            vColor += vSample;
        }
    }

    vColor /= 16.0f;
		
    vColor = float4(exp(vColor.r), 1.0f, 1.0f, 1.0f);
	
    return vColor;
}

// Upscales or downscales using hardware bilinear filtering
float4 HWScalePS(VertexShaderOutput input) : COLOR0
{
    return tex2D(LinearSampler0, input.TexCoord);
}


technique Downscale4
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL DownscalePS();
    }
}

technique Downscale4Luminance
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL DownscalePSDecode();
    }
}

technique ScaleHW
{
    pass p0
    {
        VertexShader = compile VS_SHADERMODEL PostProcessVS();
        PixelShader = compile PS_SHADERMODEL HWScalePS();
    }
}