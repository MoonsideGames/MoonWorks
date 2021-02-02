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
            T[] data,
            uint offsetInElements,
            uint lengthInElements
        ) where T : unmanaged
        {
            var elementSize = Marshal.SizeOf<T>();

            fixed (T* ptr = &data[0])
            {
                Refresh.Refresh_SetBufferData(
                    Device.Handle,
                    Handle,
                    (uint) (offsetInElements * elementSize),
                    (IntPtr) ptr,
                    (uint) (lengthInElements * elementSize)
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

        public void SetData(
            IntPtr data,
            uint offsetInBytes,
            uint dataLengthInBytes
        ) {
            Refresh.Refresh_SetBufferData(
                Device.Handle,
                Handle,
                offsetInBytes,
                data,
                dataLengthInBytes
            );
        }

        public unsafe void SetData<T>(
            T* data,
            uint offsetInElements,
            uint lengthInElements
        ) where T : unmanaged {
            var elementSize = Marshal.SizeOf<T>();
            Refresh.Refresh_SetBufferData(
                Device.Handle,
                Handle,
                (uint) (offsetInElements * elementSize),
                (IntPtr) data,
                (uint) (lengthInElements * elementSize)
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
