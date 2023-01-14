using System;
using System.Collections.Generic;
using System.IO;
using RefreshCS;

namespace MoonWorks.Graphics
{
	public class GraphicsDevice : IDisposable
	{
		public IntPtr Handle { get; }
		public Backend Backend { get; }

		private uint windowFlags;
		public SDL2.SDL.SDL_WindowFlags WindowFlags => (SDL2.SDL.SDL_WindowFlags) windowFlags;

		// Built-in video pipeline
		private ShaderModule VideoVertexShader { get; }
		private ShaderModule VideoFragmentShader { get; }
		internal GraphicsPipeline VideoPipeline { get; }

		public bool IsDisposed { get; private set; }

		private readonly List<WeakReference<GraphicsResource>> resources = new List<WeakReference<GraphicsResource>>();

		public GraphicsDevice(
			Backend preferredBackend,
			bool debugMode
		)
		{
			Backend = (Backend) Refresh.Refresh_SelectBackend((Refresh.Backend) preferredBackend, out windowFlags);

			if (Backend == Backend.Invalid)
			{
				throw new System.Exception("Could not set graphics backend!");
			}

			Handle = Refresh.Refresh_CreateDevice(
				Conversions.BoolToByte(debugMode)
			);

			VideoVertexShader = new ShaderModule(this, GetEmbeddedResource("MoonWorks.Shaders.FullscreenVert.spv"));
			VideoFragmentShader = new ShaderModule(this, GetEmbeddedResource("MoonWorks.Shaders.YUV2RGBAFrag.spv"));

			VideoPipeline = new GraphicsPipeline(
				this,
				new GraphicsPipelineCreateInfo
				{
					AttachmentInfo = new GraphicsPipelineAttachmentInfo(
						new ColorAttachmentDescription(TextureFormat.R8G8B8A8, ColorAttachmentBlendState.None)
					),
					DepthStencilState = DepthStencilState.Disable,
					VertexShaderInfo = GraphicsShaderInfo.Create(VideoVertexShader, "main", 0),
					FragmentShaderInfo = GraphicsShaderInfo.Create(VideoFragmentShader, "main", 3),
					VertexInputState = VertexInputState.Empty,
					RasterizerState = RasterizerState.CCW_CullNone,
					PrimitiveType = PrimitiveType.TriangleList,
					MultisampleState = MultisampleState.None
				}
			);
		}

		public bool ClaimWindow(Window window, PresentMode presentMode)
		{
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
					window.SwapchainTexture = new Texture(this, IntPtr.Zero, window.SwapchainFormat, 0, 0);
				}
			}

			return success;
		}

		public void UnclaimWindow(Window window)
		{
			Refresh.Refresh_UnclaimWindow(
				Handle,
				window.Handle
			);
			window.Claimed = false;
		}

		public void SetPresentMode(Window window, PresentMode presentMode)
		{
			Refresh.Refresh_SetSwapchainPresentMode(
				Handle,
				window.Handle,
				(Refresh.PresentMode) presentMode
			);
		}

		public CommandBuffer AcquireCommandBuffer()
		{
			return new CommandBuffer(this, Refresh.Refresh_AcquireCommandBuffer(Handle));
		}

		public unsafe void Submit(CommandBuffer commandBuffer)
		{
			var commandBufferPtrs = stackalloc IntPtr[1];

			commandBufferPtrs[0] = commandBuffer.Handle;

			Refresh.Refresh_Submit(
				Handle,
				1,
				(IntPtr) commandBufferPtrs
			);
		}

		public unsafe void Submit(
			CommandBuffer commandBufferOne,
			CommandBuffer commandBufferTwo
		) {
			var commandBufferPtrs = stackalloc IntPtr[2];

			commandBufferPtrs[0] = commandBufferOne.Handle;
			commandBufferPtrs[1] = commandBufferTwo.Handle;

			Refresh.Refresh_Submit(
				Handle,
				2,
				(IntPtr) commandBufferPtrs
			);
		}

		public unsafe void Submit(
			CommandBuffer commandBufferOne,
			CommandBuffer commandBufferTwo,
			CommandBuffer commandBufferThree
		) {
			var commandBufferPtrs = stackalloc IntPtr[3];

			commandBufferPtrs[0] = commandBufferOne.Handle;
			commandBufferPtrs[1] = commandBufferTwo.Handle;
			commandBufferPtrs[2] = commandBufferThree.Handle;

			Refresh.Refresh_Submit(
				Handle,
				3,
				(IntPtr) commandBufferPtrs
			);
		}

		public unsafe void Submit(
			CommandBuffer commandBufferOne,
			CommandBuffer commandBufferTwo,
			CommandBuffer commandBufferThree,
			CommandBuffer commandBufferFour
		) {
			var commandBufferPtrs = stackalloc IntPtr[4];

			commandBufferPtrs[0] = commandBufferOne.Handle;
			commandBufferPtrs[1] = commandBufferTwo.Handle;
			commandBufferPtrs[2] = commandBufferThree.Handle;
			commandBufferPtrs[3] = commandBufferFour.Handle;

			Refresh.Refresh_Submit(
				Handle,
				4,
				(IntPtr) commandBufferPtrs
			);
		}

		public unsafe void Submit(params CommandBuffer[] commandBuffers)
		{
			var commandBufferPtrs = stackalloc IntPtr[commandBuffers.Length];

			for (var i = 0; i < commandBuffers.Length; i += 1)
			{
				commandBufferPtrs[i] = commandBuffers[i].Handle;
			}

			Refresh.Refresh_Submit(
				Handle,
				(uint) commandBuffers.Length,
				(IntPtr) commandBufferPtrs
			);
		}

		public void Wait()
		{
			Refresh.Refresh_Wait(Handle);
		}

		private TextureFormat GetSwapchainFormat(Window window)
		{
			return (TextureFormat) Refresh.Refresh_GetSwapchainFormat(Handle, window.Handle);
		}

		internal void AddResourceReference(WeakReference<GraphicsResource> resourceReference)
		{
			lock (resources)
			{
				resources.Add(resourceReference);
			}
		}

		internal void RemoveResourceReference(WeakReference<GraphicsResource> resourceReference)
		{
			lock (resources)
			{
				resources.Remove(resourceReference);
			}
		}

		private static Stream GetEmbeddedResource(string name)
		{
			return typeof(GraphicsDevice).Assembly.GetManifestResourceStream(name);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					lock (resources)
					{
						for (var i = resources.Count - 1; i >= 0; i--)
						{
							var resource = resources[i];
							if (resource.TryGetTarget(out var target))
							{
								target.Dispose();
							}
						}
						resources.Clear();
					}

					Refresh.Refresh_DestroyDevice(Handle);
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
}
