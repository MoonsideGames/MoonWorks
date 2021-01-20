using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
    public class StaticSound : Sound, IDisposable
    {
        internal override FAudio.FAudioWaveFormatEx Format { get; }
        internal FAudio.FAudioBuffer Handle;

        public uint LoopStart { get; set; } = 0;
        public uint LoopLength { get; set; } = 0;

        private bool IsDisposed;

        public static StaticSound LoadOgg(AudioDevice device, FileInfo fileInfo)
        {
            var filePointer = FAudio.stb_vorbis_open_filename(fileInfo.FullName, out var error, IntPtr.Zero);

            if (error != 0)
            {
                throw new AudioLoadException("Error loading file!");
            }
            var info = FAudio.stb_vorbis_get_info(filePointer);
            var bufferSize = FAudio.stb_vorbis_stream_length_in_samples(filePointer) * info.channels;
            var buffer = new float[bufferSize];

            FAudio.stb_vorbis_get_samples_float_interleaved(
                filePointer,
                info.channels,
                buffer,
                (int) bufferSize
            );

            FAudio.stb_vorbis_close(filePointer);

            return new StaticSound(
                device,
                buffer,
                0,
                (uint) buffer.Length,
                (ushort) info.channels,
                info.sample_rate
            );
        }

        public StaticSound(
            AudioDevice device,
            float[] buffer,
            uint bufferOffset, /* in floats */
            uint bufferLength, /* in floats */
            ushort channels,
            uint samplesPerSecond
        ) : base(device) {
            var blockAlign = (ushort)(4 * channels);

            Format = new FAudio.FAudioWaveFormatEx
            {
                wFormatTag = 3,
                wBitsPerSample = 32,
                nChannels = channels,
                nBlockAlign = blockAlign,
                nSamplesPerSec = samplesPerSecond,
                nAvgBytesPerSec = blockAlign * samplesPerSecond
            };

            var bufferLengthInBytes = (int) (bufferLength * sizeof(float));
            Handle = new FAudio.FAudioBuffer();
            Handle.Flags = FAudio.FAUDIO_END_OF_STREAM;
            Handle.pContext = IntPtr.Zero;
            Handle.AudioBytes = (uint) bufferLengthInBytes;
            Handle.pAudioData = Marshal.AllocHGlobal(bufferLengthInBytes);
            Marshal.Copy(buffer, (int) bufferOffset, Handle.pAudioData, (int) bufferLength);
            Handle.PlayBegin = 0;
            Handle.PlayLength = 0;

            LoopStart = 0;
            LoopLength = 0;
        }

        public StaticSoundInstance CreateInstance(bool loop = false)
        {
            return new StaticSoundInstance(Device, this, false, loop);
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
