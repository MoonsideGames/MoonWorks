using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics;

/// <summary>
/// A data container that can efficiently transfer data to and from the GPU.
/// </summary>
public unsafe class TransferBuffer : RefreshResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseTransferBuffer;

	/// <summary>
	/// Size in bytes.
	/// </summary>
	public uint Size { get; }

#if DEBUG
	public bool Mapped { get; private set; }
#endif

	/// <summary>
	/// Creates a buffer of requested size given a type and element count.
	/// </summary>
	/// <typeparam name="T">The type that the buffer will contain.</typeparam>
	/// <param name="device">The GraphicsDevice.</param>
	/// <param name="elementCount">How many elements of type T the buffer will contain.</param>
	/// <returns></returns>
	public unsafe static TransferBuffer Create<T>(
		GraphicsDevice device,
		TransferBufferUsage usage,
		uint elementCount
	) where T : unmanaged
	{
		return new TransferBuffer(
			device,
			usage,
			(uint) Marshal.SizeOf<T>() * elementCount
		);
	}

	/// <summary>
	/// Creates a TransferBuffer.
	/// </summary>
	/// <param name="device">An initialized GraphicsDevice.</param>
	/// <param name="usage">Whether this will be used to upload buffers or textures.</param>
	/// <param name="sizeInBytes">The length of the buffer. Cannot be resized.</param>
	public TransferBuffer(
		GraphicsDevice device,
		TransferBufferUsage usage,
		uint sizeInBytes
	) : base(device)
	{
		Handle = Refresh.Refresh_CreateTransferBuffer(
			device.Handle,
			(Refresh.TransferBufferUsage) usage,
			sizeInBytes
		);
		Size = sizeInBytes;
	}

	/// <summary>
	/// Immediately copies data from a Span to the TransferBuffer.
	/// Returns the length of the copy in bytes.
	///
	/// If cycle is set to true and this TransferBuffer was used in an Upload command,
	/// that command will still use the correct data at the cost of increased memory usage.
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
		AssertNotMapped();
#endif

		fixed (T* dataPtr = data)
		{
			Refresh.Refresh_SetTransferData(
				Device.Handle,
				(nint) dataPtr,
				new Refresh.TransferBufferRegion
				{
					TransferBuffer = Handle,
					Offset = bufferOffsetInBytes,
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
		AssertNotMapped();
#endif

		fixed (T* dataPtr = data)
		{
			Refresh.Refresh_GetTransferData(
				Device.Handle,
				new Refresh.TransferBufferRegion
				{
					TransferBuffer = Handle,
					Offset = bufferOffsetInBytes,
					Size = dataLengthInBytes
				},
				(nint) dataPtr
			);
		}
	}

	/// <summary>
	/// Maps the transfer buffer into application address space.
	/// You must call Unmap before encoding transfer commands.
	/// </summary>
	public unsafe void Map(bool cycle, out byte* data)
	{
#if DEBUG
		AssertNotMapped();
#endif

		Refresh.Refresh_MapTransferBuffer(
			Device.Handle,
			Handle,
			Conversions.BoolToInt(cycle),
			out data
		);

#if DEBUG
		Mapped = true;
#endif
	}

	/// <summary>
	/// Unmaps the transfer buffer.
	/// The pointer given by Map is no longer valid.
	/// </summary>
	public void Unmap()
	{
		Refresh.Refresh_UnmapTransferBuffer(
			Device.Handle,
			Handle
		);

#if DEBUG
		Mapped = false;
#endif
	}

#if DEBUG
	private void AssertBufferBoundsCheck(uint bufferLengthInBytes, uint offsetInBytes, uint copyLengthInBytes)
	{
		if (copyLengthInBytes > bufferLengthInBytes + offsetInBytes)
		{
			throw new InvalidOperationException($"Data overflow! Transfer buffer length {bufferLengthInBytes}, offset {offsetInBytes}, copy length {copyLengthInBytes}");
		}
	}

	private void AssertNotMapped()
	{
		if (Mapped)
		{
			throw new InvalidOperationException("Transfer buffer must not be mapped!");
		}
	}
#endif
}
