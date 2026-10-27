using System;
using System.Runtime.CompilerServices;
using Miniaudio;

namespace MoonWorks.Audio;

public class Sound
{
    public readonly Interop.ma_sound Native;

    [Flags]
    public enum Flags : uint
    {
        Stream,
        Decode,
        Async,
        WaitInit,
        UnknownLength,
        FlagLooping,
        NoDefaultAttachment,
        NoPitch,
        NoSpatialization
    }

    public Sound Create(AudioEngine engine, ResourceManagerDataSource dataSource, Flags flags)
    {
        var result = Interop.ma_sound_init_from_data_source(
            engine.Engine,
            dataSource.Native, // TODO: should we just be malloc'ing this stuff?
            (uint) flags,
            Unsafe.NullRef<Interop.ma_sound>(),
            out var sound);

        if (result != Interop.ma_result.MA_SUCCESS)
        {
            return null;
        }
        
        return new Sound(sound);
    }

    private Sound(Interop.ma_sound sound)
    {
        Native = sound;
    }
}
