using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;
namespace MoonWorks.Graphics;

/// <summary>
/// Render passes are begun in command buffers and are used to apply render state and issue draw calls.
/// Render passes are pooled and should not be referenced after calling EndRenderPass.
/// </summary>
public class RenderPass
{
	public nint Handle { get; private set; }

#if DEBUG
	internal uint colorAttachmentCount;
	internal SampleCount colorAttachmentSampleCount;
	internal TextureFormat colorFormatOne;
	internal TextureFormat colorFormatTwo;
	internal TextureFormat colorFormatThree;
	internal TextureFormat colorFormatFour;
	internal bool hasDepthStencilAttachment;
	internal SampleCount depthStencilAttachmentSampleCount;
	internal TextureFormat depthStencilFormat;

	GraphicsPipeline currentGraphicsPipeline;
#endif

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
#if DEBUG
		AssertRenderPassPipelineFormatMatch(graphicsPipeline);

		if (colorAttachmentCount > 0)
		{
			if (graphicsPipeline.SampleCount != colorAttachmentSampleCount)
			{
				throw new System.InvalidOperationException($"Sample count mismatch! Graphics pipeline sample count: {graphicsPipeline.SampleCount}, Color attachment sample count: {colorAttachmentSampleCount}");
			}
		}

		if (hasDepthStencilAttachment)
		{
			if (graphicsPipeline.SampleCount != depthStencilAttachmentSampleCount)
			{
				throw new System.InvalidOperationException($"Sample count mismatch! Graphics pipeline sample count: {graphicsPipeline.SampleCount}, Depth stencil attachment sample count: {depthStencilAttachmentSampleCount}");
			}
		}
#endif

		SDL.SDL_BindGPUGraphicsPipeline(
			Handle,
			graphicsPipeline.Handle
		);

#if DEBUG
		currentGraphicsPipeline = graphicsPipeline;
#endif
	}

	/// <summary>
	/// Sets the viewport.
	/// </summary>
	public void SetViewport(in Viewport viewport)
	{
		SDL.SDL_SetGPUViewport(
			Handle,
			viewport
		);
	}

	/// <summary>
	/// Sets the scissor area.
	/// </summary>
	public void SetScissor(in Rect scissor)
	{
#if DEBUG
		if (scissor.X < 0 || scissor.Y < 0 || scissor.W <= 0 || scissor.H <= 0)
		{
			throw new System.ArgumentOutOfRangeException("Scissor position cannot be negative and dimensions must be positive!");
		}
#endif

		SDL.SDL_SetGPUScissor(
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
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		SDL.SDL_BindGPUVertexBuffers(
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
	)
	{
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		SDL.SDL_BindGPUIndexBuffer(
			Handle,
			bufferBinding,
			(SDL.SDL_GPUIndexElementSize) indexElementSize
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
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertTextureSamplerBindingNonNull(textureSamplerBinding);
		AssertTextureHasSamplerFlag(textureSamplerBinding.Texture);
#endif

		SDL.SDL_BindGPUVertexSamplers(
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
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertTextureNonNull(texture);
		AssertTextureHasGraphicsStorageReadFlag(texture);
#endif

		SDL.SDL_BindGPUVertexStorageTextures(
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
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertBufferNonNull(buffer);
		AssertBufferHasGraphicsStorageReadFlag(buffer);
#endif

		SDL.SDL_BindGPUVertexStorageBuffers(
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
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertTextureSamplerBindingNonNull(textureSamplerBinding);
		AssertTextureHasSamplerFlag(textureSamplerBinding.Texture);
#endif

		SDL.SDL_BindGPUFragmentSamplers(
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
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertTextureNonNull(texture);
		AssertTextureHasGraphicsStorageReadFlag(texture);
#endif

		SDL.SDL_BindGPUFragmentStorageTextures(
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
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertBufferNonNull(buffer);
		AssertBufferHasGraphicsStorageReadFlag(buffer);
#endif

		SDL.SDL_BindGPUFragmentStorageBuffers(
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
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		SDL.SDL_DrawGPUIndexedPrimitives(
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
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		SDL.SDL_DrawGPUPrimitives(
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
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		SDL.SDL_DrawGPUPrimitivesIndirect(
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
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		SDL.SDL_DrawGPUIndexedPrimitivesIndirect(
			Handle,
			buffer.Handle,
			offsetInBytes,
			drawCount
		);
	}

#if DEBUG
	private void AssertRenderPassPipelineFormatMatch(GraphicsPipeline graphicsPipeline)
	{
		for (var i = 0; i < graphicsPipeline.AttachmentInfo.ColorAttachmentDescriptions.Length; i += 1)
		{
			TextureFormat format;
			if (i == 0)
			{
				format = colorFormatOne;
			}
			else if (i == 1)
			{
				format = colorFormatTwo;
			}
			else if (i == 2)
			{
				format = colorFormatThree;
			}
			else
			{
				format = colorFormatFour;
			}

			var pipelineFormat = graphicsPipeline.AttachmentInfo.ColorAttachmentDescriptions[i].Format;
			if (pipelineFormat != format)
			{
				throw new System.InvalidOperationException($"Color texture format mismatch! Pipeline expects {pipelineFormat}, render pass attachment is {format}");
			}
		}

		if (graphicsPipeline.AttachmentInfo.HasDepthStencilAttachment)
		{
			var pipelineDepthFormat = graphicsPipeline.AttachmentInfo.DepthStencilFormat;

			if (!hasDepthStencilAttachment)
			{
				throw new System.InvalidOperationException("Pipeline expects depth attachment!");
			}

			if (pipelineDepthFormat != depthStencilFormat)
			{
				throw new System.InvalidOperationException($"Depth texture format mismatch! Pipeline expects {pipelineDepthFormat}, render pass attachment is {depthStencilFormat}");
			}
		}
	}

	private void AssertGraphicsPipelineBound(string message = "No graphics pipeline is bound!")
	{
		if (currentGraphicsPipeline == null)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertTextureNonNull(in Texture texture)
	{
		if (texture == null)
		{
			throw new NullReferenceException("Texture must not be null!");
		}
	}

	private void AssertTextureSamplerBindingNonNull(in TextureSamplerBinding binding)
	{
		if (binding.Texture == null || binding.Texture.Handle == IntPtr.Zero)
		{
			throw new NullReferenceException("Texture binding must not be null!");
		}

		if (binding.Sampler == null || binding.Sampler.Handle == IntPtr.Zero)
		{
			throw new NullReferenceException("Sampler binding must not be null!");
		}
	}

	private void AssertTextureHasSamplerFlag(Texture texture)
	{
		if ((texture.UsageFlags & TextureUsageFlags.Sampler) == 0)
		{
			throw new System.ArgumentException("The bound Texture's UsageFlags must include TextureUsageFlags.Sampler!");
		}
	}

	private void AssertTextureHasGraphicsStorageReadFlag(Texture texture)
	{
		if ((texture.UsageFlags & TextureUsageFlags.GraphicsStorageRead) == 0)
		{
			throw new System.ArgumentException("The bound Texture's UsageFlags must include TextureUsageFlags.GraphicsStorage!");
		}
	}

	private void AssertBufferNonNull(Buffer buffer)
	{
		if (buffer == null || buffer.Handle == IntPtr.Zero)
		{
			throw new System.NullReferenceException("Buffer must not be null!");
		}
	}

	private void AssertBufferHasGraphicsStorageReadFlag(Buffer buffer)
	{
		if ((buffer.UsageFlags & BufferUsageFlags.GraphicsStorageRead) == 0)
		{
			throw new System.ArgumentException("The bound Buffer's UsageFlags must include BufferUsageFlag.GraphicsStorage!");
		}
	}
#endif
}
