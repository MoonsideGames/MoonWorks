using System;
using System.Runtime.InteropServices;
using MoonWorks.Storage;
using SDL3;

namespace MoonWorks.Graphics;

public static class ImageUtils
{
	/// <summary>
	/// Gets pointer to pixel data from compressed image byte data.
	///
	/// The returned pointer must be freed by calling FreePixelData.
	/// </summary>
	public static unsafe byte* GetPixelDataFromBytes(
		ReadOnlySpan<byte> data,
		out uint width,
		out uint height,
		out uint sizeInBytes
	) {
		fixed (byte* ptr = data)
		{
			var pixelData =
				IRO.IRO_LoadImage(
				(nint) ptr,
				(uint) data.Length,
				out var w,
				out var h,
				out var len
			);

			width = w;
			height = h;
			sizeInBytes = len;

			return (byte*) pixelData;
		}
	}

	/// <summary>
	/// Gets pointer to pixel data from a compressed image file.
	///
	/// The returned pointer must be freed by calling FreePixelData.
	/// </summary>
	public static unsafe byte* GetPixelDataFromFile(
		TitleStorage storage,
		string path,
		out uint width,
		out uint height,
		out uint sizeInBytes
	) {
		width = 0;
		height = 0;
		sizeInBytes = 0;

		if (!storage.GetFileSize(path, out var size))
		{
			return null;
		}

		var buffer = NativeMemory.Alloc((nuint) size);
		var span = new Span<byte>(buffer, (int) size);
		if (!storage.ReadFile(path, span))
		{
			return null;
		}

		var result = GetPixelDataFromBytes(span, out width, out height, out sizeInBytes);
		NativeMemory.Free(buffer);
		return result;
	}

	/// <summary>
	/// Get metadata from compressed image bytes.
	/// </summary>
	public static unsafe bool ImageInfoFromBytes(
		ReadOnlySpan<byte> data,
		out uint width,
		out uint height,
		out uint sizeInBytes
	) {
		fixed (byte* ptr = data)
		{
			var result =
				IRO.IRO_GetImageInfo(
				(nint) ptr,
				(uint) data.Length,
				out var w,
				out var h,
				out var len
			);

			width = w;
			height = h;
			sizeInBytes = len;

			return result;
		}
	}

	/// <summary>
	/// Get metadata from a compressed image file.
	/// </summary>
	public static unsafe bool ImageInfoFromFile(
		TitleStorage storage,
		string path,
		out uint width,
		out uint height,
		out uint sizeInBytes
	) {
		width = 0;
		height = 0;
		sizeInBytes = 0;

		if (!storage.GetFileSize(path, out var size))
		{
			return false;
		}

		var buffer = NativeMemory.Alloc((nuint) size);
		var span = new Span<byte>(buffer, (int) size);
		if (!storage.ReadFile(path, span))
		{
			return false;
		}

		var result = ImageInfoFromBytes(span, out width, out height, out sizeInBytes);
		NativeMemory.Free(buffer);

		return result;
	}

	/// <summary>
	/// Frees pixel data obtained from GetPixelData methods.
	/// </summary>
	public unsafe static void FreePixelData(byte* pixels)
	{
		IRO.IRO_FreeImage((nint) pixels);
	}

	/// <summary>
	/// Encodes pixel data to a PNG buffer.
	/// You must call FreePNGBuffer when you are done with the data.
	/// </summary>
	public static unsafe IntPtr EncodePNGBuffer(
		Span<Color> pixels,
		uint width,
		uint height,
		bool bgra,
		out int size
	) {
		IntPtr pngBuffer;

		if (bgra)
		{
			var bgraPtr = NativeMemory.Alloc(width * height * 4);
			var bgraColors = new Span<Color>(bgraPtr, (int) (width * height * 4));
			for (var i = 0; i < width * height; i += 1)
			{
				bgraColors[i].R = pixels[i].B;
				bgraColors[i].G = pixels[i].G;
				bgraColors[i].B = pixels[i].R;
				bgraColors[i].A = pixels[i].A;
			}

			pngBuffer = IRO.IRO_EncodePNG((nint) bgraPtr, width, height, out size);
			NativeMemory.Free(bgraPtr);
		}
		else
		{
			fixed (Color* pixelPtr = pixels)
			{
				pngBuffer = IRO.IRO_EncodePNG((nint) pixelPtr, width, height, out size);
			}
		}

		return pngBuffer;
	}

	public static unsafe void FreePNGBuffer(IntPtr buffer)
	{
		SDL.SDL_free(buffer);
	}

	// DDS loading extension, based on MojoDDS
	// Taken from https://github.com/FNA-XNA/FNA/blob/1e49f868f595f62bc6385db45949a03186a7cd7f/src/Graphics/Texture.cs#L194
	internal static bool ParseDDS(
		ref ByteSpanReader reader,
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

		// Assign defaults
		format = TextureFormat.Invalid;
		height = 0;
		width = 0;
		levels = 0;
		isCube = false;

		// File should start with 'DDS '
		if (reader.Read<uint>() != DDS_MAGIC)
		{
			Logger.LogError("Not a DDS!");
			return false;
		}

		// Texture info
		uint size = reader.Read<uint>();
		if (size != DDS_HEADERSIZE)
		{
			Logger.LogError("Invalid DDS header!");
			return false;
		}
		uint flags = reader.Read<uint>();
		if ((flags & DDSD_REQ) != DDSD_REQ)
		{
			Logger.LogError("Invalid DDS flags!");
			return false;
		}
		if ((flags & pitchAndLinear) == pitchAndLinear)
		{
			Logger.LogError("Invalid DDS flags!");
			return false;
		}
		height = reader.Read<int>();
		width = reader.Read<int>();
		reader.Read<uint>(); // dwPitchOrLinearSize, unused
		reader.Read<uint>(); // dwDepth, unused
		levels = reader.Read<int>();

		// "Reserved"
		reader.Advance(4 * 11);

		// Format info
		uint formatSize = reader.Read<uint>();
		if (formatSize != DDS_PIXFMTSIZE)
		{
			Logger.LogError("Bogus PIXFMTSIZE!");
			return false;
		}
		uint formatFlags = reader.Read<uint>();
		uint formatFourCC = reader.Read<uint>();
		uint formatRGBBitCount = reader.Read<uint>();
		uint formatRBitMask = reader.Read<uint>();
		uint formatGBitMask = reader.Read<uint>();
		uint formatBBitMask = reader.Read<uint>();
		uint formatABitMask = reader.Read<uint>();

		// dwCaps "stuff"
		uint caps = reader.Read<uint>();
		if ((caps & DDSCAPS_TEXTURE) == 0)
		{
			Logger.LogError("Not a texture!");
			return false;
		}

		isCube = false;

		uint caps2 = reader.Read<uint>();
		if (caps2 != 0)
		{
			if ((caps2 & DDSCAPS2_CUBEMAP) == DDSCAPS2_CUBEMAP)
			{
				isCube = true;
			}
			else
			{
				Logger.LogError("Invalid caps2!");
				return false;
			}
		}

		reader.Read<uint>(); // dwCaps3, unused
		reader.Read<uint>(); // dwCaps4, unused

		// "Reserved"
		reader.Read<uint>();

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
					format = TextureFormat.R16G16B16A16Float;
					break;
				case 0x74: // D3DFMT_A32B32G32R32F
					format = TextureFormat.R32G32B32A32Float;
					break;
				case FOURCC_DXT1:
					format = TextureFormat.BC1_RGBAUnorm;
					break;
				case FOURCC_DXT3:
					format = TextureFormat.BC2_RGBAUnorm;
					break;
				case FOURCC_DXT5:
					format = TextureFormat.BC3_RGBAUnorm;
					break;
				case FOURCC_DX10:
					// If the fourCC is DX10, there is an extra header with additional format information.
					uint dxgiFormat = reader.Read<uint>();

					// These values are taken from the DXGI_FORMAT enum.
					switch (dxgiFormat)
					{
						case 2:
							format = TextureFormat.R32G32B32A32Float;
							break;

						case 10:
							format = TextureFormat.R16G16B16A16Float;
							break;

						case 71:
							format = TextureFormat.BC1_RGBAUnorm;
							break;

						case 74:
							format = TextureFormat.BC2_RGBAUnorm;
							break;

						case 77:
							format = TextureFormat.BC3_RGBAUnorm;
							break;

						case 98:
							format = TextureFormat.BC7_RGBAUnorm;
							break;

						default:
							Logger.LogError("Unsupported DDS texture format");
							return false;
					}

					uint resourceDimension = reader.Read<uint>();

					// These values are taken from the D3D10_RESOURCE_DIMENSION enum.
					switch (resourceDimension)
					{
						case 0: // Unknown
						case 1: // Buffer
							Logger.LogError("Unsupported DDS texture format");
							return false;
						default:
							break;
					}

					/*
					 * This flag seemingly only indicates if the texture is a cube map.
					 * This is already determined above. Cool!
					 */
					uint miscFlag = reader.Read<uint>();

					/*
					 * Indicates the number of elements in the texture array.
					 * We don't support texture arrays so just return false if it's greater than 1.
					 */
					uint arraySize = reader.Read<uint>();

					if (arraySize > 1)
					{
						Logger.LogError("Unsupported DDS texture format");
						return false;
					}

					reader.Read<uint>(); // reserved

					break;
				default:
					Logger.LogError("Unsupported DDS texture format");
					return false;
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
				Logger.LogError("Unsupported DDS texture format");
				return false;
			}

			format = TextureFormat.B8G8R8A8Unorm;
		}
		else
		{
			Logger.LogError("Unsupported DDS texture format");
			return false;
		}

		return true;
	}

	public static int CalculateDDSLevelSize(
		int width,
		int height,
		TextureFormat format
	) {
		if (format == TextureFormat.R8G8B8A8Unorm)
		{
			return (((width * 32) + 7) / 8) * height;
		}
		else if (format == TextureFormat.R16G16B16A16Float)
		{
			return (((width * 64) + 7) / 8) * height;
		}
		else if (format == TextureFormat.R32G32B32A32Float)
		{
			return (((width * 128) + 7) / 8) * height;
		}
		else
		{
			int blockSize = 16;
			if (format == TextureFormat.BC1_RGBAUnorm)
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
