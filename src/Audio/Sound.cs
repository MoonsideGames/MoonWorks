using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
    public class Sound
    {
        internal FAudio.FAudioBuffer Handle;
        internal FAudio.FAudioWaveFormatEx Format;

        public uint LoopStart { get; set; } = 0;
        public uint LoopLength { get; set; } = 0;

        public static Sound FromFile(FileInfo fileInfo)
        {
            var filePointer = FAudio.stb_vorbis_open_filename(fileInfo.FullName, out var error, IntPtr.Zero);

            if (error != 0)
            {
                throw new AudioLoadException("Error loading file!");
            }
            var info = FAudio.stb_vorbis_get_info(filePointer);
            var bufferSize =  (uint)(info.sample_rate * info.channels);
            var buffer = new float[bufferSize];
            var align = (ushort) (4 * info.channels);

            FAudio.stb_vorbis_close(filePointer);

            return new Sound(
                buffer,
                0,
                (ushort) info.channels,
                info.sample_rate,
                align
            );
        }

        /* we only support float decoding! WAV sucks! */
        public Sound(
            float[] buffer,
            uint bufferOffset,
            ushort channels,
            uint samplesPerSecond,
            ushort blockAlign
        ) {
            var bufferLength = 4 * buffer.Length;

            Format = new FAudio.FAudioWaveFormatEx();
            Format.wFormatTag = 3;
            Format.wBitsPerSample = 32;
            Format.nChannels = channels;
            Format.nBlockAlign = (ushort) (4 * Format.nChannels);
            Format.nSamplesPerSec = samplesPerSecond;
            Format.nAvgBytesPerSec = Format.nBlockAlign * Format.nSamplesPerSec;
            Format.nBlockAlign = blockAlign;
            Format.cbSize = 0;

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
    }
}
