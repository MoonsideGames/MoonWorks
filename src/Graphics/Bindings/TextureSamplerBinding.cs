using SDL2_gpuCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A texture-sampler pair to be used when binding samplers.
	/// </summary>
	public readonly record struct TextureSamplerBinding(
		Texture Texture,
		Sampler Sampler
	) {
		public SDL_Gpu.TextureSamplerBinding ToSDL()
		{
			return new SDL_Gpu.TextureSamplerBinding
			{
				Texture = Texture.Handle,
				Sampler = Sampler.Handle
			};
		}
	}
}
