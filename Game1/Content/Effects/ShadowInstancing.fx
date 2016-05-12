//-----------------------------------------------------------------------------
// InstancedModel.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	//float4 Color : COLOR0;
};


struct VertexShaderOutput
{
	float4 Position : POSITION0;
	//float4 Color : COLOR0;
	float2 Depth : TEXCOORD0;
};


// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input, float4x4 instanceTransform)
{
	VertexShaderOutput output;

	// Apply the world and camera matrices to compute the output position.
	float4 worldPosition = mul(input.Position, instanceTransform);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	//output.Color = input.Color;
	output.Depth = output.Position.zw;

	return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input,
	float4x4 instanceTransform : BLENDWEIGHT)
{
	return VertexShaderCommon(input, mul(World, transpose(instanceTransform)));
}


// When instancing is disabled we take the world transform from an effect parameter.
VertexShaderOutput NoInstancingVertexShader(VertexShaderInput input)
{
	return VertexShaderCommon(input, World);
}


// Both techniques share this same pixel shader.
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return float4(input.Depth.x / input.Depth.y, 0, 0, 1);
}


// Hardware instancing technique.
technique HardwareInstancing
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 HardwareInstancingVertexShader();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}


// For rendering without instancing.
technique NoInstancing
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 NoInstancingVertexShader();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}
