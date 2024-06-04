using System;
using System.Runtime.InteropServices;
using SDL2_gpuCS;
namespace MoonWorks.Graphics;

/// <summary>
/// Render passes are begun in command buffers and are used to apply render state and issue draw calls.
/// Render passes are pooled and should not be referenced after calling EndRenderPass.
/// </summary>
public class RenderPass
{
	public nint Handle { get; internal set; }

#if DEBUG
	internal bool active;
	GraphicsPipeline currentGraphicsPipeline;
	uint colorAttachmentCount;
	SampleCount colorAttachmentSampleCount;
	TextureFormat colorFormatOne;
	TextureFormat colorFormatTwo;
	TextureFormat colorFormatThree;
	TextureFormat colorFormatFour;
	bool hasDepthStencilAttachment;
	SampleCount depthStencilAttachmentSampleCount;
	TextureFormat depthStencilFormat;
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

		SDL_Gpu.SDL_GpuBindGraphicsPipeline(
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

		SDL_Gpu.SDL_GpuSetViewport(
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

		SDL_Gpu.SDL_GpuSetScissor(
			Handle,
			scissor.ToRefresh()
		);
	}

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="bufferBinding">Buffer to bind and associated offset.</param>
	/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
	public unsafe void BindVertexBuffers(
		in BufferBinding bufferBinding,
		uint firstBinding = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		var bindingArray = stackalloc SDL_Gpu.BufferBinding[1];
		bindingArray[0] = bufferBinding.ToRefresh();

		SDL_Gpu.SDL_GpuBindVertexBuffers(
			Handle,
			firstBinding,
			bindingArray,
			1
		);
	}

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="bufferBindingOne">Buffer to bind and associated offset.</param>
	/// <param name="bufferBindingTwo">Buffer to bind and associated offset.</param>
	/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
	public unsafe void BindVertexBuffers(
		in BufferBinding bufferBindingOne,
		in BufferBinding bufferBindingTwo,
		uint firstBinding = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		var bindingArray = stackalloc SDL_Gpu.BufferBinding[2];
		bindingArray[0] = bufferBindingOne.ToRefresh();
		bindingArray[1] = bufferBindingTwo.ToRefresh();

		SDL_Gpu.SDL_GpuBindVertexBuffers(
			Handle,
			firstBinding,
			bindingArray,
			2
		);
	}

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="bufferBindingOne">Buffer to bind and associated offset.</param>
	/// <param name="bufferBindingTwo">Buffer to bind and associated offset.</param>
	/// <param name="bufferBindingThree">Buffer to bind and associated offset.</param>
	/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
	public unsafe void BindVertexBuffers(
		in BufferBinding bufferBindingOne,
		in BufferBinding bufferBindingTwo,
		in BufferBinding bufferBindingThree,
		uint firstBinding = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		var bindingArray = stackalloc SDL_Gpu.BufferBinding[3];
		bindingArray[0] = bufferBindingOne.ToRefresh();
		bindingArray[1] = bufferBindingTwo.ToRefresh();
		bindingArray[2] = bufferBindingThree.ToRefresh();

		SDL_Gpu.SDL_GpuBindVertexBuffers(
			Handle,
			firstBinding,
			bindingArray,
			3
		);
	}

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="bufferBindingOne">Buffer to bind and associated offset.</param>
	/// <param name="bufferBindingTwo">Buffer to bind and associated offset.</param>
	/// <param name="bufferBindingThree">Buffer to bind and associated offset.</param>
	/// <param name="bufferBindingFour">Buffer to bind and associated offset.</param>
	/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
	public unsafe void BindVertexBuffers(
		in BufferBinding bufferBindingOne,
		in BufferBinding bufferBindingTwo,
		in BufferBinding bufferBindingThree,
		in BufferBinding bufferBindingFour,
		uint firstBinding = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		var bindingArray = stackalloc SDL_Gpu.BufferBinding[4];
		bindingArray[0] = bufferBindingOne.ToRefresh();
		bindingArray[1] = bufferBindingTwo.ToRefresh();
		bindingArray[2] = bufferBindingThree.ToRefresh();
		bindingArray[3] = bufferBindingFour.ToRefresh();

		SDL_Gpu.SDL_GpuBindVertexBuffers(
			Handle,
			firstBinding,
			bindingArray,
			4
		);
	}

	/// <summary>
	/// Binds vertex buffers to be used by subsequent draw calls.
	/// </summary>
	/// <param name="bufferBindings">Spawn of buffers to bind and their associated offsets.</param>
	/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
	public unsafe void BindVertexBuffers(
		in Span<BufferBinding> bufferBindings,
		uint firstBinding = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
#endif

		SDL_Gpu.BufferBinding* bufferBindingsArray = (SDL_Gpu.BufferBinding*) NativeMemory.Alloc((nuint) (Marshal.SizeOf<SDL_Gpu.BufferBinding>() * bufferBindings.Length));

		for (var i = 0; i < bufferBindings.Length; i += 1)
		{
			bufferBindingsArray[i] = bufferBindings[i].ToRefresh();
		}

		SDL_Gpu.SDL_GpuBindVertexBuffers(
			Handle,
			firstBinding,
			bufferBindingsArray,
			(uint) bufferBindings.Length
		);

		NativeMemory.Free(bufferBindingsArray);
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

		SDL_Gpu.SDL_GpuBindIndexBuffer(
			Handle,
			bufferBinding.ToRefresh(),
			(SDL_Gpu.IndexElementSize) indexElementSize
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

		var bindingArray = stackalloc SDL_Gpu.TextureSamplerBinding[1];
		bindingArray[0] = textureSamplerBinding.ToSDL();

		SDL_Gpu.SDL_GpuBindVertexSamplers(
			Handle,
			slot,
			bindingArray,
			1
		);
	}

	public unsafe void BindVertexSamplers(
		in Span<TextureSamplerBinding> textureSamplerBindings,
		uint firstSlot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();

		for (var i = 0; i < textureSamplerBindings.Length; i += 1)
		{
			AssertTextureSamplerBindingNonNull(textureSamplerBindings[i]);
			AssertTextureHasSamplerFlag(textureSamplerBindings[i].Texture);
		}
#endif

		SDL_Gpu.TextureSamplerBinding* samplerBindingsArray =
			(SDL_Gpu.TextureSamplerBinding*) NativeMemory.Alloc(
				(nuint) (Marshal.SizeOf<SDL_Gpu.TextureSamplerBinding>() * textureSamplerBindings.Length)
			);

		for (var i = 0; i < textureSamplerBindings.Length; i += 1)
		{
			samplerBindingsArray[i] = textureSamplerBindings[i].ToSDL();
		}

		SDL_Gpu.SDL_GpuBindVertexSamplers(
			Handle,
			firstSlot,
			samplerBindingsArray,
			(uint) textureSamplerBindings.Length
		);

		NativeMemory.Free(samplerBindingsArray);
	}

	public unsafe void BindVertexStorageTexture(
		in TextureSlice storageTextureSlice,
		uint slot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();
		AssertTextureNonNull(storageTextureSlice.Texture);
		AssertTextureHasGraphicsStorageFlag(storageTextureSlice.Texture);
#endif

		var sliceArray = stackalloc SDL_Gpu.TextureSlice[1];
		sliceArray[0] = storageTextureSlice.ToSDL();

		SDL_Gpu.SDL_GpuBindVertexStorageTextures(
			Handle,
			slot,
			sliceArray,
			1
		);
	}

	public unsafe void BindVertexStorageTextures(
		in Span<TextureSlice> storageTextureSlices,
		uint firstSlot = 0
	) {
#if DEBUG
		AssertGraphicsPipelineBound();

		for (var i = 0; i < storageTextureSlices.Length; i += 1)
		{
			AssertTextureNonNull(storageTextureSlices[i].Texture);
			AssertTextureHasGraphicsStorageFlag(storageTextureSlices[i].Texture);
		}
#endif

		SDL_Gpu.TextureSlice* sliceArray =
			(SDL_Gpu.TextureSlice*) NativeMemory.Alloc(
				(nuint) (Marshal.SizeOf<SDL_Gpu.TextureSlice>() * storageTextureSlices.Length)
			);

		for (var i = 0; i < storageTextureSlices.Length; i += 1)
		{
			sliceArray[i] = storageTextureSlices[i].ToSDL();
		}

		SDL_Gpu.SDL_GpuBindVertexStorageTextures(
			Handle,
			firstSlot,
			sliceArray,
			(uint) storageTextureSlices.Length
		);

		NativeMemory.Free(sliceArray);
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
#endif
}
