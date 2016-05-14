#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

float4x4 World;
float4x4 View;
float4x4 Projection;
 
texture SkyBoxTexture;
sampler cubeTextureSampler = sampler_state
{
    texture = <SkyBoxTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

struct SkyBoxVertexInput
{
    float4 Position : SV_Position0;
};

struct SkyBoxVertexToPixel
{
    float4 Position : SV_Position;
    float3 Pos3D : TEXCOORD0;
};

SkyBoxVertexToPixel SkyBoxVS(SkyBoxVertexInput input)
{
    SkyBoxVertexToPixel output;
    float4x4 preViewProjection = mul(View, Projection);
    float4x4 preWorldViewProjection = mul(World, preViewProjection);

    output.Position = mul(input.Position, preWorldViewProjection);
    output.Pos3D = input.Position;

    return output;
}

struct SkyBoxPixelToFrame
{
    float4 Color : COLOR0;
};

SkyBoxPixelToFrame SkyBoxPS(SkyBoxVertexToPixel PSIn)
{
    SkyBoxPixelToFrame output;
    output.Color = texCUBE(cubeTextureSampler, PSIn.Pos3D);

    return output;
}

technique Skybox
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL SkyBoxVS();
		PixelShader = compile PS_SHADERMODEL SkyBoxPS();
	}
};