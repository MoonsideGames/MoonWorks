using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct TextureCreateInfo
    {
        public uint Width;
        public uint Height;
        public uint Depth;
        public bool IsCube;
        public Refresh.SampleCount SampleCount;
        public uint LevelCount;
        public Refresh.ColorFormat Format;
        public Refresh.TextureUsageFlags UsageFlags;

        public Refresh.TextureCreateInfo ToRefreshTextureCreateInfo()
        {
            return new Refresh.TextureCreateInfo
            {
                width = Width,
                height = Height,
                depth = Depth,
                isCube = Conversions.BoolToByte(IsCube),
                sampleCount = SampleCount,
                levelCount = LevelCount,
                format = Format,
                usageFlags = UsageFlags
            };
        }
    }
}
