using System;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A multi-dimensional data container that can be efficiently used by the GPU.
	/// </summary>
	public class Texture : SDLGPUResource
	{
		public TextureType Type { get; private init; }
		public uint Width { get; internal set; }
		public uint Height { get; internal set; }
		public uint LayerCountOrDepth { get; private init; }
		public TextureFormat Format { get; internal set; }
		public uint LevelCount { get; private init; }
		public SampleCount SampleCount { get; private init; }
		public TextureUsageFlags UsageFlags { get; private init; }
		public uint Size { get; private init; }

		// FIXME: this allocates a delegate instance
		protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUTexture;

		/// <summary>
		/// Creates a 2D texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		/// <param name="sampleCount">The sample count of the texture.</param>
		public static Texture Create2D(
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
				Type = TextureType.TwoDimensional,
				Format = format,
				Usage = usageFlags,
				Width = width,
				Height = height,
				LayerCountOrDepth = 1,
				NumLevels = levelCount,
				SampleCount = sampleCount,
				Props = 0
			};

			return Create(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a named 2D texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		/// <param name="sampleCount">The sample count of the texture.</param>
		public static Texture Create2D(
			GraphicsDevice device,
			string name,
			uint width,
			uint height,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1,
			SampleCount sampleCount = SampleCount.One
		) {
			var props = SDL3.SDL.SDL_CreateProperties();
			SDL3.SDL.SDL_SetStringProperty(props, SDL3.SDL.SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING, name);

			var textureCreateInfo = new TextureCreateInfo
			{
				Type = TextureType.TwoDimensional,
				Format = format,
				Usage = usageFlags,
				Width = width,
				Height = height,
				LayerCountOrDepth = 1,
				NumLevels = levelCount,
				SampleCount = sampleCount,
				Props = props
			};

			var result = Create(device, textureCreateInfo);

			SDL3.SDL.SDL_DestroyProperties(props);
			return result;
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
		public static Texture Create2DArray(
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
				Type = TextureType.TwoDimensionalArray,
				Format = format,
				Usage = usageFlags,
				Width = width,
				Height = height,
				LayerCountOrDepth = layerCount,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = 0
			};

			return Create(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a named 2D texture array.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="width">The width of the texture.</param>
		/// <param name="height">The height of the texture.</param>
		/// <param name="layerCount">The layer count of the texture.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture Create2DArray(
			GraphicsDevice device,
			string name,
			uint width,
			uint height,
			uint layerCount,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1
		) {
			var props = SDL3.SDL.SDL_CreateProperties();
			SDL3.SDL.SDL_SetStringProperty(props, SDL3.SDL.SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING, name);

			var textureCreateInfo = new TextureCreateInfo
			{
				Type = TextureType.TwoDimensionalArray,
				Format = format,
				Usage = usageFlags,
				Width = width,
				Height = height,
				LayerCountOrDepth = layerCount,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = props
			};

			var result = Create(device, textureCreateInfo);

			SDL3.SDL.SDL_DestroyProperties(props);
			return result;
		}

		/// <summary>
		/// Creates a 3D texture.
		/// Note that the width, height and depth all form one slice and cannot be subdivided in a texture slice.
		/// </summary>
		public static Texture Create3D(
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
				Type = TextureType.ThreeDimensional,
				Format = format,
				Usage = usageFlags,
				Width = width,
				Height = height,
				LayerCountOrDepth = depth,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = 0
			};

			return Create(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a named 3D texture.
		/// Note that the width, height and depth all form one slice and cannot be subdivided in a texture slice.
		/// </summary>
		public static Texture Create3D(
			GraphicsDevice device,
			string name,
			uint width,
			uint height,
			uint depth,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1
		) {
			var props = SDL3.SDL.SDL_CreateProperties();
			SDL3.SDL.SDL_SetStringProperty(props, SDL3.SDL.SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING, name);

			var textureCreateInfo = new TextureCreateInfo
			{
				Type = TextureType.ThreeDimensional,
				Format = format,
				Usage = usageFlags,
				Width = width,
				Height = height,
				LayerCountOrDepth = depth,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = props
			};

			var result = Create(device, textureCreateInfo);

			SDL3.SDL.SDL_DestroyProperties(props);
			return result;
		}

		/// <summary>
		/// Creates a cube texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="size">The length of one side of the cube.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateCube(
			GraphicsDevice device,
			uint size,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1
		) {
			var textureCreateInfo = new TextureCreateInfo
			{
				Type = TextureType.Cube,
				Format = format,
				Usage = usageFlags,
				Width = size,
				Height = size,
				LayerCountOrDepth = 6,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = 0
			};

			return Create(device, textureCreateInfo);
		}

		/// <summary>
		/// Creates a named cube texture.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="size">The length of one side of the cube.</param>
		/// <param name="format">The format of the texture.</param>
		/// <param name="usageFlags">Specifies how the texture will be used.</param>
		/// <param name="levelCount">Specifies the number of mip levels.</param>
		public static Texture CreateCube(
			GraphicsDevice device,
			string name,
			uint size,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint levelCount = 1
		) {
			var props = SDL3.SDL.SDL_CreateProperties();
			SDL3.SDL.SDL_SetStringProperty(props, SDL3.SDL.SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING, name);

			var textureCreateInfo = new TextureCreateInfo
			{
				Type = TextureType.Cube,
				Format = format,
				Usage = usageFlags,
				Width = size,
				Height = size,
				LayerCountOrDepth = 6,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = props
			};

			var result = Create(device, textureCreateInfo);

			SDL3.SDL.SDL_DestroyProperties(props);
			return result;
		}

		public static Texture CreateCubeArray(
			GraphicsDevice device,
			uint size,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint arrayCount,
			uint levelCount = 1
		) {
			var textureCreateInfo = new TextureCreateInfo
			{
				Type = TextureType.CubeArray,
				Format = format,
				Usage = usageFlags,
				Width = size,
				Height = size,
				LayerCountOrDepth = arrayCount * 6,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = 0
			};

			return Create(device, textureCreateInfo);
		}

		public static Texture CreateCubeArray(
			GraphicsDevice device,
			string name,
			uint size,
			TextureFormat format,
			TextureUsageFlags usageFlags,
			uint arrayCount,
			uint levelCount = 1
		) {
			var props = SDL3.SDL.SDL_CreateProperties();
			SDL3.SDL.SDL_SetStringProperty(props, SDL3.SDL.SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING, name);

			var textureCreateInfo = new TextureCreateInfo
			{
				Type = TextureType.CubeArray,
				Format = format,
				Usage = usageFlags,
				Width = size,
				Height = size,
				LayerCountOrDepth = arrayCount * 6,
				NumLevels = levelCount,
				SampleCount = SampleCount.One,
				Props = props
			};

			var result = Create(device, textureCreateInfo);

			SDL3.SDL.SDL_DestroyProperties(props);
			return result;
		}

		public static Texture Create(
			GraphicsDevice device,
			in TextureCreateInfo createInfo
		) {
			var handle = SDL.SDL_CreateGPUTexture(device.Handle, createInfo);

			if (handle == IntPtr.Zero)
			{
				Logger.LogError(SDL3.SDL.SDL_GetError());
				return null;
			}

			return new Texture(device)
			{
				Handle = handle,
				Type = createInfo.Type,
				Width = createInfo.Width,
				Height = createInfo.Height,
				LayerCountOrDepth = createInfo.LayerCountOrDepth,
				Format = createInfo.Format,
				LevelCount = createInfo.NumLevels,
				SampleCount = createInfo.SampleCount,
				UsageFlags = createInfo.Usage,
				Size = CalculateSize(createInfo.Format, createInfo.Width, createInfo.Height, createInfo.LayerCountOrDepth),
				Name = SDL3.SDL.SDL_GetStringProperty(createInfo.Props, SDL3.SDL.SDL_PROP_GPU_TEXTURE_CREATE_NAME_STRING, "Texture")
			};
		}

		private Texture(GraphicsDevice device) : base(device) { }

		// Used by Window. Swapchain texture handles are managed by the driver backend.
		internal Texture(
			GraphicsDevice device,
			TextureFormat format
		) : base(device)
		{
			Handle = IntPtr.Zero;
			Type = TextureType.TwoDimensional;
			Format = format;
			Width = 0;
			Height = 0;
			LayerCountOrDepth = 1;
			LevelCount = 1;
			SampleCount = SampleCount.One;
			UsageFlags = TextureUsageFlags.ColorTarget;
		}

		public static uint CalculateSize(TextureFormat format, uint width, uint height, uint layerCountOrDepth)
		{
			return SDL.SDL_CalculateGPUTextureFormatSize(
				format,
				width,
				height,
				layerCountOrDepth
			);
		}
	}
}
