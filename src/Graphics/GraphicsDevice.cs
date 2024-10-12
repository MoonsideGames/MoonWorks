using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MoonWorks.Video;
using SDL = MoonWorks.Graphics.SDL_GPU;

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

	// For depth formats greater than 16-bit, have to query the supported format!
	public TextureFormat SupportedDepthFormat { get; private set; }
	public TextureFormat SupportedDepthStencilFormat { get; private set; }

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
			fullscreenVertShader = Shader.CreateFromFile(
				this,
				fullscreenVertPath,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Vertex,
					Format = ShaderFormat.Private,
				}
			);
		}
		else
		{
			// use defaults
			var assembly = typeof(GraphicsDevice).Assembly;
			using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.Fullscreen.vert.spv");
			fullscreenVertShader = Shader.CreateFromStream(
				this,
				vertStream,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Vertex,
					Format = ShaderFormat.SPIRV
				}
			);
		}

		if (File.Exists(videoFragPath))
		{
			videoFragShader = Shader.CreateFromFile(
				this,
				videoFragPath,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Fragment,
					Format = ShaderFormat.Private,
					NumSamplers = 3
				}
			);
		}
		else
		{
			// use defaults
			var assembly = typeof(GraphicsDevice).Assembly;
			using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.VideoYUV2RGBA.frag.spv");
			videoFragShader = Shader.CreateFromStream(
				this,
				fragStream,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Fragment,
					Format = ShaderFormat.SPIRV,
					NumSamplers = 3
				}
			);
		}

		if (File.Exists(textVertPath) && File.Exists(textFragPath))
		{
			textVertShader = Shader.CreateFromFile(
				this,
				textVertPath,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Vertex,
					Format = ShaderFormat.Private,
					NumUniformBuffers = 1
				}
			);

			textFragShader = Shader.CreateFromFile(
				this,
				textFragPath,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Fragment,
					Format = ShaderFormat.Private,
					NumSamplers = 1,
					NumUniformBuffers = 1
				}
			);
		}
		else
		{
			// use defaults
			var assembly = typeof(GraphicsDevice).Assembly;

			using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextTransform.vert.spv");
			using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextMSDF.frag.spv");

			textVertShader = Shader.CreateFromStream(
				this,
				vertStream,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Vertex,
					Format = ShaderFormat.SPIRV,
					NumUniformBuffers = 1
				}
			);

			textFragShader = Shader.CreateFromStream(
				this,
				fragStream,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Fragment,
					Format = ShaderFormat.SPIRV,
					NumSamplers = 1,
					NumUniformBuffers = 1
				}
			);
		}

		VideoPipeline = GraphicsPipeline.Create(
			this,
			new GraphicsPipelineCreateInfo
			{
				TargetInfo = new GraphicsPipelineTargetInfo
				{
					ColorTargetDescriptions =
					[
						new ColorTargetDescription
						{
							Format = TextureFormat.R8G8B8A8Unorm,
							BlendState = ColorTargetBlendState.None
						}
					]
				},
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

		TextVertexInputState = VertexInputState.CreateSingleBinding<Font.Vertex>(0);

		PointSampler = Sampler.Create(this, SamplerCreateInfo.PointClamp);
		LinearSampler = Sampler.Create(this, SamplerCreateInfo.LinearClamp);

		FencePool = new FencePool(this);
		CommandBufferPool = new CommandBufferPool(this);

		SupportedDepthFormat = SDL.SDL_GPUTextureSupportsFormat(
			Handle,
			TextureFormat.D24Unorm,
			TextureType.TwoDimensional,
			TextureUsageFlags.DepthStencilTarget
		) ? TextureFormat.D24Unorm : TextureFormat.D32Float;

		SupportedDepthStencilFormat = SDL.SDL_GPUTextureSupportsFormat(
			Handle,
			TextureFormat.D24UnormS8Uint,
			TextureType.TwoDimensional,
			TextureUsageFlags.DepthStencilTarget
		) ? TextureFormat.D24UnormS8Uint : TextureFormat.D32FloatS8Uint;
	}

	/// <summary>
	/// Prepares a window so that frames can be presented to it.
	/// </summary>
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
			Logger.LogError(SDL3.SDL.SDL_GetError());
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
			swapchainComposition,
			presentMode
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
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var commandBuffer = CommandBufferPool.Obtain();
		commandBuffer.SetHandle(commandBufferHandle);
		return commandBuffer;
	}

	/// <summary>
	/// Submits a command buffer to the GPU for processing.
	/// </summary>
	public void Submit(CommandBuffer commandBuffer)
	{
		bool result = SDL.SDL_SubmitGPUCommandBuffer(commandBuffer.Handle);
		if (!result)
		{
			// submit errors are not recoverable so let's just fail hard
			throw new InvalidOperationException(SDL3.SDL.SDL_GetError());
		}

		CommandBufferPool.Return(commandBuffer);
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
			throw new InvalidOperationException(SDL3.SDL.SDL_GetError());
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
			Logger.LogError(SDL3.SDL.SDL_GetError());
		}
	}

	/// <summary>
	/// Waits for the given fence to become signaled.
	/// </summary>
	public unsafe void WaitForFence(Fence fence)
	{
		SDL.SDL_WaitForGPUFences(
			Handle,
			true,
			[fence.Handle],
			1
		);
	}

	/// <summary>
	/// Wait for one or more fences to become signaled.
	/// </summary>
	/// <param name="waitAll">If true, will wait for all given fences to be signaled.</param>
	public unsafe void WaitForFences(Span<Fence> fences, bool waitAll)
	{
		Span<IntPtr> handlePtr = stackalloc nint[fences.Length];

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
