using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

/// <summary>
/// A class that buffers commands to be submitted to the GPU.
/// 
/// These commands include operations like copying data, draw calls, and compute dispatches.
/// Commands only begin executing once the command buffer is submitted.
/// If you need to know when the command buffer is done, use GraphicsDevice.SubmitAndAcquireFence
/// </summary>
public class CommandBuffer
{
	/// <summary>Gets the <see cref="GraphicsDevice"/> the command buffer is bound to.</summary>
	public GraphicsDevice Device { get; }

	/// <summary>Gets the pointer value to the command buffer.</summary>
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
	/// Acquires a swapchain texture for a given <see cref="Window"/> instance that has been claimed.
	/// This texture will be presented to the given window when the command buffer is submitted.
	///
	/// Return <see cref="null"/> if the swapchain is unavailable. The user should *ALWAYS* handle the case where this occurs.
	/// If null is returned, presentation will not occur.
	/// </summary>
	/// <param name="window">The claimed window instance.</param>
	/// <remarks>
	/// It is an error to acquire two swapchain textures from the same window in one command buffer.
	/// It is an error to dispose the swapchain texture. If you do this your game WILL crash. DO NOT DO THIS.
	/// </remarks>
	public Texture AcquireSwapchainTexture(
		Window window
	)
	{
		if (!SDL.SDL_AcquireGPUSwapchainTexture(
			Handle,
			window.Handle,
			out var texturePtr,
			out var width,
			out var height
		))
		{
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
	/// Blocks until a swapchain texture is available and then acquires a swapchain texture for a
	/// claimed <see cref="Window"/> instance. This texture will be presented to the given window
	/// when the command buffer is submitted.
	/// </summary>
	/// <param name="window">The claimed window instance.</param>
	/// <remarks>
	/// It is an error to acquire two swapchain textures from the same window in one command buffer.
	/// It is an error to dispose the swapchain texture. If you do this your game WILL crash. DO NOT DO THIS.
	/// </remarks>
	public Texture WaitAndAcquireSwapchainTexture(
		Window window
	)
	{
		if (!SDL.SDL_WaitAndAcquireGPUSwapchainTexture(
			Handle,
			window.Handle,
			out var texturePtr,
			out var width,
			out var height
		))
		{
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
	/// </summary>
	/// <param name="uniformsPtr">Pointer to the data to push to the GPU.</param>
	/// <param name="size">The size of the uniform data in bytes.</param>
	/// <param name="slot">The vertex uniform slot to push the data to.</param>
	/// <remarks>It is legal to push uniforms during a render or compute pass.</remarks>
	public unsafe void PushVertexUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	)
	{
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
	/// </summary>
	/// <typeparam name="T">An unmanaged data type.</typeparam>
	/// <param name="uniforms">Data of an unmanaged type to be pushed to the GPU.</param>
	/// <param name="slot">The vertex uniform slot to push the data to.</param>
	/// <remarks>It is legal to push uniforms during a render or compute pass.</remarks>
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
	/// </summary>
	/// <param name="uniformsPtr">Pointer to the data to push to the GPU.</param>
	/// <param name="size">The size of the uniform data in bytes.</param>
	/// <param name="slot">The fragment uniform slot to push the data to.</param>
	/// <remarks>It is legal to push uniforms during a pass.</remarks>
	public unsafe void PushFragmentUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	)
	{
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
	/// </summary>
	/// <typeparam name="T">An unmanaged data type.</typeparam>
	/// <param name="uniforms">Data of an unmanaged type to be pushed to the GPU.</param>
	/// <param name="slot">The fragment uniform slot to push the data to.</param>
	/// <remarks>It is legal to push uniforms during a pass.</remarks>
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
	/// </summary>
	/// <param name="uniformsPtr">Pointer to the data to push to the GPU.</param>
	/// <param name="size">The size of the uniform data in bytes.</param>
	/// <param name="slot">The uniform slot to push the data to.</param>
	/// <remarks>It is legal to push uniforms during a pass.</remarks>
	public unsafe void PushComputeUniformData(
		void* uniformsPtr,
		uint size,
		uint slot = 0
	)
	{
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
	/// </summary>
	/// <typeparam name="T">An unmanaged data type.</typeparam>
	/// <param name="uniforms">Data of an unmanaged type to be pushed to the GPU.</param>
	/// <param name="slot">The uniform slot to push the data to.</param>
	/// <remarks>It is legal to push uniforms during a pass.</remarks>
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
	/// </summary>
	/// <param name="colorTargetInfos">A variadic amount of color targets to use in the render pass.</param>
	/// <returns>A <see cref="RenderPass"/> instance representing the pass that has begun.</returns>
	/// <remarks>It is an error to call this during any kind of pass.</remarks>
	public unsafe RenderPass BeginRenderPass(
		params Span<ColorTargetInfo> colorTargetInfos
	)
	{
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
	/// </summary>
	/// <param name="depthStencilTargetInfo">The depth stencil target to use in the render pass.</param>
	/// <param name="colorTargetInfos">A variadic amount of color targets to use in the render pass.</param>
	/// <returns>A <see cref="RenderPass"/> instance representing the pass that has begun.</returns>
	/// <remarks>It is an error to call this during any kind of pass.</remarks>
	public unsafe RenderPass BeginRenderPass(
		in DepthStencilTargetInfo depthStencilTargetInfo,
		params Span<ColorTargetInfo> colorTargetInfos
	)
	{
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
	/// </summary>
	/// <param name="renderPass">The current render pass to end.</param>
	/// <remarks>This must be called before beginning another render pass or submitting the command buffer.</remarks>
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
	/// <param name="blitInfo">Parameter data used for the blit operation.</param>
	public void Blit(in BlitInfo blitInfo)
	{
		SDL.SDL_BlitGPUTexture(Handle, blitInfo);
	}

	/// <summary>
	/// Convenience method that blits a 2D texture to a 2D texture.
	/// </summary>
	/// <param name="source">The texture to use as the source region.</param>
	/// <param name="destination">The texture to use as the destination region.</param>
	/// <param name="filter">Indicates the filter mode to use when blitting.</param>
	/// <param name="cycle">Indicates if the destination texture is to be cycled when already bound.</param>
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

	/// <summary>
	/// Begins a compute pass.
	/// All compute commands must be made within a compute pass.
	/// </summary>
	/// <param name="readWriteTextureBindings">Variadic amount parameter data describing the storage textures to bind to the compute pass.</param>
	/// <param name="readWriteBufferBindings">Variadic amount parameter data describing the storage buffers to bind to the compute pass.</param>
	/// <returns>A <see cref="ComputePass"/> instance representing the pass that has begun.</returns>
	/// <remarks>It is an error to call this during any kind of pass.</remarks>
	public ComputePass BeginComputePass(
		Span<StorageTextureReadWriteBinding> readWriteTextureBindings,
		Span<StorageBufferReadWriteBinding> readWriteBufferBindings
	)
	{
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

	/// <summary>
	/// Begins a compute pass.
	/// All compute commands must be made within a compute pass.
	/// </summary>
	/// <param name="readWriteTextureBinding">Parameter data describing the storage textures to bind to the compute pass.</param>
	/// <param name="readWriteBufferBinding">Parameter data describing the storage buffers to bind to the compute pass.</param>
	/// <returns>A <see cref="ComputePass"/> instance representing the pass that has begun.</returns>
	/// <remarks>It is an error to call this during any kind of pass.</remarks>
	public ComputePass BeginComputePass(
		in StorageTextureReadWriteBinding readWriteTextureBinding,
		in StorageBufferReadWriteBinding readWriteBufferBinding
	)
	{
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

	/// <summary>
	/// Begins a compute pass.
	/// All compute commands must be made within a compute pass.
	/// </summary>
	/// <param name="readWriteTextureBinding">Parameter data describing the storage textures to bind to the compute pass.</param>
	/// <returns>A <see cref="ComputePass"/> instance representing the pass that has begun.</returns>
	/// <remarks>It is an error to call this during any kind of pass.</remarks>
	public ComputePass BeginComputePass(
		in StorageTextureReadWriteBinding readWriteTextureBinding
	)
	{
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

	/// <summary>
	/// Begins a compute pass.
	/// All compute commands must be made within a compute pass.
	/// </summary>
	/// <param name="readWriteBufferBinding">Parameter data describing the storage buffers to bind to the compute pass.</param>
	/// <returns>A <see cref="ComputePass"/> instance representing the pass that has begun.</returns>
	/// <remarks>It is an error to call this during any kind of pass.</remarks>
	public ComputePass BeginComputePass(
		in StorageBufferReadWriteBinding readWriteBufferBinding
	)
	{
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

	/// <summary>
	/// Ends the current compute pass.
	/// </summary>
	/// <param name="computePass">The current compute pass to end.</param>
	/// <remarks>This must be called before beginning another render pass or submitting the command buffer.</remarks>
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
	/// </summary>
	/// <returns>A <see cref="CopyPass"/> instance representing the pass that has begun.</returns>
	/// <remarks>It is an error to call this during any kind of pass.</remarks>
	public CopyPass BeginCopyPass()
	{
		var copyPassHandle = SDL.SDL_BeginGPUCopyPass(Handle);

		var copyPass = Device.CopyPassPool.Obtain();
		copyPass.Handle = copyPassHandle;
		copyPass.CommandBuffer = this;

		return copyPass;
	}

	/// <summary>
	/// Ends the current copy pass.
	/// </summary>
	/// <param name="copyPass">The current copy pass to end.</param>
	/// <remarks>This must be called before beginning another render pass or submitting the command buffer.</remarks>
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
