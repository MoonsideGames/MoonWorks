using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MoonWorks.Graphics
{
	// TODO: give this a Name property for debugging use
	public abstract class GraphicsResource : IDisposable
	{
		public GraphicsDevice Device { get; }
		public IntPtr Handle { get => handle; internal set => handle = value; }
		private nint handle;

		public bool IsDisposed { get; private set; }
		protected abstract Action<IntPtr, IntPtr> QueueDestroyFunction { get; }

		private GCHandle SelfReference;
		protected GraphicsResource(GraphicsDevice device)
		{
			Device = device;

			SelfReference = GCHandle.Alloc(this, GCHandleType.Weak);
			Device.AddResourceReference(SelfReference);
		}

		protected void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Device.RemoveResourceReference(SelfReference);
					SelfReference.Free();
				}

				// Atomically call destroy function in case this is called from the finalizer thread
				var toDispose = Interlocked.Exchange(ref handle, IntPtr.Zero);
				if (toDispose != IntPtr.Zero)
				{
					QueueDestroyFunction(Device.Handle, toDispose);
				}

				IsDisposed = true;
			}
		}

		~GraphicsResource()
		{
			#if DEBUG
			// If you see this log message, you leaked a graphics resource without disposing it!
			// We'll try to clean it up for you but you really should fix this.
			Logger.LogWarn($"A resource of type {GetType().Name} was not Disposed.");
			#endif

			Dispose(false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
