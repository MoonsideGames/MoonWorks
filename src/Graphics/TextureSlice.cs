using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A texture slice specifies a subregion of a texture.
	/// Many operations can use texture slices in place of textures for the sake of convenience.
	/// </summary>
	public struct TextureSlice
	{
		public Texture Texture;
		public uint MipLevel;
		public uint BaseLayer;
		public uint LayerCount;
		public uint X;
		public uint Y;
		public uint Z;
		public uint Width;
		public uint Height;
		public uint Depth;

		public uint Size => (Width * Height * Depth * LayerCount * Texture.BytesPerPixel(Texture.Format) / Texture.BlockSizeSquared(Texture.Format)) >> (int) MipLevel;

		public TextureSlice(Texture texture)
		{
			Texture = texture;
			MipLevel = 0;
			BaseLayer = 0;
			LayerCount = (uint) (texture.IsCube ? 6 : 1);
			X = 0;
			Y = 0;
			Z = 0;
			Width = texture.Width;
			Height = texture.Height;
			Depth = texture.Depth;
		}

		public Refresh.TextureSlice ToRefreshTextureSlice()
		{
			Refresh.TextureSlice textureSlice = new Refresh.TextureSlice
			{
				texture = Texture.Handle,
				mipLevel = MipLevel,
				baseLayer = BaseLayer,
				layerCount = LayerCount,
				x = X,
				y = Y,
				z = Z,
				w = Width,
				h = Height,
				d = Depth
			};

			return textureSlice;
		}
	}
}
