using System;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

public class ComputePass
{
	public nint Handle { get; internal set; }
	public CommandBuffer CommandBuffer { get; internal set;}

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

	/// <summary>
	/// Binds samplers to the compute shader.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
	public void BindSamplers(
		uint slot,
		params Span<TextureSamplerBinding> textureSamplerBindings
	) {
		SDL.SDL_BindGPUComputeSamplers(
			Handle,
			slot,
			textureSamplerBindings,
			(uint) textureSamplerBindings.Length
		);
	}

	/// <summary>
	/// Binds samplers to the compute shader starting at slot 0.
	/// </summary>
	/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
	public void BindSamplers(params Span<TextureSamplerBinding> textureSamplerBindings) => BindSamplers(0, textureSamplerBindings);

	/// <summary>
	/// Binds readonly storage textures to the compute shader.
	/// This texture must have been created with the ComputeShaderRead usage flag.
	/// </summary>
	public void BindStorageTextures(
		uint slot,
		Span<Texture> textures
	) {
		Span<IntPtr> handlePtr = stackalloc nint[textures.Length];

		for (var i = 0; i < textures.Length; i += 1) {
			handlePtr[i] = textures[i].Handle;
		}

		SDL.SDL_BindGPUComputeStorageTextures(
			Handle,
			slot,
			handlePtr,
			(uint) textures.Length
		);
	}

	public void BindStorageTextures(params Span<Texture> textures) => BindStorageTextures(0, textures);

	/// <summary>
	/// Binds a readonly storage buffer to the compute shader.
	/// This buffer must have been created with the ComputeShaderRead usage flag.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="buffers">The buffers to bind.</param>
	public void BindStorageBuffers(
		uint slot,
		params Span<Buffer> buffers
	) {
		Span<IntPtr> handlePtr = stackalloc nint[buffers.Length];

		for (var i = 0; i < buffers.Length; i += 1) {
			handlePtr[i] = buffers[i].Handle;
		}

		SDL.SDL_BindGPUComputeStorageBuffers(
			Handle,
			slot,
			handlePtr,
			(uint) buffers.Length
		);
	}

	/// <summary>
	/// Binds a readonly storage buffer to the compute shader.
	/// This buffer must have been created with the ComputeShaderRead usage flag.
	/// </summary>
	/// <param name="buffers">The buffers to bind.</param>
	public void BindStorageBuffers(params Span<Buffer> buffers) => BindStorageBuffers(0, buffers);

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
