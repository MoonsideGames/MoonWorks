#version 450

layout (location = 0) in vec2 TexCoord;

layout (location = 0) out vec4 FragColor;

layout (binding = 0, set = 1) uniform sampler2D TexSampler;

void main()
{
	FragColor = texture(TexSampler, TexCoord);
}
