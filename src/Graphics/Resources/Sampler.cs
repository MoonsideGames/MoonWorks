using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class Sampler : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroySampler;

        public Sampler(
            GraphicsDevice device,
            ref SamplerState samplerState
        ) : base(device)
        {
            var refreshSamplerStateCreateInfo = samplerState.ToRefreshSamplerStateCreateInfo();

            Handle = Refresh.Refresh_CreateSampler(
                device.Handle,
                ref refreshSamplerStateCreateInfo
            );
        }
    }
}
