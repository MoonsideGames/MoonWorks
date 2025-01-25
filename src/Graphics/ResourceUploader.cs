using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics;

/// <summary>
/// A convenience structure for creating resources and uploading data to them.
/// If the data provided is too long for the size of the uploader, it will resize itself.
///
/// Note that Upload or UploadAndWait must be called after the Create methods for the data to actually be uploaded.
///
/// Note that this structure does not magically keep memory usage down -
/// you may want to stagger uploads over multiple submissions to minimize memory usage.
/// </summary>
public unsafe class ResourceUploader : GraphicsResource
{
	TransferBuffer TransferBuffer;
	uint WriteOffset = 0;

	record struct BufferUpload(uint Offset, BufferRegion BufferRegion, bool Cycle);
	record struct TextureUpload(uint Offset, TextureRegion TextureRegion, bool Cycle);

	List<BufferUpload> BufferUploads = [];
	List<TextureUpload> TextureUploads = [];

	/// <summary>
	/// If a size value is not provided, the uploader will automatically resize itself on the next upload command.
	/// </summary>
	public ResourceUploader(GraphicsDevice device, uint size = 0) : base(device)
	{
		Name = "ResourceUploader";

		if (size != 0)
		{
			TransferBuffer = TransferBuffer.Create<byte>(device, "ResourceUploader TransferBuffer", TransferBufferUsage.Upload, size);

			TransferBuffer.Map(false);
		}
	}

	// Buffers

	/// <summary>
	/// Creates a named Buffer with data to be uploaded.
	/// </summary>
	public Buffer CreateBuffer<T>(string name, Span<T> data, BufferUsageFlags usageFlags) where T : unmanaged
	{
		var buffer = Buffer.Create<T>(Device, name, usageFlags, (uint) data.Length);
		SetBufferData(buffer, 0, data, false);
		return buffer;
	}

	/// <summary>
	/// Creates a Buffer with data to be uploaded.
	/// </summary>
	public Buffer CreateBuffer<T>(Span<T> data, BufferUsageFlags usageFlags) where T : unmanaged =>
		CreateBuffer(null, data, usageFlags);

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
			resourceOffset = CopyBufferData(data);
		}

		BufferUploads.Add(new BufferUpload(
			resourceOffset,
			new BufferRegion
			{
				Buffer = buffer.Handle,
				Offset = offsetInBytes,
				Size = lengthInBytes
			},
			cycle
		));
	}

	// Textures

	public Texture CreateTexture2D<T>(string name, Span<T> pixelData, TextureFormat format, TextureUsageFlags usage, uint width, uint height) where T : unmanaged
	{
		var texture = Texture.Create2D(Device, name, width, height, format, usage);
		SetTextureData(
			new TextureRegion
			{
				Texture = texture.Handle,
				W = width,
				H = height,
				D = 1
			},
			pixelData,
			false
		);
		return texture;
	}

	public Texture CreateTexture2D<T>(Span<T> pixelData, TextureFormat format, TextureUsageFlags usage, uint width, uint height) where T : unmanaged =>
		CreateTexture2D(null, pixelData, format, usage, width, height);

	/// <summary>
	/// Creates a named 2D Texture from compressed image data to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(string name, Span<byte> compressedImageData, TextureFormat format, TextureUsageFlags usage)
	{
		ImageUtils.ImageInfoFromBytes(compressedImageData, out var width, out var height, out var _);
		var texture = Texture.Create2D(Device, name, width, height, format, usage);
		SetTextureDataFromCompressed(
			new TextureRegion
			{
				Texture = texture.Handle,
				W = width,
				H = height,
				D = 1
			},
			compressedImageData
		);
		return texture;
	}

	/// <summary>
	/// Creates a 2D Texture from compressed image data to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(Span<byte> compressedImageData, TextureFormat format, TextureUsageFlags usage) =>
		CreateTexture2DFromCompressed(null, compressedImageData, format, usage);

	/// <summary>
	/// Creates a named 2D Texture from a compressed image stream to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(string name, Stream compressedImageStream, TextureFormat format, TextureUsageFlags usage)
	{
		var length = compressedImageStream.Length;
		var buffer = NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, (int) length);
		compressedImageStream.ReadExactly(span);

		var texture = CreateTexture2DFromCompressed(name, span, format, usage);

		NativeMemory.Free(buffer);

		return texture;
	}

	/// <summary>
	/// Creates a 2D Texture from a compressed image stream to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(Stream compressedImageStream, TextureFormat format, TextureUsageFlags usage) =>
		CreateTexture2DFromCompressed(null, compressedImageStream, format, usage);

	/// <summary>
	/// Creates a named 2D Texture from a compressed image file to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(string name, string compressedImageFilePath, TextureFormat format, TextureUsageFlags usage)
	{
		using var fileStream = new FileStream(compressedImageFilePath, FileMode.Open, FileAccess.Read);
		return CreateTexture2DFromCompressed(name, fileStream, format, usage);
	}

	/// <summary>
	/// Creates a 2D Texture from a compressed image file to be uploaded.
	/// </summary>
	public Texture CreateTexture2DFromCompressed(string compressedImageFilePath, TextureFormat format, TextureUsageFlags usage) =>
		CreateTexture2DFromCompressed(Path.GetFileNameWithoutExtension(compressedImageFilePath), compressedImageFilePath, format, usage);


	/// <summary>
	/// Creates a texture from a DDS stream.
	/// </summary>
	public Texture CreateTextureFromDDS(string name, Stream stream)
	{
		using var reader = new BinaryReader(stream);
		Texture texture;
		int faces;
		ImageUtils.ParseDDS(reader, out var format, out var width, out var height, out var levels, out var isCube);

		if (isCube)
		{
			texture = Texture.CreateCube(Device, name, (uint) width, format, TextureUsageFlags.Sampler, (uint) levels);
			faces = 6;
		}
		else
		{
			texture = Texture.Create2D(Device, name, (uint) width, (uint) height, format, TextureUsageFlags.Sampler, (uint) levels);
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

	public Texture CreateTextureFromDDS(Stream stream) =>
		CreateTextureFromDDS(null, stream);

	/// <summary>
	/// Creates a texture from a DDS file.
	/// </summary>
	public Texture CreateTextureFromDDS(string name, string path)
	{
		using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		return CreateTextureFromDDS(name, stream);
	}

	public Texture CreateTextureFromDDS(string path) =>
		CreateTextureFromDDS(Path.GetFileNameWithoutExtension(path), path);

	public void SetTextureDataFromCompressed(TextureRegion textureRegion, ReadOnlySpan<byte> compressedImageData)
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
		using var fileStream = new FileStream(compressedImageFilePath, FileMode.Open, FileAccess.Read);
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
			resourceOffset = CopyTextureData(data, 16); // Align to biggest possible pixel size
		}

		TextureUploads.Add(new TextureUpload(
			resourceOffset,
			textureRegion,
			cycle
		));
	}

	// Upload

	/// <summary>
	/// Uploads all the data corresponding to the created resources.
	/// </summary>
	public void Upload()
	{
		Flush();
	}

	/// <summary>
	/// Uploads and then blocks until the upload is finished.
	/// This is useful for keeping memory usage down during threaded upload.
	/// </summary>
	public void UploadAndWait()
	{
		Flush(true);
	}

	// Helper methods

	private void Flush(bool wait = false)
	{
		if (TransferBuffer == null) { return; }

		TransferBuffer.Unmap();
		var commandBuffer = Device.AcquireCommandBuffer();
		var copyPass = commandBuffer.BeginCopyPass();
		for (var i = 0; i < BufferUploads.Count; i += 1)
		{
			copyPass.UploadToBuffer(
				new TransferBufferLocation
				{
					TransferBuffer = TransferBuffer.Handle,
					Offset = BufferUploads[i].Offset
				},
				BufferUploads[i].BufferRegion,
				BufferUploads[i].Cycle
			);
		}
		for (var i = 0; i < TextureUploads.Count; i += 1)
		{
			copyPass.UploadToTexture(
				new TextureTransferInfo
				{
					TransferBuffer = TransferBuffer.Handle,
					Offset = TextureUploads[i].Offset
				},
				TextureUploads[i].TextureRegion,
				TextureUploads[i].Cycle
			);
		}
		commandBuffer.EndCopyPass(copyPass);

		if (wait)
		{
			var fence = Device.SubmitAndAcquireFence(commandBuffer);
			Device.WaitForFence(fence);
			Device.ReleaseFence(fence);
		}
		else
		{
			Device.Submit(commandBuffer);
		}

		TransferBuffer.Map(true);
		WriteOffset = 0;
		BufferUploads.Clear();
		TextureUploads.Clear();
	}

	private uint CopyBufferData<T>(Span<T> span) where T : unmanaged
	{
		uint lengthInBytes = (uint) (Marshal.SizeOf<T>() * span.Length);
		CheckAndResizeTransferBuffer(lengthInBytes);

		if (WriteOffset + lengthInBytes > TransferBuffer.Size)
		{
			Flush();
		}

		var resourceOffset = WriteOffset;
		span.CopyTo(TransferBuffer.MappedSpan<T>(resourceOffset));
		WriteOffset += lengthInBytes;

		return resourceOffset;
	}

	private uint CopyTextureData<T>(Span<T> span, uint alignment) where T : unmanaged
	{
		uint lengthInBytes = (uint) (Marshal.SizeOf<T>() * span.Length);
		CheckAndResizeTransferBuffer(lengthInBytes);

		WriteOffset = RoundToAlignment(WriteOffset, alignment);
		if (WriteOffset + lengthInBytes >= TransferBuffer.Size)
		{
			Flush();
		}

		var resourceOffset = WriteOffset;
		span.CopyTo(TransferBuffer.MappedSpan<T>(resourceOffset));
		WriteOffset += lengthInBytes;

		return resourceOffset;
	}

	private void CheckAndResizeTransferBuffer(uint dataLengthInBytes)
	{
		if (TransferBuffer == null)
		{
			TransferBuffer = TransferBuffer.Create<byte>(Device, "ResourceUploader TransferBuffer", TransferBufferUsage.Upload, dataLengthInBytes);
			TransferBuffer.Map(false);
		}
		else if (dataLengthInBytes > TransferBuffer.Size)
		{
			Logger.LogInfo("Resizing resource uploader!");
			Flush();
			TransferBuffer.Unmap();
			TransferBuffer.Dispose();
			TransferBuffer = TransferBuffer.Create<byte>(Device, "ResourceUploader TransferBuffer", TransferBufferUsage.Upload, dataLengthInBytes);
			TransferBuffer.Map(false);
		}
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
				TransferBuffer?.Dispose();
			}
		}
		base.Dispose(disposing);
	}
}
