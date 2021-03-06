﻿#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_5_0
#define PS_SHADERMODEL ps_5_0
#endif

float Radius;
float Power;
int SampleKernelSize = 16;
float2 NoiseScale;
float3 SampleKernel[16];

matrix InvertProjection;
matrix View;
matrix Projection;

float3 FrustumCornersVS[4];

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
    float3 Position : SV_POSITION;
    float3 TexCoordAndRayIndex : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 FrustumCornerVS : TEXCOORD1;
};

VSOutput MainVS(in VSInput input)
{
    VSOutput output;

    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoordAndRayIndex.xy;
    output.FrustumCornerVS = FrustumCornersVS[input.TexCoordAndRayIndex.z];

    return output;
}

float3 getNormal(in float2 uv)
{
    return tex2D(normalSampler, uv).xyz * 2.0f - 1.0f;
}

float4 MainPS(VSOutput input) : COLOR
{
    float4 output;
    output.rgb = 0.0f;

    float depthVal = tex2D(depthSampler, input.TexCoord);
    clip(-0.0001f + depthVal); //skip skybox

    float3 origin = input.FrustumCornerVS * depthVal;
    float3 normal = mul(getNormal(input.TexCoord), View);

    float3 rvec = tex2D(randomSampler, input.TexCoord * NoiseScale).xyz * 2.0 - 1.0;
    float3 tangent = normalize(rvec - normal * dot(rvec, normal));
    float3 bitangent = cross(normal, tangent);
    float3x3 tbn = float3x3(tangent, bitangent, normal);
    
    float occlusion = 0.0;
    for (int i = 0; i < SampleKernelSize; ++i)
    {
        //get sample position
        float3 sample = mul(SampleKernel[i], tbn);
        sample = sample * Radius + origin;

        float3 sampleDir = normalize(sample - origin);
        
        float NdotS = max(dot(normal, sampleDir), 0);

        //clip(0.966 - NdotS);
    
        //project sample position
        float4 offset = float4(sample, 1.0);
        offset = mul(offset, Projection); // to clip space
        offset.xy /= offset.w; // to NDC
        offset.xy = offset.xy * 0.5 + 0.5; // to texture coords
        offset.y = 1 - offset.y;
    
        //get sample depth
        float sampleDepth = tex2D(depthSampler, offset.xy);
        sampleDepth = input.FrustumCornerVS.z * sampleDepth;

        //range check & accumulate

        //float rangeCheck = abs(origin.z - sampleDepth) < Radius ? 1.0 : 0.0;
        //occlusion += (sampleDepth <= sample.z ? 0.0 : 1.0) * rangeCheck;

        float rangeCheck = smoothstep(0.0, 1.0, Radius / abs(origin.z - sampleDepth));
        occlusion += rangeCheck * step(sample.z, sampleDepth) * NdotS;
    }

    occlusion = 1.0 - (occlusion / SampleKernelSize);

    float finalOcclusion = pow(occlusion, Power);
    return float4(finalOcclusion, finalOcclusion, finalOcclusion, 1.0);
}

technique SSAO
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};