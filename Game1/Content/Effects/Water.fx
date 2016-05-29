#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
// Water pixel shader
// Copyright (C) Wojciech Toman 2009

Matrix World;
Matrix View;
Matrix ReflectionView;
Matrix Projection;

Texture2D ReflectionMap;
sampler ReflectionSampler = sampler_state
{
    texture = <ReflectionMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

struct VSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 ReflectionMapSamplingPos : TEXCOORD0;
};

VSOutput WaterVS(VSInput input)
{
    VSOutput output = (VSOutput) 0;

    float4x4 preViewProjection = mul(View, Projection);
    float4x4 preWorldViewProjection = mul(World, preViewProjection);
    float4x4 preReflectionViewProjection = mul(ReflectionView, Projection);
    float4x4 preWorldReflectionViewProjection = mul(World, preReflectionViewProjection);

    output.Position = mul(input.Position, preWorldViewProjection);
    output.ReflectionMapSamplingPos = mul(input.Position, preWorldReflectionViewProjection);

    //float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
    //float4 viewPosition = mul(worldPosition, View);
    //output.Position = mul(viewPosition, Projection);
    //
    //float4 reflectionViewPosition = mul(worldPosition, ReflectionView);
    //output.ReflectionMapSamplingPos = output.Position;// mul(reflectionViewPosition, Projection);

    return output;
}

float4 WaterPS(VSOutput input) : COLOR0
{
    float4 output;
    
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = input.ReflectionMapSamplingPos.x / input.ReflectionMapSamplingPos.w / 2.0f + 0.5f;
    ProjectedTexCoords.y = -input.ReflectionMapSamplingPos.y / input.ReflectionMapSamplingPos.w / 2.0f + 0.5f;

    output = tex2D(ReflectionSampler, ProjectedTexCoords);
    
    return output;
}

technique Water
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL WaterVS();
		PixelShader = compile PS_SHADERMODEL WaterPS();
	}
};