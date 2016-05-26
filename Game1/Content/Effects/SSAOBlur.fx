#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

int blurSize = 4;
float2 texelSize;

Texture2D SSAO;
sampler ssaoSampler = sampler_state
{
    Texture = (SSAO);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

struct VSInput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

VSOutput MainVS(in VSInput input)
{
	VSOutput output = (VSOutput)0;

	output.Position = input.Position;
	output.TexCoord = input.TexCoord;

	return output;
}

float4 MainPS(VSOutput input) : COLOR
{
    float4 output = 0.0;
    float2 hlim = float2((float) -blurSize * 0.5 + 0.5, (float) -blurSize * 0.5 + 0.5);

    float result = 0.0;
    for (int x = 0; x < 4; ++x)
    {
        for (int y = 0; y < 4; ++y)
        {
            float2 offset = float2((float) x, (float) y);
            offset += hlim;
            offset *= texelSize;

            result += tex2D(ssaoSampler, input.TexCoord + offset);
        }

    }

    result = result / (blurSize * blurSize);
    output = result;
    return output;
}

technique SSAOBlur
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};