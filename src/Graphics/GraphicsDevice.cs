using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MoonWorks.Video;
using SDL3;

namespace MoonWorks.Graphics;

/// <summary>
/// Manages all graphics-related concerns.
/// </summary>
public class GraphicsDevice : IDisposable
{
	public IntPtr Handle { get; }
	public string Backend { get; }
	public bool DebugMode { get; }

	// Built-in video pipeline
	internal GraphicsPipeline VideoPipeline { get; }

	// Built-in text shader info
	public Shader TextVertexShader;
	public Shader TextFragmentShader;
	public VertexInputState TextVertexInputState;

	// Built-in samplers
	public Sampler PointSampler { get; }
	public Sampler LinearSampler { get; }

	public bool IsDisposed { get; private set; }

	private readonly HashSet<GCHandle> resources = new HashSet<GCHandle>();
	private CommandBufferPool CommandBufferPool;
	private FencePool FencePool;
	internal RenderPassPool RenderPassPool = new RenderPassPool();
	internal ComputePassPool ComputePassPool = new ComputePassPool();
	internal CopyPassPool CopyPassPool = new CopyPassPool();

	internal unsafe GraphicsDevice(
		ShaderFormat shaderFormats, // TODO: replace with enum flags
		bool debugMode,
		string backendName = null
	) {
		if (shaderFormats == 0)
		{
			throw new System.Exception("Need at least one shader format!");
		}

		Handle = SDL.SDL_CreateGPUDevice(
			shaderFormats,
			debugMode,
			backendName
		);

		DebugMode = debugMode;
		// TODO: check for CreateDevice fail

		Backend = SDL.SDL_GetGPUDeviceDriver(Handle);

		// Check for replacement stock shaders
		string basePath = System.AppContext.BaseDirectory;

		string fullscreenVertPath = Path.Combine(basePath, "fullscreen.vert.refresh");

		string textVertPath = Path.Combine(basePath, "text_transform.vert.refresh");
		string textFragPath = Path.Combine(basePath, "text_msdf.frag.refresh");

		string videoFragPath = Path.Combine(basePath, "video_yuv2rgba.frag.refresh");

		Shader fullscreenVertShader;

		Shader textVertShader;
		Shader textFragShader;

		Shader videoFragShader;

		if (File.Exists(fullscreenVertPath))
		{
			fullscreenVertShader = new Shader(
				this,
				fullscreenVertPath,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage = ShaderStage.Vertex,
					ShaderFormat = ShaderFormat.SECRET,
				}
			);
		}
		else
		{
			// use defaults
			var assembly = typeof(GraphicsDevice).Assembly;
			using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.Fullscreen.vert.spv");
			fullscreenVertShader = new Shader(
				this,
				vertStream,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage =ShaderStage.Vertex,
					ShaderFormat = ShaderFormat.SPIRV
				}
			);
		}

		if (File.Exists(videoFragPath))
		{
			videoFragShader = new Shader(
				this,
				videoFragPath,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage =ShaderStage.Fragment,
					ShaderFormat = ShaderFormat.SECRET,
					SamplerCount = 3
				}
			);
		}
		else
		{
			// use defaults
			var assembly = typeof(GraphicsDevice).Assembly;
			using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.VideoYUV2RGBA.frag.spv");
			videoFragShader = new Shader(
				this,
				fragStream,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage =ShaderStage.Fragment,
					ShaderFormat = ShaderFormat.SPIRV,
					SamplerCount = 3
				}
			);
		}

		if (File.Exists(textVertPath) && File.Exists(textFragPath))
		{
			textVertShader = new Shader(
				this,
				textVertPath,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage = ShaderStage.Vertex,
					ShaderFormat = ShaderFormat.SECRET,
					UniformBufferCount = 1
				}
			);

			textFragShader = new Shader(
				this,
				textFragPath,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage = ShaderStage.Fragment,
					ShaderFormat = ShaderFormat.SECRET,
					SamplerCount = 1,
					UniformBufferCount = 1
				}
			);
		}
		else
		{
			// use defaults
			var assembly = typeof(GraphicsDevice).Assembly;

			using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextTransform.vert.spv");
			using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextMSDF.frag.spv");

			textVertShader = new Shader(
				this,
				vertStream,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage = ShaderStage.Vertex,
					ShaderFormat = ShaderFormat.SPIRV,
					UniformBufferCount = 1
				}
			);

			textFragShader = new Shader(
				this,
				fragStream,
				"main",
				new ShaderCreateInfo
				{
					ShaderStage = ShaderStage.Fragment,
					ShaderFormat = ShaderFormat.SPIRV,
					SamplerCount = 1,
					UniformBufferCount = 1
				}
			);
		}

		VideoPipeline = new GraphicsPipeline(
			this,
			new GraphicsPipelineCreateInfo
			{
				AttachmentInfo = new GraphicsPipelineAttachmentInfo(
					new ColorAttachmentDescription(
						TextureFormat.R8G8B8A8,
						ColorAttachmentBlendState.None
					)
				),
				DepthStencilState = DepthStencilState.Disable,
				VertexShader = fullscreenVertShader,
				FragmentShader = videoFragShader,
				VertexInputState = VertexInputState.Empty,
				RasterizerState = RasterizerState.CCW_CullNone,
				PrimitiveType = PrimitiveType.TriangleList,
				MultisampleState = MultisampleState.None
			}
		);

		TextVertexShader = textVertShader;
		TextFragmentShader = textFragShader;

		TextVertexInputState = VertexInputState.CreateSingleBinding<Font.Vertex>();

		PointSampler = new Sampler(this, SamplerCreateInfo.PointClamp);
		LinearSampler = new Sampler(this, SamplerCreateInfo.LinearClamp);

		FencePool = new FencePool(this);
		CommandBufferPool = new CommandBufferPool(this);
	}

	/// <summary>
	/// Prepares a window so that frames can be presented to it.
	/// </summary>
	/// <returns>True if successfully claimed.</returns>
	public bool ClaimWindow(Window window) {
		if (window.Claimed)
		{
			Logger.LogError("Window already claimed!");
			return false;
		}

		bool result = SDL.SDL_ClaimWindowForGPUDevice(Handle, window.Handle);

		if (result)
		{
			window.Claimed = true;
			window.SwapchainComposition = SwapchainComposition.SDR;
			window.SwapchainFormat = GetSwapchainFormat(window);

			if (window.SwapchainTexture == null)
			{
				window.SwapchainTexture = new Texture(this, window.SwapchainFormat);
			}
		}
		else
		{
			Logger.LogError(SDL.SDL_GetError());
		}

		return result;
	}

	/// <summary>
	/// Unclaims a window, making it unavailable for presenting and freeing associated resources.
	/// </summary>
	public void UnclaimWindow(Window window)
	{
		if (window.Claimed)
		{
			SDL.SDL_ReleaseWindowFromGPUDevice(
				Handle,
				window.Handle
			);
			window.Claimed = false;

			// The swapchain texture doesn't actually have a permanent texture reference, so we zero the handle before disposing.
			window.SwapchainTexture.Handle = IntPtr.Zero;
			window.SwapchainTexture.Dispose();
			window.SwapchainTexture = null;
		}
	}

	/// <summary>
	/// Changes the present mode of a claimed window. Does nothing if the window is not claimed.
	/// </summary>
	public bool SetSwapchainParameters(
		Window window,
		SwapchainComposition swapchainComposition,
		PresentMode presentMode
	) {
		if (!window.Claimed)
		{
			Logger.LogError("Cannot set present mode on unclaimed window!");
			return false;
		}

		bool result = SDL.SDL_SetGPUSwapchainParameters(
			Handle,
			window.Handle,
			(SDL.SDL_GPUSwapchainComposition) swapchainComposition,
			(SDL.SDL_GPUPresentMode) presentMode
		);

		if (result)
		{
			window.SwapchainComposition = swapchainComposition;
			window.SwapchainFormat = GetSwapchainFormat(window);

			if (window.SwapchainTexture != null)
			{
				window.SwapchainTexture.Format = window.SwapchainFormat;
			}
		}

		return result;
	}

	/// <summary>
	/// Acquires a command buffer.
	/// This is the start of your rendering process.
	/// </summary>
	/// <returns></returns>
	public CommandBuffer AcquireCommandBuffer()
	{
		var commandBufferHandle = SDL.SDL_AcquireGPUCommandBuffer(Handle);
		if (commandBufferHandle == IntPtr.Zero)
		{
			Logger.LogError(SDL.SDL_GetError());
			return null;
		}

		var commandBuffer = CommandBufferPool.Obtain();
		commandBuffer.SetHandle(commandBufferHandle);
#if DEBUG
		commandBuffer.ResetStateTracking();
#endif
		return commandBuffer;
	}

	/// <summary>
	/// Submits a command buffer to the GPU for processing.
	/// </summary>
	public void Submit(CommandBuffer commandBuffer)
	{
#if DEBUG
		if (commandBuffer.Submitted)
		{
			throw new System.InvalidOperationException("Command buffer already submitted!");
		}
#endif

		bool result = SDL.SDL_SubmitGPUCommandBuffer(Handle);
		if (!result)
		{
			// submit errors are not recoverable so let's just fail hard
			throw new InvalidOperationException(SDL.SDL_GetError());
		}

		CommandBufferPool.Return(commandBuffer);

#if DEBUG
		commandBuffer.Submitted = true;
#endif
	}

	/// <summary>
	/// Submits a command buffer to the GPU for processing and acquires a fence associated with the submission.
	/// </summary>
	/// <returns></returns>
	public Fence SubmitAndAcquireFence(CommandBuffer commandBuffer)
	{
		var fenceHandle = SDL.SDL_SubmitGPUCommandBufferAndAcquireFence(commandBuffer.Handle);

		if (fenceHandle == IntPtr.Zero)
		{
			// submit errors are not recoverable so let's just fail hard
			throw new InvalidOperationException(SDL.SDL_GetError());
		}

		var fence = FencePool.Obtain();
		fence.SetHandle(fenceHandle);
		return fence;
	}

	/// <summary>
	/// Wait for the graphics device to become idle.
	/// </summary>
	public void Wait()
	{
		if (!SDL.SDL_WaitForGPUIdle(Handle))
		{
			Logger.LogError(SDL.SDL_GetError());
		}
	}

	/// <summary>
	/// Waits for the given fence to become signaled.
	/// </summary>
	public unsafe void WaitForFence(Fence fence)
	{
		var fenceHandle = fence.Handle;

		SDL.SDL_WaitForGPUFences(
			Handle,
			true,
			&fenceHandle,
			1
		);
	}

	/// <summary>
	/// Wait for one or more fences to become signaled.
	/// </summary>
	/// <param name="waitAll">If true, will wait for all given fences to be signaled.</param>
	public unsafe void WaitForFences(Span<Fence> fences, bool waitAll)
	{
		var handlePtr = stackalloc nint[fences.Length];

		for (var i = 0; i < fences.Length; i += 1)
		{
			handlePtr[i] = fences[i].Handle;
		}

		SDL.SDL_WaitForGPUFences(
			Handle,
			waitAll,
			handlePtr,
			(uint) fences.Length
		);
	}

	/// <summary>
	/// Returns true if the fence is signaled, indicating that the associated command buffer has finished processing.
	/// </summary>
	public bool QueryFence(Fence fence)
	{
		return SDL.SDL_QueryGPUFence(Handle, fence.Handle);
	}

	/// <summary>
	/// Release reference to an acquired fence, enabling it to be reused.
	/// </summary>
	public void ReleaseFence(Fence fence)
	{
		SDL.SDL_ReleaseGPUFence(Handle, fence.Handle);
		fence.Handle = IntPtr.Zero;
		FencePool.Return(fence);
	}

	private TextureFormat GetSwapchainFormat(Window window)
	{
		if (!window.Claimed)
		{
			throw new System.ArgumentException("Cannot get swapchain format of unclaimed window!");
		}

		return (TextureFormat) SDL.SDL_GetGPUSwapchainTextureFormat(Handle, window.Handle);
	}

	internal void AddResourceReference(GCHandle resourceReference)
	{
		lock (resources)
		{
			resources.Add(resourceReference);
		}
	}

	internal void RemoveResourceReference(GCHandle resourceReference)
	{
		lock (resources)
		{
			resources.Remove(resourceReference);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			if (disposing)
			{
				lock (resources)
				{
					// Dispose video players first to avoid race condition on threaded decoding
					foreach (var resource in resources)
					{
						if (resource.Target is VideoPlayer player)
						{
							player.Dispose();
						}
					}

					// Dispose everything else
					foreach (var resource in resources)
					{
						if (resource.Target is IDisposable disposable)
						{
							disposable.Dispose();
						}
					}
					resources.Clear();
				}
			}

			SDL.SDL_DestroyGPUDevice(Handle);

			IsDisposed = true;
		}
	}

	~GraphicsDevice()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
