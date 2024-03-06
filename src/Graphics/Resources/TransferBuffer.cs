using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
	public unsafe class TransferBuffer : RefreshResource
	{
		protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyTransferBuffer;

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
		public unsafe static TransferBuffer Create<T>(
			GraphicsDevice device,
			uint elementCount
		) where T : unmanaged
		{
			return new TransferBuffer(
				device,
				(uint) Marshal.SizeOf<T>() * elementCount
			);
		}

		/// <summary>
		/// Creates a TransferBuffer.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="sizeInBytes">The length of the buffer. Cannot be resized.</param>
		public TransferBuffer(
			GraphicsDevice device,
			uint sizeInBytes
		) : base(device)
		{
			Handle = Refresh.Refresh_CreateTransferBuffer(
				device.Handle,
				sizeInBytes
			);
			Size = sizeInBytes;
		}

		/// <summary>
		/// Immediately copies data from a Span to the TransferBuffer.
		/// Returns the length of the copy in bytes.
		///
		/// If setDataOption is DISCARD and this TransferBuffer was used in an Upload command,
		/// that command will still use the correct data at the cost of increased memory usage.
		///
		/// If setDataOption is OVERWRITE and this TransferBuffer was used in an Upload command,
		/// the data will be overwritten immediately, which could cause a data race.
		/// </summary>
		public unsafe uint SetData<T>(
			Span<T> data,
			uint bufferOffsetInBytes,
			TransferOptions setDataOption
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * data.Length);

#if DEBUG
			AssertBufferBoundsCheck(Size, bufferOffsetInBytes, dataLengthInBytes);
#endif

			fixed (T* dataPtr = data)
			{
				Refresh.Refresh_SetTransferData(
					Device.Handle,
					(nint) dataPtr,
					Handle,
					new Refresh.BufferCopy
					{
						srcOffset = 0,
						dstOffset = bufferOffsetInBytes,
						size = dataLengthInBytes
					},
					(Refresh.TransferOptions) setDataOption
				);
			}

			return dataLengthInBytes;
		}

		/// <summary>
		/// Immediately copies data from a Span to the TransferBuffer.
		/// Returns the length of the copy in bytes.
		///
		/// If setDataOption is DISCARD and this TransferBuffer was used in an Upload command,
		/// that command will still use the correct data at the cost of increased memory usage.
		///
		/// If setDataOption is OVERWRITE and this TransferBuffer was used in an Upload command,
		/// the data will be overwritten immediately, which could cause a data race.
		/// </summary>
		public unsafe uint SetData<T>(
			Span<T> data,
			TransferOptions setDataOption
		) where T : unmanaged
		{
			return SetData(data, 0, setDataOption);
		}

		/// <summary>
		/// Immediately copies data from the TransferBuffer into a Span.
		/// </summary>
		public unsafe void GetData<T>(
			Span<T> data,
			uint bufferOffsetInBytes = 0
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * data.Length);

#if DEBUG
			AssertBufferBoundsCheck(Size, bufferOffsetInBytes, dataLengthInBytes);
#endif

			fixed (T* dataPtr = data)
			{
				Refresh.Refresh_GetTransferData(
					Device.Handle,
					Handle,
					(nint) dataPtr,
					new Refresh.BufferCopy
					{
						srcOffset = bufferOffsetInBytes,
						dstOffset = 0,
						size = dataLengthInBytes
					}
				);
			}
		}

#if DEBUG
		private void AssertBufferBoundsCheck(uint bufferLengthInBytes, uint offsetInBytes, uint copyLengthInBytes)
		{
			if (copyLengthInBytes > bufferLengthInBytes + offsetInBytes)
			{
				throw new InvalidOperationException($"Data overflow! Transfer buffer length {bufferLengthInBytes}, offset {offsetInBytes}, copy length {copyLengthInBytes}");
			}
		}
#endif
	}
}
