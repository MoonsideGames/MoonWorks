using System;

namespace MoonWorks.Graphics
{
	public struct TextureSamplerBinding
	{
		public IntPtr TextureHandle;
		public IntPtr SamplerHandle;

		public TextureSamplerBinding(Texture texture, Sampler sampler)
		{
			TextureHandle = texture.Handle;
			SamplerHandle = sampler.Handle;
		}

		public TextureSamplerBinding(IntPtr textureHandle, IntPtr samplerHandle)
		{
			TextureHandle = textureHandle;
			SamplerHandle = samplerHandle;
		}
	}
}
