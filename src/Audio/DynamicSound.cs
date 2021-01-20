using System;
using System.IO;

namespace MoonWorks.Audio
{
    /// <summary>
    /// For streaming long playback.
    /// </summary>
    public class DynamicSound : Sound, IDisposable
    {
        public const int BUFFER_SIZE = 1024 * 128;

        internal IntPtr FileHandle { get; }
        internal FAudio.stb_vorbis_info Info { get; }

        private bool IsDisposed;

        // FIXME: what should this value be?

        public DynamicSound(FileInfo fileInfo, ushort channels, uint samplesPerSecond) : base(channels, samplesPerSecond)
        {
            FileHandle = FAudio.stb_vorbis_open_filename(fileInfo.FullName, out var error, IntPtr.Zero);

            if (error != 0)
            {
                Logger.LogError("Error opening OGG file!");
                throw new AudioLoadException("Error opening OGG file!");
            }

            Info = FAudio.stb_vorbis_get_info(FileHandle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                FAudio.stb_vorbis_close(FileHandle);
                IsDisposed = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~DynamicSound()
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
