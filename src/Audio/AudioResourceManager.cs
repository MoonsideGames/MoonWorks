using System;
using System.Runtime.CompilerServices;
using Miniaudio;

namespace MoonWorks.Audio;

public class AudioResourceManager
{
    private readonly Interop.ma_resource_manager ResourceManager;

    public static AudioResourceManager Create()
    {
        if (Interop.ma_resource_manager_init(Unsafe.NullRef<Interop.ma_resource_manager_config>(), out var resourceManager) != Interop.ma_result.MA_SUCCESS)
        {
            return null;
        }

        return new AudioResourceManager(resourceManager);
    }

    private AudioResourceManager(Interop.ma_resource_manager resourceManager)
    {
        ResourceManager = resourceManager;
    }

    public void RegisterDecodedData(string name, IntPtr decodedData, ulong frameCount, Format format)
    {
        Interop.ma_resource_manager_register_decoded_data_w(
            ResourceManager,
            name,
            decodedData,
            frameCount,
            (Interop.ma_format) format.SampleFormat,
            format.Channels,
            format.SampleRate);
    }

    public DataSource CreateDataSource(string name, ResourceManagerDataSource.Flags flags)
    {
        var result = Interop.ma_resource_manager_data_source_init_w(
            ResourceManager,
            name,
            (uint) flags,
            ref Unsafe.NullRef<Interop.ma_resource_manager_pipeline_notifications>(), // TODO
            out var dataSource);

        if (result != Interop.ma_result.MA_SUCCESS)
        {
            return null;
        }

        return new ResourceManagerDataSource(dataSource);
    }
}
