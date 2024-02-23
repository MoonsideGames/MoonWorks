using System;
using System.IO;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A container for pixel data.
	/// </summary>
	public class Texture : RefreshResource
	{
		public uint Width { get; internal set; }
		public uint Height { get; internal set; }
		public uint Depth { get; }
		public TextureFormat Format { get; internal set; }
		public bool IsCube { get; }
		public uint LevelCount { get; }
		public SampleCount SampleCount { get; }
		public TextureUsageFlags UsageFlags { get; }
		public uint Size { get; }

		// FIXME: this allocates a delegate instance
		protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyTexture;

		/// <summary>
		/// Creates a 2D texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateTexture2D(
			GraphicsDevice device,
			uint width,
			uint height,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1,
			SampleCount sampleCount = SampleCount.One
		) {
			var textureCreateInfo = new TextureCreateInfo
			{
				Width = width,
				Height = height,
				Depth = 1,
				IsCube = false,
				LevelCount = levelCount,
				SampleCount = sampleCount,
				Format = format,
				UsageFlags = usageFlags
			};

			return new Texture(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a 3D texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="depth">The depth of the texture.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateTexture3D(
			GraphicsDevice device,
			uint width,
			uint height,
			uint depth,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1
		) {
			var textureCreateInfo = new TextureCreateInfo
			{
				Width = width,
				Height = height,
				Depth = depth,
				IsCube = false,
				LevelCount = levelCount,
				Format = format,
				UsageFlags = usageFlags
			};

			return new Texture(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a cube texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="size">The length of one side of the cube.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateTextureCube(
			GraphicsDevice device,
			uint size,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1
		) {
			var textureCreateInfo = new TextureCreateInfo
			{
				Width = size,
				Height = size,
				Depth = 1,
				IsCube = true,
				LevelCount = levelCount,
				Format = format,
				UsageFlags = usageFlags
			};

			return new Texture(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a new texture using a TextureCreateInfo struct.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="textureCreateInfo">The parameters to use when creating the texture.</param>
		public Texture(
			GraphicsDevice device,
			in TextureCreateInfo textureCreateInfo
		) : base(device)
		{
			Handle = Refresh.Refresh_CreateTexture(
				device.Handle,
				textureCreateInfo.ToRefreshTextureCreateInfo()
			);

			Format = textureCreateInfo.Format;
			Width = textureCreateInfo.Width;
			Height = textureCreateInfo.Height;
			Depth = textureCreateInfo.Depth;
			IsCube = textureCreateInfo.IsCube;
			LevelCount = textureCreateInfo.LevelCount;
			SampleCount = textureCreateInfo.SampleCount;
			UsageFlags = textureCreateInfo.UsageFlags;
			Size = Width * Height * BytesPerPixel(Format) / BlockSizeSquared(Format);
		}

		public static implicit operator TextureSlice(Texture t) => new TextureSlice(t);

		// Used by AcquireSwapchainTexture.
		// Should not be tracked, because swapchain textures are managed by Vulkan.
		internal Texture(
			GraphicsDevice device,
			TextureFormat format
		) : base(device)
		{
			Handle = IntPtr.Zero;

			Format = format;
			Width = 0;
			Height = 0;
			Depth = 1;
			IsCube = false;
			LevelCount = 1;
			SampleCount = SampleCount.One;
			UsageFlags = TextureUsageFlags.ColorTarget;
			Size = Width * Height * BytesPerPixel(Format) / BlockSizeSquared(Format);
		}

		// DDS loading extension, based on MojoDDS
		// Taken from https://github.com/FNA-XNA/FNA/blob/1e49f868f595f62bc6385db45949a03186a7cd7f/src/Graphics/Texture.cs#L194
		private static void ParseDDS(
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

		private static int CalculateDDSLevelSize(
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

		public static uint BytesPerPixel(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.R8:
				case TextureFormat.R8_UINT:
					return 1;
				case TextureFormat.R5G6B5:
				case TextureFormat.B4G4R4A4:
				case TextureFormat.A1R5G5B5:
				case TextureFormat.R16_SFLOAT:
				case TextureFormat.R8G8_SNORM:
				case TextureFormat.R8G8_UINT:
				case TextureFormat.R16_UINT:
				case TextureFormat.D16:
					return 2;
				case TextureFormat.D16S8:
					return 3;
				case TextureFormat.R8G8B8A8:
				case TextureFormat.B8G8R8A8:
				case TextureFormat.R32_SFLOAT:
				case TextureFormat.R16G16:
				case TextureFormat.R16G16_SFLOAT:
				case TextureFormat.R8G8B8A8_SNORM:
				case TextureFormat.A2R10G10B10:
				case TextureFormat.R8G8B8A8_UINT:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.D32:
					return 4;
				case TextureFormat.D32S8:
					return 5;
				case TextureFormat.R16G16B16A16_SFLOAT:
				case TextureFormat.R16G16B16A16:
				case TextureFormat.R32G32_SFLOAT:
				case TextureFormat.R16G16B16A16_UINT:
				case TextureFormat.BC1:
					return 8;
				case TextureFormat.R32G32B32A32_SFLOAT:
				case TextureFormat.BC2:
				case TextureFormat.BC3:
				case TextureFormat.BC7:
					return 16;
				default:
					Logger.LogError("Texture format not recognized!");
					return 0;
			}
		}

		public static uint BlockSizeSquared(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.BC1:
				case TextureFormat.BC2:
				case TextureFormat.BC3:
				case TextureFormat.BC7:
					return 16;
				case TextureFormat.R8G8B8A8:
				case TextureFormat.B8G8R8A8:
				case TextureFormat.R5G6B5:
				case TextureFormat.A1R5G5B5:
				case TextureFormat.B4G4R4A4:
				case TextureFormat.A2R10G10B10:
				case TextureFormat.R16G16:
				case TextureFormat.R16G16B16A16:
				case TextureFormat.R8:
				case TextureFormat.R8G8_SNORM:
				case TextureFormat.R8G8B8A8_SNORM:
				case TextureFormat.R16_SFLOAT:
				case TextureFormat.R16G16_SFLOAT:
				case TextureFormat.R16G16B16A16_SFLOAT:
				case TextureFormat.R32_SFLOAT:
				case TextureFormat.R32G32_SFLOAT:
				case TextureFormat.R32G32B32A32_SFLOAT:
				case TextureFormat.R8_UINT:
				case TextureFormat.R8G8_UINT:
				case TextureFormat.R8G8B8A8_UINT:
				case TextureFormat.R16_UINT:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.R16G16B16A16_UINT:
				case TextureFormat.D16:
				case TextureFormat.D32:
				case TextureFormat.D16S8:
				case TextureFormat.D32S8:
					return 1;
				default:
					Logger.LogError("Texture format not recognized!");
					return 0;
			}
		}
	}
}
