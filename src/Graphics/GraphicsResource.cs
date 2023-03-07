using System;

namespace MoonWorks.Graphics
{
	public abstract class GraphicsResource : IDisposable
	{
		public GraphicsDevice Device { get; }
		public IntPtr Handle { get; internal set; }

		public bool IsDisposed { get; private set; }
		protected abstract Action<IntPtr, IntPtr> QueueDestroyFunction { get; }

		internal WeakReference<GraphicsResource> weakReference;

		public GraphicsResource(GraphicsDevice device, bool trackResource = true)
		{
			Device = device;

			if (trackResource)
			{
				weakReference = new WeakReference<GraphicsResource>(this);
				Device.AddResourceReference(weakReference);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (weakReference != null)
				{
					QueueDestroyFunction(Device.Handle, Handle);
					Device.RemoveResourceReference(weakReference);
					weakReference = null;
				}

				IsDisposed = true;
			}
		}

		~GraphicsResource()
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
