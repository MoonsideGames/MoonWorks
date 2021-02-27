using System;

namespace MoonWorks.Audio
{
    public abstract class AudioResource : IDisposable
    {
        public AudioDevice Device { get; }

        public bool IsDisposed { get; private set; }

        private WeakReference<AudioResource> selfReference;

        public AudioResource(AudioDevice device)
        {
            Device = device;

            selfReference = new WeakReference<AudioResource>(this);
            Device.AddResourceReference(selfReference);
        }

        protected abstract void Destroy();

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Destroy();

                if (selfReference != null)
                {
                    Device.RemoveResourceReference(selfReference);
                    selfReference = null;
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
