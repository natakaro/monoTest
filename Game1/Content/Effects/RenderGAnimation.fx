#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

#define MaxBones 72

float4x4 World;
float4x4 Bones[MaxBones];
float4x4 View;
float FarClip;
float4x4 Projection;
float specularIntensity = 0.8f;
float specularPower = 0.5f;

float DissolveThreshold = 0.0f;
float EdgeSize = 0.15f;

float Emissive = 0.0f;

float4 OverlayColor = (1, 1, 1, 1);

texture Texture;
sampler diffuseSampler = sampler_state
{
	Texture = (Texture);
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler specularSampler = sampler_state
{
	Texture = (SpecularMap);
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture NormalMap;
sampler normalSampler = sampler_state
{
	Texture = (NormalMap);
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	Mipfilter = LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture DissolveMap;
sampler dissolveSampler = sampler_state
{
    Texture = (DissolveMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture EdgeMap;
sampler edgeSampler = sampler_state
{
    Texture = (EdgeMap);
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 Binormal : BINORMAL0;
	float3 Tangent : TANGENT0;
	int4   Indices  : BLENDINDICES0;
	float4 Weights  : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float Depth : TEXCOORD1;
	nointerpolation float3x3 tangentToWorld : TEXCOORD2;
};


void Skin(inout VertexShaderInput vin, uniform int boneCount)
{
	float4x3 skinning = 0;

	[unroll]
	for (int i = 0; i < boneCount; i++)
	{
		skinning += Bones[vin.Indices[i]] * vin.Weights[i];
	}

	vin.Position.xyz = mul(vin.Position, skinning);
	vin.Normal = mul(vin.Normal, (float3x3)skinning);
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	Skin(input, 4);

	float4 worldPosition = mul(float4(input.Position.xyz, 1), World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;

	//output.Depth.x = output.Position.z;
	//output.Depth.y = output.Position.w;

	output.Depth = viewPosition.z;

	// calculate tangent space to world space matrix using the world space tangent,
	// binormal, and normal as basis vectors
	output.tangentToWorld[0] = mul(input.Tangent, World);
	output.tangentToWorld[1] = mul(input.Binormal, World);
	output.tangentToWorld[2] = mul(input.Normal, World);

	return output;
}
struct PixelShaderOutput
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
	float Depth : COLOR2;
    float4 Emissive : COLOR3;
};

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;

	output.Color = tex2D(diffuseSampler, input.TexCoord);
    output.Color *= OverlayColor;

    output.Emissive = output.Color * Emissive;

    float4 dissolve = tex2D(dissolveSampler, input.TexCoord);

    float val = dissolve - DissolveThreshold;
    clip(val);

    if (val < EdgeSize && DissolveThreshold > 0 && DissolveThreshold < 1)
    {
        output.Emissive = tex2D(edgeSampler, float2(val * (1 / EdgeSize), 0)) * 10;
        output.Color *= output.Emissive;
    }

	float4 specularAttributes = tex2D(specularSampler, input.TexCoord);
	//specular Intensity
	output.Color.a = specularAttributes.r;

	// read the normal from the normal map
	float3 normalFromMap = tex2D(normalSampler, input.TexCoord);
	//tranform to [-1,1]
	normalFromMap = 2.0f * normalFromMap - 1.0f;
	//transform into world space
	normalFromMap = mul(normalFromMap, input.tangentToWorld);
	//normalize the result
	normalFromMap = normalize(normalFromMap);
	//output the normal, in [0,1] space
	output.Normal.rgb = 0.5f * (normalFromMap + 1.0f);

	//specular Power
	output.Normal.a = specularAttributes.a;

	//output.Depth = input.Depth.x / input.Depth.y;
	output.Depth = -input.Depth / FarClip;
	return output;
}

technique Technique1
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};