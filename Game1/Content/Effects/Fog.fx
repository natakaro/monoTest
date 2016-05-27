#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 FogColor;
float FogDensity;

float NearClip;
float FarClip;

texture depthMap;

sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PixelShaderFunctionLinear(VertexShaderOutput input) : COLOR0
{
    float depthVal = tex2D(depthSampler, input.TexCoord);
    //clip(-depthVal + 0.9999f);
    clip(-0.0001f + depthVal);
    float mix = depthVal;

    return float4(FogColor.rgb, mix);
}

float4 PixelShaderFunctionExp(VertexShaderOutput input) : COLOR0
{
    float depthVal = tex2D(depthSampler, input.TexCoord);
    //clip(-depthVal + 0.9999f);
    clip(-0.0001f + depthVal);
    float mix = saturate(1 - exp(-depthVal * FogDensity));

    return float4(FogColor.rgb, mix);
}

float4 PixelShaderFunctionExp2(VertexShaderOutput input) : COLOR0
{
    float depthVal = tex2D(depthSampler, input.TexCoord);
    //clip(-depthVal + 0.9999f);
    clip(-0.0001f + depthVal);
    float mix = saturate(1 - exp(-(depthVal * FogDensity * depthVal * FogDensity)));
    return float4(FogColor.rgb, mix);
}

technique FogLinear
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunctionLinear();
    }
};

technique FogExp
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunctionExp();
    }
};

technique FogExp2
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunctionExp2();
    }
};