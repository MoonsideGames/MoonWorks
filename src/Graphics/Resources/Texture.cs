using System;
using System.IO;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A container for pixel data.
	/// </summary>
	public class Texture : GraphicsResource
	{
		public uint Width { get; }
		public uint Height { get; }
		public uint Depth { get; }
		public TextureFormat Format { get; }
		public bool IsCube { get; }
		public uint LevelCount { get; }
		public SampleCount SampleCount { get; }
		public TextureUsageFlags UsageFlags { get; }

		protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyTexture;

		/// <summary>
		/// Loads a PNG from a file path.
		/// NOTE: You can queue as many of these as you want on to a command buffer but it MUST be submitted!
		/// </summary>
		/// <param name="device"></param>
		/// <param name="commandBuffer"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static Texture LoadPNG(GraphicsDevice device, CommandBuffer commandBuffer, string filePath)
		{
			var pixels = Refresh.Refresh_Image_Load(
				filePath,
				out var width,
				out var height,
				out var channels
			);

			var byteCount = (uint) (width * height * channels);

			TextureCreateInfo textureCreateInfo;
			textureCreateInfo.Width = (uint) width;
			textureCreateInfo.Height = (uint) height;
			textureCreateInfo.Depth = 1;
			textureCreateInfo.Format = TextureFormat.R8G8B8A8;
			textureCreateInfo.IsCube = false;
			textureCreateInfo.LevelCount = 1;
			textureCreateInfo.SampleCount = SampleCount.One;
			textureCreateInfo.UsageFlags = TextureUsageFlags.Sampler;

			var texture = new Texture(device, textureCreateInfo);
			commandBuffer.SetTextureData(texture, pixels, byteCount);

			Refresh.Refresh_Image_Free(pixels);

			return texture;
		}

		/// <summary>
		/// Saves RGBA or BGRA pixel data to a file in PNG format.
		/// </summary>
		public unsafe static void SavePNG(string path, int width, int height, TextureFormat format, byte[] pixels)
		{
			if (format != TextureFormat.R8G8B8A8 && format != TextureFormat.B8G8R8A8)
			{
				throw new ArgumentException("Texture format must be RGBA8 or BGRA8!", "format");
			}

			fixed (byte* ptr = &pixels[0])
			{
				Refresh.Refresh_Image_SavePNG(path, width, height, Conversions.BoolToByte(format == TextureFormat.B8G8R8A8), (IntPtr) ptr);
			}
		}

		public static Texture LoadDDS(GraphicsDevice graphicsDevice, CommandBuffer commandBuffer, System.IO.Stream stream)
		{
			using (var reader = new BinaryReader(stream))
			{
				ParseDDS(reader, out var format, out var width, out var height, out var levels);
				Texture texture = CreateTexture2D(graphicsDevice, (uint) width, (uint) height, format, TextureUsageFlags.Sampler, SampleCount.One, (uint) levels);

				for (int i = 0; i < levels; i += 1)
				{
					var levelWidth = width >> i;
					var levelHeight = height >> i;

					var pixels = reader.ReadBytes(
						Texture.CalculateDDSLevelSize(
							levelWidth,
							levelHeight,
							format
						)
					);

					var textureSlice = new TextureSlice(texture, new Rect(0, 0, levelWidth, levelHeight), 0, 0, (uint) i);
					commandBuffer.SetTextureData(textureSlice, pixels);
				}

				return texture;
			}
		}

		/// <summary>
		/// Creates a 2D texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="sampleCount">Specifies the multisample count.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateTexture2D(
			GraphicsDevice device,
			uint width,
			uint height,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			SampleCount sampleCount = SampleCount.One,
			uint levelCount = 1
		)
		{
			var textureCreateInfo = new TextureCreateInfo
			{
				Width = width,
				Height = height,
				Depth = 1,
				IsCube = false,
				SampleCount = sampleCount,
				LevelCount = levelCount,
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
		/// <param name="sampleCount">Specifies the multisample count.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateTexture3D(
			GraphicsDevice device,
			uint width,
			uint height,
			uint depth,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			SampleCount sampleCount = SampleCount.One,
			uint levelCount = 1
		)
		{
			var textureCreateInfo = new TextureCreateInfo
			{
				Width = width,
				Height = height,
				Depth = depth,
				IsCube = false,
				SampleCount = sampleCount,
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
		/// <param name="sampleCount">Specifies the multisample count.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateTextureCube(
			GraphicsDevice device,
			uint size,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			SampleCount sampleCount = SampleCount.One,
			uint levelCount = 1
		)
		{
			var textureCreateInfo = new TextureCreateInfo
			{
				Width = size,
				Height = size,
				Depth = 1,
				IsCube = true,
				SampleCount = sampleCount,
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
			SampleCount = textureCreateInfo.SampleCount;
			LevelCount = textureCreateInfo.LevelCount;
			UsageFlags = textureCreateInfo.UsageFlags;
		}

		public static implicit operator TextureSlice(Texture t) => new TextureSlice(t);

		// Used by AcquireSwapchainTexture.
		// Should not be tracked, because swapchain textures are managed by Vulkan.
		internal Texture(
			GraphicsDevice device,
			IntPtr handle,
			TextureFormat format,
			uint width,
			uint height
		) : base(device, false)
		{
			Handle = handle;

			Format = format;
			Width = width;
			Height = height;
			Depth = 1;
			IsCube = false;
			SampleCount = SampleCount.One;
			LevelCount = 1;
			UsageFlags = TextureUsageFlags.ColorTarget;
		}

		// DDS loading extension, based on MojoDDS
		// Taken from https://github.com/FNA-XNA/FNA/blob/1e49f868f595f62bc6385db45949a03186a7cd7f/src/Graphics/Texture.cs#L194
		private static void ParseDDS(
			BinaryReader reader,
			out TextureFormat format,
			out int width,
			out int height,
			out int levels
		) {
			// A whole bunch of magic numbers, yay DDS!
			const uint DDS_MAGIC = 0x20534444;
			const uint DDS_HEADERSIZE = 124;
			const uint DDS_PIXFMTSIZE = 32;
			const uint DDSD_CAPS = 0x1;
			const uint DDSD_HEIGHT = 0x2;
			const uint DDSD_WIDTH = 0x4;
			const uint DDSD_PITCH = 0x8;
			const uint DDSD_FMT = 0x1000;
			const uint DDSD_LINEARSIZE = 0x80000;
			const uint DDSD_REQ = (
				DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_FMT
			);
			const uint DDSCAPS_MIPMAP = 0x400000;
			const uint DDSCAPS_TEXTURE = 0x1000;
			const uint DDSCAPS2_CUBEMAP = 0x200;
			const uint DDPF_FOURCC = 0x4;
			const uint DDPF_RGB = 0x40;
			const uint FOURCC_DXT1 = 0x31545844;
			const uint FOURCC_DXT3 = 0x33545844;
			const uint FOURCC_DXT5 = 0x35545844;
			const uint FOURCC_BPTC = 0x30315844;
			// const uint FOURCC_DX10 = 0x30315844;
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
			uint caps2 = reader.ReadUInt32();
			if (	caps2 != 0 &&
				(caps2 & DDSCAPS2_CUBEMAP) != DDSCAPS2_CUBEMAP	)
			{
				throw new NotSupportedException("Invalid caps2!");
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
					case 0x71:
						format = TextureFormat.R16G16B16A16_SFLOAT;
						break;
					case 0x74:
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
					case FOURCC_BPTC:
						format = TextureFormat.BC7;
						// These next 5 uints are part of the DX10 DDS header.
						// They contain a little extra information but aren't that important.
						uint dxgiFormat = reader.ReadUInt32();
						uint resourceDimension = reader.ReadUInt32();
						uint miscFlag = reader.ReadUInt32();
						uint arraySize = reader.ReadUInt32();
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
	}
}
