float4x4 World;
float4x4 View;
float4x4 Projection;

float time;
float numTiles;

float CloudCover;
float CloudSharpness;

float4 SunColor;

float3 v3SunDir;
float3 cameraPosition;

texture permTexture;

sampler permSampler = sampler_state
{
	Texture = <permTexture>;
	MinFilter = Linear;
	MipFilter = Linear;
	MagFilter = Linear;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 WorldPos : POSITION1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.WorldPos = worldPosition;
    output.TexCoord = (input.TexCoord * numTiles);

    return output;
}


#define ONE 0.00390625
#define ONEHALF 0.001953125
// The numbers above are 1/256 and 0.5/256, change accordingly
// if you change the code to use another texture size.
//#define ONE 0.0009765625
//#define ONEHALF 0.00048828125

float fade(float t) {
  // return t*t*(3.0-2.0*t);
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}


float noise(float2 P)
{
  float2 Pi = ONE*floor(P)+ONEHALF;
  float2 Pf = frac(P);

  float2 grad00 = tex2D(permSampler, Pi).rg * 4.0 - 1.0;
  float n00 = dot(grad00, Pf);

  float2 grad10 = tex2D(permSampler, Pi + float2(ONE, 0.0)).rg * 4.0 - 1.0;
  float n10 = dot(grad10, Pf - float2(1.0, 0.0));

  float2 grad01 = tex2D(permSampler, Pi + float2(0.0, ONE)).rg * 4.0 - 1.0;
  float n01 = dot(grad01, Pf - float2(0.0, 1.0));

  float2 grad11 = tex2D(permSampler, Pi + float2(ONE, ONE)).rg * 4.0 - 1.0;
  float n11 = dot(grad11, Pf - float2(1.0, 1.0));

  float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade(Pf.x));

  float n_xy = lerp(n_x.x, n_x.y, fade(Pf.y));

  return n_xy;
}

float HeyneyGreenstein(float3 inLightVector, float3 inViewVector, float inG)
{
    float cos_angle = dot(normalize(inLightVector),
                          normalize(inViewVector));
    return ((1.0 - inG * inG) / pow((1.0 + inG * inG - 2.0 * inG * cos_angle), 3.0 / 2.0)) / 4.0 * 3.1415;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float n = noise(input.TexCoord + time);  
	float n2 = noise(input.TexCoord * 2 + time);
	float n3 = noise(input.TexCoord * 4 + time);
	float n4 = noise(input.TexCoord * 8 + time);
	
	float nFinal = n + (n2 / 2) + (n3 / 4) + (n4 / 8);
	
    float c = nFinal - CloudCover;
    if (c < 0) 
		c=0;
 
    float CloudDensity = 1.0 - pow(CloudSharpness,c);

    float3 V = -(cameraPosition - input.WorldPos.xyz);
    float3 L = v3SunDir;
    if (L.y < -0.2)
        L = -L;

    float4 retColor = CloudDensity; // * HeyneyGreenstein(L, V, 0.2);
    retColor *= SunColor;
    
    return retColor;

}

technique Noise
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}
