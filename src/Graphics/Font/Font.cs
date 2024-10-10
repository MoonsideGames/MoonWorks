using System;
using System.IO;
using System.Runtime.InteropServices;
using WellspringCS;

namespace MoonWorks.Graphics.Font
{
	public unsafe class Font : GraphicsResource
	{
		public Texture Texture { get; }
		public float PixelsPerEm { get; }
		public float DistanceRange { get; }

		internal IntPtr Handle { get; }

		private byte* StringBytes;
		private int StringBytesLength;

		/// <summary>
		/// Loads a TTF or OTF font from a path for use in MSDF rendering.
		/// Note that there must be an msdf-atlas-gen JSON and image file alongside.
		/// </summary>
		/// <returns></returns>
		public unsafe static Font Load(
			GraphicsDevice graphicsDevice,
			CommandBuffer commandBuffer,
			string fontPath
		) {
			var fontFileStream = new FileStream(fontPath, FileMode.Open, FileAccess.Read);
			var fontFileByteBuffer = NativeMemory.Alloc((nuint) fontFileStream.Length);
			var fontFileByteSpan = new Span<byte>(fontFileByteBuffer, (int) fontFileStream.Length);
			fontFileStream.ReadExactly(fontFileByteSpan);
			fontFileStream.Close();

			var atlasFileStream = new FileStream(Path.ChangeExtension(fontPath, ".json"), FileMode.Open, FileAccess.Read);
			var atlasFileByteBuffer = NativeMemory.Alloc((nuint) atlasFileStream.Length);
			var atlasFileByteSpan = new Span<byte>(atlasFileByteBuffer, (int) atlasFileStream.Length);
			atlasFileStream.ReadExactly(atlasFileByteSpan);
			atlasFileStream.Close();

			var handle = Wellspring.Wellspring_CreateFont(
				(IntPtr) fontFileByteBuffer,
				(uint) fontFileByteSpan.Length,
				(IntPtr) atlasFileByteBuffer,
				(uint) atlasFileByteSpan.Length,
				out float pixelsPerEm,
				out float distanceRange
			);

			var imagePath = Path.ChangeExtension(fontPath, ".png");
			var uploader = new ResourceUploader(graphicsDevice);
			var texture = uploader.CreateTexture2DFromCompressed(imagePath, TextureFormat.R8G8B8A8Unorm, TextureUsageFlags.Sampler);
			uploader.Upload();
			uploader.Dispose();

			NativeMemory.Free(fontFileByteBuffer);
			NativeMemory.Free(atlasFileByteBuffer);

			return new Font(graphicsDevice, handle, texture, pixelsPerEm, distanceRange);
		}

		private Font(GraphicsDevice device, IntPtr handle, Texture texture, float pixelsPerEm, float distanceRange) : base(device)
		{
			Handle = handle;
			Texture = texture;
			PixelsPerEm = pixelsPerEm;
			DistanceRange = distanceRange;

			StringBytesLength = 32;
			StringBytes = (byte*) NativeMemory.Alloc((nuint) StringBytesLength);
		}

		public unsafe bool TextBounds(
			string text,
			int pixelSize,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment,
			out Wellspring.Rectangle rectangle
		) {
			var byteCount = System.Text.Encoding.UTF8.GetByteCount(text);

			if (StringBytesLength < byteCount)
			{
				StringBytes = (byte*) NativeMemory.Realloc(StringBytes, (nuint) byteCount);
			}

			fixed (char* chars = text)
			{
				System.Text.Encoding.UTF8.GetBytes(chars, text.Length, StringBytes, byteCount);

				var result = Wellspring.Wellspring_TextBounds(
					Handle,
					pixelSize,
					(Wellspring.HorizontalAlignment) horizontalAlignment,
					(Wellspring.VerticalAlignment) verticalAlignment,
					(IntPtr) StringBytes,
					(uint) byteCount,
					out rectangle
				);

				if (result == 0)
				{
					Logger.LogWarn("Could not decode string: " + text);
					return false;
				}
			}

			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Texture.Dispose();
				}

				Wellspring.Wellspring_DestroyFont(Handle);
			}
			base.Dispose(disposing);
		}
	}
}
