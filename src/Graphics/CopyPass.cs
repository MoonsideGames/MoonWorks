using System.Runtime.InteropServices;
using SDL = MoonWorks.Graphics.SDL_GPU;

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
		SDL.SDL_UploadToGPUTexture(
			Handle,
			source,
			destination,
			cycle
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
			new TextureTransferInfo
			{
				TransferBuffer = source.Handle,
				Offset = 0
			},
			new TextureRegion
			{
				Texture = destination.Handle,
				W = destination.Width,
				H = destination.Height
			},
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
		SDL.SDL_UploadToGPUBuffer(
			Handle,
			source,
			destination,
			cycle
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
			new TransferBufferLocation
			{
				TransferBuffer = source.Handle,
				Offset = 0
			},
			new BufferRegion
			{
				Buffer = destination.Handle,
				Offset = 0,
				Size = destination.Size
			},
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
			new TransferBufferLocation
			{
				TransferBuffer = source.Handle,
				Offset = srcOffsetInBytes
			},
			new BufferRegion
			{
				Buffer = destination.Handle,
				Offset = dstOffsetInBytes,
				Size = dataLengthInBytes
			},
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
		SDL.SDL_CopyGPUTextureToTexture(
			Handle,
			source,
			destination,
			w,
			h,
			d,
			cycle
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
		SDL.SDL_CopyGPUBufferToBuffer(
			Handle,
			source,
			destination,
			size,
			cycle
		);
	}

	public void DownloadFromBuffer(
		in BufferRegion source,
		in TransferBufferLocation destination
	) {
		SDL.SDL_DownloadFromGPUBuffer(
			Handle,
			source,
			destination
		);
	}

	public void DownloadFromTexture(
		in TextureRegion source,
		in TextureTransferInfo destination
	) {
		SDL.SDL_DownloadFromGPUTexture(
			Handle,
			source,
			destination
		);
	}
}
