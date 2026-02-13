struct Input
{
	float2 Position : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	uint ChunkIndex : TEXCOORD2;
};

struct Output
{
	float2 TexCoord : TEXCOORD0;
	float4 Color : TEXCOORD1;
	float DistanceRange : TEXCOORD2;
	nointerpolation uint FontIndex : TEXCOORD3;
	float4 Position : SV_Position;
};

struct ChunkData
{
	float4x4 MatrixTransform;
	float4 Color;
	float DistanceRange;
	uint FontIndex;
	float2 Padding;
};

StructuredBuffer<ChunkData> ChunkDataBuffer : register(t0, space0);

cbuffer UniformBlock : register(b0, space1)
{
	float4x4 ViewProjectionMatrix : packoffset(c0);
};

Output main(Input input)
{
	Output output;
	float4x4 transform = mul(ViewProjectionMatrix, ChunkDataBuffer[input.ChunkIndex].MatrixTransform);
	output.Position = mul(transform, float4(input.Position, 0.0, 1.0));
	output.DistanceRange = ChunkDataBuffer[input.ChunkIndex].DistanceRange;
	output.Color = ChunkDataBuffer[input.ChunkIndex].Color;
	output.FontIndex = ChunkDataBuffer[input.ChunkIndex].FontIndex;
	output.TexCoord = input.TexCoord;
	return output;
}
