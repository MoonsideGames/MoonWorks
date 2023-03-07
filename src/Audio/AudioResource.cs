using System;

namespace MoonWorks.Audio
{
	public abstract class AudioResource : IDisposable
	{
		public AudioDevice Device { get; }

		public bool IsDisposed { get; private set; }

		internal WeakReference weakReference;

		public AudioResource(AudioDevice device)
		{
			Device = device;

			weakReference = new WeakReference(this);
			Device.AddResourceReference(this);
		}

		protected abstract void Destroy();

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Destroy();

				if (weakReference != null)
				{
					Device.RemoveResourceReference(this);
					weakReference = null;
				}

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
