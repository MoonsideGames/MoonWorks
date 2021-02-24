using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
    /// <summary>
    /// Buffers are generic data containers that can be used by the GPU.
    /// </summary>
    public class Buffer : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyBuffer;

        /// <summary>
        /// Creates a buffer.
        /// </summary>
        /// <param name="device">An initialized GraphicsDevice.</param>
        /// <param name="usageFlags">Specifies how the buffer will be used.</param>
        /// <param name="sizeInBytes">The length of the array. Cannot be resized.</param>
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

        /// <summary>
        /// Asynchronously copies data into the buffer.
        /// </summary>
        /// <param name="data">An array of data to copy into the buffer.</param>
        /// <param name="offsetInElements">Specifies where to start copying out of the array.</param>
        /// <param name="lengthInElements">Specifies how many elements to copy.</param>
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

        /// <summary>
        /// Asynchronously copies data into the buffer.
        /// This variant of this method copies the entire array.
        /// </summary>
        /// <param name="data">An array of data to copy.</param>
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

        /// <summary>
        /// Asynchronously copies data into the buffer.
        /// </summary>
        /// <param name="data">A pointer to an array.</param>
        /// <param name="offsetInBytes">Specifies where to start copying the data, in bytes.</param>
        /// <param name="dataLengthInBytes">Specifies how many bytes of data to copy.</param>
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

        /// <summary>
        /// Asynchronously copies data into the buffer.
        /// </summary>
        /// <param name="data">A pointer to an array.</param>
        /// <param name="offsetInBytes">Specifies where to start copying the data, in bytes.</param>
        /// <param name="dataLengthInBytes">Specifies how many bytes of data to copy.</param>
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

        /// <summary>
        /// Reads data out of a buffer and into an array.
        /// This operation is only guaranteed to read up-to-date data if GraphicsDevice.Wait is called first.
        /// </summary>
        /// <param name="data">The array that data will be copied to.</param>
        /// <param name="dataLengthInBytes">The length of the data to read.</param>
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
