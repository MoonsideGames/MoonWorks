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
		protected override Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyBuffer;

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
					(IntPtr) ptr,
					dataLengthInBytes
				);
			}
		}
	}
}
