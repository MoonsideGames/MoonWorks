using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A render pass describes the kind of render targets that will be used in rendering.
	/// </summary>
	public class RenderPass : GraphicsResource
	{
		protected override Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyRenderPass;

		/// <summary>
		/// Creates a render pass using color target descriptions.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="colorTargetDescriptions">Up to 4 color target descriptions may be provided.</param>
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

		/// <summary>
		/// Creates a render pass using a depth/stencil target description and optional color target descriptions.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="depthStencilTargetDescription">A depth/stencil target description.</param>
		/// <param name="colorTargetDescriptions">Up to 4 color target descriptions may be provided.</param>
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
				renderPassCreateInfo.colorTargetCount = (uint) colorTargetDescriptions.Length;
				renderPassCreateInfo.colorTargetDescriptions = (IntPtr) colorPtr;
				renderPassCreateInfo.depthStencilTargetDescription = (IntPtr) depthStencilPtr;

				Handle = Refresh.Refresh_CreateRenderPass(device.Handle, renderPassCreateInfo);
			}
		}
	}
}
