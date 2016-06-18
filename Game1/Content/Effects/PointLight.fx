#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float FarClip;

//color of the light 
float3 Color; 

//position of the camera, for specular light
float3 cameraPosition; 

//this is used to compute the world-position
float4x4 InvertViewProjection; 
float4x4 InvertView;

//this is the position of the light
float3 lightPosition;

//how far does this light reach
float lightRadius;

//control the brightness of the light
float lightIntensity = 1.0f;

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
    float3 Position : SV_Position;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
    float4 PositionCS : TEXCOORD0;
    float3 PositionVS : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    float4 worldPosition = mul(float4(input.Position,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.PositionCS = output.Position;
    output.PositionVS = viewPosition;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    //obtain screen position
    input.PositionCS.xy /= input.PositionCS.w;

    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.PositionCS.x,-input.PositionCS.y) + 1);

    //read depth
    float depthVal = tex2D(depthSampler, texCoord);
    clip(-0.0001f + depthVal);

    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler,texCoord);
    //tranform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    //get specular power
    float specularPower = normalData.a * 255;
    //get specular intensity from the colorMap
    float specularIntensity = tex2D(colorSampler, texCoord).a;

    float3 viewRay = input.PositionVS.xyz * (FarClip / -input.PositionVS.z);
    float3 positionVS = viewRay * depthVal;
    float3 position = mul(float4(positionVS, 1), InvertView);

    //surface-to-light vector
    float3 lightVector = lightPosition - position;

    //compute attenuation based on distance - linear attenuation
    float attenuation = saturate(1.0f - length(lightVector)/lightRadius); 

    //normalize light vector
    lightVector = normalize(lightVector); 

    //compute diffuse light
    float NdL = max(0,dot(normal,lightVector));
    float3 diffuseLight = NdL * Color.rgb;

    //reflection vector
    float3 reflectionVector = normalize(reflect(-lightVector, normal));
    //camera-to-surface vector
    float3 directionToCamera = normalize(cameraPosition - position);
    //compute specular light
    float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

    //take into account attenuation and lightIntensity.
    return attenuation * lightIntensity * float4(diffuseLight.rgb,specularLight);
}

technique Technique1
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};