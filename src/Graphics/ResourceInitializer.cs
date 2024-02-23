using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A convenience structure for simultaneously creating resources and uploading their data.
	///
	/// Note that Upload must be called after the Create methods for the data to actually be uploaded.
	/// </summary>
	public unsafe class ResourceInitializer : GraphicsResource
	{
		TransferBuffer TransferBuffer;

		byte* data;
		uint dataOffset = 0;
		uint dataSize = 1024;

		List<(GpuBuffer, uint, uint)> BufferUploads = new List<(GpuBuffer, uint, uint)>();
		List<(TextureSlice, uint)> TextureUploads = new List<(TextureSlice, uint)>();

		public ResourceInitializer(GraphicsDevice device) : base(device)
		{
			data = (byte*) NativeMemory.Alloc(dataSize);
		}

		/// <summary>
		/// Creates a GpuBuffer with data to be uploaded.
		/// </summary>
		public GpuBuffer CreateBuffer<T>(Span<T> data, BufferUsageFlags usageFlags) where T : unmanaged
		{
			var lengthInBytes = (uint) (Marshal.SizeOf<T>() * data.Length);
			var gpuBuffer = new GpuBuffer(Device, usageFlags, lengthInBytes);

			uint resourceOffset;
			fixed (void* spanPtr = data)
			{
				resourceOffset = CopyData(spanPtr, lengthInBytes);
			}

			BufferUploads.Add((gpuBuffer, resourceOffset, lengthInBytes));

			return gpuBuffer;
		}

		/// <summary>
		/// Creates a 2D Texture from compressed image data to be uploaded.
		/// </summary>
		public Texture CreateTexture2D(Span<byte> data)
		{
			var pixelData = ImageUtils.GetPixelDataFromBytes(data, out var width, out var height, out var lengthInBytes);
			var texture = Texture.CreateTexture2D(Device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler);

			var resourceOffset = CopyData((void*) pixelData, texture.Size);
			ImageUtils.FreePixelData(pixelData);

			TextureUploads.Add((texture, resourceOffset));
			return texture;
		}

		/// <summary>
		/// Creates a 2D Texture from a compressed image stream to be uploaded.
		/// </summary>
		public Texture CreateTexture2D(Stream stream)
		{
			var length = stream.Length;
			var buffer = NativeMemory.Alloc((nuint) length);
			var span = new Span<byte>(buffer, (int) length);
			stream.ReadExactly(span);

			var texture = CreateTexture2D(span);

			NativeMemory.Free(buffer);

			return texture;
		}

		/// <summary>
		/// Creates a 2D Texture from a compressed image file to be uploaded.
		/// </summary>
		public Texture CreateTexture2D(string path)
		{
			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return CreateTexture2D(fileStream);
		}

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

					var textureSlice = new TextureSlice
					{
						Texture = texture,
						MipLevel = (uint) level,
						BaseLayer = (uint) face,
						LayerCount = 1,
						X = 0,
						Y = 0,
						Z = 0,
						Width = (uint) levelWidth,
						Height = (uint) levelHeight,
						Depth = 1
					};

					var resourceOffset = CopyDataAligned(byteBuffer, (uint) levelSize, Texture.TexelSize(format));
					TextureUploads.Add((textureSlice, resourceOffset));

					NativeMemory.Free(byteBuffer);
				}
			}

			return texture;
		}

		public Texture CreateTextureFromDDS(string path)
		{
			var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return CreateTextureFromDDS(stream);
		}

		/// <summary>
		/// Uploads all the data corresponding to the created resources.
		/// </summary>
		public void Upload()
		{
			if (TransferBuffer == null || TransferBuffer.Size < dataSize)
			{
				TransferBuffer?.Dispose();
				TransferBuffer = new TransferBuffer(Device, dataSize);
			}

			var dataSpan = new Span<byte>(data, (int) dataSize);
			TransferBuffer.SetData(dataSpan, SetDataOptions.Discard);

			var commandBuffer = Device.AcquireCommandBuffer();

			commandBuffer.BeginCopyPass();

			foreach (var (gpuBuffer, offset, size) in BufferUploads)
			{
				commandBuffer.UploadToBuffer(
					TransferBuffer,
					gpuBuffer,
					new BufferCopy(
						offset,
						0,
						size
					)
				);
			}

			foreach (var (textureSlice, offset) in TextureUploads)
			{
				commandBuffer.UploadToTexture(
					TransferBuffer,
					textureSlice,
					new BufferImageCopy(
						offset,
						0,
						0
					)
				);
			}

			commandBuffer.EndCopyPass();
			Device.Submit(commandBuffer);

			BufferUploads.Clear();
			TextureUploads.Clear();
			dataOffset = 0;
		}

		private uint CopyData(void* ptr, uint lengthInBytes)
		{
			if (dataOffset + lengthInBytes >= dataSize)
			{
				dataSize = dataOffset + lengthInBytes;
				data = (byte*) NativeMemory.Realloc(data, dataSize);
			}

			var resourceOffset = dataOffset;

			NativeMemory.Copy(ptr, data + dataOffset, lengthInBytes);
			dataOffset += lengthInBytes;

			return resourceOffset;
		}

		private uint CopyDataAligned(void* ptr, uint lengthInBytes, uint alignment)
		{
			dataOffset = RoundToAlignment(dataOffset, alignment);
			return CopyData(ptr, lengthInBytes);
		}

		private uint RoundToAlignment(uint value, uint alignment)
		{
			return alignment * ((value + alignment - 1) / alignment);
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					TransferBuffer.Dispose();
				}

				NativeMemory.Free(data);
			}
			base.Dispose(disposing);
		}
	}
}
