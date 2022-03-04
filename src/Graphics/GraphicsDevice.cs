using System;
using System.Collections.Generic;
using RefreshCS;

namespace MoonWorks.Graphics
{
	public class GraphicsDevice : IDisposable
	{
		public IntPtr Handle { get; }

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
