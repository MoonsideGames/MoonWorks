using System;
using Miniaudio;

namespace MoonWorks.Audio;

public abstract class DataSource
{
}

public class ResourceManagerDataSource : DataSource
{
    public readonly Interop.ma_resource_manager_data_source Native;

    [Flags]
    public enum Flags : uint
    {
        Stream = 1,
        Decode = 2,
        Async = 4,
        WaitInit = 8,
        UnknownLength = 16,
        Looping = 32
    }

    internal ResourceManagerDataSource(Interop.ma_resource_manager_data_source native)
    {
        Native = native;
    }
}
