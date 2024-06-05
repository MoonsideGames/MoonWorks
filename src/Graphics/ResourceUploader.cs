using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A convenience structure for creating resources and uploading their data.
	///
	/// Note that Upload or UploadAndWait must be called after the Create methods for the data to actually be uploaded.
	///
	/// Note that this structure does not magically keep memory usage down -
	/// you may want to stagger uploads over multiple submissions to minimize memory usage.
	/// </summary>
	public unsafe class ResourceUploader : GraphicsResource
	{
		TransferBuffer BufferTransferBuffer;
		TransferBuffer TextureTransferBuffer;

		byte* bufferData;
		uint bufferDataOffset = 0;
		uint bufferDataSize = 1024;

		byte* textureData;
		uint textureDataOffset = 0;
		uint textureDataSize = 1024;

		List<(GpuBuffer, BufferCopy, bool)> BufferUploads = new List<(GpuBuffer, BufferCopy, bool)>();
		List<(TextureRegion, uint, bool)> TextureUploads = new List<(TextureRegion, uint, bool)>();

		public ResourceUploader(GraphicsDevice device) : base(device)
		{
			bufferData = (byte*) NativeMemory.Alloc(bufferDataSize);
			textureData = (byte*) NativeMemory.Alloc(textureDataSize);
		}

		// Buffers

		/// <summary>
		/// Creates a GpuBuffer with data to be uploaded.
		/// </summary>
		public GpuBuffer CreateBuffer<T>(Span<T> data, BufferUsageFlags usageFlags) where T : unmanaged
		{
			var lengthInBytes = (uint) (Marshal.SizeOf<T>() * data.Length);
			var gpuBuffer = new GpuBuffer(Device, usageFlags, lengthInBytes);

			SetBufferData(gpuBuffer, 0, data, false);

			return gpuBuffer;
		}

		/// <summary>
		/// Prepares upload of data into a GpuBuffer.
		/// </summary>
		public void SetBufferData<T>(GpuBuffer buffer, uint bufferOffsetInElements, Span<T> data, bool cycle) where T : unmanaged
		{
			uint elementSize = (uint) Marshal.SizeOf<T>();
			uint offsetInBytes = elementSize * bufferOffsetInElements;
			uint lengthInBytes = (uint) (elementSize * data.Length);

			uint resourceOffset;
			fixed (void* spanPtr = data)
			{
				resourceOffset = CopyBufferData(spanPtr, lengthInBytes);
			}

			var bufferCopyParams = new BufferCopy(resourceOffset, offsetInBytes, lengthInBytes);
			BufferUploads.Add((buffer, bufferCopyParams, cycle));
		}

		// Textures

		public Texture CreateTexture2D<T>(Span<T> pixelData, uint width, uint height) where T : unmanaged
		{
			var texture = Texture.CreateTexture2D(Device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler);
			SetTextureData(texture, pixelData, false);
			return texture;
		}

		/// <summary>
		/// Creates a 2D Texture from compressed image data to be uploaded.
		/// </summary>
		public Texture CreateTexture2DFromCompressed(Span<byte> compressedImageData)
		{
			ImageUtils.ImageInfoFromBytes(compressedImageData, out var width, out var height, out var _);
			var texture = Texture.CreateTexture2D(Device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler);
			SetTextureDataFromCompressed(texture, compressedImageData);
			return texture;
		}

		/// <summary>
		/// Creates a 2D Texture from a compressed image stream to be uploaded.
		/// </summary>
		public Texture CreateTexture2DFromCompressed(Stream compressedImageStream)
		{
			var length = compressedImageStream.Length;
			var buffer = NativeMemory.Alloc((nuint) length);
			var span = new Span<byte>(buffer, (int) length);
			compressedImageStream.ReadExactly(span);

			var texture = CreateTexture2DFromCompressed(span);

			NativeMemory.Free(buffer);

			return texture;
		}

		/// <summary>
		/// Creates a 2D Texture from a compressed image file to be uploaded.
		/// </summary>
		public Texture CreateTexture2DFromCompressed(string compressedImageFilePath)
		{
			var fileStream = new FileStream(compressedImageFilePath, FileMode.Open, FileAccess.Read);
			return CreateTexture2DFromCompressed(fileStream);
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
						TextureSlice = new TextureSlice
						{
							Texture = texture,
							Layer = (uint) face,
							MipLevel = (uint) level
						},
						X = 0,
						Y = 0,
						Z = 0,
						Width = (uint) levelWidth,
						Height = (uint) levelHeight,
						Depth = 1
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

			TextureUploads.Add((textureRegion, resourceOffset, cycle));
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
			Device.WaitForFences(fence);
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
					BufferTransferBuffer = new TransferBuffer(Device, TransferUsage.Buffer, TransferBufferMapFlags.Write, bufferDataSize);
				}

				var dataSpan = new Span<byte>(bufferData, (int) bufferDataSize);
				BufferTransferBuffer.SetData(dataSpan, true);
			}


			if (TextureUploads.Count > 0)
			{
				if (TextureTransferBuffer == null || TextureTransferBuffer.Size < textureDataSize)
				{
					TextureTransferBuffer?.Dispose();
					TextureTransferBuffer = new TransferBuffer(Device, TransferUsage.Texture, TransferBufferMapFlags.Write, textureDataSize);
				}

				var dataSpan = new Span<byte>(textureData, (int) textureDataSize);
				TextureTransferBuffer.SetData(dataSpan, true);
			}
		}

		private void RecordUploadCommands(CommandBuffer commandBuffer)
		{
			var copyPass = commandBuffer.BeginCopyPass();

			foreach (var (gpuBuffer, bufferCopyParams, option) in BufferUploads)
			{
				copyPass.UploadToBuffer(
					BufferTransferBuffer,
					gpuBuffer,
					bufferCopyParams,
					option
				);
			}

			foreach (var (textureRegion, offset, option) in TextureUploads)
			{
				copyPass.UploadToTexture(
					TextureTransferBuffer,
					textureRegion,
					new BufferImageCopy(
						offset,
						0,
						0
					),
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
}
