using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MoonWorks.Video;
using SDL2;
using SDL2_gpuCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// GraphicsDevice manages all graphics-related concerns.
	/// </summary>
	public class GraphicsDevice : IDisposable
	{
		public IntPtr Handle { get; }
		public BackendFlags Backend { get; }
		public bool DebugMode { get; }

		private uint windowFlags;
		public SDL2.SDL.SDL_WindowFlags WindowFlags => (SDL2.SDL.SDL_WindowFlags) windowFlags;

		// Built-in video pipeline
		internal GraphicsPipeline VideoPipeline { get; }

		// Built-in text shader info
		public Shader TextVertexShader;
		public Shader TextFragmentShader;
		public GraphicsPipelineResourceInfo TextVertexShaderInfo { get; }
		public GraphicsPipelineResourceInfo TextFragmentShaderInfo { get; }
		public VertexInputState TextVertexInputState { get; }

		// Built-in samplers
		public Sampler PointSampler { get; }
		public Sampler LinearSampler { get; }

		public bool IsDisposed { get; private set; }

		private readonly HashSet<GCHandle> resources = new HashSet<GCHandle>();
		private CommandBufferPool CommandBufferPool;
		private FencePool FencePool;
		internal RenderPassPool RenderPassPool;
		internal ComputePassPool ComputePassPool;

		internal unsafe GraphicsDevice(
			BackendFlags preferredBackends,
			bool debugMode
		) {
			if (preferredBackends == BackendFlags.Invalid)
			{
				throw new System.Exception("Could not set graphics backend!");
			}

			Handle = SDL_Gpu.SDL_GpuCreateDevice(
				(SDL_Gpu.BackendFlags) preferredBackends,
				Conversions.BoolToByte(debugMode)
			);

			DebugMode = debugMode;
			// TODO: check for CreateDevice fail

			Backend = (BackendFlags) SDL_Gpu.SDL_GpuGetBackend(Handle);

			// Check for replacement stock shaders
			string basePath = System.AppContext.BaseDirectory;

			string fullscreenVertPath = Path.Combine(basePath, "fullscreen.vert.refresh");

			string textVertPath = Path.Combine(basePath, "text_transform.vert.refresh");
			string textFragPath = Path.Combine(basePath, "text_msdf.frag.refresh");

			string videoFragPath = Path.Combine(basePath, "video_yuv2rgba.frag.refresh");
			string blitFragPath = Path.Combine(basePath, "blit.frag.refresh");

			Shader fullscreenVertShader;

			Shader textVertShader;
			Shader textFragShader;

			Shader videoFragShader;
			Shader blitFragShader;

			if (File.Exists(fullscreenVertPath))
			{
				fullscreenVertShader = new Shader(
					this,
					fullscreenVertPath,
					"main",
					ShaderStage.Vertex,
					ShaderFormat.SECRET
				);
			}
			else
			{
				// use defaults
				var assembly = typeof(GraphicsDevice).Assembly;
				using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.Fullscreen.vert.refresh");
				fullscreenVertShader = new Shader(
					this,
					vertStream,
					"main",
					ShaderStage.Vertex,
					ShaderFormat.SPIRV
				);
			}

			if (File.Exists(videoFragPath))
			{
				videoFragShader = new Shader(
					this,
					videoFragPath,
					"main",
					ShaderStage.Fragment,
					ShaderFormat.SECRET
				);
			}
			else
			{
				// use defaults
				var assembly = typeof(GraphicsDevice).Assembly;
				using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.VideoYUV2RGBA.frag.refresh");
				videoFragShader = new Shader(
					this,
					fragStream,
					"main",
					ShaderStage.Fragment,
					ShaderFormat.SPIRV
				);
			}

			if (File.Exists(textVertPath) && File.Exists(textFragPath))
			{
				textVertShader = new Shader(
					this,
					textVertPath,
					"main",
					ShaderStage.Vertex,
					ShaderFormat.SECRET
				);

				textFragShader = new Shader(
					this,
					textFragPath,
					"main",
					ShaderStage.Fragment,
					ShaderFormat.SECRET
				);
			}
			else
			{
				// use defaults
				var assembly = typeof(GraphicsDevice).Assembly;

				using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextTransform.vert.refresh");
				using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextMSDF.frag.refresh");

				textVertShader = new Shader(
					this,
					vertStream,
					"main",
					ShaderStage.Fragment,
					ShaderFormat.SPIRV
				);

				textFragShader = new Shader(
					this,
					fragStream,
					"main",
					ShaderStage.Fragment,
					ShaderFormat.SPIRV
				);
			}

			if (File.Exists(blitFragPath))
			{
				blitFragShader = new Shader(
					this,
					blitFragPath,
					"main",
					ShaderStage.Fragment,
					ShaderFormat.SECRET
				);
			}
			else
			{
				// use defaults
				var assembly = typeof(GraphicsDevice).Assembly;

				using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.Blit.frag.refresh");
				blitFragShader = new Shader(
					this,
					fragStream,
					"main",
					ShaderStage.Fragment,
					ShaderFormat.SPIRV
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
					FragmentShaderResourceInfo = new GraphicsPipelineResourceInfo
					{
						SamplerCount = 3
					},
					VertexInputState = VertexInputState.Empty,
					RasterizerState = RasterizerState.CCW_CullNone,
					PrimitiveType = PrimitiveType.TriangleList,
					MultisampleState = MultisampleState.None
				}
			);

			TextVertexShader = textVertShader;
			TextVertexShaderInfo = new GraphicsPipelineResourceInfo();

			TextFragmentShader = textFragShader;
			TextFragmentShaderInfo = new GraphicsPipelineResourceInfo
			{
				SamplerCount = 1
			};

			TextVertexInputState = VertexInputState.CreateSingleBinding<Font.Vertex>();

			PointSampler = new Sampler(this, SamplerCreateInfo.PointClamp);
			LinearSampler = new Sampler(this, SamplerCreateInfo.LinearClamp);

			FencePool = new FencePool(this);
			CommandBufferPool = new CommandBufferPool(this);
		}

		/// <summary>
		/// Prepares a window so that frames can be presented to it.
		/// </summary>
		/// <param name="swapchainComposition">The desired composition of the swapchain. Ignore this unless you are using HDR or tonemapping.</param>
		/// <param name="presentMode">The desired presentation mode for the window. Roughly equivalent to V-Sync.</param>
		/// <returns>True if successfully claimed.</returns>
		public bool ClaimWindow(
			Window window,
			SwapchainComposition swapchainComposition,
			PresentMode presentMode
		) {
			if (window.Claimed)
			{
				Logger.LogError("Window already claimed!");
				return false;
			}

			var success = Conversions.IntToBool(
				SDL_Gpu.SDL_GpuClaimWindow(
					Handle,
					window.Handle,
					(SDL_Gpu.SwapchainComposition) swapchainComposition,
					(SDL_Gpu.PresentMode) presentMode
				)
			);

			if (success)
			{
				window.Claimed = true;
				window.SwapchainComposition = swapchainComposition;
				window.SwapchainFormat = GetSwapchainFormat(window);

				if (window.SwapchainTexture == null)
				{
					window.SwapchainTexture = new Texture(this, window.SwapchainFormat);
				}
			}

			return success;
		}

		/// <summary>
		/// Unclaims a window, making it unavailable for presenting and freeing associated resources.
		/// </summary>
		public void UnclaimWindow(Window window)
		{
			if (window.Claimed)
			{
				SDL_Gpu.SDL_GpuUnclaimWindow(
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
		/// <param name="window"></param>
		/// <param name="presentMode"></param>
		public void SetSwapchainParameters(
			Window window,
			SwapchainComposition swapchainComposition,
			PresentMode presentMode
		) {
			if (!window.Claimed)
			{
				Logger.LogError("Cannot set present mode on unclaimed window!");
				return;
			}

			SDL_Gpu.SDL_GpuSetSwapchainParameters(
				Handle,
				window.Handle,
				(SDL_Gpu.SwapchainComposition) swapchainComposition,
				(SDL_Gpu.PresentMode) presentMode
			);
		}

		/// <summary>
		/// Acquires a command buffer.
		/// This is the start of your rendering process.
		/// </summary>
		/// <returns></returns>
		public CommandBuffer AcquireCommandBuffer()
		{
			var commandBuffer = CommandBufferPool.Obtain();
			commandBuffer.SetHandle(SDL_Gpu.SDL_GpuAcquireCommandBuffer(Handle));
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

			SDL_Gpu.SDL_GpuSubmit(
				commandBuffer.Handle
			);

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
			var fenceHandle = SDL_Gpu.SDL_GpuSubmitAndAcquireFence(
				commandBuffer.Handle
			);

			var fence = FencePool.Obtain();
			fence.SetHandle(fenceHandle);

			return fence;
		}

		/// <summary>
		/// Wait for the graphics device to become idle.
		/// </summary>
		public void Wait()
		{
			SDL_Gpu.SDL_GpuWait(Handle);
		}

		/// <summary>
		/// Waits for the given fence to become signaled.
		/// </summary>
		public unsafe void WaitForFences(Fence fence)
		{
			var fenceHandle = fence.Handle;

			SDL_Gpu.SDL_GpuWaitForFences(
				Handle,
				1,
				1,
				&fenceHandle
			);
		}

		/// <summary>
		/// Wait for one or more fences to become signaled.
		/// </summary>
		/// <param name="waitAll">If true, will wait for all given fences to be signaled.</param>
		public unsafe void WaitForFences(
			Fence fenceOne,
			Fence fenceTwo,
			bool waitAll
		) {
			var handlePtr = stackalloc nint[2];
			handlePtr[0] = fenceOne.Handle;
			handlePtr[1] = fenceTwo.Handle;

			SDL_Gpu.SDL_GpuWaitForFences(
				Handle,
				Conversions.BoolToInt(waitAll),
				2,
				handlePtr
			);
		}

		/// <summary>
		/// Wait for one or more fences to become signaled.
		/// </summary>
		/// <param name="waitAll">If true, will wait for all given fences to be signaled.</param>
		public unsafe void WaitForFences(
			Fence fenceOne,
			Fence fenceTwo,
			Fence fenceThree,
			bool waitAll
		) {
			var handlePtr = stackalloc nint[3];
			handlePtr[0] = fenceOne.Handle;
			handlePtr[1] = fenceTwo.Handle;
			handlePtr[2] = fenceThree.Handle;

			SDL_Gpu.SDL_GpuWaitForFences(
				Handle,
				Conversions.BoolToInt(waitAll),
				3,
				handlePtr
			);
		}

		/// <summary>
		/// Wait for one or more fences to become signaled.
		/// </summary>
		/// <param name="waitAll">If true, will wait for all given fences to be signaled.</param>
		public unsafe void WaitForFences(
			Fence fenceOne,
			Fence fenceTwo,
			Fence fenceThree,
			Fence fenceFour,
			bool waitAll
		) {
			var handlePtr = stackalloc nint[4];
			handlePtr[0] = fenceOne.Handle;
			handlePtr[1] = fenceTwo.Handle;
			handlePtr[2] = fenceThree.Handle;
			handlePtr[3] = fenceFour.Handle;

			SDL_Gpu.SDL_GpuWaitForFences(
				Handle,
				Conversions.BoolToInt(waitAll),
				4,
				handlePtr
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

			SDL_Gpu.SDL_GpuWaitForFences(
				Handle,
				Conversions.BoolToInt(waitAll),
				4,
				handlePtr
			);
		}

		/// <summary>
		/// Returns true if the fence is signaled, indicating that the associated command buffer has finished processing.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if the fence query indicates that the graphics device has been lost.</exception>
		public bool QueryFence(Fence fence)
		{
			var result = SDL_Gpu.SDL_GpuQueryFence(Handle, fence.Handle);

			if (result < 0)
			{
				throw new InvalidOperationException("The graphics device has been lost.");
			}

			return result != 0;
		}

		/// <summary>
		/// Release reference to an acquired fence, enabling it to be reused.
		/// </summary>
		public void ReleaseFence(Fence fence)
		{
			SDL_Gpu.SDL_GpuReleaseFence(Handle, fence.Handle);
			fence.Handle = IntPtr.Zero;
			FencePool.Return(fence);
		}

		private TextureFormat GetSwapchainFormat(Window window)
		{
			if (!window.Claimed)
			{
				throw new System.ArgumentException("Cannot get swapchain format of unclaimed window!");
			}

			return (TextureFormat) SDL_Gpu.SDL_GpuGetSwapchainTextureFormat(Handle, window.Handle);
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

				SDL_Gpu.SDL_GpuDestroyDevice(Handle);

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
}
