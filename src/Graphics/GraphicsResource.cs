using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	// TODO: give this a Name property for debugging use
	public abstract class GraphicsResource : IDisposable
	{
		public GraphicsDevice Device { get; }

		private GCHandle SelfReference;

		public bool IsDisposed { get; private set; }

		protected GraphicsResource(GraphicsDevice device)
		{
			Device = device;

			SelfReference = GCHandle.Alloc(this, GCHandleType.Weak);
			Device.AddResourceReference(SelfReference);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Device.RemoveResourceReference(SelfReference);
					SelfReference.Free();
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
