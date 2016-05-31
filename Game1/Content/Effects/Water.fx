#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3
	#define PS_SHADERMODEL ps_4_0_level_9_3
    //#define VS_SHADERMODEL vs_5_0
	//#define PS_SHADERMODEL ps_5_0
#endif
// Water pixel shader
// Copyright (C) Wojciech Toman 2009

//Matrix World;
//Matrix View;
//Matrix Projection;
Matrix ViewProjection;
Matrix InvertView;

//Matrix ReflectionView;

float3 FrustumCornersVS[4];
// Position of the camera
float3 cameraPos;

Texture2D reflectionMap;
sampler reflectionSampler = sampler_state
{
    texture = <ReflectionMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = mirror;
    AddressV = mirror;
};

Texture2D colorMap;
sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

Texture2D depthMap;
sampler depthSampler = sampler_state
{
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

Texture2D heightMap;
sampler heightSampler = sampler_state
{
    Texture = (heightMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

Texture2D normalMap;
sampler normalSampler = sampler_state
{
    Texture = (normalMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

Texture2D foamMap;
sampler foamSampler = sampler_state
{
    Texture = (foamMap);
    AddressU = WRAP;
    AddressV = WRAP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

// Level at which water surface begins
float waterLevel = 5.0f;

// How fast will colours fade out. You can also think about this
// values as how clear water is. Therefore use smaller values (eg. 0.05f)
// to have crystal clear water and bigger to achieve "muddy" water.
float fadeSpeed = 0.15f;

// Timer
float timer;

// Normals scaling factor
float normalScale = 1.0f;

// R0 is a constant related to the index of refraction (IOR).
// It should be computed on the CPU and passed to the shader.
float R0 = 0.5f;

// Maximum waves amplitude
float maxAmplitude = 1.0f;

// Direction of the light
float3 lightDir;

// Colour of the sun
float3 sunColor;

// The smaller this value is, the more soft the transition between
// shore and water. If you want hard edges use very big value.
// Default is 1.0f.
float shoreHardness = 1.0f;

// This value modifies current fresnel term. If you want to weaken
// reflections use bigger value. If you want to empasize them use
// value smaller then 0. Default is 0.0f.
float refractionStrength = 0.0f;
//float refractionStrength = -0.3f;

// Modifies 4 sampled normals. Increase first values to have more
// smaller "waves" or last to have more bigger "waves"
float4 normalModifier = { 1.0f, 2.0f, 4.0f, 8.0f };

// Strength of displacement along normal.
float displace = 1.7f;

// Describes at what depth foam starts to fade out and
// at what it is completely invisible. The fird value is at
// what height foam for waves appear (+ waterLevel).
float3 foamExistence = { 0.65f, 1.35f, 0.7f };

float sunScale = 3.0f;

Matrix ReflectionMatrix =
{
    { 0.5f, 0.0f, 0.0f, 0.5f },
    { 0.0f, 0.5f, 0.0f, 0.5f },
    { 0.0f, 0.0f, 0.0f, 0.5f },
    { 0.0f, 0.0f, 0.0f, 1.0f }
};

float shininess = 0.7f;
float specular_intensity = 0.32;

// Colour of the water surface
float3 depthColour = { 0.0078f, 0.5176f, 0.65f };
// Colour of the water depth
float3 bigDepthColour = { 0.0039f, 0.00196f, 0.145f };
float3 extinction = { 7.0f, 30.0f, 40.0f }; // Horizontal

// Water transparency along eye vector.
float visibility = 4.0f;

// Increase this value to have more smaller waves.
float2 scale = { 0.005f, 0.005f };
float refractionScale = 0.005f;

// Wind force in x and z axes.
float2 wind = { -0.3f, 0.7f };


struct VSInput
{
    float3 Position : SV_POSITION;
    float3 TexCoordAndRayIndex : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
    float3 FrustumCornerVS : TEXCOORD1;
    float3 FrustumRayWS : TEXCOORD2;
};

VSOutput WaterVS(VSInput input)
{
    VSOutput output = (VSOutput) 0;

    output.Position = float4(input.Position, 1);
    output.TexCoord = input.TexCoordAndRayIndex.xy;
    output.FrustumCornerVS = FrustumCornersVS[input.TexCoordAndRayIndex.z];
    output.FrustumRayWS = mul(output.FrustumCornerVS, InvertView);

    return output;
}

float3x3 compute_tangent_frame(float3 N, float3 P, float2 UV)
{
    float3 dp1 = ddx(P);
    float3 dp2 = ddy(P);
    float2 duv1 = ddx(UV);
    float2 duv2 = ddy(UV);
	
    float3x3 M = float3x3(dp1, dp2, cross(dp1, dp2));
    float2x3 inverseM = float2x3(cross(M[1], M[2]), cross(M[2], M[0]));
    float3 T = mul(float2(duv1.x, duv2.x), inverseM);
    float3 B = mul(float2(duv1.y, duv2.y), inverseM);
	
    return float3x3(normalize(T), normalize(B), N);
}

// Function calculating fresnel term.
// - normal - normalized normal vector
// - eyeVec - normalized eye vector
float fresnelTerm(float3 normal, float3 eyeVec)
{
    float angle = 1.0f - saturate(dot(normal, eyeVec));
    float fresnel = angle * angle;
    fresnel = fresnel * fresnel;
    fresnel = fresnel * angle;
    return saturate(fresnel * (1.0f - saturate(R0)) + R0 - refractionStrength);
}

float4 WaterPS(VSOutput input) : COLOR0
{
    float3 color2 = colorMap.Sample(colorSampler, input.TexCoord).rgb;
    float3 color = color2;

    float depthVal = depthMap.Sample(depthSampler, input.TexCoord);
    if (depthVal == 0)
        depthVal = 1; //horizon hack
    float3 positionWS = cameraPos + depthVal * input.FrustumRayWS;

    float level = waterLevel;
    float depth = 0.0f;

    // If we are underwater let's leave out complex computations
    if (level >= cameraPos.y)
        return float4(color2, 1.0f);

    if (positionWS.y <= level + maxAmplitude)
    {
        float3 eyeVec = positionWS - cameraPos;
        float diff = level - positionWS.y;
        float cameraDepth = cameraPos.y - positionWS.y;
		
		// Find intersection with water surface
        float3 eyeVecNorm = normalize(eyeVec);
        float t = (level - cameraPos.y) / eyeVecNorm.y;
        float3 surfacePoint = cameraPos + eyeVecNorm * t;
		
        eyeVecNorm = normalize(eyeVecNorm);

        float2 texCoord;
        for (int i = 0; i < 10; ++i)
        {
            texCoord = (surfacePoint.xz + eyeVecNorm.xz * 0.1f) * scale + timer * 0.000005f * wind;
			
            float bias = heightMap.Sample(heightSampler, texCoord).r;
	
            bias *= 0.1f;
            level += bias * maxAmplitude;
            t = (level - cameraPos.y) / eyeVecNorm.y;
            surfacePoint = cameraPos + eyeVecNorm * t;
        }

        depth = length(positionWS - surfacePoint);
        float depth2 = surfacePoint.y - positionWS.y;

        eyeVecNorm = normalize(cameraPos - surfacePoint);
        
        float normal1 = heightMap.Sample(heightSampler, (texCoord + float2(-1, 0) / 256)).r;
        float normal2 = heightMap.Sample(heightSampler, (texCoord + float2(1, 0) / 256)).r;
        float normal3 = heightMap.Sample(heightSampler, (texCoord + float2(0, -1) / 256)).r;
        float normal4 = heightMap.Sample(heightSampler, (texCoord + float2(0, 1) / 256)).r;
		
        float3 myNormal = normalize(float3((normal1 - normal2) * maxAmplitude,
										   normalScale,
										   (normal3 - normal4) * maxAmplitude));
		
        texCoord = surfacePoint.xz * 1.6 + wind * timer * 0.00016;
        float3x3 tangentFrame = compute_tangent_frame(myNormal, eyeVecNorm, texCoord);
        float3 normal0a = normalize(mul(2.0f * normalMap.Sample(normalSampler, texCoord).xyz - 1.0f, tangentFrame));

        texCoord = surfacePoint.xz * 0.8 + wind * timer * 0.00008;
        tangentFrame = compute_tangent_frame(myNormal, eyeVecNorm, texCoord);
        float3 normal1a = normalize(mul(2.0f * normalMap.Sample(normalSampler, texCoord).xyz - 1.0f, tangentFrame));
		
        texCoord = surfacePoint.xz * 0.4 + wind * timer * 0.00004;
        tangentFrame = compute_tangent_frame(myNormal, eyeVecNorm, texCoord);
        float3 normal2a = normalize(mul(2.0f * normalMap.Sample(normalSampler, texCoord).xyz - 1.0f, tangentFrame));
		
        texCoord = surfacePoint.xz * 0.1 + wind * timer * 0.00002;
        tangentFrame = compute_tangent_frame(myNormal, eyeVecNorm, texCoord);
        float3 normal3a = normalize(mul(2.0f * normalMap.Sample(normalSampler, texCoord).xyz - 1.0f, tangentFrame));
		
        float3 normal = normalize(normal0a * normalModifier.x + normal1a * normalModifier.y +
								  normal2a * normalModifier.z + normal3a * normalModifier.w);

        texCoord = input.TexCoord.xy;
        texCoord.x += sin(timer * 0.002f + 3.0f * abs(positionWS.y)) * (refractionScale * min(depth2, 1.0f));
        float3 refraction = colorMap.Sample(colorSampler, texCoord).rgb;

        float refractionDepthSample = depthMap.Sample(depthSampler, texCoord);
        float3 refractionPositionWS = cameraPos + depthVal * input.FrustumRayWS;

        if (refractionPositionWS.y > level)
            refraction = color2;

        matrix TextureProjectionMatrix = mul(ViewProjection, ReflectionMatrix);

        //float3 waterPosition = surfacePoint.xyz;
        //waterPosition.y -= (level - waterLevel);
        //float4 texCoordProj = mul(float4(waterPosition, 1.0f), TextureProjectionMatrix);
        //
        //float4 dPos;
        //dPos.x = texCoordProj.x + displace * normal.x;
        //dPos.z = texCoordProj.z + displace * normal.z;
        //dPos.yw = texCoordProj.yw;
        //texCoordProj = dPos;

        //float3 reflect = reflectionMap.Sample(reflectionSampler, texCoordProj);
        float2 texCoordProj = input.TexCoord.xy;
        texCoordProj.x = texCoordProj.x + displace * normal.x;
        texCoordProj.y = texCoordProj.y + displace * normal.z;
        float3 reflect = reflectionMap.Sample(reflectionSampler, texCoordProj);


        float fresnel = fresnelTerm(normal, eyeVecNorm);

        float3 depthN = depth * fadeSpeed;
        float3 waterCol = saturate(length(sunColor) / sunScale);
        refraction = lerp(lerp(refraction, depthColour * waterCol, saturate(depthN / visibility)),
						  bigDepthColour * waterCol, saturate(depth2 / extinction));

        float foam = 0.0f;

        texCoord = (surfacePoint.xz + eyeVecNorm.xz * 0.1) * 0.05 + timer * 0.00001f * wind + sin(timer * 0.001 + positionWS.x) * 0.005;
        float2 texCoord2 = (surfacePoint.xz + eyeVecNorm.xz * 0.1) * 0.05 + timer * 0.00002f * wind + sin(timer * 0.001 + positionWS.z) * 0.005;  

        if (depth2 < foamExistence.x)
            foam = (foamMap.Sample(foamSampler, texCoord) + foamMap.Sample(foamSampler, texCoord2)) * 0.5f;
        else if (depth2 < foamExistence.y)
        {
            foam = lerp((foamMap.Sample(foamSampler, texCoord) + foamMap.Sample(foamSampler, texCoord2)) * 0.5f, 0.0f,
						 (depth2 - foamExistence.x) / (foamExistence.y - foamExistence.x));
        }

        if (maxAmplitude - foamExistence.z > 0.0001f)
        {
            foam += (foamMap.Sample(foamSampler, texCoord) + foamMap.Sample(foamSampler, texCoord2)) * 0.5f *
				saturate((level - (waterLevel + foamExistence.z)) / (maxAmplitude - foamExistence.z));
        }

        half3 specular = 0.0f;

        half3 mirrorEye = (2.0f * dot(eyeVecNorm, normal) * normal - eyeVecNorm);
        half dotSpec = saturate(dot(mirrorEye.xyz, lightDir) * 0.5f + 0.5f);
<<<<<<< HEAD
        specular = (1.0f - fresnel) * saturate(lightDir.y) * ((pow(dotSpec, 512.0f)) * (shininess * 1.8f + 0.2f)) * sunColor;
        specular += specular * 25 * saturate(shininess - 0.05f) * sunColor;
=======
        specular = (1.0f - fresnel) * saturate(lightDir.y) * ((pow(dotSpec, 512.0f)) * (shininess * 1.8f + 0.2f)) * min(0.1, sunColor);
		specular += specular * 25 * saturate(shininess - 0.05f) * min(0.1, sunColor);
>>>>>>> a3531435539c161199fec4d30eef7df2eed969dc

        color = lerp(refraction, reflect, fresnel);
        color = saturate(color + max(specular, foam * sunColor));
		
        color = lerp(refraction, color, saturate(depth * shoreHardness));
    }
    
    if (positionWS.y > level)
        color = color2;

    return float4(color, 1);
}

technique Water
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL WaterVS();
		PixelShader = compile PS_SHADERMODEL WaterPS();
	}
};