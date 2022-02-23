using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A sampler specifies how a texture will be sampled in a shader.
	/// </summary>
	public class Sampler : GraphicsResource
	{
		protected override Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroySampler;

		public Sampler(
			GraphicsDevice device,
			in SamplerCreateInfo samplerCreateInfo
		) : base(device)
		{
			Handle = Refresh.Refresh_CreateSampler(
				device.Handle,
				samplerCreateInfo.ToRefreshSamplerStateCreateInfo()
			);
		}
	}
}
