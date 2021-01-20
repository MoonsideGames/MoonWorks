namespace MoonWorks.Audio
{
    public abstract class Sound
    {
        internal AudioDevice Device { get; }
        internal abstract FAudio.FAudioWaveFormatEx Format { get; }

        public Sound(AudioDevice device)
        {
            Device = device;
        }
    }
}
