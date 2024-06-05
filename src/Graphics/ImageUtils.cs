using System;
using System.IO;
using System.Runtime.InteropServices;
using RefreshCS;
using SDL2;

namespace MoonWorks.Graphics
{
	public static class ImageUtils
	{
		/// <summary>
		/// Gets pointer to pixel data from compressed image byte data.
		///
		/// The returned pointer must be freed by calling FreePixelData.
		/// </summary>
		public static unsafe IntPtr GetPixelDataFromBytes(
			Span<byte> data,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			fixed (byte* ptr = data)
			{
				var pixelData =
					Refresh.Refresh_Image_Load(
					ptr,
					data.Length,
					out var w,
					out var h,
					out var len
				);

				width = (uint) w;
				height = (uint) h;
				sizeInBytes = (uint) len;

				return (nint) pixelData;
			}
		}

		/// <summary>
		/// Gets pointer to pixel data from a compressed image stream.
		///
		/// The returned pointer must be freed by calling FreePixelData.
		/// </summary>
		public static unsafe IntPtr GetPixelDataFromStream(
			Stream stream,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var length = stream.Length;
			var buffer = NativeMemory.Alloc((nuint) length);
			var span = new Span<byte>(buffer, (int) length);
			stream.ReadExactly(span);

			var pixelData = GetPixelDataFromBytes(span, out width, out height, out sizeInBytes);

			NativeMemory.Free(buffer);

			return pixelData;
		}

		/// <summary>
		/// Gets pointer to pixel data from a compressed image file.
		///
		/// The returned pointer must be freed by calling FreePixelData.
		/// </summary>
		public static IntPtr GetPixelDataFromFile(
			string path,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return GetPixelDataFromStream(fileStream, out width, out height, out sizeInBytes);
		}

		/// <summary>
		/// Get metadata from compressed image bytes.
		/// </summary>
		public static unsafe bool ImageInfoFromBytes(
			Span<byte> data,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			fixed (byte* ptr = data)
			{
				var result =
					Refresh.Refresh_Image_Info(
					ptr,
					data.Length,
					out var w,
					out var h,
					out var len
				);

				width = (uint) w;
				height = (uint) h;
				sizeInBytes = (uint) len;

				return Conversions.IntToBool(result);
			}
		}

		/// <summary>
		/// Get metadata from a compressed image stream.
		/// </summary>
		public static unsafe bool ImageInfoFromStream(
			Stream stream,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var length = stream.Length;
			var buffer = NativeMemory.Alloc((nuint) length);
			var span = new Span<byte>(buffer, (int) length);
			stream.ReadExactly(span);

			var result = ImageInfoFromBytes(span, out width, out height, out sizeInBytes);

			NativeMemory.Free(buffer);

			return result;
		}

		/// <summary>
		/// Get metadata from a compressed image file.
		/// </summary>
		public static bool ImageInfoFromFile(
			string path,
			out uint width,
			out uint height,
			out uint sizeInBytes
		) {
			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			return ImageInfoFromStream(fileStream, out width, out height, out sizeInBytes);
		}

		/// <summary>
		/// Frees pixel data obtained from GetPixelData methods.
		/// </summary>
		public unsafe static void FreePixelData(IntPtr pixels)
		{
			Refresh.Refresh_Image_Free((byte*) pixels);
		}

		/// <summary>
		/// Saves pixel data contained in a TransferBuffer to a PNG file.
		/// </summary>
		public static unsafe void SavePNG(
			string path,
			TransferBuffer transferBuffer,
			uint bufferOffsetInBytes,
			int width,
			int height,
			bool bgra
		) {
			var sizeInBytes = width * height * 4;

			var pixelsPtr = (byte*) NativeMemory.Alloc((nuint) sizeInBytes);
			var pixelsSpan = new Span<byte>(pixelsPtr, sizeInBytes);

			transferBuffer.GetData(pixelsSpan, bufferOffsetInBytes);

			if (bgra)
			{
				// if data is bgra, we have to swap the R and B channels
				var rgbaPtr = (byte*) NativeMemory.Alloc((nuint) sizeInBytes);
				var rgbaSpan = new Span<byte>(rgbaPtr, sizeInBytes);

				for (var i = 0; i < sizeInBytes; i += 4)
				{
					rgbaSpan[i] = pixelsSpan[i + 2];
					rgbaSpan[i + 1] = pixelsSpan[i + 1];
					rgbaSpan[i + 2] = pixelsSpan[i];
					rgbaSpan[i + 3] = pixelsSpan[i + 3];
				}

				NativeMemory.Free(pixelsPtr);
				pixelsPtr = rgbaPtr;
			}

			Refresh.Refresh_Image_SavePNG(path, pixelsPtr, width, height);
			NativeMemory.Free(pixelsPtr);
		}

		// DDS loading extension, based on MojoDDS
		// Taken from https://github.com/FNA-XNA/FNA/blob/1e49f868f595f62bc6385db45949a03186a7cd7f/src/Graphics/Texture.cs#L194
		public static void ParseDDS(
			BinaryReader reader,
			out TextureFormat format,
			out int width,
			out int height,
			out int levels,
			out bool isCube
		) {
			// A whole bunch of magic numbers, yay DDS!
			const uint DDS_MAGIC = 0x20534444;
			const uint DDS_HEADERSIZE = 124;
			const uint DDS_PIXFMTSIZE = 32;
			const uint DDSD_HEIGHT = 0x2;
			const uint DDSD_WIDTH = 0x4;
			const uint DDSD_PITCH = 0x8;
			const uint DDSD_LINEARSIZE = 0x80000;
			const uint DDSD_REQ = (
				DDSD_HEIGHT | DDSD_WIDTH
			);
			const uint DDSCAPS_MIPMAP = 0x400000;
			const uint DDSCAPS_TEXTURE = 0x1000;
			const uint DDSCAPS2_CUBEMAP = 0x200;
			const uint DDPF_FOURCC = 0x4;
			const uint DDPF_RGB = 0x40;
			const uint FOURCC_DXT1 = 0x31545844;
			const uint FOURCC_DXT3 = 0x33545844;
			const uint FOURCC_DXT5 = 0x35545844;
			const uint FOURCC_DX10 = 0x30315844;
			const uint pitchAndLinear = (
				DDSD_PITCH | DDSD_LINEARSIZE
			);

			// File should start with 'DDS '
			if (reader.ReadUInt32() != DDS_MAGIC)
			{
				throw new NotSupportedException("Not a DDS!");
			}

			// Texture info
			uint size = reader.ReadUInt32();
			if (size != DDS_HEADERSIZE)
			{
				throw new NotSupportedException("Invalid DDS header!");
			}
			uint flags = reader.ReadUInt32();
			if ((flags & DDSD_REQ) != DDSD_REQ)
			{
				throw new NotSupportedException("Invalid DDS flags!");
			}
			if ((flags & pitchAndLinear) == pitchAndLinear)
			{
				throw new NotSupportedException("Invalid DDS flags!");
			}
			height = reader.ReadInt32();
			width = reader.ReadInt32();
			reader.ReadUInt32(); // dwPitchOrLinearSize, unused
			reader.ReadUInt32(); // dwDepth, unused
			levels = reader.ReadInt32();

			// "Reserved"
			reader.ReadBytes(4 * 11);

			// Format info
			uint formatSize = reader.ReadUInt32();
			if (formatSize != DDS_PIXFMTSIZE)
			{
				throw new NotSupportedException("Bogus PIXFMTSIZE!");
			}
			uint formatFlags = reader.ReadUInt32();
			uint formatFourCC = reader.ReadUInt32();
			uint formatRGBBitCount = reader.ReadUInt32();
			uint formatRBitMask = reader.ReadUInt32();
			uint formatGBitMask = reader.ReadUInt32();
			uint formatBBitMask = reader.ReadUInt32();
			uint formatABitMask = reader.ReadUInt32();

			// dwCaps "stuff"
			uint caps = reader.ReadUInt32();
			if ((caps & DDSCAPS_TEXTURE) == 0)
			{
				throw new NotSupportedException("Not a texture!");
			}

			isCube = false;

			uint caps2 = reader.ReadUInt32();
			if (caps2 != 0)
			{
				if ((caps2 & DDSCAPS2_CUBEMAP) == DDSCAPS2_CUBEMAP)
				{
					isCube = true;
				}
				else
				{
					throw new NotSupportedException("Invalid caps2!");
				}
			}

			reader.ReadUInt32(); // dwCaps3, unused
			reader.ReadUInt32(); // dwCaps4, unused

			// "Reserved"
			reader.ReadUInt32();

			// Mipmap sanity check
			if ((caps & DDSCAPS_MIPMAP) != DDSCAPS_MIPMAP)
			{
				levels = 1;
			}

			// Determine texture format
			if ((formatFlags & DDPF_FOURCC) == DDPF_FOURCC)
			{
				switch (formatFourCC)
				{
					case 0x71: // D3DFMT_A16B16G16R16F
						format = TextureFormat.R16G16B16A16_SFLOAT;
						break;
					case 0x74: // D3DFMT_A32B32G32R32F
						format = TextureFormat.R32G32B32A32_SFLOAT;
						break;
					case FOURCC_DXT1:
						format = TextureFormat.BC1;
						break;
					case FOURCC_DXT3:
						format = TextureFormat.BC2;
						break;
					case FOURCC_DXT5:
						format = TextureFormat.BC3;
						break;
					case FOURCC_DX10:
						// If the fourCC is DX10, there is an extra header with additional format information.
						uint dxgiFormat = reader.ReadUInt32();

						// These values are taken from the DXGI_FORMAT enum.
						switch (dxgiFormat)
						{
							case 2:
								format = TextureFormat.R32G32B32A32_SFLOAT;
								break;

							case 10:
								format = TextureFormat.R16G16B16A16_SFLOAT;
								break;

							case 71:
								format = TextureFormat.BC1;
								break;

							case 74:
								format = TextureFormat.BC2;
								break;

							case 77:
								format = TextureFormat.BC3;
								break;

							case 98:
								format = TextureFormat.BC7;
								break;

							default:
								throw new NotSupportedException(
									"Unsupported DDS texture format"
								);
						}

						uint resourceDimension = reader.ReadUInt32();

						// These values are taken from the D3D10_RESOURCE_DIMENSION enum.
						switch (resourceDimension)
						{
							case 0: // Unknown
							case 1: // Buffer
								throw new NotSupportedException(
									"Unsupported DDS texture format"
								);
							default:
								break;
						}

						/*
						 * This flag seemingly only indicates if the texture is a cube map.
						 * This is already determined above. Cool!
						 */
						uint miscFlag = reader.ReadUInt32();

						/*
						 * Indicates the number of elements in the texture array.
						 * We don't support texture arrays so just throw if it's greater than 1.
						 */
						uint arraySize = reader.ReadUInt32();

						if (arraySize > 1)
						{
							throw new NotSupportedException(
								"Unsupported DDS texture format"
							);
						}

						reader.ReadUInt32(); // reserved

						break;
					default:
						throw new NotSupportedException(
							"Unsupported DDS texture format"
						);
				}
			}
			else if ((formatFlags & DDPF_RGB) == DDPF_RGB)
			{
				if (	formatRGBBitCount != 32 ||
					formatRBitMask != 0x00FF0000 ||
					formatGBitMask != 0x0000FF00 ||
					formatBBitMask != 0x000000FF ||
					formatABitMask != 0xFF000000	)
				{
					throw new NotSupportedException(
						"Unsupported DDS texture format"
					);
				}

				format = TextureFormat.B8G8R8A8;
			}
			else
			{
				throw new NotSupportedException(
					"Unsupported DDS texture format"
				);
			}
		}

		public static int CalculateDDSLevelSize(
			int width,
			int height,
			TextureFormat format
		) {
			if (format == TextureFormat.R8G8B8A8)
			{
				return (((width * 32) + 7) / 8) * height;
			}
			else if (format == TextureFormat.R16G16B16A16_SFLOAT)
			{
				return (((width * 64) + 7) / 8) * height;
			}
			else if (format == TextureFormat.R32G32B32A32_SFLOAT)
			{
				return (((width * 128) + 7) / 8) * height;
			}
			else
			{
				int blockSize = 16;
				if (format == TextureFormat.BC1)
				{
					blockSize = 8;
				}
				width = System.Math.Max(width, 1);
				height = System.Math.Max(height, 1);
				return (
					((width + 3) / 4) *
					((height + 3) / 4) *
					blockSize
				);
			}
		}
	}
}
