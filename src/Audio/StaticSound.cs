using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
    public class StaticSound : AudioResource
    {
        internal FAudio.FAudioBuffer Handle;
        public ushort Channels { get; }
        public uint SamplesPerSecond { get; }

        public uint LoopStart { get; set; } = 0;
        public uint LoopLength { get; set; } = 0;

        public static StaticSound LoadOgg(AudioDevice device, string filePath)
        {
            var filePointer = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

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
                (ushort) info.channels,
                info.sample_rate,
                buffer,
                0,
                (uint) buffer.Length
            );
        }

        public StaticSound(
            AudioDevice device,
            ushort channels,
            uint samplesPerSecond,
            float[] buffer,
            uint bufferOffset, /* in floats */
            uint bufferLength  /* in floats */
        ) : base(device)
        {
            Channels = channels;
            SamplesPerSecond = samplesPerSecond;

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

        protected override void Destroy()
        {
            Marshal.FreeHGlobal(Handle.pAudioData);
        }
    }
}
