using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A texture-sampler pair to be used when binding samplers.
	/// </summary>
	public readonly record struct TextureSamplerBinding(
		Texture Texture,
		Sampler Sampler
	) {
		public Refresh.TextureSamplerBinding ToRefresh()
		{
			return new Refresh.TextureSamplerBinding
			{
				texture = Texture.Handle,
				sampler = Sampler.Handle
			};
		}
	}
}
