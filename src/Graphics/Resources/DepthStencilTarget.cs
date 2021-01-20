using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class DepthStencilTarget : GraphicsResource
    {
        public uint Width { get; }
        public uint Height { get; }
        public DepthFormat Format { get; }

        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyDepthStencilTarget;

        public DepthStencilTarget(
            GraphicsDevice device,
            uint width,
            uint height,
            DepthFormat depthFormat
        ) : base(device)
        {
            Handle = Refresh.Refresh_CreateDepthStencilTarget(
                device.Handle, 
                width, 
                height, 
                (Refresh.DepthFormat) depthFormat
            );
            Width = width;
            Height = height;
            Format = depthFormat;
        }
    }
}
