using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A multi-dimensional data container that can be efficiently used by the GPU.
	/// </summary>
	public class Texture : RefreshResource
	{
		public uint Width { get; internal set; }
		public uint Height { get; internal set; }
		public uint Depth { get; }
		public TextureFormat Format { get; internal set; }
		public bool IsCube { get; }
		public uint LayerCount { get; }
		public uint LevelCount { get; }
		public SampleCount SampleCount { get; }
		public TextureUsageFlags UsageFlags { get; }
		public uint Size { get; }

		private string name;
		public string Name
		{
			get => name;

			set
			{
				if (Device.DebugMode)
				{
					Refresh.Refresh_SetTextureName(
						Device.Handle,
						Handle,
						value
					);
				}

				name = value;
			}
		}

		// FIXME: this allocates a delegate instance
		protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseTexture;

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
				LayerCount = 1,
				LevelCount = levelCount,
				SampleCount = sampleCount,
				Format = format,
				UsageFlags = usageFlags
			};

			return new Texture(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a 2D texture array.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="layerCount">The layer count of the texture.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateTexture2DArray(
			GraphicsDevice device,
			uint width,
			uint height,
			uint layerCount,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1
		) {
			var textureCreateInfo = new TextureCreateInfo
			{
				Width = width,
				Height = height,
				Depth = 1,
				IsCube = false,
				LayerCount = layerCount,
				LevelCount = levelCount,
				Format = format,
				UsageFlags = usageFlags
			};

			return new Texture(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a 3D texture.
		/// Note that the width, height and depth all form one slice and cannot be subdivided in a texture slice.
		/// </summary>
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
				LayerCount = 1,
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
				LayerCount = 6,
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
				textureCreateInfo.ToRefresh()
			);

			Format = textureCreateInfo.Format;
			Width = textureCreateInfo.Width;
			Height = textureCreateInfo.Height;
			Depth = textureCreateInfo.Depth;
			IsCube = textureCreateInfo.IsCube;
			LayerCount = textureCreateInfo.LayerCount;
			LevelCount = textureCreateInfo.LevelCount;
			SampleCount = textureCreateInfo.SampleCount;
			UsageFlags = textureCreateInfo.UsageFlags;
			Size = Width * Height * BytesPerPixel(Format) / BlockSizeSquared(Format);
			name = "";
		}

		// Used by Window. Swapchain texture handles are managed by the driver backend.
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

		public static uint BytesPerPixel(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.R8:
				case TextureFormat.A8:
				case TextureFormat.R8_UINT:
					return 1;
				case TextureFormat.B5G6R5:
				case TextureFormat.B4G4R4A4:
				case TextureFormat.B5G5R5A1:
				case TextureFormat.R16_SFLOAT:
				case TextureFormat.R8G8_SNORM:
				case TextureFormat.R8G8_UINT:
				case TextureFormat.R16_UINT:
				case TextureFormat.D16_UNORM:
					return 2;
				case TextureFormat.R8G8B8A8:
				case TextureFormat.B8G8R8A8:
				case TextureFormat.R32_SFLOAT:
				case TextureFormat.R16G16:
				case TextureFormat.R16G16_SFLOAT:
				case TextureFormat.R8G8B8A8_SNORM:
				case TextureFormat.R10G10B10A2:
				case TextureFormat.R8G8B8A8_UINT:
				case TextureFormat.R16G16_UINT:
				case TextureFormat.D24_UNORM_S8_UINT:
				case TextureFormat.D32_SFLOAT:
					return 4;
				case TextureFormat.D32_SFLOAT_S8_UINT:
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

		public static uint TexelSize(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.BC2:
				case TextureFormat.BC3:
				case TextureFormat.BC7:
					return 16;
				case TextureFormat.BC1:
					return 8;
				default:
					return 1;
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
				case TextureFormat.B5G6R5:
				case TextureFormat.B5G5R5A1:
				case TextureFormat.B4G4R4A4:
				case TextureFormat.R10G10B10A2:
				case TextureFormat.R16G16:
				case TextureFormat.R16G16B16A16:
				case TextureFormat.R8:
				case TextureFormat.A8:
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
				case TextureFormat.D16_UNORM:
				case TextureFormat.D24_UNORM:
				case TextureFormat.D24_UNORM_S8_UINT:
				case TextureFormat.D32_SFLOAT:
				case TextureFormat.D32_SFLOAT_S8_UINT:
					return 1;
				default:
					Logger.LogError("Texture format not recognized!");
					return 0;
			}
		}

		public static implicit operator TextureSlice(Texture t) => new TextureSlice(t);
		public static implicit operator TextureRegion(Texture t) => new TextureRegion(t);
	}
}
