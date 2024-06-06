/*
 * This effect is based on the YUV-to-RGBA GLSL shader found in SDL.
 * Thus, it also released under the zlib license:
 * http://libsdl.org/license.php
 */
#version 450

layout(location = 0) in vec2 TexCoord;

layout(location = 0) out vec4 FragColor;

layout(set = 2, binding = 0) uniform sampler2D YSampler;
layout(set = 2, binding = 1) uniform sampler2D USampler;
layout(set = 2, binding = 2) uniform sampler2D VSampler;

/* More info about colorspace conversion:
 * http://www.equasys.de/colorconversion.html
 * http://www.equasys.de/colorformat.html
 */

const vec3 offset = vec3(-0.0625, -0.5, -0.5);
const vec3 Rcoeff = vec3(1.164,  0.000,  1.793);
const vec3 Gcoeff = vec3(1.164, -0.213, -0.533);
const vec3 Bcoeff = vec3(1.164,  2.112,  0.000);

void main()
{
	vec3 yuv;
	yuv.x = texture(YSampler, TexCoord).r;
	yuv.y = texture(USampler, TexCoord).r;
	yuv.z = texture(VSampler, TexCoord).r;
	yuv += offset;

	FragColor.r = dot(yuv, Rcoeff);
	FragColor.g = dot(yuv, Gcoeff);
	FragColor.b = dot(yuv, Bcoeff);
	FragColor.a = 1.0;
}
