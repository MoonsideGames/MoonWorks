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
		/// Size in bytes.
		/// </summary>
		public uint Size { get; }

		/// <summary>
		/// Creates a buffer of appropriate size given a type and element count.
		/// </summary>
		/// <typeparam name="T">The type that the buffer will contain.</typeparam>
		/// <param name="device">The GraphicsDevice.</param>
		/// <param name="usageFlags">Specifies how the buffer will be used.</param>
		/// <param name="elementCount">How many elements of type T the buffer will contain.</param>
		/// <returns></returns>
		public unsafe static Buffer Create<T>(
			GraphicsDevice device,
			BufferUsageFlags usageFlags,
			uint elementCount
		) where T : unmanaged
		{
			return new Buffer(
				device,
				usageFlags,
				(uint) Marshal.SizeOf<T>() * elementCount
			);
		}

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
			Size = sizeInBytes;
		}

		/// <summary>
		/// Reads data out of a buffer and into a span.
		/// This operation is only guaranteed to read up-to-date data if GraphicsDevice.Wait or GraphicsDevice.WaitForFences is called first.
		/// </summary>
		/// <param name="data">The span that data will be copied to.</param>
		/// <param name="dataLengthInBytes">The length of the data to read.</param>
		public unsafe void GetData<T>(
			Span<T> data,
			uint dataLengthInBytes
		) where T : unmanaged
		{
#if DEBUG
			if (dataLengthInBytes > Size)
			{
				Logger.LogWarn("Requested too many bytes from buffer!");
			}

			if (dataLengthInBytes > data.Length)
			{
				Logger.LogWarn("Data length is larger than the provided Span!");
			}
#endif

			fixed (T* ptr = data)
			{
				Refresh.Refresh_GetBufferData(
					Device.Handle,
					Handle,
					(IntPtr) ptr,
					dataLengthInBytes
				);
			}
		}

		/// <summary>
		/// Reads data out of a buffer and into an array.
		/// This operation is only guaranteed to read up-to-date data if GraphicsDevice.Wait or GraphicsDevice.WaitForFences is called first.
		/// </summary>
		/// <param name="data">The span that data will be copied to.</param>
		/// <param name="dataLengthInBytes">The length of the data to read.</param>
		public unsafe void GetData<T>(
			T[] data,
			uint dataLengthInBytes
		) where T : unmanaged
		{
			GetData(new Span<T>(data), dataLengthInBytes);
		}

		/// <summary>
		/// Reads data out of a buffer and into a span.
		/// This operation is only guaranteed to read up-to-date data if GraphicsDevice.Wait or GraphicsDevice.WaitForFences is called first.
		/// Fills the span with as much data from the buffer as it can.
		/// </summary>
		/// <param name="data">The span that data will be copied to.</param>
		public unsafe void GetData<T>(
			Span<T> data
		) where T : unmanaged
		{
			var lengthInBytes = System.Math.Min(data.Length * Marshal.SizeOf<T>(), Size);
			GetData(data, (uint) lengthInBytes);
		}

		/// <summary>
		/// Reads data out of a buffer and into an array.
		/// This operation is only guaranteed to read up-to-date data if GraphicsDevice.Wait or GraphicsDevice.WaitForFences is called first.
		/// Fills the array with as much data from the buffer as it can.
		/// </summary>
		/// <param name="data">The span that data will be copied to.</param>
		public unsafe void GetData<T>(
			T[] data
		) where T : unmanaged
		{
			var lengthInBytes = System.Math.Min(data.Length * Marshal.SizeOf<T>(), Size);
			GetData(new Span<T>(data), (uint) lengthInBytes);
		}

		public static implicit operator BufferBinding(Buffer b)
		{
			return new BufferBinding(b, 0);
		}
	}
}
