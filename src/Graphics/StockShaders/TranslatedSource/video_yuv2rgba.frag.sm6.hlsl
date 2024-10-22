Texture2D<float4> YSampler : register(t0, space2);
SamplerState _YSampler_sampler : register(s0, space2);
Texture2D<float4> USampler : register(t1, space2);
SamplerState _USampler_sampler : register(s1, space2);
Texture2D<float4> VSampler : register(t2, space2);
SamplerState _VSampler_sampler : register(s2, space2);

static float2 TexCoord;
static float4 FragColor;

struct SPIRV_Cross_Input
{
    float2 TexCoord : TEXCOORD0;
};

struct SPIRV_Cross_Output
{
    float4 FragColor : SV_Target0;
};

void frag_main()
{
    float3 yuv;
    yuv.x = YSampler.Sample(_YSampler_sampler, TexCoord).x;
    yuv.y = USampler.Sample(_USampler_sampler, TexCoord).x;
    yuv.z = VSampler.Sample(_VSampler_sampler, TexCoord).x;
    yuv += float3(-0.0625f, -0.5f, -0.5f);
    FragColor.x = dot(yuv, float3(1.164000034332275390625f, 0.0f, 1.7929999828338623046875f));
    FragColor.y = dot(yuv, float3(1.164000034332275390625f, -0.212999999523162841796875f, -0.53299999237060546875f));
    FragColor.z = dot(yuv, float3(1.164000034332275390625f, 2.111999988555908203125f, 0.0f));
    FragColor.w = 1.0f;
}

SPIRV_Cross_Output main(SPIRV_Cross_Input stage_input)
{
    TexCoord = stage_input.TexCoord;
    frag_main();
    SPIRV_Cross_Output stage_output;
    stage_output.FragColor = FragColor;
    return stage_output;
}
