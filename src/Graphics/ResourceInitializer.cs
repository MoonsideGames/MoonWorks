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
		List<(Texture, uint, uint)> TextureUploads = new List<(Texture, uint, uint)>();

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

			BufferUploads.Add((gpuBuffer, dataOffset, lengthInBytes));

			ResizeDataIfNeeded(lengthInBytes);

			fixed (void* spanPtr = data)
			{
				CopyData(spanPtr, lengthInBytes);
			}

			return gpuBuffer;
		}

		/// <summary>
		/// Creates a 2D Texture from compressed image data to be uploaded.
		/// </summary>
		public Texture CreateTexture2D(Span<byte> data)
		{
			var pixelData = ImageUtils.GetPixelDataFromBytes(data, out var width, out var height, out var lengthInBytes);
			var texture = Texture.CreateTexture2D(Device, width, height, TextureFormat.R8G8B8A8, TextureUsageFlags.Sampler);
			TextureUploads.Add((texture, dataOffset, lengthInBytes));

			ResizeDataIfNeeded(lengthInBytes);
			CopyData((void*) pixelData, lengthInBytes);
			ImageUtils.FreePixelData(pixelData);

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

			foreach (var (texture, offset, size) in TextureUploads)
			{
				commandBuffer.UploadToTexture(
					TransferBuffer,
					texture,
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

		private void ResizeDataIfNeeded(uint lengthInBytes)
		{
			if (dataOffset + lengthInBytes >= dataSize)
			{
				dataSize = dataOffset + lengthInBytes;
				data = (byte*) NativeMemory.Realloc(data, dataSize);
			}
		}

		private void CopyData(void* ptr, uint lengthInBytes)
		{
			NativeMemory.Copy(ptr, data + dataOffset, lengthInBytes);
			dataOffset += lengthInBytes;
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
