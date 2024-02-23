using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
	public unsafe class CpuBuffer : RefreshResource
	{
		protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyCpuBuffer;

		/// <summary>
		/// Size in bytes.
		/// </summary>
		public uint Size { get; }

		/// <summary>
		/// Creates a buffer of requested size given a type and element count.
		/// </summary>
		/// <typeparam name="T">The type that the buffer will contain.</typeparam>
		/// <param name="device">The GraphicsDevice.</param>
		/// <param name="elementCount">How many elements of type T the buffer will contain.</param>
		/// <returns></returns>
		public unsafe static CpuBuffer Create<T>(
			GraphicsDevice device,
			uint elementCount
		) where T : unmanaged
		{
			return new CpuBuffer(
				device,
				(uint) Marshal.SizeOf<T>() * elementCount
			);
		}

		/// <summary>
		/// Creates a CpuBuffer.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="sizeInBytes">The length of the buffer. Cannot be resized.</param>
		public CpuBuffer(
			GraphicsDevice device,
			uint sizeInBytes
		) : base(device)
		{
			Handle = Refresh.Refresh_CreateCpuBuffer(
				device.Handle,
				sizeInBytes
			);
			Size = sizeInBytes;
		}

		/// <summary>
		/// Immediately copies data from a data pointer to the CpuBuffer.
		///
		/// If setDataOption is DISCARD and this CpuBuffer was used in an Upload command,
		/// that command will still use the correct data at the cost of increased memory usage.
		///
		/// If setDataOption is OVERWRITE and this CpuBuffer was used in an Upload command,
		/// this could cause a data race.
		/// </summary>
		public unsafe void SetData(
			byte* dataPtr,
			in BufferCopy copyParams,
			SetDataOptions setDataOption
		) {
			Refresh.Refresh_SetData(
				Device.Handle,
				(nint) dataPtr,
				Handle,
				copyParams.ToRefresh(),
				(Refresh.SetDataOptions) setDataOption
			);
		}

		/// <summary>
		/// Immediately copies data from a Span to the CpuBuffer.
		/// Returns the length of the copy in bytes.
		///
		/// If setDataOption is DISCARD and this CpuBuffer was used in an Upload command,
		/// that command will still use the correct data at the cost of increased memory usage.
		///
		/// If setDataOption is OVERWRITE and this CpuBuffer was used in an Upload command,
		/// the data will be overwritten immediately, which could cause a data race.
		/// </summary>
		public unsafe uint SetData<T>(
			Span<T> data,
			uint bufferOffsetInBytes,
			SetDataOptions setDataOption
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * data.Length);

			fixed (T* dataPtr = data)
			{
				SetData(
					(byte*) dataPtr,
					new BufferCopy(0, bufferOffsetInBytes, dataLengthInBytes),
					setDataOption
				);
			}

			return dataLengthInBytes;
		}

		/// <summary>
		/// Immediately copies data from a Span to the CpuBuffer.
		/// Returns the length of the copy in bytes.
		///
		/// If setDataOption is DISCARD and this CpuBuffer was used in an Upload command,
		/// that command will still use the correct data at the cost of increased memory usage.
		///
		/// If setDataOption is OVERWRITE and this CpuBuffer was used in an Upload command,
		/// the data will be overwritten immediately, which could cause a data race.
		/// </summary>
		public unsafe uint SetData<T>(
			Span<T> data,
			SetDataOptions setDataOption
		) where T : unmanaged
		{
			return SetData(data, 0, setDataOption);
		}

		/// <summary>
		/// Immediately copies data from the CpuBuffer into a data pointer.
		/// </summary>
		public unsafe void GetData(
			byte* dataPtr,
			in BufferCopy copyParams
		) {
			Refresh.Refresh_GetData(
				Device.Handle,
				Handle,
				(nint) dataPtr,
				copyParams.ToRefresh()
			);
		}

		/// <summary>
		/// Immediately copies data from the CpuBuffer into a Span.
		/// </summary>
		public unsafe void GetData<T>(
			Span<T> data,
			uint bufferOffsetInBytes
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * data.Length);

			fixed (T* dataPtr = data)
			{
				GetData(
					(byte*) dataPtr,
					new BufferCopy(bufferOffsetInBytes, 0, dataLengthInBytes)
				);
			}
		}
	}
}
