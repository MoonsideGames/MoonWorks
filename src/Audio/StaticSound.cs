using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
    public class StaticSound : Sound, IDisposable
    {
        internal FAudio.FAudioBuffer Handle;
        private bool IsDisposed;

        public uint LoopStart { get; set; } = 0;
        public uint LoopLength { get; set; } = 0;

        public static StaticSound FromOgg(FileInfo fileInfo)
        {
            var filePointer = FAudio.stb_vorbis_open_filename(fileInfo.FullName, out var error, IntPtr.Zero);

            if (error != 0)
            {
                throw new AudioLoadException("Error loading file!");
            }
            var info = FAudio.stb_vorbis_get_info(filePointer);
            var bufferSize =  (uint)(info.sample_rate * info.channels);
            var buffer = new float[bufferSize];

            FAudio.stb_vorbis_close(filePointer);

            return new StaticSound(
                buffer,
                0,
                (ushort) info.channels,
                info.sample_rate
            );
        }

        public StaticSound(
            float[] buffer,
            uint bufferOffset,
            ushort channels,
            uint samplesPerSecond
        ) : base(channels, samplesPerSecond) {
            var bufferLength = 4 * buffer.Length;

            Handle = new FAudio.FAudioBuffer();
            Handle.Flags = FAudio.FAUDIO_END_OF_STREAM;
            Handle.pContext = IntPtr.Zero;
            Handle.AudioBytes = (uint) bufferLength;
            Handle.pAudioData = Marshal.AllocHGlobal((int) bufferLength);
            Marshal.Copy(buffer, (int) bufferOffset, Handle.pAudioData, (int) bufferLength);
            Handle.PlayBegin = 0;
            Handle.PlayLength = (
                Handle.AudioBytes /
                (uint) Format.nChannels /
                (uint) (Format.wBitsPerSample / 8)
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                Marshal.FreeHGlobal(Handle.pAudioData);
                IsDisposed = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~StaticSound()
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
