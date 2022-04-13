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

		private bool IsDisposed;

		public unsafe Packer(GraphicsDevice graphicsDevice, string path, uint textureWidth, uint textureHeight, uint padding = 1)
		{
			var bytes = File.ReadAllBytes(path);
			fixed (byte* pByte = &bytes[0])
			{
				Handle = Wellspring.Wellspring_CreatePacker((IntPtr) pByte, (uint) bytes.Length, textureWidth, textureHeight, 0, padding);
			}

			Texture = Texture.CreateTexture2D(graphicsDevice, textureWidth, textureHeight, TextureFormat.R8, TextureUsageFlags.Sampler);
		}

		public unsafe bool PackFontRanges(params FontRange[] fontRanges)
		{
			fixed (FontRange *pFontRanges = &fontRanges[0])
			{
				var nativeSize = fontRanges.Length * Marshal.SizeOf<Wellspring.FontRange>();
				void* fontRangeMemory = NativeMemory.Alloc((nuint) fontRanges.Length, (nuint) Marshal.SizeOf<Wellspring.FontRange>());
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
