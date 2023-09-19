namespace MoonWorks.Graphics
{
	/// <summary>
	/// A texture-sampler pair to be used when binding samplers.
	/// </summary>
	public struct TextureSamplerBinding
	{
		public Texture Texture;
		public Sampler Sampler;

		public TextureSamplerBinding(Texture texture, Sampler sampler)
		{
			Texture = texture;
			Sampler = sampler;
		}
	}
}
