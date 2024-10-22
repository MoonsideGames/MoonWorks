cbuffer UBO : register(b0)
{
    row_major float4x4 ubo_ViewProjection : packoffset(c0);
};


static float4 gl_Position;
static float3 inPos;
static float2 outTexCoord;
static float2 inTexCoord;
static float4 outColor;
static float4 inColor;

struct SPIRV_Cross_Input
{
    float3 inPos : TEXCOORD0;
    float2 inTexCoord : TEXCOORD1;
    float4 inColor : TEXCOORD2;
};

struct SPIRV_Cross_Output
{
    float2 outTexCoord : TEXCOORD0;
    float4 outColor : TEXCOORD1;
    float4 gl_Position : SV_Position;
};

void vert_main()
{
    gl_Position = mul(float4(inPos, 1.0f), ubo_ViewProjection);
    outTexCoord = inTexCoord;
    outColor = inColor;
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    inPos = stage_input.inPos;
    inTexCoord = stage_input.inTexCoord;
    inColor = stage_input.inColor;
    vert_main();
    SPIRV_Cross_Output stage_output;
    stage_output.gl_Position = gl_Position;
    stage_output.outTexCoord = outTexCoord;
    stage_output.outColor = outColor;
    return stage_output;
}
