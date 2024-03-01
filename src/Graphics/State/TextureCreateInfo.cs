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

		public Refresh.TextureCreateInfo ToRefreshTextureCreateInfo()
		{
			return new Refresh.TextureCreateInfo
			{
				width = Width,
				height = Height,
				depth = Depth,
				isCube = Conversions.BoolToByte(IsCube),
				layerCount = LayerCount,
				levelCount = LevelCount,
				sampleCount = (Refresh.SampleCount) SampleCount,
				format = (Refresh.TextureFormat) Format,
				usageFlags = (Refresh.TextureUsageFlags) UsageFlags
			};
		}
	}
}
