using System;
using System.IO;

namespace MoonWorks.Audio
{
    public class StreamingSoundOgg : StreamingSound
    {
        // FIXME: what should this value be?
        public const int BUFFER_SIZE = 1024 * 128;

        internal IntPtr FileHandle { get; }
        internal FAudio.stb_vorbis_info Info { get; }

        private readonly float[] buffer;

        public override SoundState State { get; protected set; }

        private bool IsDisposed;

        public static StreamingSoundOgg Load(
            AudioDevice device,
            string filePath,
            bool is3D = false,
            bool loop = false
        ) {
            var fileHandle = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);
            if (error != 0)
            {
                Logger.LogError("Error opening OGG file!");
                throw new AudioLoadException("Error opening OGG file!");
            }

            var info = FAudio.stb_vorbis_get_info(fileHandle);

            return new StreamingSoundOgg(
                device,
                fileHandle,
                info,
                is3D,
                loop
            );
        }

        internal StreamingSoundOgg(
            AudioDevice device,
            IntPtr fileHandle,
            FAudio.stb_vorbis_info info,
            bool is3D,
            bool loop
        ) : base(device, (ushort) info.channels, info.sample_rate, is3D, loop)
        {
            FileHandle = fileHandle;
            Info = info;
            buffer = new float[BUFFER_SIZE];

            device.AddDynamicSoundInstance(this);
        }

        protected override void AddBuffer(
            out float[] buffer,
            out uint bufferOffset,
            out uint bufferLength,
            out bool reachedEnd
        ) {
            buffer = this.buffer;

            /* NOTE: this function returns samples per channel, not total samples */
            var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
                FileHandle,
                Info.channels,
                buffer,
                buffer.Length
            );

            var sampleCount = samples * Info.channels;
            bufferOffset = 0;
            bufferLength = (uint) sampleCount;
            reachedEnd = sampleCount < buffer.Length;
        }

        protected override void SeekStart()
        {
            FAudio.stb_vorbis_seek_start(FileHandle);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // dispose unmanaged state
                FAudio.stb_vorbis_close(FileHandle);

                IsDisposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
