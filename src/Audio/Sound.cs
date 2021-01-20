namespace MoonWorks.Audio
{
    public abstract class Sound
    {
        internal FAudio.FAudioWaveFormatEx Format { get; }

        /* NOTE: we only support float decoding! WAV sucks! */
        public Sound(
            ushort channels,
            uint samplesPerSecond
        ) {
            var blockAlign = (ushort) (4 * channels);

            Format = new FAudio.FAudioWaveFormatEx
            {
                wFormatTag = 3,
                wBitsPerSample = 32,
                nChannels = channels,
                nBlockAlign = blockAlign,
                nSamplesPerSec = samplesPerSecond,
                nAvgBytesPerSec = blockAlign * samplesPerSecond,
                cbSize = 0
            };
        }
    }
}
