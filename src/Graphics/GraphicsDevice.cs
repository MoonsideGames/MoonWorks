using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MoonWorks.Video;
using RefreshCS;
using WellspringCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// GraphicsDevice manages all graphics-related concerns.
	/// </summary>
	public class GraphicsDevice : IDisposable
	{
		public IntPtr Handle { get; }
		public Backend Backend { get; }

		private uint windowFlags;
		public SDL2.SDL.SDL_WindowFlags WindowFlags => (SDL2.SDL.SDL_WindowFlags) windowFlags;

		// Built-in video pipeline
		internal GraphicsPipeline VideoPipeline { get; }

		// Built-in text shader info
		public GraphicsShaderInfo TextVertexShaderInfo { get; }
		public GraphicsShaderInfo TextFragmentShaderInfo { get; }
		public VertexInputState TextVertexInputState { get; }

		// Built-in samplers
		public Sampler PointSampler { get; }
		public Sampler LinearSampler { get; }

		public bool IsDisposed { get; private set; }

		private readonly HashSet<GCHandle> resources = new HashSet<GCHandle>();
		private FencePool FencePool;

		internal GraphicsDevice(
			Backend preferredBackend,
			bool debugMode
		) {
			Backend = (Backend) Refresh.Refresh_SelectBackend((Refresh.Backend) preferredBackend, out windowFlags);

			if (Backend == Backend.Invalid)
			{
				throw new System.Exception("Could not set graphics backend!");
			}

			Handle = Refresh.Refresh_CreateDevice(
				Conversions.BoolToByte(debugMode)
			);

			// TODO: check for CreateDevice fail

			// Check for replacement stock shaders
			string basePath = System.AppContext.BaseDirectory;

			string videoVertPath = Path.Combine(basePath, "video_fullscreen.vert.refresh");
			string videoFragPath = Path.Combine(basePath, "video_yuv2rgba.frag.refresh");

			string textVertPath = Path.Combine(basePath, "text_transform.vert.refresh");
			string textFragPath = Path.Combine(basePath, "text_msdf.frag.refresh");

			ShaderModule videoVertShader;
			ShaderModule videoFragShader;

			ShaderModule textVertShader;
			ShaderModule textFragShader;

			if (File.Exists(videoVertPath) && File.Exists(videoFragPath))
			{
				videoVertShader = new ShaderModule(this, videoVertPath);
				videoFragShader = new ShaderModule(this, videoFragPath);
			}
			else
			{
				// use defaults
				var assembly = typeof(GraphicsDevice).Assembly;

				using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.VideoFullscreen.vert.refresh");
				using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.VideoYUV2RGBA.frag.refresh");

				videoVertShader = new ShaderModule(this, vertStream);
				videoFragShader = new ShaderModule(this, fragStream);
			}

			if (File.Exists(textVertPath) && File.Exists(textFragPath))
			{
				textVertShader = new ShaderModule(this, textVertPath);
				textFragShader = new ShaderModule(this, textFragPath);
			}
			else
			{
				// use defaults
				var assembly = typeof(GraphicsDevice).Assembly;

				using var vertStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextTransform.vert.refresh");
				using var fragStream = assembly.GetManifestResourceStream("MoonWorks.Graphics.StockShaders.TextMSDF.frag.refresh");

				textVertShader = new ShaderModule(this, vertStream);
				textFragShader = new ShaderModule(this, fragStream);
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
					VertexShaderInfo = GraphicsShaderInfo.Create(
						videoVertShader,
						"main",
						0
					),
					FragmentShaderInfo = GraphicsShaderInfo.Create(
						videoFragShader,
						"main",
						3
					),
					VertexInputState = VertexInputState.Empty,
					RasterizerState = RasterizerState.CCW_CullNone,
					PrimitiveType = PrimitiveType.TriangleList,
					MultisampleState = MultisampleState.None
				}
			);

			TextVertexShaderInfo = GraphicsShaderInfo.Create<Math.Float.Matrix4x4>(textVertShader, "main", 0);
			TextFragmentShaderInfo = GraphicsShaderInfo.Create<float>(textFragShader, "main", 1);
			TextVertexInputState = VertexInputState.CreateSingleBinding<Font.Vertex>();

			PointSampler = new Sampler(this, SamplerCreateInfo.PointClamp);
			LinearSampler = new Sampler(this, SamplerCreateInfo.LinearClamp);

			FencePool = new FencePool(this);
		}

		/// <summary>
		/// Prepares a window so that frames can be presented to it.
		/// </summary>
		/// <param name="presentMode">The desired presentation mode for the window. Roughly equivalent to V-Sync.</param>
		/// <returns>True if successfully claimed.</returns>
		public bool ClaimWindow(Window window, PresentMode presentMode)
		{
			if (window.Claimed)
			{
				Logger.LogError("Window already claimed!");
				return false;
			}

			var success = Conversions.ByteToBool(
				Refresh.Refresh_ClaimWindow(
					Handle,
					window.Handle,
					(Refresh.PresentMode) presentMode
				)
			);

			if (success)
			{
				window.Claimed = true;
				window.SwapchainFormat = GetSwapchainFormat(window);
				if (window.SwapchainTexture == null)
				{
					window.SwapchainTexture = new Texture(this);
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
		public void SetPresentMode(Window window, PresentMode presentMode)
		{
			if (!window.Claimed)
			{
				Logger.LogError("Cannot set present mode on unclaimed window!");
				return;
			}

			Refresh.Refresh_SetSwapchainPresentMode(
				Handle,
				window.Handle,
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
			return new CommandBuffer(this, Refresh.Refresh_AcquireCommandBuffer(Handle));
		}

		/// <summary>
		/// Submits a command buffer to the GPU for processing.
		/// </summary>
		public void Submit(CommandBuffer commandBuffer)
		{
			Refresh.Refresh_Submit(
				Handle,
				commandBuffer.Handle
			);
		}

		/// <summary>
		/// Submits a command buffer to the GPU for processing and acquires a fence associated with the submission.
		/// </summary>
		/// <returns></returns>
		public Fence SubmitAndAcquireFence(CommandBuffer commandBuffer)
		{
			var fenceHandle = Refresh.Refresh_SubmitAndAcquireFence(
				Handle,
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
		public unsafe void WaitForFences(Fence fence)
		{
			var handlePtr = stackalloc nint[1];
			handlePtr[0] = fence.Handle;

			Refresh.Refresh_WaitForFences(
				Handle,
				1,
				1,
				(nint) handlePtr
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

			Refresh.Refresh_WaitForFences(
				Handle,
				Conversions.BoolToByte(waitAll),
				2,
				(nint) handlePtr
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

			Refresh.Refresh_WaitForFences(
				Handle,
				Conversions.BoolToByte(waitAll),
				3,
				(nint) handlePtr
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

			Refresh.Refresh_WaitForFences(
				Handle,
				Conversions.BoolToByte(waitAll),
				4,
				(nint) handlePtr
			);
		}

		/// <summary>
		/// Wait for one or more fences to become signaled.
		/// </summary>
		/// <param name="waitAll">If true, will wait for all given fences to be signaled.</param>
		public unsafe void WaitForFences(Fence[] fences, bool waitAll)
		{
			var handlePtr = stackalloc nint[fences.Length];

			for (var i = 0; i < fences.Length; i += 1)
			{
				handlePtr[i] = fences[i].Handle;
			}

			Refresh.Refresh_WaitForFences(
				Handle,
				Conversions.BoolToByte(waitAll),
				4,
				(nint) handlePtr
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
			return (TextureFormat) Refresh.Refresh_GetSwapchainFormat(Handle, window.Handle);
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
