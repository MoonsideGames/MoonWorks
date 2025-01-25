struct Input
{
	float2 TexCoord : TEXCOORD0;
	float4 Color : TEXCOORD1;
	float DistanceRange : TEXCOORD2;
	nointerpolation uint FontIndex : TEXCOORD3;
};

struct Output
{
	float4 Color : SV_Target0;
};

Texture2D<float4> msdf[4] : register(t0, space2);
SamplerState msdfSampler[4] : register(s0, space2);

float median(float x, float y, float z)
{
	return max(min(x, y), min(max(x, y), z));
}

float screenPxRange(Texture2D<float4> msdf, float pxRange, float2 texcoord)
{
	float2 textureSize;
	msdf.GetDimensions(textureSize.x, textureSize.y);
	float2 unitRange = (pxRange/textureSize).xx;
	float2 screenTexSize = (1.0f/fwidth(texcoord)).xx;
	return max(0.5 * dot(unitRange, screenTexSize), 1.0);
}

Output main(Input input)
{
	Output output;

	float3 msd = msdf[input.FontIndex].Sample(msdfSampler[input.FontIndex], input.TexCoord).xyz;
	float sd = median(msd.x, msd.y, msd.z);
	float screenPxDistance = screenPxRange(msdf[input.FontIndex], input.DistanceRange, input.TexCoord) * (sd - 0.5);
	float opacity = clamp(screenPxDistance + 0.5, 0, 1);

	output.Color = lerp(0.0f.xxxx, input.Color, opacity);
	return output;
}
