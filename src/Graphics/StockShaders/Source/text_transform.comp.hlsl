struct ComputeData
{
    float3 position;
    float4 rotationQuaternion;
    float2 scale;
};

struct TextVertex
{
	float4 Position;
	float2 TexCoord;
	float4 Color;
};

float4x4 quaternion_to_matrix(float4 quat)
{
    float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

    float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
    float x2 = x + x, y2 = y + y, z2 = z + z;
    float xx = x * x2, xy = x * y2, xz = x * z2;
    float yy = y * y2, yz = y * z2, zz = z * z2;
    float wx = w * x2, wy = w * y2, wz = w * z2;

    m[0][0] = 1.0 - (yy + zz);
    m[0][1] = xy - wz;
    m[0][2] = xz + wy;

    m[1][0] = xy + wz;
    m[1][1] = 1.0 - (xx + zz);
    m[1][2] = yz - wx;

    m[2][0] = xz - wy;
    m[2][1] = yz + wx;
    m[2][2] = 1.0 - (xx + yy);

    m[3][3] = 1.0;

    return m;
}

StructuredBuffer<ComputeData> ComputeBuffer : register(t0, space0);
RWStructuredBuffer<TextVertex> VertexBuffer : register(u0, space1);

[numthreads(64, 1, 1)]
void main(uint3 GlobalInvocationID : SV_DispatchThreadID)
{
    uint n = GlobalInvocationID.x;
    ComputeData currentSpriteData = ComputeBuffer[n];

    float4x4 Scale = float4x4(
        float4(currentSpriteData.scale.x, 0.0f, 0.0f, 0.0f),
        float4(0.0f, currentSpriteData.scale.y, 0.0f, 0.0f),
        float4(0.0f, 0.0f, 1.0f, 0.0f),
        float4(0.0f, 0.0f, 0.0f, 1.0f)
    );

	float4x4 Rotation = quaternion_to_matrix(currentSpriteData.rotationQuaternion);

	float4x4 Translation = float4x4(
        float4(1.0f, 0.0f, 0.0f, 0.0f),
        float4(0.0f, 1.0f, 0.0f, 0.0f),
        float4(0.0f, 0.0f, 1.0f, 0.0f),
        float4(currentSpriteData.position.x, currentSpriteData.position.y, currentSpriteData.position.z, 1.0f)
    );

	float4x4 Model = mul(Scale, mul(Rotation, Translation));

    float4 topLeft = float4(0.0f, 0.0f, 0.0f, 1.0f);
    float4 topRight = float4(1.0f, 0.0f, 0.0f, 1.0f);
    float4 bottomLeft = float4(0.0f, 1.0f, 0.0f, 1.0f);
    float4 bottomRight = float4(1.0f, 1.0f, 0.0f, 1.0f);

    VertexBuffer[n * 4u]    .position = mul(topLeft, Model);
    VertexBuffer[n * 4u + 1].position = mul(topRight, Model);
    VertexBuffer[n * 4u + 2].position = mul(bottomLeft, Model);
    VertexBuffer[n * 4u + 3].position = mul(bottomRight, Model);

    VertexBuffer[n * 4u]    .texcoord = currentSpriteData.uv0;
    VertexBuffer[n * 4u + 1].texcoord = currentSpriteData.uv1;
    VertexBuffer[n * 4u + 2].texcoord = currentSpriteData.uv2;
    VertexBuffer[n * 4u + 3].texcoord = currentSpriteData.uv3;

    VertexBuffer[n * 4u]    .color = currentSpriteData.color;
    VertexBuffer[n * 4u + 1].color = currentSpriteData.color;
    VertexBuffer[n * 4u + 2].color = currentSpriteData.color;
    VertexBuffer[n * 4u + 3].color = currentSpriteData.color;
}
