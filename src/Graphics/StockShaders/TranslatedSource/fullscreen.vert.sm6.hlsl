static float4 gl_Position;
static int gl_VertexIndex;
static float2 outTexCoord;

struct SPIRV_Cross_Input
{
    uint gl_VertexIndex : SV_VertexID;
};

struct SPIRV_Cross_Output
{
    float2 outTexCoord : TEXCOORD0;
    float4 gl_Position : SV_Position;
};

void vert_main()
{
    outTexCoord = float2(float((gl_VertexIndex << 1) & 2), float(gl_VertexIndex & 2));
    gl_Position = float4((outTexCoord * float2(2.0f, -2.0f)) + float2(-1.0f, 1.0f), 0.0f, 1.0f);
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    gl_VertexIndex = int(stage_input.gl_VertexIndex);
    vert_main();
    SPIRV_Cross_Output stage_output;
    stage_output.gl_Position = gl_Position;
    stage_output.outTexCoord = outTexCoord;
    return stage_output;
}
