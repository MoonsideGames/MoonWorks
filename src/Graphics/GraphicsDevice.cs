using System;
using System.Collections.Generic;
using System.IO;
using RefreshCS;

namespace MoonWorks.Graphics
{
	public class GraphicsDevice : IDisposable
	{
		public IntPtr Handle { get; }

		// Built-in video pipeline
		private ShaderModule VideoVertexShader { get; }
		private ShaderModule VideoFragmentShader { get; }
		internal GraphicsPipeline VideoPipeline { get; }

		public bool IsDisposed { get; private set; }

		private readonly List<WeakReference<GraphicsResource>> resources = new List<WeakReference<GraphicsResource>>();

		public GraphicsDevice(
			IntPtr deviceWindowHandle,
			Refresh.PresentMode presentMode,
			bool debugMode
		)
		{
			var presentationParameters = new Refresh.PresentationParameters
			{
				deviceWindowHandle = deviceWindowHandle,
				presentMode = presentMode
			};

			Handle = Refresh.Refresh_CreateDevice(
				presentationParameters,
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

		public CommandBuffer AcquireCommandBuffer()
		{
			return new CommandBuffer(this, Refresh.Refresh_AcquireCommandBuffer(Handle, 0));
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

		public TextureFormat GetSwapchainFormat(Window window)
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
