using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

public class ComputePass
{
	public nint Handle { get; private set; }

	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}

	/// <summary>
	/// Binds a compute pipeline so that compute work may be dispatched.
	/// </summary>
	/// <param name="computePipeline">The compute pipeline to bind.</param>
	public void BindComputePipeline(
		ComputePipeline computePipeline
	) {
		SDL.SDL_BindGPUComputePipeline(
			Handle,
			computePipeline.Handle
		);
	}

	public unsafe void BindSampler(
		TextureSamplerBinding textureSamplerBinding,
		uint slot = 0
	) {
		SDL.SDL_BindGPUComputeSamplers(
			Handle,
			slot,
			[textureSamplerBinding],
			1
		);
	}

	/// <summary>
	/// Binds a texture to be used in the compute shader.
	/// This texture must have been created with the ComputeShaderRead usage flag.
	/// </summary>
	public unsafe void BindStorageTexture(
		Texture texture,
		uint slot = 0
	) {
		SDL.SDL_BindGPUComputeStorageTextures(
			Handle,
			slot,
			[texture.Handle],
			1
		);
	}

	/// <summary>
	/// Binds a buffer to be used in the compute shader.
	/// This buffer must have been created with the ComputeShaderRead usage flag.
	/// </summary>
	public unsafe void BindStorageBuffer(
		Buffer buffer,
		uint slot = 0
	) {
		SDL.SDL_BindGPUComputeStorageBuffers(
			Handle,
			slot,
			[buffer.Handle],
			1
		);
	}

	/// <summary>
	/// Dispatches compute work.
	/// </summary>
	public void Dispatch(
		uint groupCountX,
		uint groupCountY,
		uint groupCountZ
	) {
		SDL.SDL_DispatchGPUCompute(
			Handle,
			groupCountX,
			groupCountY,
			groupCountZ
		);
	}

	/// <summary>
	/// Indirect dispatch.
	/// </summary>
	public void DispatchIndirect(
		Buffer buffer,
		uint offset = 0
	) {
		SDL.SDL_DispatchGPUComputeIndirect(
			Handle,
			buffer.Handle,
			offset
		);
	}
}
