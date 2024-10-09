using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics;

// FIXME: can we map the transfer buffer instead of maintaining a local pointer

/// <summary>
/// A convenience structure for creating resources and uploading data to them.
///
/// Note that Upload or UploadAndWait must be called after the Create methods for the data to actually be uploaded.
///
/// Note that this structure does not magically keep memory usage down -
/// you may want to stagger uploads over multiple submissions to minimize memory usage.
/// </summary>
public unsafe class ResourceUploader : GraphicsResource
{
	// FIXME: we no longer need two separate buffers
	TransferBuffer BufferTransferBuffer;
	TransferBuffer TextureTransferBuffer;

	byte* bufferData;
	uint bufferDataOffset = 0;
	uint bufferDataSize = 1024;

	byte* textureData;
	uint textureDataOffset = 0;
	uint textureDataSize = 1024;

	List<(uint, BufferRegion, bool)> BufferUploads = new List<(uint, BufferRegion, bool)>();
	List<(uint, TextureRegion, bool)> TextureUploads = new List<(uint, TextureRegion, bool)>();

	public ResourceUploader(GraphicsDevice device) : base(device)
	{
		bufferData = (byte*) NativeMemory.Alloc(bufferDataSize);
		textureData = (byte*) NativeMemory.Alloc(textureDataSize);
	}

	// Buffers

	/// <summary>
	/// Creates a Buffer with data to be uploaded.
	/// </summary>
	public Buffer CreateBuffer<T>(Span<T> data, BufferUsageFlags usageFlags) where T : unmanaged
	{
		var buffer = Buffer.Create<T>(Device, usageFlags, (uint) data.Length);

		SetBufferData(buffer, 0, data, false);

		return buffer;
	}

	/// <summary>
	/// Prepares upload of data into a Buffer.
	/// </summary>
	public void SetBufferData<T>(Buffer buffer, uint bufferOffsetInElements, Span<T> data, bool cycle) where T : unmanaged
	{
		uint elementSize = (uint) Marshal.SizeOf<T>();
		uint offsetInBytes = elementSize * bufferOffsetInElements;
		uint lengthInBytes = (uint) (elementSize * data.Length);

		uint resourceOffset;
		fixed (void* spanPtr = data)
		{
			resourceOffset = CopyBufferData(spanPtr, lengthInBytes);
		}

		var bufferRegion = new BufferRegion
		{
			Buffer = buffer.Handle,
			Offset = offsetInBytes,
			Size = lengthInBytes
		};

		BufferUploads.Add((resourceOffset, bufferRegion, cycle));
	}

	// Textures

	public Texture CreateTexture2D<T>(Span<T> pixelData, TextureFormat format, uint width, uint height) where T : unmanaged
	{
		var texture = Texture.CreateTexture2D(Device, width, height, format, TextureUsageFlags.Sampler);
		SetTextureData(texture, pixelData, false);
		return texture;
	}

	/// <summary>
	/// Creates a 2D Texture from compressed image data to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(Span<byte> compressedImageData, TextureFormat format)
	{
		ImageUtils.ImageInfoFromBytes(compressedImageData, out var width, out var height, out var _);
		var texture = Texture.CreateTexture2D(Device, width, height, format, TextureUsageFlags.Sampler);
		SetTextureDataFromCompressed(texture, compressedImageData);
		return texture;
	}

	/// <summary>
	/// Creates a 2D Texture from a compressed image stream to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(Stream compressedImageStream, TextureFormat format)
	{
		var length = compressedImageStream.Length;
		var buffer = NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, (int) length);
		compressedImageStream.ReadExactly(span);

		var texture = CreateTexture2DFromCompressed(span, format);

		NativeMemory.Free(buffer);

		return texture;
	}

	/// <summary>
	/// Creates a 2D Texture from a compressed image file to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(string compressedImageFilePath, TextureFormat format)
	{
		var fileStream = new FileStream(compressedImageFilePath, FileMode.Open, FileAccess.Read);
		return CreateTexture2DFromCompressed(fileStream, format);
	}

	/// <summary>
	/// Creates a texture from a DDS stream.
	/// </summary>
	public Texture CreateTextureFromDDS(Stream stream)
	{
		using var reader = new BinaryReader(stream);
		Texture texture;
		int faces;
		ImageUtils.ParseDDS(reader, out var format, out var width, out var height, out var levels, out var isCube);

		if (isCube)
		{
			texture = Texture.CreateTextureCube(Device, (uint) width, format, TextureUsageFlags.Sampler, (uint) levels);
			faces = 6;
		}
		else
		{
			texture = Texture.CreateTexture2D(Device, (uint) width, (uint) height, format, TextureUsageFlags.Sampler, (uint) levels);
			faces = 1;
		}

		for (int face = 0; face < faces; face += 1)
		{
			for (int level = 0; level < levels; level += 1)
			{
				var levelWidth = width >> level;
				var levelHeight = height >> level;

				var levelSize = ImageUtils.CalculateDDSLevelSize(levelWidth, levelHeight, format);
				var byteBuffer = NativeMemory.Alloc((nuint) levelSize);
				var byteSpan = new Span<byte>(byteBuffer, levelSize);
				stream.ReadExactly(byteSpan);

				var textureRegion = new TextureRegion
				{
					Texture = texture.Handle,
					Layer = (uint) face,
					MipLevel = (uint) level,
					X = 0,
					Y = 0,
					Z = 0,
					W = (uint) levelWidth,
					H = (uint) levelHeight,
					D = 1
				};

				SetTextureData(textureRegion, byteSpan, false);

				NativeMemory.Free(byteBuffer);
			}
		}

		return texture;
	}

	/// <summary>
	/// Creates a texture from a DDS file.
	/// </summary>
	public Texture CreateTextureFromDDS(string path)
	{
		var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		return CreateTextureFromDDS(stream);
	}

	public void SetTextureDataFromCompressed(TextureRegion textureRegion, Span<byte> compressedImageData)
	{
		var pixelData = ImageUtils.GetPixelDataFromBytes(compressedImageData, out var _, out var _, out var sizeInBytes);
		var pixelSpan = new Span<byte>((void*) pixelData, (int) sizeInBytes);

		SetTextureData(textureRegion, pixelSpan, false);

		ImageUtils.FreePixelData(pixelData);
	}

	public void SetTextureDataFromCompressed(TextureRegion textureRegion, Stream compressedImageStream)
	{
		var length = compressedImageStream.Length;
		var buffer = NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, (int) length);
		compressedImageStream.ReadExactly(span);
		SetTextureDataFromCompressed(textureRegion, span);
		NativeMemory.Free(buffer);
	}

	public void SetTextureDataFromCompressed(TextureRegion textureRegion, string compressedImageFilePath)
	{
		var fileStream = new FileStream(compressedImageFilePath, FileMode.Open, FileAccess.Read);
		SetTextureDataFromCompressed(textureRegion, fileStream);
	}

	/// <summary>
	/// Prepares upload of pixel data into a TextureSlice.
	/// </summary>
	public void SetTextureData<T>(TextureRegion textureRegion, Span<T> data, bool cycle) where T : unmanaged
	{
		var elementSize = Marshal.SizeOf<T>();
		var dataLengthInBytes = (uint) (elementSize * data.Length);

		uint resourceOffset;
		fixed (T* dataPtr = data)
		{
			resourceOffset = CopyTextureData(dataPtr, dataLengthInBytes, Texture.BytesPerPixel(textureRegion.TextureSlice.Texture.Format));
		}

		TextureUploads.Add((resourceOffset, textureRegion, cycle));
	}

	// Upload

	/// <summary>
	/// Uploads all the data corresponding to the created resources.
	/// </summary>
	public void Upload()
	{
		CopyToTransferBuffer();

		var commandBuffer = Device.AcquireCommandBuffer();
		RecordUploadCommands(commandBuffer);
		Device.Submit(commandBuffer);
	}

	/// <summary>
	/// Uploads and then blocks until the upload is finished.
	/// This is useful for keeping memory usage down during threaded upload.
	/// </summary>
	public void UploadAndWait()
	{
		CopyToTransferBuffer();

		var commandBuffer = Device.AcquireCommandBuffer();
		RecordUploadCommands(commandBuffer);
		var fence = Device.SubmitAndAcquireFence(commandBuffer);
		Device.WaitForFence(fence);
		Device.ReleaseFence(fence);
	}

	// Helper methods

	private void CopyToTransferBuffer()
	{
		if (BufferUploads.Count > 0)
		{
			if (BufferTransferBuffer == null || BufferTransferBuffer.Size < bufferDataSize)
			{
				BufferTransferBuffer?.Dispose();
				BufferTransferBuffer = TransferBuffer.Create<byte>(Device, TransferBufferUsage.Upload, bufferDataSize);
			}

			var dataSpan = new Span<byte>(bufferData, (int) bufferDataSize);
			var transferBufferSpan = BufferTransferBuffer.Map<byte>(true);
			dataSpan.CopyTo(transferBufferSpan);
			BufferTransferBuffer.Unmap();
		}


		if (TextureUploads.Count > 0)
		{
			if (TextureTransferBuffer == null || TextureTransferBuffer.Size < textureDataSize)
			{
				TextureTransferBuffer?.Dispose();
				TextureTransferBuffer = TransferBuffer.Create<byte>(Device, TransferBufferUsage.Upload, textureDataSize);
			}

			var dataSpan = new Span<byte>(textureData, (int) textureDataSize);
			var transferBufferSpan = TextureTransferBuffer.Map<byte>(true);
			dataSpan.CopyTo(transferBufferSpan);
			TextureTransferBuffer.Unmap();
		}
	}

	private void RecordUploadCommands(CommandBuffer commandBuffer)
	{
		var copyPass = commandBuffer.BeginCopyPass();

		foreach (var (transferOffset, bufferRegion, option) in BufferUploads)
		{
			copyPass.UploadToBuffer(
				new TransferBufferLocation
				{
					TransferBuffer = BufferTransferBuffer.Handle,
					Offset = transferOffset
				},
				bufferRegion,
				option
			);
		}

		foreach (var (transferOffset, textureRegion, option) in TextureUploads)
		{
			copyPass.UploadToTexture(
				new TextureTransferInfo
				{
					TransferBuffer = TextureTransferBuffer.Handle,
					Offset = transferOffset
				},
				textureRegion,
				option
			);
		}

		commandBuffer.EndCopyPass(copyPass);

		BufferUploads.Clear();
		TextureUploads.Clear();
		bufferDataOffset = 0;
	}

	private uint CopyBufferData(void* ptr, uint lengthInBytes)
	{
		if (bufferDataOffset + lengthInBytes >= bufferDataSize)
		{
			bufferDataSize = bufferDataOffset + lengthInBytes;
			bufferData = (byte*) NativeMemory.Realloc(bufferData, bufferDataSize);
		}

		var resourceOffset = bufferDataOffset;

		NativeMemory.Copy(ptr, bufferData + bufferDataOffset, lengthInBytes);
		bufferDataOffset += lengthInBytes;

		return resourceOffset;
	}

	private uint CopyTextureData(void* ptr, uint lengthInBytes, uint alignment)
	{
		textureDataOffset = RoundToAlignment(textureDataOffset, alignment);

		if (textureDataOffset + lengthInBytes >= textureDataSize)
		{
			textureDataSize = textureDataOffset + lengthInBytes;
			textureData = (byte*) NativeMemory.Realloc(textureData, textureDataSize);
		}

		var resourceOffset = textureDataOffset;

		NativeMemory.Copy(ptr, textureData + textureDataOffset, lengthInBytes);
		textureDataOffset += lengthInBytes;

		return resourceOffset;
	}

	private uint RoundToAlignment(uint value, uint alignment)
	{
		return alignment * ((value + alignment - 1) / alignment);
	}

	// Dispose

	/// <summary>
	/// It is valid to immediately call Dispose after calling Upload.
	/// </summary>
	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			if (disposing)
			{
				BufferTransferBuffer?.Dispose();
				TextureTransferBuffer?.Dispose();
			}

			NativeMemory.Free(bufferData);
		}
		base.Dispose(disposing);
	}
}
