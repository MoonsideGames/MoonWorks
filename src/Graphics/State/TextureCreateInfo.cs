using RefreshCS;

namespace MoonWorks.Graphics
{
	public struct TextureCreateInfo
	{
		public uint Width;
		public uint Height;
		public uint Depth;
		public bool IsCube;
		public uint LevelCount;
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
				levelCount = LevelCount,
				format = (Refresh.TextureFormat) Format,
				usageFlags = (Refresh.TextureUsageFlags) UsageFlags
			};
		}
	}
}
