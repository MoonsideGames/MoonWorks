using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics;

public class CopyPass
{
	public nint Handle { get; private set; }

	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}

	/// <summary>
	/// Uploads data from a TransferBuffer to a TextureSlice.
	/// This copy occurs on the GPU timeline.
	///
	/// Overwriting the contents of the TransferBuffer before the command buffer
	/// has finished execution will cause undefined behavior.
	///
	/// You MAY assume that the copy has finished for subsequent commands.
	/// </summary>
	/// <param name="cycle">If true, cycles the texture if the given slice is bound.</param>
	public void UploadToTexture(
		in TextureTransferInfo source,
		in TextureRegion destination,
		bool cycle
	) {
#if DEBUG
		AssertTransferBufferNotMapped(source.TransferBuffer);
#endif

		Refresh.Refresh_UploadToTexture(
			Handle,
			source.ToRefresh(),
			destination.ToRefresh(),
			Conversions.BoolToInt(cycle)
		);
	}

	/// <summary>
	/// Uploads the contents of an entire buffer to a 2D texture with no mips.
	/// </summary>
	public void UploadToTexture(
		TransferBuffer source,
		Texture destination,
		bool cycle
	) {
		UploadToTexture(
			new TextureTransferInfo(source),
			new TextureRegion(destination),
			cycle
		);
	}

	/// <summary>
	/// Uploads data from a TransferBuffer to a Buffer.
	/// This copy occurs on the GPU timeline.
	///
	/// Overwriting the contents of the TransferBuffer before the command buffer
	/// has finished execution will cause undefined behavior.
	///
	/// You MAY assume that the copy has finished for subsequent commands.
	/// </summary>
	/// <param name="cycle">If true, cycles the buffer if it is bound.</param>
	public void UploadToBuffer(
		in TransferBufferLocation source,
		in BufferRegion destination,
		bool cycle
	) {
#if DEBUG
		AssertBufferBoundsCheck(source.TransferBuffer.Size, source.Offset, destination.Size);
		AssertBufferBoundsCheck(destination.Buffer.Size, destination.Offset, destination.Size);
		AssertTransferBufferNotMapped(source.TransferBuffer);
#endif

		Refresh.Refresh_UploadToBuffer(
			Handle,
			source.ToRefresh(),
			destination.ToRefresh(),
			Conversions.BoolToInt(cycle)
		);
	}

	/// <summary>
	/// Copies the entire contents of a TransferBuffer to a Buffer.
	/// </summary>
	public void UploadToBuffer(
		TransferBuffer source,
		Buffer destination,
		bool cycle
	) {
		UploadToBuffer(
			new TransferBufferLocation(source),
			new BufferRegion(destination, 0, destination.Size),
			cycle
		);
	}

	/// <summary>
	/// Copies data element-wise into from a TransferBuffer to a Buffer.
	/// </summary>
	public void UploadToBuffer<T>(
		TransferBuffer source,
		Buffer destination,
		uint sourceStartElement,
		uint destinationStartElement,
		uint numElements,
		bool cycle
	) where T : unmanaged
	{
		var elementSize = Marshal.SizeOf<T>();
		var dataLengthInBytes = (uint) (elementSize * numElements);
		var srcOffsetInBytes = (uint) (elementSize * sourceStartElement);
		var dstOffsetInBytes = (uint) (elementSize * destinationStartElement);

		UploadToBuffer(
			new TransferBufferLocation(source, srcOffsetInBytes),
			new BufferRegion(destination, dstOffsetInBytes, dataLengthInBytes),
			cycle
		);
	}

	/// <summary>
	/// Copies the contents of a TextureLocation to another TextureLocation.
	/// This copy occurs on the GPU timeline.
	///
	/// You MAY assume that the copy has finished in subsequent commands.
	/// </summary>
	public void CopyTextureToTexture(
		in TextureLocation source,
		in TextureLocation destination,
		uint w,
		uint h,
		uint d,
		bool cycle
	) {
#if DEBUG
		AssertTextureBoundsCheck(source, w, h, d);
		AssertTextureBoundsCheck(destination, w, h, d);
#endif

		Refresh.Refresh_CopyTextureToTexture(
			Handle,
			source.ToRefresh(),
			destination.ToRefresh(),
			w,
			h,
			d,
			Conversions.BoolToInt(cycle)
		);
	}

	/// <summary>
	/// Copies data from a Buffer to another Buffer.
	/// This copy occurs on the GPU timeline.
	///
	/// You MAY assume that the copy has finished in subsequent commands.
	/// </summary>
	public void CopyBufferToBuffer(
		in BufferLocation source,
		in BufferLocation destination,
		uint size,
		bool cycle
	) {
#if DEBUG
		AssertBufferBoundsCheck(source.Buffer.Size, source.Offset, size);
		AssertBufferBoundsCheck(destination.Buffer.Size, destination.Offset, size);
#endif

		Refresh.Refresh_CopyBufferToBuffer(
			Handle,
			source.ToRefresh(),
			destination.ToRefresh(),
			size,
			Conversions.BoolToInt(cycle)
		);
	}

	public void DownloadFromBuffer(
		in BufferRegion source,
		in TransferBufferLocation destination
	) {
#if DEBUG
		AssertBufferBoundsCheck(source.Buffer.Size, source.Offset, source.Size);
		AssertBufferBoundsCheck(destination.TransferBuffer.Size, destination.Offset, source.Size);
		AssertTransferBufferNotMapped(destination.TransferBuffer);
#endif

		Refresh.Refresh_DownloadFromBuffer(
			Handle,
			source.ToRefresh(),
			destination.ToRefresh()
		);
	}

	public void DownloadFromTexture(
		in TextureRegion source,
		in TextureTransferInfo destination
	) {
#if DEBUG
		AssertTransferBufferNotMapped(destination.TransferBuffer);
#endif

		Refresh.Refresh_DownloadFromTexture(
			Handle,
			source.ToRefresh(),
			destination.ToRefresh()
		);
	}

#if DEBUG
	private void AssertBufferBoundsCheck(uint bufferLengthInBytes, uint offsetInBytes, uint copyLengthInBytes)
	{
		if (copyLengthInBytes > bufferLengthInBytes + offsetInBytes)
		{
			throw new System.InvalidOperationException($"SetBufferData overflow! buffer length {bufferLengthInBytes}, offset {offsetInBytes}, copy length {copyLengthInBytes}");
		}
	}

	private void AssertTextureBoundsCheck(in TextureLocation textureLocation, uint w, uint h, uint d)
	{
		if (
			textureLocation.X + w > textureLocation.TextureSlice.Texture.Width ||
			textureLocation.Y + h > textureLocation.TextureSlice.Texture.Height ||
			textureLocation.Z + d > textureLocation.TextureSlice.Texture.Depth
		) {
			throw new System.InvalidOperationException($"Texture data is out of bounds!");
		}
	}

	private void AssertTransferBufferNotMapped(TransferBuffer transferBuffer)
	{
		if (transferBuffer.Mapped)
		{
			throw new System.InvalidOperationException("Transfer buffer must not be mapped!");
		}
	}
#endif
}
