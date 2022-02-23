using System;
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

		protected override Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyTexture;

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

		public unsafe static void SavePNG(string path, int width, int height, byte[] pixels)
		{
			fixed (byte* ptr = &pixels[0])
			{
				Refresh.Refresh_Image_SavePNG(path, width, height, (IntPtr) ptr);
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
			SampleCount = textureCreateInfo.SampleCount;
			UsageFlags = textureCreateInfo.UsageFlags;
		}
	}
}
