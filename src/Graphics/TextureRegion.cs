using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A texture region specifies a subregion of a texture.
	/// These are used by copy commands.
	/// </summary>
	public struct TextureRegion
	{
		public TextureSlice TextureSlice;
		public uint X;
		public uint Y;
		public uint Z;
		public uint Width;
		public uint Height;
		public uint Depth;

		public uint Size => (Width * Height * Depth * Texture.BytesPerPixel(TextureSlice.Texture.Format) / Texture.BlockSizeSquared(TextureSlice.Texture.Format)) >> (int) TextureSlice.MipLevel;

		public TextureRegion(Texture texture)
		{
			TextureSlice = new TextureSlice(texture);
			X = 0;
			Y = 0;
			Z = 0;
			Width = texture.Width;
			Height = texture.Height;
			Depth = texture.Depth;
		}

		public Refresh.TextureRegion ToRefreshTextureRegion()
		{
			return new Refresh.TextureRegion
			{
				textureSlice = TextureSlice.ToRefreshTextureSlice(),
				x = X,
				y = Y,
				z = Z,
				w = Width,
				h = Height,
				d = Depth
			};
		}
	}
}
