using System;
using System.Runtime.InteropServices;
using RefreshCS;
namespace MoonWorks.Graphics;

/// <summary>
/// Render passes are begun in command buffers and are used to apply render state and issue draw calls.
/// Render passes are pooled and should not be referenced after calling EndRenderPass.
/// </summary>
public class RenderPass
{
	public nint Handle { get; private set; }

#if DEBUG
	internal bool active;
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
		AssertRenderPassActive();
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

		Refresh.Refresh_BindGraphicsPipeline(
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
#if DEBUG
		AssertRenderPassActive();
#endif

		Refresh.Refresh_SetViewport(
			Handle,
			viewport.ToRefresh()
		);
	}

	/// <summary>
	/// Sets the scissor area.
	/// </summary>
	public void SetScissor(in Rect scissor)
	{
#if DEBUG
		AssertRenderPassActive();

		if (scissor.X < 0 || scissor.Y < 0 || scissor.W <= 0 || scissor.H <= 0)
		{
			throw new System.ArgumentOutOfRangeException("Scissor position cannot be negative and dimensions must be positive!");
		}
#endif

		Refresh.Refresh_SetScissor(
			Handle,
			scissor.ToRefresh()
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

		var refreshBufferBinding = bufferBinding.ToRefresh();

		Refresh.Refresh_BindVertexBuffers(
			Handle,
			firstBinding,
			&refreshBufferBinding,
			1
		);
	}

	/// <summary>
	/// Binds an index buffer to be used by subsequent draw calls.
	/// </summary>
	/// <param name="indexBuffer">The index buffer to bind.</param>
	/// <param name="indexElementSize">The size in bytes of the index buffer elements.</param>
	/// <param name="offset">The offset index for the buffer.</param>
	public void BindIndexBuffer(
		BufferBinding bufferBinding,
		IndexElementSize indexElementSize
	)
	{
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		Refresh.Refresh_BindIndexBuffer(
			Handle,
			bufferBinding.ToRefresh(),
			(Refresh.IndexElementSize) indexElementSize
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

		var refreshTextureSamplerBinding = textureSamplerBinding.ToRefresh();

		Refresh.Refresh_BindVertexSamplers(
			Handle,
			slot,
			&refreshTextureSamplerBinding,
			1
		);
	}

	public unsafe void BindVertexStorageTexture(
		in TextureSlice textureSlice,
		uint slot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertTextureNonNull(textureSlice.Texture);
		AssertTextureHasGraphicsStorageFlag(textureSlice.Texture);
#endif

		var refreshTextureSlice = textureSlice.ToRefresh();

		Refresh.Refresh_BindVertexStorageTextures(
			Handle,
			slot,
			&refreshTextureSlice,
			1
		);
	}

	public unsafe void BindVertexStorageBuffer(
		GpuBuffer buffer,
		uint slot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertBufferNonNull(buffer);
		AssertBufferHasGraphicsStorageFlag(buffer);
#endif

		var bufferHandle = buffer.Handle;

		Refresh.Refresh_BindVertexStorageBuffers(
			Handle,
			slot,
			&bufferHandle,
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

		var refreshTextureSamplerBinding = textureSamplerBinding.ToRefresh();

		Refresh.Refresh_BindFragmentSamplers(
			Handle,
			slot,
			&refreshTextureSamplerBinding,
			1
		);
	}

	public unsafe void BindFragmentStorageTexture(
		in TextureSlice textureSlice,
		uint slot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertTextureNonNull(textureSlice.Texture);
		AssertTextureHasGraphicsStorageFlag(textureSlice.Texture);
#endif

		var refreshTextureSlice = textureSlice.ToRefresh();

		Refresh.Refresh_BindFragmentStorageTextures(
			Handle,
			slot,
			&refreshTextureSlice,
			1
		);
	}

	public unsafe void BindFragmentStorageBuffer(
		GpuBuffer buffer,
		uint slot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertBufferNonNull(buffer);
		AssertBufferHasGraphicsStorageFlag(buffer);
#endif

		var bufferHandle = buffer.Handle;

		Refresh.Refresh_BindFragmentStorageBuffers(
			Handle,
			slot,
			&bufferHandle,
			1
		);
	}

	public unsafe void PushVertexUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();

		if (slot >= currentGraphicsPipeline.VertexShaderResourceInfo.UniformBufferCount)
		{
			throw new System.ArgumentException($"Slot {slot} given, but {currentGraphicsPipeline.VertexShaderResourceInfo.UniformBufferCount} uniform buffers are used on the shader!");
		}
#endif

		Refresh.Refresh_PushVertexUniformData(
			Handle,
			slot,
			(nint) uniformsPtr,
			size
		);
	}

	public unsafe void PushVertexUniformData<T>(
		in T uniforms,
		uint slot = 0
	) where T : unmanaged
	{
		fixed (T* uniformsPtr = &uniforms)
		{
			PushVertexUniformData(uniformsPtr, (uint) Marshal.SizeOf<T>(), slot);
		}
	}

	public unsafe void PushFragmentUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();

		if (slot >= currentGraphicsPipeline.FragmentShaderResourceInfo.UniformBufferCount)
		{
			throw new System.ArgumentException($"Slot {slot} given, but {currentGraphicsPipeline.FragmentShaderResourceInfo.UniformBufferCount} uniform buffers are used on the shader!");
		}
#endif

		Refresh.Refresh_PushFragmentUniformData(
			Handle,
			slot,
			(nint) uniformsPtr,
			size
		);
	}

	public unsafe void PushFragmentUniformData<T>(
		in T uniforms,
		uint slot = 0
	) where T : unmanaged
	{
		fixed (T* uniformsPtr = &uniforms)
		{
			PushFragmentUniformData(uniformsPtr, (uint) Marshal.SizeOf<T>(), slot);
		}
	}

	/// <summary>
	/// Draws using a vertex buffer and an index buffer, and an optional instance count.
	/// </summary>
	/// <param name="baseVertex">The starting index offset for the vertex buffer.</param>
	/// <param name="startIndex">The starting index offset for the index buffer.</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	/// <param name="instanceCount">The number of instances to draw.</param>
	public void DrawIndexedPrimitives(
		uint baseVertex,
		uint startIndex,
		uint primitiveCount,
		uint instanceCount = 1
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		Refresh.Refresh_DrawIndexedPrimitives(
			Handle,
			baseVertex,
			startIndex,
			primitiveCount,
			instanceCount
		);
	}

	/// <summary>
	/// Draws using a vertex buffer and an index buffer.
	/// </summary>
	/// <param name="baseVertex">The starting index offset for the vertex buffer.</param>
	/// <param name="startIndex">The starting index offset for the index buffer.</param>
	/// <param name="primitiveCount">The number of primitives to draw.</param>
	public void DrawPrimitives(
		uint vertexStart,
		uint primitiveCount
	)
	{
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		Refresh.Refresh_DrawPrimitives(
			Handle,
			vertexStart,
			primitiveCount
		);
	}

	/// <summary>
	/// Similar to DrawPrimitives, but parameters are set from a buffer.
	/// The buffer must have the Indirect usage flag set.
	/// </summary>
	/// <param name="buffer">The draw parameters buffer.</param>
	/// <param name="offsetInBytes">The offset to start reading from the draw parameters buffer.</param>
	/// <param name="drawCount">The number of draw parameter sets that should be read from the buffer.</param>
	/// <param name="stride">The byte stride between sets of draw parameters.</param>
	public void DrawPrimitivesIndirect(
		GpuBuffer buffer,
		uint offsetInBytes,
		uint drawCount,
		uint stride
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		Refresh.Refresh_DrawPrimitivesIndirect(
			Handle,
			buffer.Handle,
			offsetInBytes,
			drawCount,
			stride
		);
	}

	/// <summary>
	/// Similar to DrawIndexedPrimitives, but parameters are set from a buffer.
	/// The buffer must have the Indirect usage flag set.
	/// </summary>
	/// <param name="buffer">The draw parameters buffer.</param>
	/// <param name="offsetInBytes">The offset to start reading from the draw parameters buffer.</param>
	/// <param name="drawCount">The number of draw parameter sets that should be read from the buffer.</param>
	/// <param name="stride">The byte stride between sets of draw parameters.</param>
	public void DrawIndexedPrimitivesIndirect(
		GpuBuffer buffer,
		uint offsetInBytes,
		uint drawCount,
		uint stride
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		Refresh.Refresh_DrawIndexedPrimitivesIndirect(
			Handle,
			buffer.Handle,
			offsetInBytes,
			drawCount,
			stride
		);
	}

#if DEBUG
	private void AssertRenderPassActive(string message = "Render pass is not active!")
	{
		if (!active)
		{
			throw new System.InvalidOperationException(message);
		}
	}

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

	private void AssertTextureNonNull(in TextureSlice textureSlice)
	{
		if (textureSlice.Texture == null)
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

	private void AssertTextureHasGraphicsStorageFlag(Texture texture)
	{
		if ((texture.UsageFlags & TextureUsageFlags.GraphicsStorage) == 0)
		{
			throw new System.ArgumentException("The bound Texture's UsageFlags must include TextureUsageFlags.GraphicsStorage!");
		}
	}

	private void AssertBufferNonNull(GpuBuffer buffer)
	{
		if (buffer == null || buffer.Handle == IntPtr.Zero)
		{
			throw new System.NullReferenceException("Buffer must not be null!");
		}
	}

	private void AssertBufferHasGraphicsStorageFlag(GpuBuffer buffer)
	{
		if ((buffer.UsageFlags & BufferUsageFlags.GraphicsStorage) == 0)
		{
			throw new System.ArgumentException("The bound Buffer's UsageFlags must include BufferUsageFlag.GraphicsStorage!");
		}
	}
#endif
}
