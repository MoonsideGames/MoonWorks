using RefreshCS;

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

		public Refresh.TextureCreateInfo ToRefresh()
		{
			return new Refresh.TextureCreateInfo
			{
				Width = Width,
				Height = Height,
				Depth = Depth,
				IsCube = Conversions.BoolToInt(IsCube),
				LayerCount = LayerCount,
				LevelCount = LevelCount,
				SampleCount = (Refresh.SampleCount) SampleCount,
				Format = (Refresh.TextureFormat) Format,
				UsageFlags = (Refresh.TextureUsageFlags) UsageFlags
			};
		}
	}
}
