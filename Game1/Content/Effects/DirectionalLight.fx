#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1

//direction of the light
float3 LightDirection;

//color of the light 
float3 LightColor; 

float SkyIntensity;

float3 FrustumCornersVS[4];

//position of the camera, for specular light
float3 CameraPosWS; 

//this is used to compute the world-position
float4x4 InvertView; 

// diffuse color, and specularIntensity in the alpha channel
texture colorMap; 
// normals, and specularPower in the alpha channel
texture normalMap;
//depth
texture depthMap;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
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
sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput
{
    float3 Position : SV_POSITION;
    float3 TexCoordAndRayIndex : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
    float3 FrustumCornerVS : TEXCOORD1;
    float3 FrustumRayWS : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoordAndRayIndex.xy;
    output.FrustumCornerVS = FrustumCornersVS[input.TexCoordAndRayIndex.z];
    output.FrustumRayWS = mul(output.FrustumCornerVS, InvertView);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    //get depth
    float depthVal = tex2D(depthSampler, input.TexCoord);

    if (depthVal == 0) //skybox
        return float4(1, 1, 1, 0) * SkyIntensity; //over 1.0 for hdr

    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler, input.TexCoord);
    //transform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    //get specular power, and get it into [0,255] range]
    float specularPower = normalData.a * 255;

    //get color data from the colorMap
    float4 colorData = tex2D(colorSampler, input.TexCoord);
    //get specular intensity from the colorData
    float specularIntensity = colorData.a;
    //get diffuse color from the colorData
    float3 diffuseColor = colorData.rgb;

    //get worldspace position
    float3 positionWS = CameraPosWS + depthVal * input.FrustumRayWS;

    // Convert color to grayscale, just beacuse it looks nicer.
    float diffuseValue = 0.299 * diffuseColor.r + 0.587 * diffuseColor.g + 0.114 * diffuseColor.b;
    float3 diffuseAlbedo = float3(diffuseValue, diffuseValue, diffuseValue);

    float nDotL = saturate(dot(normal, LightDirection));

    float3 lighting = 0.0f;

    // Add the directional light.
    lighting += nDotL * (LightColor + float3(0.5f, 0.5f, 0.5f)) * diffuseAlbedo * (1.0f / 3.14159f);

    // Ambient light.
    lighting += float3(0.2f, 0.2f, 0.2f) * 1.0f * diffuseAlbedo;

    //reflection vector
    float3 reflectionVector = -(normalize(reflect(LightDirection, normal)));
    //camera-to-surface vector
    float3 directionToCamera = normalize(CameraPosWS - positionWS.xyz);
    //compute specular light
    float specularLight = 0; //specularIntensity * (LightColor + float3(0.2f, 0.2f, 0.2f)) * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower);

    return float4(max(lighting, 0.0001f), specularLight);
}

technique Technique0
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};