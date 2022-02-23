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
		private Dictionary<IntPtr, Action<IntPtr, IntPtr, IntPtr>> resourcesToDestroy = new Dictionary<IntPtr, Action<IntPtr, IntPtr, IntPtr>>();

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

		internal void SubmitDestroyCommandBuffer()
		{
			if (resourcesToDestroy.Count > 0)
			{
				var commandBuffer = AcquireCommandBuffer();

				foreach (var kv in resourcesToDestroy)
				{
					kv.Value.Invoke(Handle, commandBuffer.Handle, kv.Key);
				}

				Submit(commandBuffer);
			}
		}

		internal void PrepareDestroyResource(GraphicsResource resource, Action<IntPtr, IntPtr, IntPtr> destroyFunction)
		{
			resourcesToDestroy.Add(resource.Handle, destroyFunction);
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

					SubmitDestroyCommandBuffer();
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
