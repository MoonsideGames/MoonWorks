using System;

namespace MoonWorks.Graphics
{
	public abstract class GraphicsResource : IDisposable
	{
		public GraphicsDevice Device { get; }
		public IntPtr Handle { get; protected set; }

		public bool IsDisposed { get; private set; }
		protected abstract Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction { get; }

		private WeakReference<GraphicsResource> selfReference;

		public GraphicsResource(GraphicsDevice device, bool trackResource = true)
		{
			Device = device;

			if (trackResource)
			{
				selfReference = new WeakReference<GraphicsResource>(this);
				Device.AddResourceReference(selfReference);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Device.PrepareDestroyResource(this, QueueDestroyFunction);

				if (selfReference != null)
				{
					Device.RemoveResourceReference(selfReference);
					selfReference = null;
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
