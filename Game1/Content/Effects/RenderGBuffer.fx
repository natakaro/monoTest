#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0

float4x4 World;
float4x4 View;
float4x4 Projection;
float FarClip;
float specularIntensity = 0.8f;
float specularPower = 0.5f; 
float4 heightcolor = float4(1, 0, 0, 1);

bool Clipping = false;
float4 ClipPlane;

float DissolveThreshold = 0.0f;
float EdgeSize = 0.15f;

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
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float Depth : TEXCOORD1;
    nointerpolation float3x3 tangentToWorld : TEXCOORD2;
	float4x4 instanceTransform : BLENDWEIGHT;

    float4 Clip : SV_ClipDistance0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float4x4 instanceTransform)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), instanceTransform);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TexCoord = input.TexCoord;
    //classic depth
    //output.Depth.x = output.Position.z;
    //output.Depth.y = output.Position.w;
    //linear depth
    output.Depth = viewPosition.z;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors
    output.tangentToWorld[0] = mul(input.Tangent, instanceTransform);
    output.tangentToWorld[1] = mul(input.Binormal, instanceTransform);
    output.tangentToWorld[2] = mul(input.Normal, instanceTransform);
	output.instanceTransform = instanceTransform;

    if(Clipping)
        output.Clip = dot(worldPosition, ClipPlane);
    else
        output.Clip = 0;
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

    float4 dissolve = tex2D(dissolveSampler, input.TexCoord);

    float val = dissolve - DissolveThreshold;
    clip(val);

    if(val < EdgeSize && DissolveThreshold > 0 && DissolveThreshold < 1)
    {
        output.Emissive = tex2D(edgeSampler, float2(val * (1 / EdgeSize), 0)) * 10;
        output.Color *= output.Emissive;
    }

	//output.Color *= heightcolor * World[3][1];
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

    //classic depth
    //output.Depth = input.Depth.x / input.Depth.y;
    //linear depth
    output.Depth = -input.Depth / FarClip;
    return output;
}


PixelShaderOutput PixelShaderFunctionColor(VertexShaderOutput input)
{
	PixelShaderOutput output;

	//output.Color = tex2D(diffuseSampler, input.TexCoord);
	if (input.instanceTransform[3][1] > 0)
	{
		output.Color = tex2D(diffuseSampler, float2(0, input.instanceTransform[3][1] / 255));
	}
	else
	{
		output.Color = tex2D(diffuseSampler, float2(0, 0));
	}

    float4 dissolve = tex2D(dissolveSampler, input.TexCoord);

    float val = dissolve - DissolveThreshold;
    clip(val);

    if (val < EdgeSize && DissolveThreshold > 0 && DissolveThreshold < 1)
    {
        output.Emissive = tex2D(edgeSampler, float2(val * (1 / EdgeSize), 0)) * 10;
        output.Color *= output.Emissive;
    }

	//output.Color *= float4 (0.01*input.instanceTransform[3][1], 1-0.01*input.instanceTransform[3][1], 0, 1);
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

	//classic depth
    //output.Depth = input.Depth.x / input.Depth.y;
    //linear depth
    output.Depth = -input.Depth / FarClip;
	return output;
}


VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input, float4x4 instanceTransform : BLENDWEIGHT)
{
	return VertexShaderFunction(input, mul(World, transpose(instanceTransform)));
}

VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
	return VertexShaderFunction(input, World);
}


technique Technique1
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL NoInstancingVertexShader();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};

technique Instancing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL HardwareInstancingVertexShader();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};

technique InstancingColor
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL HardwareInstancingVertexShader();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunctionColor();
	}
};