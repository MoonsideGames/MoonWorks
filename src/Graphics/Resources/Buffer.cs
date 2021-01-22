using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class Buffer : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyBuffer;

        public Buffer(
            GraphicsDevice device,
            BufferUsageFlags usageFlags,
            uint sizeInBytes
        ) : base(device)
        {
            Handle = Refresh.Refresh_CreateBuffer(
                device.Handle,
                (Refresh.BufferUsageFlags) usageFlags,
                sizeInBytes
            );
        }

        public unsafe void SetData<T>(
            uint offsetInBytes,
            T[] data,
            uint dataLengthInBytes
        ) where T : unmanaged
        {
            fixed (T* ptr = &data[0])
            {
                Refresh.Refresh_SetBufferData(
                    Device.Handle,
                    Handle,
                    offsetInBytes,
                    (IntPtr) ptr,
                    dataLengthInBytes
                );
            }
        }

        public unsafe void SetData<T>(
            T[] data
        ) where T : unmanaged
        {
            fixed (T* ptr = &data[0])
            {
                Refresh.Refresh_SetBufferData(
                    Device.Handle,
                    Handle,
                    0,
                    (IntPtr)ptr,
                    (uint) (data.Length * Marshal.SizeOf<T>())
                );
            }
        }

        public unsafe void SetData<T>(
            uint offsetInBytes,
            T* data,
            uint dataLengthInBytes
        ) where T : unmanaged
        {
            Refresh.Refresh_SetBufferData(
                Device.Handle,
                Handle,
                offsetInBytes,
                (IntPtr) data,
                dataLengthInBytes
            );
        }

        // NOTE: You want to wait on the device before calling this
        public unsafe void GetData<T>(
            T[] data,
            uint dataLengthInBytes
        ) where T : unmanaged
        {
            fixed (T* ptr = &data[0])
            {
                Refresh.Refresh_GetBufferData(
                    Device.Handle,
                    Handle,
                    (IntPtr)ptr,
                    dataLengthInBytes
                );
            }
        }
    }
}
