//======================================================================
//
//	HDRSample
//
//		by MJP
//		09/20/08
//
//======================================================================
//
//	File:		pp_Common.fx
//
//	Desc:		Common samplers, global variable, and vertex shader 
//				shared by post-processing shaders.
//
//======================================================================

float2	g_vSourceDimensions;
float2	g_vDestinationDimensions;

texture2D SourceTexture0;
sampler2D PointSampler0 = sampler_state
{
    Texture = <SourceTexture0>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D LinearSampler0 = sampler_state
{
    Texture = <SourceTexture0>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

texture2D SourceTexture1;
sampler2D PointSampler1 = sampler_state
{
    Texture = <SourceTexture1>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D LinearSampler1 = sampler_state
{
    Texture = <SourceTexture1>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

texture2D SourceTexture2;
sampler2D PointSampler2 = sampler_state
{
    Texture = <SourceTexture2>;
    MinFilter = point;
    MagFilter = point;
    MipFilter = point;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D LinearSampler2 = sampler_state
{
    Texture = <SourceTexture2>;
    MinFilter = linear;
    MagFilter = linear;
    MipFilter = point;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VertexShaderInput
{
    float3 Position : POSITION;
    float3 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput PostProcessVS(VertexShaderInput input)
{
    VertexShaderOutput ouput = (VertexShaderOutput) 0;
	
    ouput.Position = float4(input.Position, 1);
    ouput.TexCoord = input.TexCoord.xy;
	
    return ouput;
}


	