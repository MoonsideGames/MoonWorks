using System;
using System.Collections.Generic;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A framebuffer is a collection of render targets that is rendered to during a render pass.
	/// </summary>
	public class Framebuffer : GraphicsResource
	{
		protected override Action<IntPtr, IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyFramebuffer;

		public RenderTarget DepthStencilTarget { get; }

		private RenderTarget[] colorTargets { get; }
		public IEnumerable<RenderTarget> ColorTargets => colorTargets;

		public RenderPass RenderPass { get; }

		/// <summary>
		/// Creates a framebuffer.
		/// </summary>
		/// <param name="device">An initialized GraphicsDevice.</param>
		/// <param name="width">The width of the framebuffer.</param>
		/// <param name="height">The height of the framebuffer.</param>
		/// <param name="renderPass">The reference render pass for the framebuffer.</param>
		/// <param name="depthStencilTarget">The depth stencil target. Can be null.</param>
		/// <param name="colorTargets">Anywhere from 0-4 color targets can be provided.</param>
		public unsafe Framebuffer(
			GraphicsDevice device,
			uint width,
			uint height,
			RenderPass renderPass,
			RenderTarget depthStencilTarget,
			params RenderTarget[] colorTargets
		) : base(device)
		{
			IntPtr[] colorTargetHandles = new IntPtr[colorTargets.Length];
			for (var i = 0; i < colorTargets.Length; i += 1)
			{
				colorTargetHandles[i] = colorTargets[i].Handle;
			}

			IntPtr depthStencilTargetHandle;
			if (depthStencilTarget == null)
			{
				depthStencilTargetHandle = IntPtr.Zero;
			}
			else
			{
				depthStencilTargetHandle = depthStencilTarget.Handle;
			}

			fixed (IntPtr* colorTargetHandlesPtr = colorTargetHandles)
			{
				Refresh.FramebufferCreateInfo framebufferCreateInfo = new Refresh.FramebufferCreateInfo
				{
					width = width,
					height = height,
					colorTargetCount = (uint) colorTargets.Length,
					pColorTargets = (IntPtr) colorTargetHandlesPtr,
					depthStencilTarget = depthStencilTargetHandle,
					renderPass = renderPass.Handle
				};

				Handle = Refresh.Refresh_CreateFramebuffer(device.Handle, framebufferCreateInfo);
			}

			DepthStencilTarget = depthStencilTarget;

			this.colorTargets = new RenderTarget[colorTargets.Length];
			for (var i = 0; i < colorTargets.Length; i++)
			{
				this.colorTargets[i] = colorTargets[i];
			}

			RenderPass = renderPass;
		}
	}
}
