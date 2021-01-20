using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class RenderPass : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyRenderPass;

        public unsafe RenderPass(
            GraphicsDevice device,
            params Refresh.ColorTargetDescription[] colorTargetDescriptions
        ) : base(device)
        {
            fixed (Refresh.ColorTargetDescription* ptr = colorTargetDescriptions)
            {
                Refresh.RenderPassCreateInfo renderPassCreateInfo;
                renderPassCreateInfo.colorTargetCount = (uint) colorTargetDescriptions.Length;
                renderPassCreateInfo.colorTargetDescriptions = (IntPtr) ptr;
                renderPassCreateInfo.depthStencilTargetDescription = IntPtr.Zero;

                Handle = Refresh.Refresh_CreateRenderPass(device.Handle, ref renderPassCreateInfo);
            }
        }

        public unsafe RenderPass(
            GraphicsDevice device,
            Refresh.DepthStencilTargetDescription depthStencilTargetDescription,
            params Refresh.ColorTargetDescription[] colorTargetDescriptions
        ) : base(device)
        {
            Refresh.DepthStencilTargetDescription* depthStencilPtr = &depthStencilTargetDescription;

            fixed (Refresh.ColorTargetDescription* colorPtr = colorTargetDescriptions)
            {
                Refresh.RenderPassCreateInfo renderPassCreateInfo;
                renderPassCreateInfo.colorTargetCount = (uint)colorTargetDescriptions.Length;
                renderPassCreateInfo.colorTargetDescriptions = (IntPtr)colorPtr;
                renderPassCreateInfo.depthStencilTargetDescription = (IntPtr) depthStencilPtr;

                Handle = Refresh.Refresh_CreateRenderPass(device.Handle, ref renderPassCreateInfo);
            }
        }
    }
}
