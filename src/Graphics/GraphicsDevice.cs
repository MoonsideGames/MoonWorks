using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MoonWorks.Video;
using RefreshCS;

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

		// Built-in video pipeline
		internal GraphicsPipeline VideoPipeline { get; }

		// Built-in text shader info
		public Shader TextVertexShader;
		public Shader TextFragmentShader;
		public VertexInputState TextVertexInputState { get; }

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
			BackendFlags preferredBackends,
			bool debugMode
		) {
			if (preferredBackends == BackendFlags.Invalid)
			{
				throw new System.Exception("Could not set graphics backend!");
			}

			Handle = Refresh.Refresh_CreateDevice(
				(Refresh.BackendFlags) preferredBackends,
				Conversions.BoolToInt(debugMode)
			);

			DebugMode = debugMode;
			// TODO: check for CreateDevice fail

			Backend = (BackendFlags) Refresh.Refresh_GetBackend(Handle);

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
				Refresh.Refresh_ClaimWindow(
					Handle,
					window.Handle,
					(Refresh.SwapchainComposition) swapchainComposition,
					(Refresh.PresentMode) presentMode
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
				Refresh.Refresh_UnclaimWindow(
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

			Refresh.Refresh_SetSwapchainParameters(
				Handle,
				window.Handle,
				(Refresh.SwapchainComposition) swapchainComposition,
				(Refresh.PresentMode) presentMode
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
			commandBuffer.SetHandle(Refresh.Refresh_AcquireCommandBuffer(Handle));
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

			Refresh.Refresh_Submit(
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
			var fenceHandle = Refresh.Refresh_SubmitAndAcquireFence(
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
			Refresh.Refresh_Wait(Handle);
		}

		/// <summary>
		/// Waits for the given fence to become signaled.
		/// </summary>
		public unsafe void WaitForFence(Fence fence)
		{
			var fenceHandle = fence.Handle;

			Refresh.Refresh_WaitForFences(
				Handle,
				1,
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

			Refresh.Refresh_WaitForFences(
				Handle,
				Conversions.BoolToInt(waitAll),
				handlePtr,
				(uint) fences.Length
			);
		}

		/// <summary>
		/// Returns true if the fence is signaled, indicating that the associated command buffer has finished processing.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if the fence query indicates that the graphics device has been lost.</exception>
		public bool QueryFence(Fence fence)
		{
			var result = Refresh.Refresh_QueryFence(Handle, fence.Handle);

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
			Refresh.Refresh_ReleaseFence(Handle, fence.Handle);
			fence.Handle = IntPtr.Zero;
			FencePool.Return(fence);
		}

		private TextureFormat GetSwapchainFormat(Window window)
		{
			if (!window.Claimed)
			{
				throw new System.ArgumentException("Cannot get swapchain format of unclaimed window!");
			}

			return (TextureFormat) Refresh.Refresh_GetSwapchainTextureFormat(Handle, window.Handle);
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

				Refresh.Refresh_DestroyDevice(Handle);

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
