using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public abstract class AudioResource : IDisposable
	{
		public AudioDevice Device { get; }

		public bool IsDisposed { get; private set; }

		private GCHandle SelfReference;

		protected AudioResource(AudioDevice device)
		{
			Device = device;

			SelfReference = GCHandle.Alloc(this, GCHandleType.Weak);
			Device.AddResourceReference(SelfReference);
		}

		protected virtual void DisposeManagedState() { }
		protected virtual void DisposeUnmanagedState() { }

		protected void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					DisposeManagedState();

					Device.RemoveResourceReference(SelfReference);
					SelfReference.Free();
				}

				DisposeUnmanagedState();

				IsDisposed = true;
			}
		}

		~AudioResource()
		{
			#if DEBUG
			// If the graphics device associated with this resource was already disposed, we assume
			// that your game is in the middle of shutting down.
			if (!IsDisposed && Device != null && !Device.IsDisposed)
			{
				// If you see this log message, you leaked a graphics resource without disposing it!
				// This means your game may eventually run out of native memory for mysterious reasons.
				Logger.LogWarn($"A resource of type {GetType().Name} was not Disposed.");
			}
			#endif

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
