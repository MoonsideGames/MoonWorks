using System;
using System.Runtime.InteropServices;
using RefreshCS;

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

		var texturePtr = Refresh.Refresh_AcquireSwapchainTexture(
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[1];
		refreshColorAttachmentInfos[0] = colorAttachmentInfo.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			1,
			(Refresh.DepthStencilAttachmentInfo*) nint.Zero
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[2];
		refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
		refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			2,
			(Refresh.DepthStencilAttachmentInfo*) nint.Zero
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[3];
		refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
		refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
		refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			3,
			(Refresh.DepthStencilAttachmentInfo*) nint.Zero
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[4];
		refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
		refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
		refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();
		refreshColorAttachmentInfos[3] = colorAttachmentInfoFour.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			4,
			(Refresh.DepthStencilAttachmentInfo*) nint.Zero
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

		var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			(Refresh.ColorAttachmentInfo*) nint.Zero,
			0,
			&refreshDepthStencilAttachmentInfo
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[1];
		refreshColorAttachmentInfos[0] = colorAttachmentInfo.ToRefresh();

		var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			1,
			&refreshDepthStencilAttachmentInfo
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[2];
		refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
		refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();

		var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			2,
			&refreshDepthStencilAttachmentInfo
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[3];
		refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
		refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
		refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();

		var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			3,
			&refreshDepthStencilAttachmentInfo
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

		var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[4];
		refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
		refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
		refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();
		refreshColorAttachmentInfos[3] = colorAttachmentInfoFour.ToRefresh();

		var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

		var renderPassHandle = Refresh.Refresh_BeginRenderPass(
			Handle,
			refreshColorAttachmentInfos,
			4,
			&refreshDepthStencilAttachmentInfo
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
#endif

		Refresh.Refresh_EndRenderPass(
			renderPass.Handle
		);

		renderPass.SetHandle(nint.Zero);
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
		Refresh.Refresh_Blit(
			Handle,
			source.ToRefresh(),
			destination.ToRefresh(),
			(Refresh.Filter) filter,
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

		var refreshTextureBinding = readWriteTextureBinding.ToRefresh();

		var computePassHandle = Refresh.Refresh_BeginComputePass(
			Handle,
			&refreshTextureBinding,
			1,
			(Refresh.StorageBufferReadWriteBinding*) nint.Size,
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

		var refreshBufferBinding = readWriteBufferBinding.ToRefresh();

		var computePassHandle = Refresh.Refresh_BeginComputePass(
			Handle,
			(Refresh.StorageTextureReadWriteBinding*) nint.Zero,
			0,
			&refreshBufferBinding,
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

		var refreshTextureBinding = readWriteTextureBinding.ToRefresh();
		var refreshBufferBinding = readWriteBufferBinding.ToRefresh();

		var computePassHandle = Refresh.Refresh_BeginComputePass(
			Handle,
			&refreshTextureBinding,
			1,
			&refreshBufferBinding,
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
		Span<StorageTextureReadWriteBinding> readWriteTextureBindings,
		Span<StorageBufferReadWriteBinding> readWriteBufferBindings
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin compute pass while in another pass!");
		computePassActive = true;
#endif

		var refreshTextureBindings = NativeMemory.Alloc(
			(nuint) (readWriteTextureBindings.Length * Marshal.SizeOf<StorageTextureReadWriteBinding>())
		);

		var refreshBufferBindings = NativeMemory.Alloc(
			(nuint) (readWriteBufferBindings.Length * Marshal.SizeOf<StorageBufferReadWriteBinding>())
		);

		var computePassHandle = Refresh.Refresh_BeginComputePass(
			Handle,
			(Refresh.StorageTextureReadWriteBinding*) refreshTextureBindings,
			(uint) readWriteTextureBindings.Length,
			(Refresh.StorageBufferReadWriteBinding*) refreshBufferBindings,
			(uint) readWriteBufferBindings.Length
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

#if DEBUG
		computePass.active = true;
#endif

		NativeMemory.Free(refreshTextureBindings);
		NativeMemory.Free(refreshBufferBindings);

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

		Refresh.Refresh_EndComputePass(
			computePass.Handle
		);

		computePass.SetHandle(nint.Zero);
		Device.ComputePassPool.Return(computePass);
	}

	// Copy Pass

	/// <summary>
	/// Begins a copy pass.
	/// All copy commands must be made within a copy pass.
	/// It is an error to call this during any kind of pass.
	/// </summary>
	public CopyPass BeginCopyPass()
	{
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin copy pass while in another pass!");
		copyPassActive = true;
#endif

		var copyPassHandle = Refresh.Refresh_BeginCopyPass(Handle);

		var copyPass = Device.CopyPassPool.Obtain();
		copyPass.SetHandle(copyPassHandle);

		return copyPass;
	}

	public void EndCopyPass(CopyPass copyPass)
	{
#if DEBUG
		AssertNotSubmitted();
		AssertInCopyPass("Cannot end copy pass while not in a copy pass!");
		copyPassActive = false;
#endif

		Refresh.Refresh_EndCopyPass(
			copyPass.Handle
		);

		copyPass.SetHandle(nint.Zero);
		Device.CopyPassPool.Return(copyPass);
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

		if ((depthStencilAttachmentInfo.TextureSlice.Texture.UsageFlags & TextureUsageFlags.DepthStencil) == 0)
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
