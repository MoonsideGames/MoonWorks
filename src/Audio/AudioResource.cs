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

		protected abstract void Destroy();

		protected void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Destroy();

				Device.RemoveResourceReference(SelfReference);
				SelfReference.Free();

				IsDisposed = true;
			}
		}

		~AudioResource()
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
