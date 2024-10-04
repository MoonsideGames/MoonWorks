using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL3;

namespace MoonWorks.Graphics;

/// <summary>
/// A structure that buffers commands to be submitted to the GPU.
/// These commands include operations like copying data, draw calls, and compute dispatches.
/// Commands only begin executing once the command buffer is submitted.
/// If you need to know when the command buffer is done, use GraphicsDevice.SubmitAndAcquireFence
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

		if (!SDL.SDL_AcquireGPUSwapchainTexture(
			Handle,
			window.Handle,
			out var texturePtr,
			out var width,
			out var height
		)) {
			// FIXME: should we throw?
			Logger.LogError(SDL.SDL_GetError());
			return null;
		}

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
	/// Pushes data to a vertex uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a render or compute pass.
	/// </summary>
	public unsafe void PushVertexUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	) {
		SDL.SDL_PushGPUVertexUniformData(
			Handle,
			slot,
			(nint) uniformsPtr,
			size
		);
	}

	/// <summary>
	/// Pushes data to a vertex uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a render or compute pass.
	/// </summary>
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

	/// <summary>
	/// Pushes data to a fragment uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
	public unsafe void PushFragmentUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	) {
		SDL.SDL_PushGPUFragmentUniformData(
			Handle,
			slot,
			(nint) uniformsPtr,
			size
		);
	}

	/// <summary>
	/// Pushes data to a fragment uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
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
	/// Pushes data to a compute uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
	public unsafe void PushComputeUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	) {
		SDL.SDL_PushGPUComputeUniformData(
			Handle,
			slot,
			(nint) uniformsPtr,
			size
		);
	}

	/// <summary>
	/// Pushes data to a compute uniform slot on the command buffer.
	/// Subsequent draw calls will use this uniform data.
	/// It is legal to push uniforms during a pass.
	/// </summary>
	public unsafe void PushComputeUniformData<T>(
		in T uniforms,
		uint slot = 0
	) where T : unmanaged
	{
		fixed (T* uniformsPtr = &uniforms)
		{
			PushComputeUniformData(uniformsPtr, (uint) Marshal.SizeOf<T>(), slot);
		}
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this during any kind of pass.
	/// </summary>
	/// <param name="colorTargetInfo">The color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin a render pass inside another pass!");
		AssertTextureNotNull(colorTargetInfo);
		AssertColorTarget(colorTargetInfo);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfo],
			1,
			Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>()
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 1;
		renderPass.colorAttachmentSampleCount = colorTargetInfo.Texture.SampleCount;
		renderPass.colorFormatOne = colorTargetInfo.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorTargetInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoTwo">The second color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfoOne,
		in ColorTargetInfo colorTargetInfoTwo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin a render pass inside another pass!");

		AssertTextureNotNull(colorTargetInfoOne);
		AssertColorTarget(colorTargetInfoOne);

		AssertTextureNotNull(colorTargetInfoTwo);
		AssertColorTarget(colorTargetInfoTwo);

		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoTwo.Texture);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo],
			2,
			Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>()
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 2;
		renderPass.colorAttachmentSampleCount = colorTargetInfoOne.Texture.SampleCount;
		renderPass.colorFormatOne = colorTargetInfoOne.Texture.Format;
		renderPass.colorFormatTwo = colorTargetInfoTwo.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorTargetInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoThree">The third color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfoOne,
		in ColorTargetInfo colorTargetInfoTwo,
		in ColorTargetInfo colorTargetInfoThree
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertNotInPass("Cannot begin a render pass inside another pass!");

		AssertTextureNotNull(colorTargetInfoOne);
		AssertColorTarget(colorTargetInfoOne);

		AssertTextureNotNull(colorTargetInfoTwo);
		AssertColorTarget(colorTargetInfoTwo);

		AssertTextureNotNull(colorTargetInfoThree);
		AssertColorTarget(colorTargetInfoThree);

		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoTwo.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoThree.Texture);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree],
			3,
			Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>()
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 3;
		renderPass.colorAttachmentSampleCount = colorTargetInfoOne.Texture.SampleCount;
		renderPass.colorFormatOne = colorTargetInfoOne.Texture.Format;
		renderPass.colorFormatTwo = colorTargetInfoTwo.Texture.Format;
		renderPass.colorFormatThree = colorTargetInfoThree.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorTargetInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoThree">The third color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoFour">The four color attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfoOne,
		in ColorTargetInfo colorTargetInfoTwo,
		in ColorTargetInfo colorTargetInfoThree,
		in ColorTargetInfo colorTargetInfoFour
	) {
#if DEBUG
		AssertNotSubmitted();

		AssertTextureNotNull(colorTargetInfoOne);
		AssertColorTarget(colorTargetInfoOne);

		AssertTextureNotNull(colorTargetInfoTwo);
		AssertColorTarget(colorTargetInfoTwo);

		AssertTextureNotNull(colorTargetInfoThree);
		AssertColorTarget(colorTargetInfoThree);

		AssertTextureNotNull(colorTargetInfoFour);
		AssertColorTarget(colorTargetInfoFour);

		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoTwo.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoThree.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoFour.Texture);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree, colorTargetInfoFour],
			4,
			Unsafe.NullRef<SDL.SDL_GPUDepthStencilTargetInfo>()
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.colorAttachmentCount = 3;
		renderPass.colorAttachmentSampleCount = colorTargetInfoOne.Texture.SampleCount;
		renderPass.colorFormatOne = colorTargetInfoOne.Texture.Format;
		renderPass.colorFormatTwo = colorTargetInfoTwo.Texture.Format;
		renderPass.colorFormatThree = colorTargetInfoThree.Texture.Format;
		renderPass.colorFormatFour = colorTargetInfoFour.Texture.Format;
		renderPass.hasDepthStencilAttachment = false;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilTargetInfo">The depth stencil target to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilTargetInfo depthStencilTargetInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilTargetInfo);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[],
			0,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.depthStencilAttachmentSampleCount = depthStencilTargetInfo.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilTargetInfo.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilTargetInfo">The depth stencil target info to use in the render pass.</param>
	/// <param name="colorTargetInfo">The color target info to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfo,
		in DepthStencilTargetInfo depthStencilTargetInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilTargetInfo);

		AssertTextureNotNull(colorTargetInfo);
		AssertColorTarget(colorTargetInfo);
		AssertSameSampleCount(colorTargetInfo.Texture, depthStencilTargetInfo.Texture);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfo],
			1,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 1;
		renderPass.colorAttachmentSampleCount = colorTargetInfo.Texture.SampleCount;
		renderPass.colorFormatOne = colorTargetInfo.Texture.Format;
		renderPass.depthStencilAttachmentSampleCount = depthStencilTargetInfo.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilTargetInfo.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="depthStencilTargetInfo">The depth stencil target info to use in the render pass.</param>
	/// <param name="colorTargetInfoOne">The first color target info to use in the render pass.</param>
	/// <param name="colorTargetInfoTwo">The second color target info to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfoOne,
		in ColorTargetInfo colorTargetInfoTwo,
		in DepthStencilTargetInfo depthStencilTargetInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilTargetInfo);

		AssertTextureNotNull(colorTargetInfoOne);
		AssertColorTarget(colorTargetInfoOne);

		AssertTextureNotNull(colorTargetInfoTwo);
		AssertColorTarget(colorTargetInfoTwo);

		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoTwo.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, depthStencilTargetInfo.Texture);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo],
			2,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 2;
		renderPass.colorFormatOne = colorTargetInfoOne.Texture.Format;
		renderPass.colorFormatTwo = colorTargetInfoTwo.Texture.Format;
		renderPass.colorAttachmentSampleCount = colorTargetInfoOne.Texture.SampleCount;
		renderPass.depthStencilAttachmentSampleCount = depthStencilTargetInfo.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilTargetInfo.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorTargetInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoThree">The third color attachment to use in the render pass.</param>
	/// <param name="depthStencilTargetInfo">The depth stencil attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfoOne,
		in ColorTargetInfo colorTargetInfoTwo,
		in ColorTargetInfo colorTargetInfoThree,
		in DepthStencilTargetInfo depthStencilTargetInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilTargetInfo);

		AssertTextureNotNull(colorTargetInfoOne);
		AssertColorTarget(colorTargetInfoOne);

		AssertTextureNotNull(colorTargetInfoTwo);
		AssertColorTarget(colorTargetInfoTwo);

		AssertTextureNotNull(colorTargetInfoThree);
		AssertColorTarget(colorTargetInfoThree);

		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoTwo.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoThree.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, depthStencilTargetInfo.Texture);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree],
			3,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 3;
		renderPass.colorAttachmentSampleCount = colorTargetInfoOne.Texture.SampleCount;
		renderPass.colorFormatOne = colorTargetInfoOne.Texture.Format;
		renderPass.colorFormatTwo = colorTargetInfoTwo.Texture.Format;
		renderPass.colorFormatThree = colorTargetInfoThree.Texture.Format;
		renderPass.depthStencilAttachmentSampleCount = depthStencilTargetInfo.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilTargetInfo.Texture.Format;
#endif

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
	/// </summary>
	/// <param name="colorTargetInfoOne">The first color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoTwo">The second color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoThree">The third color attachment to use in the render pass.</param>
	/// <param name="colorTargetInfoFour">The four color attachment to use in the render pass.</param>
	/// <param name="depthStencilTargetInfo">The depth stencil attachment to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in ColorTargetInfo colorTargetInfoOne,
		in ColorTargetInfo colorTargetInfoTwo,
		in ColorTargetInfo colorTargetInfoThree,
		in ColorTargetInfo colorTargetInfoFour,
		in DepthStencilTargetInfo depthStencilTargetInfo
	) {
#if DEBUG
		AssertNotSubmitted();
		AssertValidDepthAttachment(depthStencilTargetInfo);

		AssertTextureNotNull(colorTargetInfoOne);
		AssertColorTarget(colorTargetInfoOne);

		AssertTextureNotNull(colorTargetInfoTwo);
		AssertColorTarget(colorTargetInfoTwo);

		AssertTextureNotNull(colorTargetInfoThree);
		AssertColorTarget(colorTargetInfoThree);

		AssertTextureNotNull(colorTargetInfoFour);
		AssertColorTarget(colorTargetInfoFour);

		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoTwo.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoThree.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, colorTargetInfoFour.Texture);
		AssertSameSampleCount(colorTargetInfoOne.Texture, depthStencilTargetInfo.Texture);
#endif

		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree, colorTargetInfoFour],
			4,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

#if DEBUG
		renderPassActive = true;
		renderPass.hasDepthStencilAttachment = true;
		renderPass.colorAttachmentCount = 4;
		renderPass.colorAttachmentSampleCount = colorTargetInfoOne.Texture.SampleCount;
		renderPass.colorFormatOne = colorTargetInfoOne.Texture.Format;
		renderPass.colorFormatTwo = colorTargetInfoTwo.Texture.Format;
		renderPass.colorFormatThree = colorTargetInfoThree.Texture.Format;
		renderPass.colorFormatFour = colorTargetInfoFour.Texture.Format;
		renderPass.depthStencilAttachmentSampleCount = depthStencilTargetInfo.Texture.SampleCount;
		renderPass.depthStencilFormat = depthStencilTargetInfo.Texture.Format;
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

		SDL.SDL_EndGPURenderPass(renderPass.Handle);
		renderPass.SetHandle(nint.Zero);
		Device.RenderPassPool.Return(renderPass);
	}

	/// <summary>
	/// Blits a texture to another texture with the specified filter.
	/// This operation cannot be performed inside any pass.
	/// </summary>
	public void Blit(in BlitInfo blitInfo)
	{
		SDL.SDL_BlitGPUTexture(Handle, blitInfo);
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

	private void AssertTextureNotNull(ColorTargetInfo colorTargetInfo)
	{
		if (colorTargetInfo.Texture == null || colorTargetInfo.Texture.Handle == IntPtr.Zero)
		{
			throw new System.ArgumentException("Render pass color target Texture cannot be null!");
		}
	}

	private void AssertColorTarget(ColorTargetInfo colorTargetInfo)
	{
		if ((colorTargetInfo.Texture.UsageFlags & TextureUsageFlags.ColorTarget) == 0)
		{
			throw new System.ArgumentException("Render pass color target UsageFlags must include TextureUsageFlags.ColorTarget!");
		}
	}

	private void AssertSameSampleCount(Texture a, Texture b)
	{
		if (a.SampleCount != b.SampleCount)
		{
			throw new System.ArgumentException("All attachments in a render pass must have the same SampleCount!");
		}
	}

	private void AssertValidDepthAttachment(DepthStencilTargetInfo depthStencilTargetInfo)
	{
		if (depthStencilTargetInfo.Texture == null ||
			depthStencilTargetInfo.Texture.Handle == IntPtr.Zero)
		{
			throw new System.ArgumentException("Render pass depth stencil attachment Texture cannot be null!");
		}

		if ((depthStencilTargetInfo.Texture.UsageFlags & TextureUsageFlags.DepthStencilTarget) == 0)
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
