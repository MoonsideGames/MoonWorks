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
	/// Can return null if the swapchain is unavailable, this is not necessarily an error and the client should ALWAYS handle the case where this occurs.
	/// If there is an error this method will throw an exception. You should not attempt to recover as the graphics driver state is unrecoverable.
	/// If null is returned, presentation will not occur.
	/// You may want to call GraphicsDevice.Cancel if acquiring the swapchain texture fails if the command buffer already has a lot of commands in it.
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
			throw new System.InvalidOperationException(SDL3.SDL.SDL_GetError());
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
	/// <param name="colorTargetInfos">The color targets to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		params Span<ColorTargetInfo> colorTargetInfos
	) {
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			colorTargetInfos,
			(uint) colorTargetInfos.Length,
			Unsafe.NullRef<DepthStencilTargetInfo>()
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.Handle = renderPassHandle;
		renderPass.CommandBuffer = this;

		return renderPass;
	}

	/// <summary>
	/// Begins a render pass.
	/// All render state, resource binding, and draw commands must be made within a render pass.
	/// It is an error to call this during any kind of pass.
	/// </summary>
	/// <param name="depthStencilTargetInfo">The depth stencil target to use in the render pass.</param>
	/// <param name="colorTargetInfos">The color targets to use in the render pass.</param>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilTargetInfo depthStencilTargetInfo,
		params Span<ColorTargetInfo> colorTargetInfos
	) {
		var renderPassHandle = SDL.SDL_BeginGPURenderPass(
			Handle,
			colorTargetInfos,
			(uint) colorTargetInfos.Length,
			depthStencilTargetInfo
		);

		if (renderPassHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var renderPass = Device.RenderPassPool.Obtain();
		renderPass.Handle = renderPassHandle;
		renderPass.CommandBuffer = this;

		return renderPass;
	}

	/// <summary>
	/// Ends the current render pass.
	/// This must be called before beginning another render pass or submitting the command buffer.
	/// </summary>
	public void EndRenderPass(RenderPass renderPass)
	{
		SDL.SDL_EndGPURenderPass(renderPass.Handle);
		renderPass.Handle = IntPtr.Zero;
		renderPass.CommandBuffer = null;
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
		computePass.Handle = computePassHandle;
		computePass.CommandBuffer = this;

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
		computePass.Handle = computePassHandle;
		computePass.CommandBuffer = this;

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
		computePass.Handle = computePassHandle;
		computePass.CommandBuffer = this;

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
		computePass.Handle = computePassHandle;
		computePass.CommandBuffer = this;

		return computePass;
	}

	public void EndComputePass(ComputePass computePass)
	{
		SDL.SDL_EndGPUComputePass(
			computePass.Handle
		);

		computePass.Handle = IntPtr.Zero;
		computePass.CommandBuffer = null;
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
		copyPass.Handle = copyPassHandle;
		copyPass.CommandBuffer = this;

		return copyPass;
	}

	public void EndCopyPass(CopyPass copyPass)
	{
		SDL.SDL_EndGPUCopyPass(copyPass.Handle);
		copyPass.Handle = IntPtr.Zero;
		copyPass.CommandBuffer = null;
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
