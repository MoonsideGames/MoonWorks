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
	public void SetViewport(in Viewport viewport)
	{
		SDL_GPU.SDL_SetGPUViewport(
			Handle,
			viewport
		);
	}

	/// <summary>
	/// Sets the scissor area.
	/// </summary>
	public void SetScissor(in Rect scissor)
	{
		SDL_GPU.SDL_SetGPUScissor(
			Handle,
			scissor
		);
	}

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="bufferBinding">Buffer to bind and associated offset.</param>
	/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
	public unsafe void BindVertexBuffer(
		in BufferBinding bufferBinding,
		uint firstBinding = 0
	) {
		SDL_GPU.SDL_BindGPUVertexBuffers(
			Handle,
			firstBinding,
			[bufferBinding],
			1
		);
	}

	/// <summary>
	/// Binds an index buffer to be used by subsequent draw calls.
	/// </summary>
	/// <param name="indexBuffer">The index buffer to bind.</param>
	/// <param name="indexElementSize">The size in bytes of the index buffer elements.</param>
	/// <param name="offset">The offset index for the buffer.</param>
	public unsafe void BindIndexBuffer(
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
	/// Binds samplers to be used by the vertex shader.
	/// </summary>
	/// <param name="textureSamplerBindings">The texture-sampler to bind.</param>
	public unsafe void BindVertexSampler(
		in TextureSamplerBinding textureSamplerBinding,
		uint slot = 0
	) {
		SDL_GPU.SDL_BindGPUVertexSamplers(
			Handle,
			slot,
			[textureSamplerBinding],
			1
		);
	}

	public unsafe void BindVertexStorageTexture(
		Texture texture,
		uint slot = 0
	) {
		SDL_GPU.SDL_BindGPUVertexStorageTextures(
			Handle,
			slot,
			[texture.Handle],
			1
		);
	}

	public unsafe void BindVertexStorageBuffer(
		Buffer buffer,
		uint slot = 0
	) {
		SDL_GPU.SDL_BindGPUVertexStorageBuffers(
			Handle,
			slot,
			[buffer.Handle],
			1
		);
	}

	public unsafe void BindFragmentSampler(
		in TextureSamplerBinding textureSamplerBinding,
		uint slot = 0
	) {
		SDL_GPU.SDL_BindGPUFragmentSamplers(
			Handle,
			slot,
			[textureSamplerBinding],
			1
		);
	}

	public unsafe void BindFragmentStorageTexture(
		in Texture texture,
		uint slot = 0
	) {
		SDL_GPU.SDL_BindGPUFragmentStorageTextures(
			Handle,
			slot,
			[texture.Handle],
			1
		);
	}

	public unsafe void BindFragmentStorageBuffer(
		Buffer buffer,
		uint slot = 0
	) {
		SDL_GPU.SDL_BindGPUFragmentStorageBuffers(
			Handle,
			slot,
			[buffer.Handle],
			1
		);
	}

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
