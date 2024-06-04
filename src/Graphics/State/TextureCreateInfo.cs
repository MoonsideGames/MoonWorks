using SDL2_gpuCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// All of the information that is used to create a texture.
	/// </summary>
	public struct TextureCreateInfo
	{
		public uint Width;
		public uint Height;
		public uint Depth;
		public bool IsCube;
		public uint LayerCount;
		public uint LevelCount;
		public SampleCount SampleCount;
		public TextureFormat Format;
		public TextureUsageFlags UsageFlags;

		public SDL_Gpu.TextureCreateInfo ToSDL()
		{
			return new SDL_Gpu.TextureCreateInfo
			{
				Width = Width,
				Height = Height,
				Depth = Depth,
				IsCube = Conversions.BoolToInt(IsCube),
				LayerCount = LayerCount,
				LevelCount = LevelCount,
				SampleCount = (SDL_Gpu.SampleCount) SampleCount,
				Format = (SDL_Gpu.TextureFormat) Format,
				UsageFlags = (SDL_Gpu.TextureUsageFlags) UsageFlags
			};
		}
	}
}
