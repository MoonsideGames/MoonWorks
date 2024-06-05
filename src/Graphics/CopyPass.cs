using System.Runtime.InteropServices;
using SDL2_gpuCS;

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
		TransferBuffer transferBuffer,
		in TextureRegion textureRegion,
		in BufferImageCopy copyParams,
		bool cycle
	) {
#if DEBUG
		AssertBufferBoundsCheck(transferBuffer.Size, copyParams.BufferOffset, textureRegion.Size);
#endif

		SDL_Gpu.SDL_GpuUploadToTexture(
			Handle,
			transferBuffer.Handle,
			textureRegion.ToSDL(),
			copyParams.ToSDL(),
			Conversions.BoolToInt(cycle)
		);
	}

	/// <summary>
	/// Uploads the contents of an entire buffer to a 2D texture with no mips.
	/// </summary>
	public void UploadToTexture(
		TransferBuffer transferBuffer,
		Texture texture,
		bool cycle
	) {
		UploadToTexture(
			transferBuffer,
			new TextureRegion(texture),
			new BufferImageCopy(0, 0, 0),
			cycle
		);
	}

	/// <summary>
	/// Uploads data from a TransferBuffer to a GpuBuffer.
	/// This copy occurs on the GPU timeline.
	///
	/// Overwriting the contents of the TransferBuffer before the command buffer
	/// has finished execution will cause undefined behavior.
	///
	/// You MAY assume that the copy has finished for subsequent commands.
	/// </summary>
	/// <param name="cycle">If true, cycles the buffer if it is bound.</param>
	public void UploadToBuffer(
		TransferBuffer transferBuffer,
		GpuBuffer buffer,
		in BufferCopy copyParams,
		bool cycle
	) {
#if DEBUG
		AssertBufferBoundsCheck(transferBuffer.Size, copyParams.SrcOffset, copyParams.Size);
		AssertBufferBoundsCheck(buffer.Size, copyParams.DstOffset, copyParams.Size);
#endif

		SDL_Gpu.SDL_GpuUploadToBuffer(
			Handle,
			transferBuffer.Handle,
			buffer.Handle,
			copyParams.ToSDL(),
			Conversions.BoolToInt(cycle)
		);
	}

	/// <summary>
	/// Copies the entire contents of a TransferBuffer to a GpuBuffer.
	/// </summary>
	public void UploadToBuffer(
		TransferBuffer transferBuffer,
		GpuBuffer buffer,
		bool cycle
	) {
		UploadToBuffer(
			transferBuffer,
			buffer,
			new BufferCopy(0, 0, transferBuffer.Size),
			cycle
		);
	}

	/// <summary>
	/// Copies data element-wise into from a TransferBuffer to a GpuBuffer.
	/// </summary>
	public void UploadToBuffer<T>(
		TransferBuffer transferBuffer,
		GpuBuffer buffer,
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
			transferBuffer,
			buffer,
			new BufferCopy(srcOffsetInBytes, dstOffsetInBytes, dataLengthInBytes),
			cycle
		);
	}

	/// <summary>
	/// Copies the contents of a TextureRegion to another TextureRegion.
	/// The regions must have the same dimensions.
	/// This copy occurs on the GPU timeline.
	///
	/// You MAY assume that the copy has finished in subsequent commands.
	/// </summary>
	public void CopyTextureToTexture(
		in TextureRegion source,
		in TextureRegion destination,
		bool cycle
	) {
#if DEBUG
		AssertTextureBoundsCheck(destination.Size, source.Size);

		if (source.Width != destination.Width || source.Height != destination.Height || source.Depth != destination.Depth)
		{
			throw new System.InvalidOperationException("Texture copy must have the same dimensions!");
		}
#endif

		SDL_Gpu.SDL_GpuCopyTextureToTexture(
			Handle,
			source.ToSDL(),
			destination.ToSDL(),
			Conversions.BoolToInt(cycle)
		);
	}

	/// <summary>
	/// Copies data from a GpuBuffer to another GpuBuffer.
	/// This copy occurs on the GPU timeline.
	///
	/// You MAY assume that the copy has finished in subsequent commands.
	/// </summary>
	public void CopyBufferToBuffer(
		GpuBuffer source,
		GpuBuffer destination,
		in BufferCopy copyParams,
		bool cycle
	) {
#if DEBUG
		AssertBufferBoundsCheck(source.Size, copyParams.SrcOffset, copyParams.Size);
		AssertBufferBoundsCheck(destination.Size, copyParams.DstOffset, copyParams.Size);
#endif

		SDL_Gpu.SDL_GpuCopyBufferToBuffer(
			Handle,
			source.Handle,
			destination.Handle,
			copyParams.ToSDL(),
			Conversions.BoolToInt(cycle)
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

	private void AssertTextureBoundsCheck(uint textureSizeInBytes, uint dataLengthInBytes)
	{
		if (dataLengthInBytes > textureSizeInBytes)
		{
			throw new System.InvalidOperationException($"SetTextureData overflow! texture size {textureSizeInBytes}, data size {dataLengthInBytes}");
		}
	}
#endif
}
