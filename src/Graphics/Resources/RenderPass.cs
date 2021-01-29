using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class RenderPass : GraphicsResource
    {
        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyRenderPass;

        public unsafe RenderPass(
            GraphicsDevice device,
            params ColorTargetDescription[] colorTargetDescriptions
        ) : base(device)
        {
            fixed (ColorTargetDescription* ptr = colorTargetDescriptions)
            {
                Refresh.RenderPassCreateInfo renderPassCreateInfo;
                renderPassCreateInfo.colorTargetCount = (uint) colorTargetDescriptions.Length;
                renderPassCreateInfo.colorTargetDescriptions = (IntPtr) ptr;
                renderPassCreateInfo.depthStencilTargetDescription = IntPtr.Zero;

                Handle = Refresh.Refresh_CreateRenderPass(device.Handle, renderPassCreateInfo);
            }
        }

        public unsafe RenderPass(
            GraphicsDevice device,
            in DepthStencilTargetDescription depthStencilTargetDescription,
            params ColorTargetDescription[] colorTargetDescriptions
        ) : base(device)
        {

            fixed (DepthStencilTargetDescription* depthStencilPtr = &depthStencilTargetDescription)
            fixed (ColorTargetDescription* colorPtr = colorTargetDescriptions)
            {
                Refresh.RenderPassCreateInfo renderPassCreateInfo;
                renderPassCreateInfo.colorTargetCount = (uint)colorTargetDescriptions.Length;
                renderPassCreateInfo.colorTargetDescriptions = (IntPtr)colorPtr;
                renderPassCreateInfo.depthStencilTargetDescription = (IntPtr) depthStencilPtr;

                Handle = Refresh.Refresh_CreateRenderPass(device.Handle, renderPassCreateInfo);
            }
        }
    }
}
