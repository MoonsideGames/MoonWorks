using System;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

/// <summary>
/// Specifies how a texture will be sampled in a shader.
/// </summary>
public class Sampler : SDLGPUResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUSampler;

	public static Sampler Create(
		GraphicsDevice device,
		in SamplerCreateInfo samplerCreateInfo
	) {
		var handle = SDL.SDL_CreateGPUSampler(
			device.Handle,
			samplerCreateInfo
		);
		if (handle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}
		return new Sampler(device)
		{
			Handle = handle
		};
	}

	private Sampler(GraphicsDevice device) : base(device) { }
}
