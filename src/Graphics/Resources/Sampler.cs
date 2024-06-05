using System;
using RefreshCS;

namespace MoonWorks.Graphics;

/// <summary>
/// A sampler specifies how a texture will be sampled in a shader.
/// </summary>
public class Sampler : RefreshResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseSampler;

	public Sampler(
		GraphicsDevice device,
		in SamplerCreateInfo samplerCreateInfo
	) : base(device)
	{
		Handle = Refresh.Refresh_CreateSampler(
			device.Handle,
			samplerCreateInfo.ToRefresh()
		);
	}
}
