#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_3
#define PS_SHADERMODEL ps_4_0_level_9_3
#endif

int random_size;
float g_sample_rad;
float g_intensity;
float g_scale;
float g_bias;
int g_screen_size;

matrix InvertProjection;
matrix View;

Texture2D normalMap;
Texture2D depthMap;
Texture2D randomMap;

sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler randomSampler = sampler_state
{
    Texture = (randomMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
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
    VSOutput output = (VSOutput) 0;

    output.Position = input.Position;
    output.TexCoord = input.TexCoord;

    return output;
}

float3 getPosition(in float2 uv, in float depthVal)
{
    float4 position;
    position.x = uv.x * 2.0f - 1.0f;
    position.y = (1 - uv.y) * 2.0f - 1.0f;
    position.z = depthVal;
    position.w = 1.0f;

	//transform to view space
    position = mul(position, InvertProjection);
    position /= position.w;

    return position;
}

float3 getNormal(in float2 uv)
{
    return normalize(tex2D(normalSampler, uv).xyz * 2.0f - 1.0f);
}

float2 getRandom(in float2 uv)
{
    return normalize(tex2D(randomSampler, g_screen_size * uv / random_size).xy * 2.0f - 1.0f);
}

float doAmbientOcclusion(in float2 tcoord, in float2 uv, in float3 p, in float3 cnorm)
{
    float depthVal = tex2D(depthSampler, tcoord + uv);
    float3 diff = getPosition(tcoord + uv, depthVal) - p;
    const float3 v = normalize(diff);
    const float d = length(diff) * g_scale;
    return max(0.0, dot(cnorm, v) - g_bias) * (1.0 / (1.0 + d)) * g_intensity;
}

float4 MainPS(VSOutput input) : COLOR
{
    float4 output;
    output.rgb = 0.0f;

    const float2 vec[4] = { float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1) };

    float depthVal = tex2D(depthSampler, input.TexCoord);
    clip(-0.0001f + depthVal);
    float3 p = getPosition(input.TexCoord, depthVal);
    float3 n = mul(getNormal(input.TexCoord), View);
    float2 rand = getRandom(input.TexCoord);

    float ao = 0.0f;
    float rad = g_sample_rad / p.z;

	//**SSAO Calculation**//
    int iterations = 4;
    for (int j = 0; j < iterations; ++j)
    {
        float2 coord1 = reflect(vec[j], rand) * rad;
        float2 coord2 = float2(coord1.x * 0.707 - coord1.y * 0.707, coord1.x * 0.707 + coord1.y * 0.707);
  
        ao += doAmbientOcclusion(input.TexCoord, coord1 * 0.25, p, n);
        ao += doAmbientOcclusion(input.TexCoord, coord2 * 0.5, p, n);
        ao += doAmbientOcclusion(input.TexCoord, coord1 * 0.75, p, n);
        ao += doAmbientOcclusion(input.TexCoord, coord2, p, n);
    }

    ao = 1.0 - (ao / (float) iterations * 4.0);
	//**END**//

    output = ao;
    return output;
}

technique SSAO
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};