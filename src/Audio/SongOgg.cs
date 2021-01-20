using System;
using System.IO;

namespace MoonWorks.Audio
{
    // for streaming long playback
    public class Song
    {
        public IntPtr Handle { get; }
        public FAudio.stb_vorbis_info Info { get; }
        public uint BufferSize { get; }
        public bool Loop { get; set; }
        private readonly float[] buffer;
        private const int bufferShrinkFactor = 8;

        public TimeSpan Duration { get; set; }

        public Song(FileInfo fileInfo)
        {
            var filePointer = FAudio.stb_vorbis_open_filename(fileInfo.FullName, out var error, IntPtr.Zero);

            if (error != 0)
            {
                throw new AudioLoadException("Error loading file!");
            }

            Info = FAudio.stb_vorbis_get_info(filePointer);
            BufferSize = (uint)(Info.sample_rate * Info.channels) / bufferShrinkFactor;

            buffer = new float[BufferSize];


            FAudio.stb_vorbis_close(filePointer);
        }
    }
}
