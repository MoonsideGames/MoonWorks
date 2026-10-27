namespace MoonWorks.Audio;

public enum SampleFormat
{
    Unknown = 0,
    Unsigned8 = 1,
    Signed16 = 2,
    Signed24 = 3,
    Signed32 = 4,
    Float32 = 5
}

public readonly record struct Format(
    SampleFormat SampleFormat,
    uint Channels,
    uint SampleRate
);
