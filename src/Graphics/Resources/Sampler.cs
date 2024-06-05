using System;
using SDL2_gpuCS;

namespace MoonWorks.Graphics;

/// <summary>
/// A sampler specifies how a texture will be sampled in a shader.
/// </summary>
public class Sampler : SDL_GpuResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL_Gpu.SDL_GpuReleaseSampler;

	public Sampler(
		GraphicsDevice device,
		in SamplerCreateInfo samplerCreateInfo
	) : base(device)
	{
		Handle = SDL_Gpu.SDL_GpuCreateSampler(
			device.Handle,
			samplerCreateInfo.ToSDL()
		);
	}
}
