using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MoonWorks.Video;
using SDL = MoonWorks.Graphics.SDL_GPU;

using MoonWorks.Storage;

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

	// Built-in fullscreen vertex shader
	public Shader FullscreenVertexShader;

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

	internal Texture DummyTexture;

	internal unsafe GraphicsDevice(
		TitleStorage rootTitleStorage,
		ShaderFormat shaderFormats,
		bool debugMode,
		string backendName = null
	) {
		if (shaderFormats == 0)
		{
			throw new System.Exception("Need at least one shader format!");
		}

		var properties = SDL3.SDL.SDL_CreateProperties();
		SDL3.SDL.SDL_SetBooleanProperty(properties, "SDL.gpu.device.create.debugmode", debugMode);
		SDL3.SDL.SDL_SetStringProperty(properties, "SDL.gpu.device.create.name", backendName);

		if ((shaderFormats & ShaderFormat.Private) != 0) {
			SDL3.SDL.SDL_SetBooleanProperty(properties, "SDL.gpu.device.create.shaders.private", true);
		}
		if ((shaderFormats & ShaderFormat.SPIRV) != 0) {
			SDL3.SDL.SDL_SetBooleanProperty(properties, "SDL.gpu.device.create.shaders.spirv", true);
		}
		if ((shaderFormats & ShaderFormat.DXBC) != 0) {
			SDL3.SDL.SDL_SetBooleanProperty(properties, "SDL.gpu.device.create.shaders.dxbc", true);
		}
		if ((shaderFormats & ShaderFormat.DXIL) != 0) {
			SDL3.SDL.SDL_SetBooleanProperty(properties, "SDL.gpu.device.create.shaders.dxil", true);
		}
		if ((shaderFormats & ShaderFormat.MSL) != 0) {
			SDL3.SDL.SDL_SetBooleanProperty(properties, "SDL.gpu.device.create.shaders.msl", true);
		}
		if ((shaderFormats & ShaderFormat.MetalLib) != 0) {
			SDL3.SDL.SDL_SetBooleanProperty(properties, "SDL.gpu.device.create.shaders.metallib", true);
		}

		Handle = SDL.SDL_CreateGPUDeviceWithProperties(properties);

		SDL3.SDL.SDL_DestroyProperties(properties);

		if (Handle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			throw new InvalidOperationException("Failed to create graphics device!");
		}

		DebugMode = debugMode;
		// TODO: check for CreateDevice fail

		Backend = SDL.SDL_GetGPUDeviceDriver(Handle);

		// Check for replacement stock shaders
		string fullscreenVertPath = "fullscreen.vert.private";

		string textVertPath = "text_transform.vert.private";
		string textFragPath = "text_msdf.frag.private";

		string videoFragPath = "video_yuv2rgba.frag.private";

		Shader textVertShader;
		Shader textFragShader;

		Shader videoFragShader;

		if (rootTitleStorage.Exists(fullscreenVertPath))
		{
			FullscreenVertexShader = Shader.Create(
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
			FullscreenVertexShader = LoadShaderFromManifest(
				Backend,
				"Fullscreen.vert",
				new ShaderCreateInfo {
					Stage = ShaderStage.Vertex
				}
			);
		}

		if (rootTitleStorage.Exists(videoFragPath))
		{
			videoFragShader = Shader.Create(
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
			videoFragShader = LoadShaderFromManifest(
				Backend,
				"VideoYUV2RGBA.frag",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Fragment,
					NumSamplers = 3
				}
			);
		}

		if (rootTitleStorage.Exists(textVertPath) && rootTitleStorage.Exists(textFragPath))
		{
			textVertShader = Shader.Create(
				this,
				textVertPath,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Vertex,
					Format = ShaderFormat.Private,
					NumStorageBuffers = 1,
					NumUniformBuffers = 1
				}
			);

			textFragShader = Shader.Create(
				this,
				textFragPath,
				"main",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Fragment,
					Format = ShaderFormat.Private,
					NumSamplers = 4
				}
			);
		}
		else
		{
			// use defaults
			textVertShader = LoadShaderFromManifest(
				Backend,
				"TextTransform.vert",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Vertex,
					NumStorageBuffers = 1,
					NumUniformBuffers = 1
				}
			);

			textFragShader = LoadShaderFromManifest(
				Backend,
				"TextMSDF.frag",
				new ShaderCreateInfo
				{
					Stage = ShaderStage.Fragment,
					NumSamplers = 4
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
							BlendState = ColorTargetBlendState.NoBlend
						}
					]
				},
				DepthStencilState = DepthStencilState.Disable,
				VertexShader = FullscreenVertexShader,
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

		DummyTexture = Texture.Create2D(this, "Dummy Texture", 1, 1, TextureFormat.R8G8B8A8Unorm, TextureUsageFlags.Sampler);

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
	/// Configures the maximum allowed number of frames in flight.<br/><br/>
	///
	/// The default value when the device is created is 2.
	/// This means that after you have submitted 2 frames for presentation, if the GPU has not finished working on the first frame, SDL_AcquireGPUSwapchainTexture() will block or return false depending on the present mode.<br/><br/>
	///
	/// Higher values increase throughput at the expense of visual latency.
	/// Lower values decrease visual latency at the expense of throughput.<br/><br/>
	///
	/// Note that calling this function will stall and flush the command queue to prevent synchronization issues.<br/><br/>
	///
	/// The minimum value of allowed frames in flight is 1, and the maximum is 3.
	/// </summary>
	/// <param name="allowedFramesInFlight">The maximum number of frames that can be pending on the GPU before AcquireSwapchainTexture blocks or returns false.</param>
	/// <returns>True on success or false on error.</returns>
	public bool SetAllowedFramesInFlight(uint allowedFramesInFlight)
	{
		var result = SDL.SDL_SetGPUAllowedFramesInFlight(Handle, allowedFramesInFlight);
		if (!result) {
			Logger.LogError(SDL3.SDL.SDL_GetError());
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
	/// Cancels a command buffer. None of the enqueued commands are executed.
	/// This must be called from the thread the command buffer was acquired on.
	/// It is an error to call this function after a swapchain texture has been acquired.
	/// </summary>
	public void Cancel(CommandBuffer commandBuffer) {
		bool result = SDL.SDL_CancelGPUCommandBuffer(commandBuffer.Handle);
		if (!result)
		{
			// command buffer errors are not recoverable so let's just fail hard
			throw new InvalidOperationException(SDL3.SDL.SDL_GetError());
		}

		CommandBufferPool.Return(commandBuffer);
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
	/// Waits for the swapchain to become available.
	/// Useful for latency-optimized workflows.
	/// </summary>
	public void WaitForSwapchain(Window window)
	{
		if (!SDL.SDL_WaitForGPUSwapchain(Handle, window.Handle))
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
	public unsafe void WaitForFences(bool waitAll, params Span<Fence> fences)
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

	private Shader LoadShaderFromManifest(string backend, string name, ShaderCreateInfo createInfo)
	{
		ShaderFormat shaderFormat;
		string extension;
		string entryPointName;
		switch (backend)
		{
			case "vulkan":
				shaderFormat = ShaderFormat.SPIRV;
				extension = "spv";
				entryPointName = "main";
				break;

			case "metal":
				shaderFormat = ShaderFormat.MSL;
				extension = "msl";
				entryPointName = "main0";
				break;

			case "direct3d11":
				shaderFormat = ShaderFormat.DXBC;
				extension = "dxbc";
				entryPointName = "main";
				break;

			case "direct3d12":
				shaderFormat = ShaderFormat.DXIL;
				extension = "dxil";
				entryPointName = "main";
				break;

			default:
				throw new ArgumentException("This shouldn't happen!");
		}

		var createInfoWithFormat = createInfo with { Format = shaderFormat };
		var path = $"MoonWorks.Graphics.StockShaders.{name}.{extension}";
		var assembly = typeof(GraphicsDevice).Assembly;
		using var stream = assembly.GetManifestResourceStream(path);
		return Shader.Create(
			this,
			stream,
			entryPointName,
			createInfoWithFormat
		);
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

			if (ShaderCross.Initialized)
			{
				ShaderCross.Quit();
			}

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
