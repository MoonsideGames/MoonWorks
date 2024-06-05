using System;
using System.Runtime.InteropServices;
using SDL2_gpuCS;

namespace MoonWorks.Graphics
{
	public unsafe class TransferBuffer : SDL_GpuResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL_Gpu.SDL_GpuReleaseTransferBuffer;

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
			TransferUsage usage,
			TransferBufferMapFlags mapFlags,
			uint elementCount
		) where T : unmanaged
		{
			return new TransferBuffer(
				device,
				usage,
				mapFlags,
				(uint) Marshal.SizeOf<T>() * elementCount
			);
		}

		/// <summary>
		/// Creates a TransferBuffer.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="sizeInBytes">The length of the buffer. Cannot be resized.</param>
		/// <param name="usage">Whether this will be used to upload buffers or textures.</param>
		public TransferBuffer(
			GraphicsDevice device,
			TransferUsage usage,
			TransferBufferMapFlags mapFlags,
			uint sizeInBytes
		) : base(device)
		{
			Handle = SDL_Gpu.SDL_GpuCreateTransferBuffer(
				device.Handle,
				(SDL_Gpu.TransferUsage) usage,
				(SDL_Gpu.TransferBufferMapFlags) mapFlags,
				sizeInBytes
			);
			Size = sizeInBytes;
		}

		/// <summary>
		/// Immediately copies data from a Span to the TransferBuffer.
		/// Returns the length of the copy in bytes.
		///
		/// If cycle is set to true and this TransferBuffer was used in an Upload command,
		/// that command will still use the corret data at the cost of increased memory usage.
		///
		/// If cycle is set to false, the data will be overwritten immediately,
		/// which could cause a data race.
		/// </summary>
		public unsafe uint SetData<T>(
			Span<T> data,
			uint bufferOffsetInBytes,
			bool cycle
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * data.Length);

#if DEBUG
			AssertBufferBoundsCheck(Size, bufferOffsetInBytes, dataLengthInBytes);
#endif

			fixed (T* dataPtr = data)
			{
				SDL_Gpu.SDL_GpuSetTransferData(
					Device.Handle,
					(nint) dataPtr,
					Handle,
					new SDL_Gpu.BufferCopy
					{
						SourceOffset = 0,
						DestinationOffset = bufferOffsetInBytes,
						Size = dataLengthInBytes
					},
					Conversions.BoolToInt(cycle)
				);
			}

			return dataLengthInBytes;
		}

		/// <summary>
		/// Immediately copies data from a Span to the TransferBuffer.
		/// Returns the length of the copy in bytes.
		///
		/// If cycle is set to true and this TransferBuffer was used in an Upload command,
		/// that command will still use the corret data at the cost of increased memory usage.
		///
		/// If cycle is set to false, the data will be overwritten immediately,
		/// which could cause a data race.
		/// </summary>
		public unsafe uint SetData<T>(
			Span<T> data,
			bool cycle
		) where T : unmanaged
		{
			return SetData(data, 0, cycle);
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
				SDL_Gpu.SDL_GpuGetTransferData(
					Device.Handle,
					Handle,
					(nint) dataPtr,
					new SDL_Gpu.BufferCopy
					{
						SourceOffset = bufferOffsetInBytes,
						DestinationOffset = 0,
						Size = dataLengthInBytes
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
