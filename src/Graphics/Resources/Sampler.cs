using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class Sampler : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroySampler;

        public Sampler(
            GraphicsDevice device,
            in SamplerState samplerState
        ) : base(device)
        {
            Handle = Refresh.Refresh_CreateSampler(
                device.Handle,
                samplerState.ToRefreshSamplerStateCreateInfo()
            );
        }
    }
}
