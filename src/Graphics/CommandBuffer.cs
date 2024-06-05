using System;
using System.Runtime.InteropServices;
using SDL2;
using SDL2_gpuCS;

namespace MoonWorks.Graphics;

/// <summary>
/// Command buffers are used to apply render state and issue draw calls.
/// NOTE: it is not recommended to hold references to command buffers long term.
/// </summary>
public class CommandBuffer
{
	public GraphicsDevice Device { get; }
	public IntPtr Handle { get; internal set; }

#if DEBUG
	bool swapchainTextureAcquired;

	ComputePipeline currentComputePipeline;

	bool renderPassActive;
	bool copyPassActive;
	bool computePassActive;

	internal bool Submitted;
#endif

	// called from CommandBufferPool
	internal CommandBuffer(GraphicsDevice device)
	{
		Device = device;
		Handle = IntPtr.Zero;

#if DEBUG
		ResetStateTracking();
#endif
	}

	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}

#if DEBUG
	internal void ResetStateTracking()
	{
		swapchainTextureAcquired = false;

		currentComputePipeline = null;

		renderPassActive = false;
		copyPassActive = false;
		computePassActive = false;

		Submitted = false;
	}
#endif

	/// <summary>
	/// Acquires a swapchain texture.
	/// This texture will be presented to the given window when the command buffer is submitted.
	/// Can return null if the swapchain is unavailable. The user should ALWAYS handle the case where this occurs.
	/// If null is returned, presentation will not occur.
	/// It is an error to acquire two swapchain textures from the same window in one command buffer.
	/// It is an error to dispose the swapchain texture. If you do this your game WILL crash. DO NOT DO THIS.
	/// </summary>
	public Texture AcquireSwapchainTexture(
		Window window
	) {
#if DEBUG
		AssertNotSubmitted();

		if (!window.Claimed)
		{
			throw new System.InvalidOperationException("Cannot acquire swapchain texture, window has not been claimed!");
		}

		if (swapchainTextureAcquired)
		{
			throw new System.InvalidOperationException("Cannot acquire two swapchain textures on the same command buffer!");
		}
#endif

		var texturePtr = SDL_Gpu.SDL_GpuAcquireSwapchainTexture(
			Handle,
			window.Handle,
			out var width,
			out var height
		);

		if (texturePtr == IntPtr.Zero)
		{
			return null;
		}

		// Override the texture properties to avoid allocating a new texture instance!
		window.SwapchainTexture.Handle = texturePtr;
		window.SwapchainTexture.Width = width;
		window.SwapchainTexture.Height = height;
		window.SwapchainTexture.Format = window.SwapchainFormat;

#if DEBUG
		swapchainTextureAcquired = true;
#endif

		return window.SwapchainTexture;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this during any kind of pass.
	/// </summary>
	/// <param name="colorAttachmentInfo">The color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorAttachmentInfo colorAttachmentInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin a render pass inside another pass!");
		AssertTextureNotNull(colorAttachmentInfo);
		AssertColorTarget(colorAttachmentInfo);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[1];
		sdlColorAttachmentInfos[0] = colorAttachmentInfo.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			1,
			(SDL_Gpu.DepthStencilAttachmentInfo*) nint.Zero
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 1;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfo.TextureSlice.Texture.SampleCount;
		renderPass.colorFormatOne = colorAttachmentInfo.TextureSlice.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorAttachmentInfo colorAttachmentInfoOne,
		in ColorAttachmentInfo colorAttachmentInfoTwo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin a render pass inside another pass!");

		AssertTextureNotNull(colorAttachmentInfoOne);
		AssertColorTarget(colorAttachmentInfoOne);

		AssertTextureNotNull(colorAttachmentInfoTwo);
		AssertColorTarget(colorAttachmentInfoTwo);

		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[2];
		sdlColorAttachmentInfos[0] = colorAttachmentInfoOne.ToSDL();
		sdlColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			2,
			(SDL_Gpu.DepthStencilAttachmentInfo*) nint.Zero
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 2;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
		renderPass.colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
		renderPass.colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorAttachmentInfo colorAttachmentInfoOne,
		in ColorAttachmentInfo colorAttachmentInfoTwo,
		in ColorAttachmentInfo colorAttachmentInfoThree
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin a render pass inside another pass!");

		AssertTextureNotNull(colorAttachmentInfoOne);
		AssertColorTarget(colorAttachmentInfoOne);

		AssertTextureNotNull(colorAttachmentInfoTwo);
		AssertColorTarget(colorAttachmentInfoTwo);

		AssertTextureNotNull(colorAttachmentInfoThree);
		AssertColorTarget(colorAttachmentInfoThree);

		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoThree.TextureSlice.Texture);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[3];
		sdlColorAttachmentInfos[0] = colorAttachmentInfoOne.ToSDL();
		sdlColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToSDL();
		sdlColorAttachmentInfos[2] = colorAttachmentInfoThree.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			3,
			(SDL_Gpu.DepthStencilAttachmentInfo*) nint.Zero
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 3;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
		renderPass.colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
		renderPass.colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
		renderPass.colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoFour">The four color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorAttachmentInfo colorAttachmentInfoOne,
		in ColorAttachmentInfo colorAttachmentInfoTwo,
		in ColorAttachmentInfo colorAttachmentInfoThree,
		in ColorAttachmentInfo colorAttachmentInfoFour
	) {
#if DEBUG
		AssertNotSubmitted();

		AssertTextureNotNull(colorAttachmentInfoOne);
		AssertColorTarget(colorAttachmentInfoOne);

		AssertTextureNotNull(colorAttachmentInfoTwo);
		AssertColorTarget(colorAttachmentInfoTwo);

		AssertTextureNotNull(colorAttachmentInfoThree);
		AssertColorTarget(colorAttachmentInfoThree);

		AssertTextureNotNull(colorAttachmentInfoFour);
		AssertColorTarget(colorAttachmentInfoFour);

		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoThree.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoFour.TextureSlice.Texture);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[4];
		sdlColorAttachmentInfos[0] = colorAttachmentInfoOne.ToSDL();
		sdlColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToSDL();
		sdlColorAttachmentInfos[2] = colorAttachmentInfoThree.ToSDL();
		sdlColorAttachmentInfos[3] = colorAttachmentInfoFour.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			4,
			(SDL_Gpu.DepthStencilAttachmentInfo*) nint.Zero
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 3;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
		renderPass.colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
		renderPass.colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
		renderPass.colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
		renderPass.colorFormatFour = colorAttachmentInfoFour.TextureSlice.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilAttachmentInfo depthStencilAttachmentInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilAttachmentInfo);
#endif

		var sdlDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			(SDL_Gpu.ColorAttachmentInfo*) nint.Zero,
			0,
			&sdlDepthStencilAttachmentInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.TextureSlice.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfo">The color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
		in ColorAttachmentInfo colorAttachmentInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilAttachmentInfo);

		AssertTextureNotNull(colorAttachmentInfo);
		AssertColorTarget(colorAttachmentInfo);
		AssertSameSampleCount(colorAttachmentInfo.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[1];
		sdlColorAttachmentInfos[0] = colorAttachmentInfo.ToSDL();

		var sdlDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			1,
			&sdlDepthStencilAttachmentInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 1;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfo.TextureSlice.Texture.SampleCount;
		renderPass.colorFormatOne = colorAttachmentInfo.TextureSlice.Texture.Format;
		renderPass.depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.TextureSlice.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
		in ColorAttachmentInfo colorAttachmentInfoOne,
		in ColorAttachmentInfo colorAttachmentInfoTwo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilAttachmentInfo);

		AssertTextureNotNull(colorAttachmentInfoOne);
		AssertColorTarget(colorAttachmentInfoOne);

		AssertTextureNotNull(colorAttachmentInfoTwo);
		AssertColorTarget(colorAttachmentInfoTwo);

		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[2];
		sdlColorAttachmentInfos[0] = colorAttachmentInfoOne.ToSDL();
		sdlColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToSDL();

		var sdlDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			2,
			&sdlDepthStencilAttachmentInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 2;
		renderPass.colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
		renderPass.colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
		renderPass.depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.TextureSlice.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
		in ColorAttachmentInfo colorAttachmentInfoOne,
		in ColorAttachmentInfo colorAttachmentInfoTwo,
		in ColorAttachmentInfo colorAttachmentInfoThree
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilAttachmentInfo);

		AssertTextureNotNull(colorAttachmentInfoOne);
		AssertColorTarget(colorAttachmentInfoOne);

		AssertTextureNotNull(colorAttachmentInfoTwo);
		AssertColorTarget(colorAttachmentInfoTwo);

		AssertTextureNotNull(colorAttachmentInfoThree);
		AssertColorTarget(colorAttachmentInfoThree);

		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoThree.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[3];
		sdlColorAttachmentInfos[0] = colorAttachmentInfoOne.ToSDL();
		sdlColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToSDL();
		sdlColorAttachmentInfos[2] = colorAttachmentInfoThree.ToSDL();

		var sdlDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			3,
			&sdlDepthStencilAttachmentInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 3;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
		renderPass.colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
		renderPass.colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
		renderPass.colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
		renderPass.depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.TextureSlice.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
	/// <param name="colorAttachmentInfoFour">The four color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
		in ColorAttachmentInfo colorAttachmentInfoOne,
		in ColorAttachmentInfo colorAttachmentInfoTwo,
		in ColorAttachmentInfo colorAttachmentInfoThree,
		in ColorAttachmentInfo colorAttachmentInfoFour
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilAttachmentInfo);

		AssertTextureNotNull(colorAttachmentInfoOne);
		AssertColorTarget(colorAttachmentInfoOne);

		AssertTextureNotNull(colorAttachmentInfoTwo);
		AssertColorTarget(colorAttachmentInfoTwo);

		AssertTextureNotNull(colorAttachmentInfoThree);
		AssertColorTarget(colorAttachmentInfoThree);

		AssertTextureNotNull(colorAttachmentInfoFour);
		AssertColorTarget(colorAttachmentInfoFour);

		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoThree.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoFour.TextureSlice.Texture);
		AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

		var sdlColorAttachmentInfos = stackalloc SDL_Gpu.ColorAttachmentInfo[4];
		sdlColorAttachmentInfos[0] = colorAttachmentInfoOne.ToSDL();
		sdlColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToSDL();
		sdlColorAttachmentInfos[2] = colorAttachmentInfoThree.ToSDL();
		sdlColorAttachmentInfos[3] = colorAttachmentInfoFour.ToSDL();

		var sdlDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToSDL();

		var renderPassHandle = SDL_Gpu.SDL_GpuBeginRenderPass(
			Handle,
			sdlColorAttachmentInfos,
			4,
			&sdlDepthStencilAttachmentInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 4;
		renderPass.colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
		renderPass.colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
		renderPass.colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
		renderPass.colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
		renderPass.colorFormatFour = colorAttachmentInfoFour.TextureSlice.Texture.Format;
		renderPass.depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.TextureSlice.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Ends the current render pass.
	/// This must be called before beginning another render pass or submitting the command buffer.
	/// </summary>
	public void EndRenderPass(RenderPass renderPass)
	{
#if DEBUG
		AssertNotSubmitted();
		AssertRenderPassActive();

		renderPassActive = false;
		renderPass.active = false;
#endif

		SDL_Gpu.SDL_GpuEndRenderPass(
			renderPass.Handle
		);

		Device.RenderPassPool.Return(renderPass);
	}

	/// <summary>
	/// Blits a texture to another texture with the specified filter.
	///
	/// This operation cannot be performed inside any pass.
	/// </summary>
	/// <param name="cycle">If true, the destination texture will cycle if bound.</param>
	public void Blit(
		in TextureRegion source,
		in TextureRegion destination,
		Filter filter,
		bool cycle
	) {
		SDL_Gpu.SDL_GpuBlit(
			Handle,
			source.ToSDL(),
			destination.ToSDL(),
			(SDL_Gpu.Filter) filter,
			Conversions.BoolToInt(cycle)
		);
	}

	public unsafe ComputePass BeginComputePass(
		in StorageTextureReadWriteBinding readWriteTextureBinding
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin compute pass while in another pass!");
		computePassActive = true;
#endif

		var sdlTextureBinding = readWriteTextureBinding.ToSDL();

		var computePassHandle = SDL_Gpu.SDL_GpuBeginComputePass(
			Handle,
			&sdlTextureBinding,
			1,
			(SDL_Gpu.StorageBufferReadWriteBinding*) nint.Size,
			0
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

#if DEBUG
		computePass.active = true;
#endif

		return computePass;
	}

	public unsafe ComputePass BeginComputePass(
		in StorageBufferReadWriteBinding readWriteBufferBinding
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin compute pass while in another pass!");
		computePassActive = true;
#endif

		var sdlBufferBinding = readWriteBufferBinding.ToSDL();

		var computePassHandle = SDL_Gpu.SDL_GpuBeginComputePass(
			Handle,
			(SDL_Gpu.StorageTextureReadWriteBinding*) nint.Zero,
			0,
			&sdlBufferBinding,
			1
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

#if DEBUG
		computePass.active = true;
#endif

		return computePass;
	}

	public unsafe ComputePass BeginComputePass(
		in StorageTextureReadWriteBinding readWriteTextureBinding,
		in StorageBufferReadWriteBinding readWriteBufferBinding
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin compute pass while in another pass!");
		computePassActive = true;
#endif

		var sdlTextureBinding = readWriteTextureBinding.ToSDL();
		var sdlBufferBinding = readWriteBufferBinding.ToSDL();

		var computePassHandle = SDL_Gpu.SDL_GpuBeginComputePass(
			Handle,
			&sdlTextureBinding,
			1,
			&sdlBufferBinding,
			1
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

#if DEBUG
		computePass.active = true;
#endif

		return computePass;
	}

	public void EndComputePass(ComputePass computePass)
	{
#if DEBUG
		AssertNotSubmitted();
		AssertInComputePass("Cannot end compute pass while not in a compute pass!");
		computePassActive = false;
		computePass.active = false;
#endif

		SDL_Gpu.SDL_GpuEndComputePass(
			computePass.Handle
		);
	}

	// Copy Pass

	/// <summary>
	/// Begins a copy pass.
	/// All copy commands must be made within a copy pass.
	/// It is an error to call this during any kind of pass.
	/// </summary>
	public void BeginCopyPass()
	{
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin copy pass while in another pass!");
		copyPassActive = true;
#endif

		Refresh.Refresh_BeginCopyPass(
			Device.Handle,
			Handle
		);
	}


	/// <summary>
	/// Uploads data from a TransferBuffer to a TextureSlice.
	/// This copy occurs on the GPU timeline.
	///
	/// Overwriting the contents of the TransferBuffer before the command buffer
	/// has finished execution will cause undefined behavior.
	///
	/// You MAY assume that the copy has finished for subsequent commands.
	/// </summary>
	/// <param name="writeOption">Specifies data dependency behavior.</param>
	public void UploadToTexture(
		TransferBuffer transferBuffer,
		in TextureRegion textureRegion,
		in BufferImageCopy copyParams,
		WriteOptions writeOption
	)
	{
#if DEBUG
		AssertNotSubmitted();
		AssertInCopyPass("Cannot upload to texture outside of copy pass!");
		AssertBufferBoundsCheck(transferBuffer.Size, copyParams.BufferOffset, textureRegion.Size);
#endif

		Refresh.Refresh_UploadToTexture(
			Device.Handle,
			Handle,
			transferBuffer.Handle,
			textureRegion.ToRefreshTextureRegion(),
			copyParams.ToRefresh(),
			(Refresh.WriteOptions) writeOption
		);
	}

	/// <summary>
	/// Uploads the contents of an entire buffer to a texture with no mips.
	/// </summary>
	public void UploadToTexture(
		TransferBuffer transferBuffer,
		Texture texture,
		WriteOptions writeOption
	) {
		UploadToTexture(
			transferBuffer,
			new TextureRegion(texture),
			new BufferImageCopy(0, 0, 0),
			writeOption
		);
	}

	/// <summary>
	/// Uploads data from a TransferBuffer to a GpuBuffer.
	/// This copy occurs on the GPU timeline.
	///
	/// Overwriting the contents of the TransferBuffer before the command buffer
	/// has finished execution will cause undefined behavior.
	///
	/// You MAY assume that the copy has finished for subsequent commands.
	/// </summary>
	public void UploadToBuffer(
		TransferBuffer transferBuffer,
		GpuBuffer gpuBuffer,
		in BufferCopy copyParams,
		WriteOptions option
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertInCopyPass("Cannot upload to texture outside of copy pass!");
		AssertBufferBoundsCheck(transferBuffer.Size, copyParams.SrcOffset, copyParams.Size);
		AssertBufferBoundsCheck(gpuBuffer.Size, copyParams.DstOffset, copyParams.Size);
#endif

		Refresh.Refresh_UploadToBuffer(
			Device.Handle,
			Handle,
			transferBuffer.Handle,
			gpuBuffer.Handle,
			copyParams.ToRefresh(),
			(Refresh.WriteOptions) option
		);
	}

	/// <summary>
	/// Copies the entire contents of a TransferBuffer to a GpuBuffer.
	/// </summary>
	public void UploadToBuffer(
		TransferBuffer transferBuffer,
		GpuBuffer gpuBuffer,
		WriteOptions option
	) {
		UploadToBuffer(
			transferBuffer,
			gpuBuffer,
			new BufferCopy(0, 0, transferBuffer.Size),
			option
		);
	}

	/// <summary>
	/// Copies data element-wise into from a TransferBuffer to a GpuBuffer.
	/// </summary>
	public void UploadToBuffer<T>(
		TransferBuffer transferBuffer,
		GpuBuffer gpuBuffer,
		uint sourceStartElement,
		uint destinationStartElement,
		uint numElements,
		WriteOptions option
	) where T : unmanaged
	{
		var elementSize = Marshal.SizeOf<T>();
		var dataLengthInBytes = (uint) (elementSize * numElements);
		var srcOffsetInBytes = (uint) (elementSize * sourceStartElement);
		var dstOffsetInBytes = (uint) (elementSize * destinationStartElement);

		UploadToBuffer(
			transferBuffer,
			gpuBuffer,
			new BufferCopy(
				srcOffsetInBytes,
				dstOffsetInBytes,
				dataLengthInBytes
			),
			option
		);
	}

	/// <summary>
	/// Copies the contents of a TextureSlice to another TextureSlice.
	/// The slices must have the same dimensions.
	/// This copy occurs on the GPU timeline.
	///
	/// You MAY assume that the copy has finished in subsequent commands.
	/// </summary>
	public void CopyTextureToTexture(
		in TextureRegion source,
		in TextureRegion destination,
		WriteOptions option
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertInCopyPass("Cannot download from texture outside of copy pass!");
		AssertTextureBoundsCheck(destination.Size, source.Size);
#endif

		Refresh.Refresh_CopyTextureToTexture(
			Device.Handle,
			Handle,
			source.ToRefreshTextureRegion(),
			destination.ToRefreshTextureRegion(),
			(Refresh.WriteOptions) option
		);
	}

	/// <summary>
	/// Copies the contents of an entire Texture with no mips to another Texture with no mips.
	/// The textures must have the same dimensions.
	/// </summary>
	public void CopyTextureToTexture(
		Texture source,
		Texture destination,
		WriteOptions option
	) {
		CopyTextureToTexture(
			new TextureRegion(source),
			new TextureRegion(destination),
			option
		);
	}

	/// <summary>
	/// Copies data from a GpuBuffer to another GpuBuffer.
	/// This copy occurs on the GPU timeline.
	///
	/// You MAY assume that the copy has finished in subsequent commands.
	/// </summary>
	public void CopyBufferToBuffer(
		GpuBuffer source,
		GpuBuffer destination,
		in BufferCopy copyParams,
		WriteOptions option
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertInCopyPass("Cannot download from texture outside of copy pass!");
		AssertBufferBoundsCheck(source.Size, copyParams.SrcOffset, copyParams.Size);
		AssertBufferBoundsCheck(destination.Size, copyParams.DstOffset, copyParams.Size);
#endif

		Refresh.Refresh_CopyBufferToBuffer(
			Device.Handle,
			Handle,
			source.Handle,
			destination.Handle,
			copyParams.ToRefresh(),
			(Refresh.WriteOptions) option
		);
	}

	/// <summary>
	/// Copies the entire contents of a GpuBuffer to another GpuBuffer.
	/// </summary>
	public void CopyBufferToBuffer(
		GpuBuffer source,
		GpuBuffer destination,
		WriteOptions option
	) {
		CopyBufferToBuffer(
			source,
			destination,
			new BufferCopy(0, 0, source.Size),
			option
		);
	}

	public void EndCopyPass()
	{
#if DEBUG
		AssertNotSubmitted();
		AssertInCopyPass("Cannot end copy pass while not in a copy pass!");
		copyPassActive = false;
#endif

		Refresh.Refresh_EndCopyPass(
			Device.Handle,
			Handle
		);
	}

#if DEBUG
	private void AssertRenderPassActive(string message = "No active render pass!")
	{
		if (!renderPassActive)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertComputePipelineBound(string message = "No compute pipeline is bound!")
	{
		if (currentComputePipeline == null)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertComputeBufferCount(int count)
	{
		if (currentComputePipeline.ComputeShaderInfo.BufferBindingCount != count)
		{
			throw new System.InvalidOperationException($"Compute pipeline expects {currentComputePipeline.ComputeShaderInfo.BufferBindingCount} buffers, but received {count}");
		}
	}

	private void AssertComputeTextureCount(int count)
	{
		if (currentComputePipeline.ComputeShaderInfo.ImageBindingCount != count)
		{
			throw new System.InvalidOperationException($"Compute pipeline expects {currentComputePipeline.ComputeShaderInfo.ImageBindingCount} textures, but received {count}");
		}
	}

	private void AssertTextureNotNull(ColorAttachmentInfo colorAttachmentInfo)
	{
		if (colorAttachmentInfo.TextureSlice.Texture == null || colorAttachmentInfo.TextureSlice.Texture.Handle == IntPtr.Zero)
		{
			throw new System.ArgumentException("Render pass color attachment Texture cannot be null!");
		}
	}

	private void AssertColorTarget(ColorAttachmentInfo colorAttachmentInfo)
	{
		if ((colorAttachmentInfo.TextureSlice.Texture.UsageFlags & TextureUsageFlags.ColorTarget) == 0)
		{
			throw new System.ArgumentException("Render pass color attachment UsageFlags must include TextureUsageFlags.ColorTarget!");
		}
	}

	private void AssertSameSampleCount(Texture a, Texture b)
	{
		if (a.SampleCount != b.SampleCount)
		{
			throw new System.ArgumentException("All attachments in a render pass must have the same SampleCount!");
		}
	}

	private void AssertValidDepthAttachment(DepthStencilAttachmentInfo depthStencilAttachmentInfo)
	{
		if (depthStencilAttachmentInfo.TextureSlice.Texture == null ||
			depthStencilAttachmentInfo.TextureSlice.Texture.Handle == IntPtr.Zero)
		{
			throw new System.ArgumentException("Render pass depth stencil attachment Texture cannot be null!");
		}

		if ((depthStencilAttachmentInfo.TextureSlice.Texture.UsageFlags & TextureUsageFlags.DepthStencilTarget) == 0)
		{
			throw new System.ArgumentException("Render pass depth stencil attachment UsageFlags must include TextureUsageFlags.DepthStencilTarget!");
		}
	}

	private void AssertNonEmptyCopy(uint dataLengthInBytes)
	{
		if (dataLengthInBytes == 0)
		{
			throw new System.InvalidOperationException("SetBufferData must have a length greater than 0 bytes!");
		}
	}

	private void AssertBufferBoundsCheck(uint bufferLengthInBytes, uint offsetInBytes, uint copyLengthInBytes)
	{
		if (copyLengthInBytes > bufferLengthInBytes + offsetInBytes)
		{
			throw new System.InvalidOperationException($"SetBufferData overflow! buffer length {bufferLengthInBytes}, offset {offsetInBytes}, copy length {copyLengthInBytes}");
		}
	}

	private void AssertTextureBoundsCheck(uint textureSizeInBytes, uint dataLengthInBytes)
	{
		if (dataLengthInBytes > textureSizeInBytes)
		{
			throw new System.InvalidOperationException($"SetTextureData overflow! texture size {textureSizeInBytes}, data size {dataLengthInBytes}");
		}
	}

	private void AssertNotInPass(string message)
	{
		if (renderPassActive || copyPassActive || computePassActive)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertInCopyPass(string message)
	{
		if (!copyPassActive)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertInComputePass(string message)
	{
		if (!computePassActive)
		{
			throw new System.InvalidOperationException(message);
		}
	}

	private void AssertNotSubmitted()
	{
		if (Submitted)
		{
			throw new System.InvalidOperationException("Cannot add commands to a submitted command buffer!");
		}
	}
#endif
}
