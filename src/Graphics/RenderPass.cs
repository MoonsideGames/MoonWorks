using System;

namespace MoonWorks.Graphics;

/// <summary>
/// Render passes are begun in command buffers and are used to apply render state and issue draw calls.
/// Render passes are pooled and should not be referenced after calling EndRenderPass.
/// </summary>
public class RenderPass
{
	public nint Handle { get; private set; }
	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}

	/// <summary>
	/// Binds a graphics pipeline so that rendering work may be performed.
	/// </summary>
	/// <param name="graphicsPipeline">The graphics pipeline to bind.</param>
	public void BindGraphicsPipeline(
		GraphicsPipeline graphicsPipeline
	) {
		SDL_GPU.SDL_BindGPUGraphicsPipeline(
			Handle,
			graphicsPipeline.Handle
		);
	}

	/// <summary>
	/// Sets the viewport.
	/// </summary>
	public void SetViewport(in Viewport viewport) => SDL_GPU.SDL_SetGPUViewport(Handle, viewport);

	/// <summary>
	/// Sets the scissor area.
	/// </summary>
	public void SetScissor(in Rect scissor) => SDL_GPU.SDL_SetGPUScissor(Handle, scissor);

	/// <summary>
	/// Sets the stencil reference.
	/// </summary>
	public void SetStencilReference(byte stencilRef) => SDL_GPU.SDL_SetGPUStencilReference(Handle, stencilRef);

	/// <summary>
	/// Sets the blend constants.
	/// </summary>
	/// <param name="blendConstants"></param>
	public void SetBlendConstants(Color blendConstants) => SDL_GPU.SDL_SetGPUBlendConstants(Handle, blendConstants);

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="slot">The index of the first vertex input binding whose state is updated by the command.</param>
	/// <param name="bufferBindings">Buffers to bind with associated offsets.</param>
	public void BindVertexBuffers(
		uint slot,
		params Span<BufferBinding> bufferBindings
	) {
		SDL_GPU.SDL_BindGPUVertexBuffers(
			Handle,
			slot,
			bufferBindings,
			(uint) bufferBindings.Length
		);
	}

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="slot">The index of the first vertex input binding whose state is updated by the command.</param>
	/// <param name="bufferBindings">Buffers to bind with associated offsets.</param>
	public void BindVertexBuffers(
		uint slot,
		params Span<Buffer> buffers
	) {
		Span<BufferBinding> bufferBindings = stackalloc BufferBinding[buffers.Length];

		for (var i = 0; i < bufferBindings.Length; i += 1) {
			bufferBindings[i].Buffer = buffers[i].Handle;
			bufferBindings[i].Offset = 0;
		}

		BindVertexBuffers(0, bufferBindings);
	}

	/// <summary>
	/// Binds vertex buffers starting at slot 0.
	/// </summary>
	/// <param name="bufferBindings">Buffers to bind with associated offsets.</param>
	public void BindVertexBuffers(params Span<BufferBinding> bufferBindings) => BindVertexBuffers(0, bufferBindings);

	/// <summary>
	/// Binds vertex buffers starting at slot 0.
	/// </summary>
	/// <param name="bufferBindings">Buffers to bind with associated offsets.</param>
	public void BindVertexBuffers(params Span<Buffer> buffers) => BindVertexBuffers(0, buffers);

	/// <summary>
	/// Binds an index buffer to be used by subsequent draw calls.
	/// </summary>
	/// <param name="indexBuffer">The index buffer to bind.</param>
	/// <param name="indexElementSize">The size in bytes of the index buffer elements.</param>
	/// <param name="offset">The offset index for the buffer.</param>
	public void BindIndexBuffer(
		BufferBinding bufferBinding,
		IndexElementSize indexElementSize
	) {
		SDL_GPU.SDL_BindGPUIndexBuffer(
			Handle,
			bufferBinding,
			indexElementSize
		);
	}

	/// <summary>
	/// Binds texture-sampler pairs to the vertex shader.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
	public void BindVertexSamplers(
		uint slot,
		params Span<TextureSamplerBinding> textureSamplerBindings
	) {
		SDL_GPU.SDL_BindGPUVertexSamplers(
			Handle,
			slot,
			textureSamplerBindings,
			(uint) textureSamplerBindings.Length
		);
	}

	/// <summary>
	/// Binds texture-sampler pairs to the vertex shader starting at slot 0.
	/// </summary>
	/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
	public void BindVertexSamplers(params Span<TextureSamplerBinding> textureSamplerBindings) => BindVertexSamplers(0, textureSamplerBindings);

	/// <summary>
	/// Binds storage textures to the vertex shader.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="textures">The textures to bind.</param>
	public void BindVertexStorageTextures(
		uint slot,
		params Span<Texture> textures
	) {
		Span<IntPtr> handlePtr = stackalloc nint[textures.Length];

		for (var i = 0; i < textures.Length; i += 1) {
			handlePtr[i] = textures[i].Handle;
		}

		SDL_GPU.SDL_BindGPUVertexStorageTextures(
			Handle,
			slot,
			handlePtr,
			(uint) textures.Length
		);
	}

	/// <summary>
	/// Binds storage textures to the vertex shader starting at slot 0.
	/// </summary>
	/// <param name="textures">The textures to bind.</param>
	public void BindVertexStorageTextures(params Span<Texture> textures) => BindVertexStorageTextures(0, textures);

	/// <summary>
	/// Binds storage buffers to the vertex shader.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="buffers">The buffers to bind.</param>
	public void BindVertexStorageBuffers(
		uint slot,
		params Span<Buffer> buffers
	) {
		Span<IntPtr> handlePtr = stackalloc nint[buffers.Length];

		for (var i = 0; i < buffers.Length; i += 1) {
			handlePtr[i] = buffers[i].Handle;
		}

		SDL_GPU.SDL_BindGPUVertexStorageBuffers(
			Handle,
			slot,
			handlePtr,
			(uint) buffers.Length
		);
	}

	/// <summary>
	/// Binds storage buffers to the vertex shader starting at slot 0.
	/// </summary>
	/// <param name="buffers">The buffers to bind.</param>
	public void BindVertexStorageBuffers(params Span<Buffer> buffers) => BindVertexStorageBuffers(0, buffers);

	/// <summary>
	/// Binds texture-sampler pairs to the fragment shader.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
	public void BindFragmentSamplers(
		uint slot,
		params Span<TextureSamplerBinding> textureSamplerBindings
	) {
		SDL_GPU.SDL_BindGPUFragmentSamplers(
			Handle,
			slot,
			textureSamplerBindings,
			(uint) textureSamplerBindings.Length
		);
	}

	/// <summary>
	/// Binds texture-sampler pairs to the fragment shader starting at slot 0.
	/// </summary>
	/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
	public void BindFragmentSamplers(params Span<TextureSamplerBinding> textureSamplerBindings) => BindFragmentSamplers(0, textureSamplerBindings);

	/// <summary>
	/// Binds storage textures to the fragment shader.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="textures">The textures to bind.</param>
	public void BindFragmentStorageTextures(
		uint slot,
		params Span<Texture> textures
	) {
		Span<IntPtr> handlePtr = stackalloc nint[textures.Length];

		for (var i = 0; i < textures.Length; i += 1) {
			handlePtr[i] = textures[i].Handle;
		}

		SDL_GPU.SDL_BindGPUFragmentStorageTextures(
			Handle,
			slot,
			handlePtr,
			(uint) textures.Length
		);
	}

	/// <summary>
	/// Binds storage textures to the fragment shader starting at slot 0.
	/// </summary>
	/// <param name="textures">The textures to bind.</param>
	public void BindFragmentStorageTextures(params Span<Texture> textures) => BindFragmentStorageTextures(0, textures);

	/// <summary>
	/// Binds storage buffers to the fragment shader.
	/// </summary>
	/// <param name="slot">The first slot whose state is updated by the command.</param>
	/// <param name="buffers">The buffers to bind.</param>
	public void BindFragmentStorageBuffers(
		uint slot,
		params Span<Buffer> buffers
	) {
		Span<IntPtr> handlePtr = stackalloc nint[buffers.Length];

		for (var i = 0; i < buffers.Length; i += 1) {
			handlePtr[i] = buffers[i].Handle;
		}

		SDL_GPU.SDL_BindGPUFragmentStorageBuffers(
			Handle,
			slot,
			handlePtr,
			(uint) buffers.Length
		);
	}

	/// <summary>
	/// Binds storage buffers to the fragment shader starting at slot 0.
	/// </summary>
	/// <param name="buffers">The buffers to bind.</param>
	public void BindFragmentStorageBuffers(params Span<Buffer> buffers) => BindFragmentStorageBuffers(buffers);

	/// <summary>
	/// Draws using a vertex buffer and an index buffer.
	/// </summary>
	public void DrawIndexedPrimitives(
		uint indexCount,
		uint instanceCount,
		uint firstIndex,
		int vertexOffset,
		uint firstInstance
	) {
		SDL_GPU.SDL_DrawGPUIndexedPrimitives(
			Handle,
			indexCount,
			instanceCount,
			firstIndex,
			vertexOffset,
			firstInstance
		);
	}

	/// <summary>
	/// Draws using a vertex buffer.
	/// </summary>
	public void DrawPrimitives(
		uint vertexCount,
		uint instanceCount,
		uint firstVertex,
		uint firstInstance
	)
	{
		SDL_GPU.SDL_DrawGPUPrimitives(
			Handle,
			vertexCount,
			instanceCount,
			firstVertex,
			firstInstance
		);
	}

	/// <summary>
	/// Similar to DrawPrimitives, but parameters are set from a buffer.
	/// The buffer must have the Indirect usage flag set.
	/// </summary>
	/// <param name="buffer">The draw parameters buffer.</param>
	/// <param name="offsetInBytes">The offset to start reading from the draw parameters buffer.</param>
	/// <param name="drawCount">The number of draw parameter sets that should be read from the buffer.</param>
	public void DrawPrimitivesIndirect(
		Buffer buffer,
		uint offsetInBytes,
		uint drawCount
	) {
		SDL_GPU.SDL_DrawGPUPrimitivesIndirect(
			Handle,
			buffer.Handle,
			offsetInBytes,
			drawCount
		);
	}

	/// <summary>
	/// Similar to DrawIndexedPrimitives, but parameters are set from a buffer.
	/// The buffer must have the Indirect usage flag set.
	/// </summary>
	/// <param name="buffer">The draw parameters buffer.</param>
	/// <param name="offsetInBytes">The offset to start reading from the draw parameters buffer.</param>
	/// <param name="drawCount">The number of draw parameter sets that should be read from the buffer.</param>
	public void DrawIndexedPrimitivesIndirect(
		Buffer buffer,
		uint offsetInBytes,
		uint drawCount
	) {
		SDL_GPU.SDL_DrawGPUIndexedPrimitivesIndirect(
			Handle,
			buffer.Handle,
			offsetInBytes,
			drawCount
		);
	}
}
