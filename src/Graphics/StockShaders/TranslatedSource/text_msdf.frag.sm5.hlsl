cbuffer UBO : register(b0)
{
    float ubo_pxRange : packoffset(c0);
};

Texture2D<float4> msdf : register(t0);
SamplerState _msdf_sampler : register(s0);

static float2 inTexCoord;
static float4 outColor;
static float4 inColor;

struct SPIRV_Cross_Input
{
    float2 inTexCoord : TEXCOORD0;
    float4 inColor : TEXCOORD1;
};

struct SPIRV_Cross_Output
{
    float4 outColor : SV_Target0;
};

uint2 spvTextureSize(Texture2D<float4> Tex, uint Level, out uint Param)
{
    uint2 ret;
    Tex.GetDimensions(Level, ret.x, ret.y, Param);
    return ret;
}

float median(float r, float g, float b)
{
    return max(min(r, g), min(max(r, g), b));
}

float screenPxRange()
{
    uint _47_dummy_parameter;
    float2 unitRange = ubo_pxRange.xx / float2(int2(spvTextureSize(msdf, uint(0), _47_dummy_parameter)));
    float2 screenTexSize = 1.0f.xx / fwidth(inTexCoord);
    return max(0.5f * dot(unitRange, screenTexSize), 1.0f);
}

void frag_main()
{
    float3 msd = msdf.Sample(_msdf_sampler, inTexCoord).xyz;
    float param = msd.x;
    float param_1 = msd.y;
    float param_2 = msd.z;
    float sd = median(param, param_1, param_2);
    float screenPxDistance = screenPxRange() * (sd - 0.5f);
    float opacity = clamp(screenPxDistance + 0.5f, 0.0f, 1.0f);
    outColor = lerp(0.0f.xxxx, inColor, opacity.xxxx);
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    inTexCoord = stage_input.inTexCoord;
    inColor = stage_input.inColor;
    frag_main();
    SPIRV_Cross_Output stage_output;
    stage_output.outColor = outColor;
    return stage_output;
}
