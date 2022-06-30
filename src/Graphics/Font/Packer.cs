using System;
using System.IO;
using System.Runtime.InteropServices;
using WellspringCS;

namespace MoonWorks.Graphics.Font
{
	public class Packer : IDisposable
	{
		public IntPtr Handle { get; }
		public Texture Texture { get; }

		public Font Font { get; }

		private byte[] StringBytes;

		private bool IsDisposed;

		public unsafe Packer(GraphicsDevice graphicsDevice, Font font, float fontSize, uint textureWidth, uint textureHeight, uint padding = 1)
		{
			Font = font;
			Handle = Wellspring.Wellspring_CreatePacker(Font.Handle, fontSize, textureWidth, textureHeight, 0, padding);
			Texture = Texture.CreateTexture2D(graphicsDevice, textureWidth, textureHeight, TextureFormat.R8, TextureUsageFlags.Sampler);
			StringBytes = new byte[128];
		}

		public unsafe bool PackFontRanges(params FontRange[] fontRanges)
		{
			fixed (FontRange *pFontRanges = &fontRanges[0])
			{
				var nativeSize = fontRanges.Length * sizeof(Wellspring.FontRange);
				void* fontRangeMemory = NativeMemory.Alloc((nuint) fontRanges.Length, (nuint) sizeof(Wellspring.FontRange));
				System.Buffer.MemoryCopy(pFontRanges, fontRangeMemory, nativeSize, nativeSize);

				var result = Wellspring.Wellspring_PackFontRanges(Handle, (IntPtr) fontRangeMemory, (uint) fontRanges.Length);

				NativeMemory.Free(fontRangeMemory);

				return result > 0;
			}
		}

		public unsafe void SetTextureData(CommandBuffer commandBuffer)
		{
			var pixelDataPointer = Wellspring.Wellspring_GetPixelDataPointer(Handle);
			commandBuffer.SetTextureData(Texture, pixelDataPointer, Texture.Width * Texture.Height);
		}

		public unsafe void TextBounds(
			string text,
			float x,
			float y,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment,
			out Wellspring.Rectangle rectangle
		) {
			var byteCount = System.Text.Encoding.UTF8.GetByteCount(text);

			if (StringBytes.Length < byteCount)
			{
				System.Array.Resize(ref StringBytes, byteCount);
			}

			fixed (char* chars = text)
			fixed (byte* bytes = StringBytes)
			{
				System.Text.Encoding.UTF8.GetBytes(chars, text.Length, bytes, byteCount);
				Wellspring.Wellspring_TextBounds(
					Handle,
					x,
					y,
					(Wellspring.HorizontalAlignment) horizontalAlignment,
					(Wellspring.VerticalAlignment) verticalAlignment,
					(IntPtr) bytes,
					(uint) byteCount,
					out rectangle
				);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Texture.Dispose();
				}

				Wellspring.Wellspring_DestroyPacker(Handle);

				IsDisposed = true;
			}
		}

		~Packer()
		{
		    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		    Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
