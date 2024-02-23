namespace MoonWorks.Graphics
{
	/// <summary>
	/// A texture-level pair to be used when binding compute textures.
	/// </summary>
	public struct TextureLevelBinding
	{
		public Texture Texture;
		public uint MipLevel;

		public TextureLevelBinding(Texture texture, uint mipLevel = 0)
		{
			Texture = texture;
			MipLevel = mipLevel;
		}
	}
}
