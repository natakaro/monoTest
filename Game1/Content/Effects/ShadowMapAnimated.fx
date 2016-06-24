#define MaxBones 72

matrix WorldViewProjection;
matrix Bones[MaxBones];

struct VSInput
{
    float4 Position : POSITION0;
    int4 Indices : BLENDINDICES0;
    float4 Weights : BLENDWEIGHT0;
};

struct VSOutput
{
    float4 position : SV_Position;
    float2 depth : TEXCOORD0;
};

void Skin(inout VSInput vin, uniform int boneCount)
{
    float4x3 skinning = 0;

	[unroll]
    for (int i = 0; i < boneCount; i++)
    {
        skinning += Bones[vin.Indices[i]] * vin.Weights[i];
    }

    vin.Position.xyz = mul(vin.Position, skinning);
    //vin.Normal = mul(vin.Normal, (float3x3) skinning);
}

VSOutput VSShadowMap(VSInput input)
{
    Skin(input, 4);

    VSOutput output;
    output.position = mul(float4(input.Position.xyz, 1), WorldViewProjection);
    output.depth = output.position.zw;
    return output;
}

float4 PSShadowMap(VSOutput input) : COLOR
{
    return float4(input.depth.x / input.depth.y, 0, 0, 1);
}

technique ShadowMap
{
    pass
    {
        VertexShader = compile vs_4_0 VSShadowMap();
        PixelShader = compile ps_4_0 PSShadowMap();
    }
}