#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define CONTRAST_POWER 1.2
#define FADE_DISTANCE 5

// Camera parameters.
matrix View;
matrix Projection;
float2 ViewportScale;

// The current time, in seconds.
float CurrentTime;

// Parameters describing how the particles animate.
float Duration;
float DurationRandomness;
float3 Gravity;
float EndVelocity;
float4 MinColor;
float4 MaxColor;

// These float2 parameters describe the min and max of a range.
// The actual value is chosen differently for each particle,
// interpolating between x and y by some random amount.
float2 RotateSpeed;
float2 StartSize;
float2 EndSize;

// Particle texture and sampler.
texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;

    AddressU = Clamp;
    AddressV = Clamp;
};

float FarClip;

texture Depth;

sampler DepthSampler = sampler_state
{
    Texture = (Depth);

    MinFilter = Point;
    MagFilter = Point;
    MipFilter = Point;

    AddressU = Clamp;
    AddressV = Clamp;
};

// Vertex shader input structure describes the start position and
// velocity of the particle, and the time at which it was created,
// along with some random values that affect its size and rotation.
struct VertexShaderInput
{
    float3 Position : SV_POSITION;
    float2 Corner : NORMAL0;
    float3 Velocity : NORMAL1;
    float4 Random : COLOR0;
    float Time : TEXCOORD0;
};

// Vertex shader output structure specifies the position and color of the particle.
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinate : COLOR1;
    
    float4 PositionCS : TEXCOORD0;
};

// Vertex shader helper for computing the position of a particle.
float4 ComputeParticlePosition(float3 position, float3 velocity,
	float age, float normalizedAge)
{
    float startVelocity = length(velocity);

	// Work out how fast the particle should be moving at the end of its life,
	// by applying a constant scaling factor to its starting velocity.
    float endVelocity = startVelocity * EndVelocity;

	// Our particles have constant acceleration, so given a starting velocity
	// S and ending velocity E, at time T their velocity should be S + (E-S)*T.
	// The particle position is the sum of this velocity over the range 0 to T.
	// To compute the position directly, we must integrate the velocity
	// equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.

    float velocityIntegral = startVelocity * normalizedAge +
		(endVelocity - startVelocity) * normalizedAge *
		normalizedAge / 2;

    position += normalize(velocity) * velocityIntegral * Duration;

	// Apply the gravitational force.
    position += Gravity * age * normalizedAge;

    // return the worldspace position
    return float4(position, 1);
}

// Vertex shader helper for computing the size of a particle.
float ComputeParticleSize(float randomValue, float normalizedAge)
{
	// Apply a random factor to make each particle a slightly different size.
    float startSize = lerp(StartSize.x, StartSize.y, randomValue);
    float endSize = lerp(EndSize.x, EndSize.y, randomValue);

	// Compute the actual size based on the age of the particle.
    float size = lerp(startSize, endSize, normalizedAge);

	// Project the size into screen coordinates.
    return size * Projection._m11;
}


// Vertex shader helper for computing the color of a particle.
float4 ComputeParticleColor(float4 projectedPosition,
	float randomValue, float normalizedAge)
{
	// Apply a random factor to make each particle a slightly different color.
    float4 color = lerp(MinColor, MaxColor, randomValue);

		// Fade the alpha based on the age of the particle. This curve is hard coded
		// to make the particle fade in fairly quickly, then fade out more slowly:
		// plot x*(1-x)*(1-x) for x=0:1 in a graphing program if you want to see what
		// this looks like. The 6.7 scaling factor normalizes the curve so the alpha
		// will reach all the way up to fully solid.

    color.a *= normalizedAge * (1 - normalizedAge) * (1 - normalizedAge) * 6.7;

    return color;
}


// Vertex shader helper for computing the rotation of a particle.
float2x2 ComputeParticleRotation(float randomValue, float age)
{
	// Apply a random factor to make each particle rotate at a different speed.
    float rotateSpeed = lerp(RotateSpeed.x, RotateSpeed.y, randomValue);

    float rotation = rotateSpeed * age;

	// Compute a 2x2 rotation matrix.
    float c = cos(rotation);
    float s = sin(rotation);

    return float2x2(c, -s, s, c);
}

// Custom vertex shader animates particles entirely on the GPU.
VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Compute the age of the particle.
    float age = CurrentTime - input.Time;

	// Apply a random factor to make different particles age at different rates.
    age *= 1 + input.Random.x * DurationRandomness;

	// Normalize the age into the range zero to one.
    float normalizedAge = saturate(age / Duration);

	// Compute the particle position, size, color, and rotation.
    float4 PositionWS = ComputeParticlePosition(input.Position, input.Velocity,
		age, normalizedAge);

    // Apply the camera view and projection transforms.
    float4 PositionVS = mul(PositionWS, View);
    output.Position = mul(PositionVS, Projection);

    float size = ComputeParticleSize(input.Random.y, normalizedAge);
    float2x2 rotation = ComputeParticleRotation(input.Random.w, age);

    output.Position.xy += mul(input.Corner, rotation) * size * ViewportScale;

    output.Color = ComputeParticleColor(output.Position, input.Random.z, normalizedAge);
    output.TextureCoordinate = (input.Corner + 1) / 2;

    output.PositionCS = output.Position;

    return output;
}

float CalculateZFade(float2 uv, float particleDepth)
{
    float zFade = 1;
    float depthVal = tex2D(DepthSampler, uv);

    if (depthVal != 0)
    {
        depthVal *= FarClip;

        float input = (depthVal - particleDepth) / FADE_DISTANCE;
        if ((input < 1) && (input > 0))
        {
            zFade = 0.5 * pow(saturate(2 * ((input > 0.5) ? (1 - input) : input)), CONTRAST_POWER);
            zFade = (input > 0.5) ? (1 - zFade) : zFade;
        }
        else
        {
            zFade = saturate(input);
        }
    }
    return zFade;
}

float4 FadeParticleAlpha(float4 color, float depth, float2 uv)
{
    float zFade = CalculateZFade(uv, depth);
    return float4(color.r * zFade, color.g * zFade, color.b * zFade, color.a * zFade);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    //obtain screen position
    input.PositionCS.xy /= input.PositionCS.w;
    //obtain textureCoordinates corresponding to the current pixel
    //the screen coordinates are in [-1,1]*[1,-1]
    //the texture coordinates need to be in [0,1]*[0,1]
    float2 texCoord = 0.5f * (float2(input.PositionCS.x, -input.PositionCS.y) + 1);
    //float depthVal = tex2D(DepthSampler, texCoord);

    //float3 viewRay = input.PositionVS.xyz * (FarClip / -input.PositionVS.z);
    //float3 positionVS = viewRay * depthVal;
    
    //float depth = input.PositionCS.z;
    //clip(depth - depthVal);

    return FadeParticleAlpha(tex2D(Sampler, input.TextureCoordinate) * input.Color, input.PositionCS.z, texCoord);
}

technique Particles
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};