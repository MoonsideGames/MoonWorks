using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDL = MoonWorks.Graphics.SDL_GPU;

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

	// called from CommandBufferPool
	internal CommandBuffer(GraphicsDevice device)
	{
		Device = device;
		Handle = IntPtr.Zero;
	}

	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}

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
		if (!SDL.SDL_AcquireGPUSwapchainTexture(
			Handle,
			window.Handle,
			out var texturePtr,
			out var width,
			out var height
		)) {
			// FIXME: should we throw?
			Logger.LogError(SDL3.SDL.SDL_GetError());
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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfo],
			1,
			Unsafe.NullRef<DepthStencilTargetInfo>()
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo],
			2,
			Unsafe.NullRef<DepthStencilTargetInfo>()
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree],
			3,
			Unsafe.NullRef<DepthStencilTargetInfo>()
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree, colorTargetInfoFour],
			4,
			Unsafe.NullRef<DepthStencilTargetInfo>()
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[],
			0,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfo],
			1,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo],
			2,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree],
			3,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

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
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			[colorTargetInfoOne, colorTargetInfoTwo, colorTargetInfoThree, colorTargetInfoFour],
			4,
			depthStencilTargetInfo
		);

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.SetHandle(renderPassHandle);

		return renderPass;
	}

	/// <summary>
	/// Ends the current render pass.
	/// This must be called before beginning another render pass or submitting the command buffer.
	/// </summary>
	public void EndRenderPass(RenderPass renderPass)
	{
		SDL.SDL_EndGPURenderPass(renderPass.Handle);
		renderPass.SetHandle(nint.Zero);
		Device.RenderPassPool.Return(renderPass);
	}

	/// <summary>
	/// Blits a texture to another texture.
	/// This operation cannot be performed inside any pass.
	/// </summary>
	public void Blit(in BlitInfo blitInfo)
	{
		SDL.SDL_BlitGPUTexture(Handle, blitInfo);
	}

	/// <summary>
	/// Convenience method that blits a 2D texture to a 2D texture.
	/// </summary>
	public void Blit(Texture source, Texture destination, Filter filter, bool cycle = false)
	{
		SDL.SDL_BlitGPUTexture(Handle, new BlitInfo
		{
			Source = new BlitRegion
			{
				Texture = source.Handle,
				W = source.Width,
				H = source.Height
			},
			Destination = new BlitRegion
			{
				Texture = destination.Handle,
				W = destination.Width,
				H = destination.Height
			},
			Filter = filter,
			LoadOp = LoadOp.DontCare,
			Cycle = cycle
		});
	}

	public ComputePass BeginComputePass(
		Span<StorageTextureReadWriteBinding> readWriteTextureBindings,
		Span<StorageBufferReadWriteBinding> readWriteBufferBindings
	) {
		var computePassHandle = SDL.SDL_BeginGPUComputePass(
			Handle,
			readWriteTextureBindings,
			(uint) readWriteTextureBindings.Length,
			readWriteBufferBindings,
			(uint) readWriteBufferBindings.Length
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

		return computePass;
	}

	public ComputePass BeginComputePass(
		in StorageTextureReadWriteBinding readWriteTextureBinding,
		in StorageBufferReadWriteBinding readWriteBufferBinding
	) {
		var computePassHandle = SDL.SDL_BeginGPUComputePass(
			Handle,
			[readWriteTextureBinding],
			1,
			[readWriteBufferBinding],
			1
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

		return computePass;
	}

	public ComputePass BeginComputePass(
		in StorageTextureReadWriteBinding readWriteTextureBinding
	) {
		var computePassHandle = SDL.SDL_BeginGPUComputePass(
			Handle,
			[readWriteTextureBinding],
			1,
			[],
			0
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

		return computePass;
	}

	public ComputePass BeginComputePass(
		in StorageBufferReadWriteBinding readWriteBufferBinding
	) {
		var computePassHandle = SDL.SDL_BeginGPUComputePass(
			Handle,
			[],
			0,
			[readWriteBufferBinding],
			1
		);

		var computePass = Device.ComputePassPool.Obtain();
		computePass.SetHandle(computePassHandle);

		return computePass;
	}

	public void EndComputePass(ComputePass computePass)
	{
		SDL.SDL_EndGPUComputePass(
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
		var copyPassHandle = SDL.SDL_BeginGPUCopyPass(Handle);

		var copyPass = Device.CopyPassPool.Obtain();
		copyPass.SetHandle(copyPassHandle);

		return copyPass;
	}

	public void EndCopyPass(CopyPass copyPass)
	{
		SDL.SDL_EndGPUCopyPass(copyPass.Handle);
		copyPass.SetHandle(nint.Zero);
		Device.CopyPassPool.Return(copyPass);
	}

	// Debug Labels

	public void InsertDebugLabel(string text)
	{
		SDL.SDL_InsertGPUDebugLabel(Handle, text);
	}

	public void PushDebugGroup(string name)
	{
		SDL.SDL_PushGPUDebugGroup(Handle, name);
	}

	public void PopDebugGroup()
	{
		SDL.SDL_PopGPUDebugGroup(Handle);
	}
}
