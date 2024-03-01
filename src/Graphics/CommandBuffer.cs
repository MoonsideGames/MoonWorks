using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Command buffers are used to apply render state and issue draw calls.
	/// NOTE: it is not recommended to hold references to command buffers long term.
	/// </summary>
	public class CommandBuffer
	{
		public GraphicsDevice Device { get; }
		public IntPtr Handle { get; internal set; }

#if DEBUG
		bool swapchainTextureAcquired;

		GraphicsPipeline currentGraphicsPipeline;
		ComputePipeline currentComputePipeline;
		bool renderPassActive;
		SampleCount colorAttachmentSampleCount;
		uint colorAttachmentCount;
		TextureFormat colorFormatOne;
		TextureFormat colorFormatTwo;
		TextureFormat colorFormatThree;
		TextureFormat colorFormatFour;
		bool hasDepthStencilAttachment;
		SampleCount depthStencilAttachmentSampleCount;
		TextureFormat depthStencilFormat;

		bool copyPassActive;

		bool computePassActive;

		internal bool Submitted;
#endif

		// called from CommandBufferPool
		internal CommandBuffer(GraphicsDevice device)
		{
			Device = device;
			Handle = IntPtr.Zero;

#if DEBUG
			ResetStateTracking();
#endif
		}

		internal void SetHandle(nint handle)
		{
			Handle = handle;
		}

#if DEBUG
		internal void ResetStateTracking()
		{
			swapchainTextureAcquired = false;

			currentGraphicsPipeline = null;
			currentComputePipeline = null;
			renderPassActive = false;
			colorAttachmentSampleCount = SampleCount.One;
			depthStencilAttachmentSampleCount = SampleCount.One;
			colorAttachmentCount = 0;
			colorFormatOne = TextureFormat.R8G8B8A8;
			colorFormatTwo = TextureFormat.R8G8B8A8;
			colorFormatThree = TextureFormat.R8G8B8A8;
			colorFormatFour = TextureFormat.R8G8B8A8;
			depthStencilFormat = TextureFormat.D16;

			copyPassActive = false;
			computePassActive = false;

			Submitted = false;
		}
#endif

		/// <summary>
		/// Acquires a swapchain texture.
		/// This texture will be presented to the given window when the command buffer is submitted.
		/// Can return null if the swapchain is unavailable. The user should ALWAYS handle the case where this occurs.
		/// If null is returned, presentation will not occur.
		/// It is an error to acquire two swapchain textures from the same window in one command buffer.
		/// It is an error to dispose the swapchain texture. If you do this your game WILL crash. DO NOT DO THIS.
		/// </summary>
		public Texture AcquireSwapchainTexture(
			Window window
		) {
#if DEBUG
			AssertNotSubmitted();

			if (!window.Claimed)
			{
				throw new System.InvalidOperationException("Cannot acquire swapchain texture, window has not been claimed!");
			}

			if (swapchainTextureAcquired)
			{
				throw new System.InvalidOperationException("Cannot acquire two swapchain textures on the same command buffer!");
			}
#endif

			var texturePtr = Refresh.Refresh_AcquireSwapchainTexture(
				Device.Handle,
				Handle,
				window.Handle,
				out var width,
				out var height
			);

			if (texturePtr == IntPtr.Zero)
			{
				return null;
			}

			// Override the texture properties to avoid allocating a new texture instance!
			window.SwapchainTexture.Handle = texturePtr;
			window.SwapchainTexture.Width = width;
			window.SwapchainTexture.Height = height;
			window.SwapchainTexture.Format = window.SwapchainFormat;

#if DEBUG
			swapchainTextureAcquired = true;
#endif

			return window.SwapchainTexture;
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this during any kind of pass.
		/// </summary>
		/// <param name="colorAttachmentInfo">The color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in ColorAttachmentInfo colorAttachmentInfo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertNotInPass("Cannot begin a render pass inside another pass!");
			AssertTextureNotNull(colorAttachmentInfo);
			AssertColorTarget(colorAttachmentInfo);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[1];
			refreshColorAttachmentInfos[0] = colorAttachmentInfo.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				(IntPtr) refreshColorAttachmentInfos,
				1,
				IntPtr.Zero
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = false;
			colorAttachmentSampleCount = colorAttachmentInfo.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 1;
			colorFormatOne = colorAttachmentInfo.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in ColorAttachmentInfo colorAttachmentInfoOne,
			in ColorAttachmentInfo colorAttachmentInfoTwo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertTextureNotNull(colorAttachmentInfoOne);
			AssertColorTarget(colorAttachmentInfoOne);

			AssertTextureNotNull(colorAttachmentInfoTwo);
			AssertColorTarget(colorAttachmentInfoTwo);

			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[2];
			refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
			refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				(IntPtr) refreshColorAttachmentInfos,
				2,
				IntPtr.Zero
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = false;
			colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 2;
			colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in ColorAttachmentInfo colorAttachmentInfoOne,
			in ColorAttachmentInfo colorAttachmentInfoTwo,
			in ColorAttachmentInfo colorAttachmentInfoThree
		) {
#if DEBUG
			AssertNotSubmitted();

			AssertTextureNotNull(colorAttachmentInfoOne);
			AssertColorTarget(colorAttachmentInfoOne);

			AssertTextureNotNull(colorAttachmentInfoTwo);
			AssertColorTarget(colorAttachmentInfoTwo);

			AssertTextureNotNull(colorAttachmentInfoThree);
			AssertColorTarget(colorAttachmentInfoThree);

			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoThree.TextureSlice.Texture);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[3];
			refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
			refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
			refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				(IntPtr) refreshColorAttachmentInfos,
				3,
				IntPtr.Zero
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = false;
			colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 3;
			colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoFour">The four color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in ColorAttachmentInfo colorAttachmentInfoOne,
			in ColorAttachmentInfo colorAttachmentInfoTwo,
			in ColorAttachmentInfo colorAttachmentInfoThree,
			in ColorAttachmentInfo colorAttachmentInfoFour
		) {
#if DEBUG
			AssertNotSubmitted();

			AssertTextureNotNull(colorAttachmentInfoOne);
			AssertColorTarget(colorAttachmentInfoOne);

			AssertTextureNotNull(colorAttachmentInfoTwo);
			AssertColorTarget(colorAttachmentInfoTwo);

			AssertTextureNotNull(colorAttachmentInfoThree);
			AssertColorTarget(colorAttachmentInfoThree);

			AssertTextureNotNull(colorAttachmentInfoFour);
			AssertColorTarget(colorAttachmentInfoFour);

			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoThree.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoFour.TextureSlice.Texture);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[4];
			refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
			refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
			refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();
			refreshColorAttachmentInfos[3] = colorAttachmentInfoFour.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				(IntPtr) refreshColorAttachmentInfos,
				4,
				IntPtr.Zero
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = false;
			colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 4;
			colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
			colorFormatFour = colorAttachmentInfoFour.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in DepthStencilAttachmentInfo depthStencilAttachmentInfo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertValidDepthAttachment(depthStencilAttachmentInfo);
#endif

			var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				(Refresh.ColorAttachmentInfo*) IntPtr.Zero,
				0,
				&refreshDepthStencilAttachmentInfo
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = true;
			depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.TextureSlice.Texture.SampleCount;
			depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfo">The color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
			in ColorAttachmentInfo colorAttachmentInfo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertValidDepthAttachment(depthStencilAttachmentInfo);

			AssertTextureNotNull(colorAttachmentInfo);
			AssertColorTarget(colorAttachmentInfo);
			AssertSameSampleCount(colorAttachmentInfo.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[1];
			refreshColorAttachmentInfos[0] = colorAttachmentInfo.ToRefresh();

			var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				refreshColorAttachmentInfos,
				1,
				&refreshDepthStencilAttachmentInfo
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = true;
			colorAttachmentSampleCount = colorAttachmentInfo.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 1;
			depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.TextureSlice.Texture.SampleCount;
			colorFormatOne = colorAttachmentInfo.TextureSlice.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
			in ColorAttachmentInfo colorAttachmentInfoOne,
			in ColorAttachmentInfo colorAttachmentInfoTwo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertValidDepthAttachment(depthStencilAttachmentInfo);

			AssertTextureNotNull(colorAttachmentInfoOne);
			AssertColorTarget(colorAttachmentInfoOne);

			AssertTextureNotNull(colorAttachmentInfoTwo);
			AssertColorTarget(colorAttachmentInfoTwo);

			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[2];
			refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
			refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();

			var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				refreshColorAttachmentInfos,
				2,
				&refreshDepthStencilAttachmentInfo
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = true;
			colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 2;
			colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
			in ColorAttachmentInfo colorAttachmentInfoOne,
			in ColorAttachmentInfo colorAttachmentInfoTwo,
			in ColorAttachmentInfo colorAttachmentInfoThree
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertValidDepthAttachment(depthStencilAttachmentInfo);

			AssertTextureNotNull(colorAttachmentInfoOne);
			AssertColorTarget(colorAttachmentInfoOne);

			AssertTextureNotNull(colorAttachmentInfoTwo);
			AssertColorTarget(colorAttachmentInfoTwo);

			AssertTextureNotNull(colorAttachmentInfoThree);
			AssertColorTarget(colorAttachmentInfoThree);

			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[3];
			refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
			refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
			refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();

			var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				refreshColorAttachmentInfos,
				3,
				&refreshDepthStencilAttachmentInfo
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = true;
			colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 3;
			colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoOne">The first color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoTwo">The second color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoThree">The third color attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfoFour">The four color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in DepthStencilAttachmentInfo depthStencilAttachmentInfo,
			in ColorAttachmentInfo colorAttachmentInfoOne,
			in ColorAttachmentInfo colorAttachmentInfoTwo,
			in ColorAttachmentInfo colorAttachmentInfoThree,
			in ColorAttachmentInfo colorAttachmentInfoFour
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertValidDepthAttachment(depthStencilAttachmentInfo);

			AssertTextureNotNull(colorAttachmentInfoOne);
			AssertColorTarget(colorAttachmentInfoOne);

			AssertTextureNotNull(colorAttachmentInfoTwo);
			AssertColorTarget(colorAttachmentInfoTwo);

			AssertTextureNotNull(colorAttachmentInfoThree);
			AssertColorTarget(colorAttachmentInfoThree);

			AssertTextureNotNull(colorAttachmentInfoFour);
			AssertColorTarget(colorAttachmentInfoFour);

			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoTwo.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoThree.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, colorAttachmentInfoFour.TextureSlice.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.TextureSlice.Texture, depthStencilAttachmentInfo.TextureSlice.Texture);
#endif

			var refreshColorAttachmentInfos = stackalloc Refresh.ColorAttachmentInfo[4];
			refreshColorAttachmentInfos[0] = colorAttachmentInfoOne.ToRefresh();
			refreshColorAttachmentInfos[1] = colorAttachmentInfoTwo.ToRefresh();
			refreshColorAttachmentInfos[2] = colorAttachmentInfoThree.ToRefresh();
			refreshColorAttachmentInfos[3] = colorAttachmentInfoFour.ToRefresh();

			var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

			Refresh.Refresh_BeginRenderPass(
				Device.Handle,
				Handle,
				refreshColorAttachmentInfos,
				4,
				&refreshDepthStencilAttachmentInfo
			);

#if DEBUG
			renderPassActive = true;
			hasDepthStencilAttachment = true;
			colorAttachmentSampleCount = colorAttachmentInfoOne.TextureSlice.Texture.SampleCount;
			colorAttachmentCount = 4;
			colorFormatOne = colorAttachmentInfoOne.TextureSlice.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.TextureSlice.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.TextureSlice.Texture.Format;
			colorFormatFour = colorAttachmentInfoFour.TextureSlice.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.TextureSlice.Texture.Format;
#endif
		}

		/// <summary>
		/// Binds a graphics pipeline so that rendering work may be performed.
		/// </summary>
		/// <param name="graphicsPipeline">The graphics pipeline to bind.</param>
		public void BindGraphicsPipeline(
			GraphicsPipeline graphicsPipeline
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassActive();
			AssertRenderPassPipelineFormatMatch(graphicsPipeline);

			if (colorAttachmentCount > 0)
			{
				if (graphicsPipeline.SampleCount != colorAttachmentSampleCount)
				{
					throw new System.InvalidOperationException($"Sample count mismatch! Graphics pipeline sample count: {graphicsPipeline.SampleCount}, Color attachment sample count: {colorAttachmentSampleCount}");
				}
			}

			if (hasDepthStencilAttachment)
			{
				if (graphicsPipeline.SampleCount != depthStencilAttachmentSampleCount)
				{
					throw new System.InvalidOperationException($"Sample count mismatch! Graphics pipeline sample count: {graphicsPipeline.SampleCount}, Depth stencil attachment sample count: {depthStencilAttachmentSampleCount}");
				}
			}
#endif

			Refresh.Refresh_BindGraphicsPipeline(
				Device.Handle,
				Handle,
				graphicsPipeline.Handle
			);

#if DEBUG
			currentGraphicsPipeline = graphicsPipeline;
#endif
		}

		/// <summary>
		/// Sets the viewport. Only valid during a render pass.
		/// </summary>
		public void SetViewport(in Viewport viewport)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassActive();
#endif

			Refresh.Refresh_SetViewport(
				Device.Handle,
				Handle,
				viewport.ToRefresh()
			);
		}

		/// <summary>
		/// Sets the scissor area. Only valid during a render pass.
		/// </summary>
		public void SetScissor(in Rect scissor)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassActive();

			if (scissor.X < 0 || scissor.Y < 0 || scissor.W <= 0 || scissor.H <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Scissor position cannot be negative and dimensions must be positive!");
			}
#endif

			Refresh.Refresh_SetScissor(
				Device.Handle,
				Handle,
				scissor.ToRefresh()
			);
		}

		/// <summary>
		/// Binds vertex buffers to be used by subsequent draw calls.
		/// </summary>
		/// <param name="bufferBinding">Buffer to bind and associated offset.</param>
		/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
		public unsafe void BindVertexBuffers(
			in BufferBinding bufferBinding,
			uint firstBinding = 0
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			var bufferPtrs = stackalloc IntPtr[1];
			var offsets = stackalloc ulong[1];

			bufferPtrs[0] = bufferBinding.Buffer.Handle;
			offsets[0] = bufferBinding.Offset;

			Refresh.Refresh_BindVertexBuffers(
				Device.Handle,
				Handle,
				firstBinding,
				1,
				(IntPtr) bufferPtrs,
				(IntPtr) offsets
			);
		}

		/// <summary>
		/// Binds vertex buffers to be used by subsequent draw calls.
		/// </summary>
		/// <param name="bufferBindingOne">Buffer to bind and associated offset.</param>
		/// <param name="bufferBindingTwo">Buffer to bind and associated offset.</param>
		/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
		public unsafe void BindVertexBuffers(
			in BufferBinding bufferBindingOne,
			in BufferBinding bufferBindingTwo,
			uint firstBinding = 0
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			var bufferPtrs = stackalloc IntPtr[2];
			var offsets = stackalloc ulong[2];

			bufferPtrs[0] = bufferBindingOne.Buffer.Handle;
			bufferPtrs[1] = bufferBindingTwo.Buffer.Handle;

			offsets[0] = bufferBindingOne.Offset;
			offsets[1] = bufferBindingTwo.Offset;

			Refresh.Refresh_BindVertexBuffers(
				Device.Handle,
				Handle,
				firstBinding,
				2,
				(IntPtr) bufferPtrs,
				(IntPtr) offsets
			);
		}

		/// <summary>
		/// Binds vertex buffers to be used by subsequent draw calls.
		/// </summary>
		/// <param name="bufferBindingOne">Buffer to bind and associated offset.</param>
		/// <param name="bufferBindingTwo">Buffer to bind and associated offset.</param>
		/// <param name="bufferBindingThree">Buffer to bind and associated offset.</param>
		/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
		public unsafe void BindVertexBuffers(
			in BufferBinding bufferBindingOne,
			in BufferBinding bufferBindingTwo,
			in BufferBinding bufferBindingThree,
			uint firstBinding = 0
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			var bufferPtrs = stackalloc IntPtr[3];
			var offsets = stackalloc ulong[3];

			bufferPtrs[0] = bufferBindingOne.Buffer.Handle;
			bufferPtrs[1] = bufferBindingTwo.Buffer.Handle;
			bufferPtrs[2] = bufferBindingThree.Buffer.Handle;

			offsets[0] = bufferBindingOne.Offset;
			offsets[1] = bufferBindingTwo.Offset;
			offsets[2] = bufferBindingThree.Offset;

			Refresh.Refresh_BindVertexBuffers(
				Device.Handle,
				Handle,
				firstBinding,
				3,
				(IntPtr) bufferPtrs,
				(IntPtr) offsets
			);
		}

		/// <summary>
		/// Binds vertex buffers to be used by subsequent draw calls.
		/// </summary>
		/// <param name="bufferBindingOne">Buffer to bind and associated offset.</param>
		/// <param name="bufferBindingTwo">Buffer to bind and associated offset.</param>
		/// <param name="bufferBindingThree">Buffer to bind and associated offset.</param>
		/// <param name="bufferBindingFour">Buffer to bind and associated offset.</param>
		/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
		public unsafe void BindVertexBuffers(
			in BufferBinding bufferBindingOne,
			in BufferBinding bufferBindingTwo,
			in BufferBinding bufferBindingThree,
			in BufferBinding bufferBindingFour,
			uint firstBinding = 0
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			var bufferPtrs = stackalloc IntPtr[4];
			var offsets = stackalloc ulong[4];

			bufferPtrs[0] = bufferBindingOne.Buffer.Handle;
			bufferPtrs[1] = bufferBindingTwo.Buffer.Handle;
			bufferPtrs[2] = bufferBindingThree.Buffer.Handle;
			bufferPtrs[3] = bufferBindingFour.Buffer.Handle;

			offsets[0] = bufferBindingOne.Offset;
			offsets[1] = bufferBindingTwo.Offset;
			offsets[2] = bufferBindingThree.Offset;
			offsets[3] = bufferBindingFour.Offset;

			Refresh.Refresh_BindVertexBuffers(
				Device.Handle,
				Handle,
				firstBinding,
				4,
				(IntPtr) bufferPtrs,
				(IntPtr) offsets
			);
		}

		/// <summary>
		/// Binds vertex buffers to be used by subsequent draw calls.
		/// </summary>
		/// <param name="bufferBindings">Spawn of buffers to bind and their associated offsets.</param>
		/// <param name="firstBinding">The index of the first vertex input binding whose state is updated by the command.</param>
		public unsafe void BindVertexBuffers(
			in Span<BufferBinding> bufferBindings,
			uint firstBinding = 0
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			var bufferPtrs = stackalloc IntPtr[bufferBindings.Length];
			var offsets = stackalloc ulong[bufferBindings.Length];

			for (var i = 0; i < bufferBindings.Length; i += 1)
			{
				bufferPtrs[i] = bufferBindings[i].Buffer.Handle;
				offsets[i] = bufferBindings[i].Offset;
			}

			Refresh.Refresh_BindVertexBuffers(
				Device.Handle,
				Handle,
				firstBinding,
				(uint) bufferBindings.Length,
				(IntPtr) bufferPtrs,
				(IntPtr) offsets
			);
		}

		/// <summary>
		/// Binds an index buffer to be used by subsequent draw calls.
		/// </summary>
		/// <param name="indexBuffer">The index buffer to bind.</param>
		/// <param name="indexElementSize">The size in bytes of the index buffer elements.</param>
		/// <param name="offset">The offset index for the buffer.</param>
		public void BindIndexBuffer(
			GpuBuffer indexBuffer,
			IndexElementSize indexElementSize,
			uint offset = 0
		)
		{
#if DEBUG
			AssertNotSubmitted();
#endif

			Refresh.Refresh_BindIndexBuffer(
				Device.Handle,
				Handle,
				indexBuffer.Handle,
				offset,
				(Refresh.IndexElementSize) indexElementSize
			);
		}

		/// <summary>
		/// Binds samplers to be used by the vertex shader.
		/// </summary>
		/// <param name="textureSamplerBindings">The texture-sampler to bind.</param>
		public unsafe void BindVertexSamplers(
			in TextureSamplerBinding textureSamplerBinding
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertVertexSamplerCount(1);
			AssertTextureSamplerBindingNonNull(textureSamplerBinding);
			AssertTextureBindingUsageFlags(textureSamplerBinding.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[1];
			var samplerPtrs = stackalloc IntPtr[1];

			texturePtrs[0] = textureSamplerBinding.Texture.Handle;
			samplerPtrs[0] = textureSamplerBinding.Sampler.Handle;

			Refresh.Refresh_BindVertexSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the vertex shader.
		/// </summary>
		/// <param name="textureSamplerBindingOne">The first texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingTwo">The second texture-sampler to bind.</param>
		public unsafe void BindVertexSamplers(
			in TextureSamplerBinding textureSamplerBindingOne,
			in TextureSamplerBinding textureSamplerBindingTwo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertVertexSamplerCount(2);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingOne);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingTwo);
			AssertTextureBindingUsageFlags(textureSamplerBindingOne.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingTwo.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[2];
			var samplerPtrs = stackalloc IntPtr[2];

			texturePtrs[0] = textureSamplerBindingOne.Texture.Handle;
			texturePtrs[1] = textureSamplerBindingTwo.Texture.Handle;

			samplerPtrs[0] = textureSamplerBindingOne.Sampler.Handle;
			samplerPtrs[1] = textureSamplerBindingTwo.Sampler.Handle;

			Refresh.Refresh_BindVertexSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the vertex shader.
		/// </summary>
		/// <param name="textureSamplerBindingOne">The first texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingTwo">The second texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingThree">The third texture-sampler to bind.</param>
		public unsafe void BindVertexSamplers(
			in TextureSamplerBinding textureSamplerBindingOne,
			in TextureSamplerBinding textureSamplerBindingTwo,
			in TextureSamplerBinding textureSamplerBindingThree
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertVertexSamplerCount(3);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingOne);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingTwo);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingThree);
			AssertTextureBindingUsageFlags(textureSamplerBindingOne.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingTwo.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingThree.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[3];
			var samplerPtrs = stackalloc IntPtr[3];

			texturePtrs[0] = textureSamplerBindingOne.Texture.Handle;
			texturePtrs[1] = textureSamplerBindingTwo.Texture.Handle;
			texturePtrs[2] = textureSamplerBindingThree.Texture.Handle;

			samplerPtrs[0] = textureSamplerBindingOne.Sampler.Handle;
			samplerPtrs[1] = textureSamplerBindingTwo.Sampler.Handle;
			samplerPtrs[2] = textureSamplerBindingThree.Sampler.Handle;

			Refresh.Refresh_BindVertexSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the vertex shader.
		/// </summary>
		/// <param name="textureSamplerBindingOne">The first texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingTwo">The second texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingThree">The third texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingThree">The fourth texture-sampler to bind.</param>
		public unsafe void BindVertexSamplers(
			in TextureSamplerBinding textureSamplerBindingOne,
			in TextureSamplerBinding textureSamplerBindingTwo,
			in TextureSamplerBinding textureSamplerBindingThree,
			in TextureSamplerBinding textureSamplerBindingFour
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertVertexSamplerCount(4);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingOne);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingTwo);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingThree);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingFour);
			AssertTextureBindingUsageFlags(textureSamplerBindingOne.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingTwo.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingThree.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingFour.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[4];
			var samplerPtrs = stackalloc IntPtr[4];

			texturePtrs[0] = textureSamplerBindingOne.Texture.Handle;
			texturePtrs[1] = textureSamplerBindingTwo.Texture.Handle;
			texturePtrs[2] = textureSamplerBindingThree.Texture.Handle;
			texturePtrs[3] = textureSamplerBindingFour.Texture.Handle;

			samplerPtrs[0] = textureSamplerBindingOne.Sampler.Handle;
			samplerPtrs[1] = textureSamplerBindingTwo.Sampler.Handle;
			samplerPtrs[2] = textureSamplerBindingThree.Sampler.Handle;
			samplerPtrs[3] = textureSamplerBindingFour.Sampler.Handle;

			Refresh.Refresh_BindVertexSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the vertex shader.
		/// </summary>
		/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
		public unsafe void BindVertexSamplers(
			in Span<TextureSamplerBinding> textureSamplerBindings
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertVertexSamplerCount(textureSamplerBindings.Length);
#endif

			var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Length];
			var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Length];

			for (var i = 0; i < textureSamplerBindings.Length; i += 1)
			{
#if DEBUG
				AssertTextureSamplerBindingNonNull(textureSamplerBindings[i]);
				AssertTextureBindingUsageFlags(textureSamplerBindings[i].Texture);
#endif

				texturePtrs[i] = textureSamplerBindings[i].Texture.Handle;
				samplerPtrs[i] = textureSamplerBindings[i].Sampler.Handle;
			}

			Refresh.Refresh_BindVertexSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the fragment shader.
		/// </summary>
		/// <param name="textureSamplerBinding">The texture-sampler to bind.</param>
		public unsafe void BindFragmentSamplers(
			in TextureSamplerBinding textureSamplerBinding
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertFragmentSamplerCount(1);
			AssertTextureSamplerBindingNonNull(textureSamplerBinding);
			AssertTextureBindingUsageFlags(textureSamplerBinding.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[1];
			var samplerPtrs = stackalloc IntPtr[1];

			texturePtrs[0] = textureSamplerBinding.Texture.Handle;
			samplerPtrs[0] = textureSamplerBinding.Sampler.Handle;

			Refresh.Refresh_BindFragmentSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the fragment shader.
		/// </summary>
		/// <param name="textureSamplerBindingOne">The first texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingTwo">The second texture-sampler to bind.</param>
		public unsafe void BindFragmentSamplers(
			in TextureSamplerBinding textureSamplerBindingOne,
			in TextureSamplerBinding textureSamplerBindingTwo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertFragmentSamplerCount(2);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingOne);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingTwo);
			AssertTextureBindingUsageFlags(textureSamplerBindingOne.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingTwo.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[2];
			var samplerPtrs = stackalloc IntPtr[2];

			texturePtrs[0] = textureSamplerBindingOne.Texture.Handle;
			texturePtrs[1] = textureSamplerBindingTwo.Texture.Handle;

			samplerPtrs[0] = textureSamplerBindingOne.Sampler.Handle;
			samplerPtrs[1] = textureSamplerBindingTwo.Sampler.Handle;

			Refresh.Refresh_BindFragmentSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the fragment shader.
		/// </summary>
		/// <param name="textureSamplerBindingOne">The first texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingTwo">The second texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingThree">The third texture-sampler to bind.</param>
		public unsafe void BindFragmentSamplers(
			in TextureSamplerBinding textureSamplerBindingOne,
			in TextureSamplerBinding textureSamplerBindingTwo,
			in TextureSamplerBinding textureSamplerBindingThree
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertFragmentSamplerCount(3);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingOne);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingTwo);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingThree);
			AssertTextureBindingUsageFlags(textureSamplerBindingOne.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingTwo.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingThree.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[3];
			var samplerPtrs = stackalloc IntPtr[3];

			texturePtrs[0] = textureSamplerBindingOne.Texture.Handle;
			texturePtrs[1] = textureSamplerBindingTwo.Texture.Handle;
			texturePtrs[2] = textureSamplerBindingThree.Texture.Handle;

			samplerPtrs[0] = textureSamplerBindingOne.Sampler.Handle;
			samplerPtrs[1] = textureSamplerBindingTwo.Sampler.Handle;
			samplerPtrs[2] = textureSamplerBindingThree.Sampler.Handle;

			Refresh.Refresh_BindFragmentSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the fragment shader.
		/// </summary>
		/// <param name="textureSamplerBindingOne">The first texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingTwo">The second texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingThree">The third texture-sampler to bind.</param>
		/// <param name="textureSamplerBindingFour">The fourth texture-sampler to bind.</param>
		public unsafe void BindFragmentSamplers(
			in TextureSamplerBinding textureSamplerBindingOne,
			in TextureSamplerBinding textureSamplerBindingTwo,
			in TextureSamplerBinding textureSamplerBindingThree,
			in TextureSamplerBinding textureSamplerBindingFour
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertFragmentSamplerCount(4);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingOne);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingTwo);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingThree);
			AssertTextureSamplerBindingNonNull(textureSamplerBindingFour);
			AssertTextureBindingUsageFlags(textureSamplerBindingOne.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingTwo.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingThree.Texture);
			AssertTextureBindingUsageFlags(textureSamplerBindingFour.Texture);
#endif

			var texturePtrs = stackalloc IntPtr[4];
			var samplerPtrs = stackalloc IntPtr[4];

			texturePtrs[0] = textureSamplerBindingOne.Texture.Handle;
			texturePtrs[1] = textureSamplerBindingTwo.Texture.Handle;
			texturePtrs[2] = textureSamplerBindingThree.Texture.Handle;
			texturePtrs[3] = textureSamplerBindingFour.Texture.Handle;

			samplerPtrs[0] = textureSamplerBindingOne.Sampler.Handle;
			samplerPtrs[1] = textureSamplerBindingTwo.Sampler.Handle;
			samplerPtrs[2] = textureSamplerBindingThree.Sampler.Handle;
			samplerPtrs[3] = textureSamplerBindingFour.Sampler.Handle;

			Refresh.Refresh_BindFragmentSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Binds samplers to be used by the fragment shader.
		/// </summary>
		/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
		public unsafe void BindFragmentSamplers(
			in Span<TextureSamplerBinding> textureSamplerBindings
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
			AssertFragmentSamplerCount(textureSamplerBindings.Length);
#endif

			var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Length];
			var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Length];

			for (var i = 0; i < textureSamplerBindings.Length; i += 1)
			{
#if DEBUG
				AssertTextureSamplerBindingNonNull(textureSamplerBindings[i]);
				AssertTextureBindingUsageFlags(textureSamplerBindings[i].Texture);
#endif

				texturePtrs[i] = textureSamplerBindings[i].Texture.Handle;
				samplerPtrs[i] = textureSamplerBindings[i].Sampler.Handle;
			}

			Refresh.Refresh_BindFragmentSamplers(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs,
				(IntPtr) samplerPtrs
			);
		}

		/// <summary>
		/// Pushes vertex shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset value to be used with draw calls.</returns>
		public unsafe void PushVertexShaderUniforms(
			void* uniformsPtr,
			uint size
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();

			if (currentGraphicsPipeline.VertexShaderInfo.UniformBufferSize == 0)
			{
				throw new InvalidOperationException("The current vertex shader does not take a uniform buffer!");
			}

			if (currentGraphicsPipeline.VertexShaderInfo.UniformBufferSize != size)
			{
				throw new InvalidOperationException("Vertex uniform data size mismatch!");
			}
#endif

			Refresh.Refresh_PushVertexShaderUniforms(
				Device.Handle,
				Handle,
				(IntPtr) uniformsPtr,
				size
			);
		}

		/// <summary>
		/// Pushes vertex shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset value to be used with draw calls.</returns>
		public unsafe void PushVertexShaderUniforms<T>(
			in T uniforms
		) where T : unmanaged
		{
			fixed (T* uniformsPtr = &uniforms)
			{
				PushVertexShaderUniforms(uniformsPtr, (uint) Marshal.SizeOf<T>());
			}
		}

		/// <summary>
		/// Pushes fragment shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with draw calls.</returns>
		public unsafe void PushFragmentShaderUniforms(
			void* uniformsPtr,
			uint size
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();

			if (currentGraphicsPipeline.FragmentShaderInfo.UniformBufferSize == 0)
			{
				throw new InvalidOperationException("The current fragment shader does not take a uniform buffer!");
			}

			if (currentGraphicsPipeline.FragmentShaderInfo.UniformBufferSize != size)
			{
				throw new InvalidOperationException("Fragment uniform data size mismatch!");
			}
#endif

			Refresh.Refresh_PushFragmentShaderUniforms(
				Device.Handle,
				Handle,
				(IntPtr) uniformsPtr,
				size
			);
		}

		/// <summary>
		/// Pushes fragment shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with draw calls.</returns>
		public unsafe void PushFragmentShaderUniforms<T>(
			in T uniforms
		) where T : unmanaged
		{
			fixed (T* uniformsPtr = &uniforms)
			{
				PushFragmentShaderUniforms(uniformsPtr, (uint) Marshal.SizeOf<T>());
			}
		}

		/// <summary>
		/// Draws using instanced rendering.
		/// </summary>
		/// <param name="baseVertex">The starting index offset for the vertex buffer.</param>
		/// <param name="startIndex">The starting index offset for the index buffer.</param>
		/// <param name="primitiveCount">The number of primitives to draw.</param>
		/// <param name="instanceCount">The number of instances to draw.</param>
		public void DrawInstancedPrimitives(
			uint baseVertex,
			uint startIndex,
			uint primitiveCount,
			uint instanceCount
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			Refresh.Refresh_DrawInstancedPrimitives(
				Device.Handle,
				Handle,
				baseVertex,
				startIndex,
				primitiveCount,
				instanceCount
			);
		}

		/// <summary>
		/// Draws using a vertex buffer and an index buffer.
		/// </summary>
		/// <param name="baseVertex">The starting index offset for the vertex buffer.</param>
		/// <param name="startIndex">The starting index offset for the index buffer.</param>
		/// <param name="primitiveCount">The number of primitives to draw.</param>
		public void DrawIndexedPrimitives(
			uint baseVertex,
			uint startIndex,
			uint primitiveCount
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			Refresh.Refresh_DrawIndexedPrimitives(
				Device.Handle,
				Handle,
				baseVertex,
				startIndex,
				primitiveCount
			);
		}

		/// <summary>
		/// Draws using a vertex buffer.
		/// </summary>
		/// <param name="vertexStart"></param>
		/// <param name="primitiveCount"></param>
		public void DrawPrimitives(
			uint vertexStart,
			uint primitiveCount
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			Refresh.Refresh_DrawPrimitives(
				Device.Handle,
				Handle,
				vertexStart,
				primitiveCount
			);
		}

		/// <summary>
		/// Similar to DrawPrimitives, but parameters are set from a buffer.
		/// </summary>
		/// <param name="buffer">The draw parameters buffer.</param>
		/// <param name="offsetInBytes">The offset to start reading from the draw parameters buffer.</param>
		/// <param name="drawCount">The number of draw parameter sets that should be read from the buffer.</param>
		/// <param name="stride">The byte stride between sets of draw parameters.</param>
		/// <param name="vertexParamOffset">An offset value obtained from PushVertexShaderUniforms. If no uniforms are required then use 0.</param>
		/// <param name="fragmentParamOffset">An offset value obtained from PushFragmentShaderUniforms. If no uniforms are required the use 0.</param>
		public void DrawPrimitivesIndirect(
			GpuBuffer buffer,
			uint offsetInBytes,
			uint drawCount,
			uint stride
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertGraphicsPipelineBound();
#endif

			Refresh.Refresh_DrawPrimitivesIndirect(
				Device.Handle,
				Handle,
				buffer.Handle,
				offsetInBytes,
				drawCount,
				stride
			);
		}

		/// <summary>
		/// Ends the current render pass.
		/// This must be called before beginning another render pass or submitting the command buffer.
		/// </summary>
		public void EndRenderPass()
		{
#if DEBUG
			AssertNotSubmitted();
#endif

			Refresh.Refresh_EndRenderPass(
				Device.Handle,
				Handle
			);

#if DEBUG
			currentGraphicsPipeline = null;
			renderPassActive = false;
#endif
		}

		/// <summary>
		/// Blits a texture to another texture with the specified filter.
		///
		/// This operation cannot be performed inside any pass.
		/// </summary>
		public void Blit(
			Texture source,
			Texture destination,
			Filter filter
		) {
			var sampler = filter == Filter.Linear ? Device.LinearSampler : Device.PointSampler;

			BeginRenderPass(new ColorAttachmentInfo(destination));
			BindGraphicsPipeline(Device.BlitPipeline);
			BindFragmentSamplers(new TextureSamplerBinding(source, sampler));
			DrawPrimitives(0, 2);
			EndRenderPass();
		}

		public void BeginComputePass()
		{
#if DEBUG
			AssertNotSubmitted();
			AssertNotInPass("Cannot begin compute pass while in another pass!");
			computePassActive = true;
#endif

			Refresh.Refresh_BeginComputePass(
				Device.Handle,
				Handle
			);
		}

		/// <summary>
		/// Binds a compute pipeline so that compute work may be dispatched.
		/// </summary>
		/// <param name="computePipeline">The compute pipeline to bind.</param>
		public void BindComputePipeline(
			ComputePipeline computePipeline
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute pipeline outside of compute pass!");
#endif

			Refresh.Refresh_BindComputePipeline(
				Device.Handle,
				Handle,
				computePipeline.Handle
			);

#if DEBUG
			currentComputePipeline = computePipeline;
#endif
		}

		/// <summary>
		/// Binds a buffer to be used in the compute shader.
		/// </summary>
		/// <param name="buffer">A buffer to bind.</param>
		public unsafe void BindComputeBuffers(
			GpuBuffer buffer
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute buffers outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeBufferCount(1);
#endif

			var bufferPtrs = stackalloc IntPtr[1];
			bufferPtrs[0] = buffer.Handle;

			Refresh.Refresh_BindComputeBuffers(
				Device.Handle,
				Handle,
				(IntPtr) bufferPtrs
			);
		}

		/// <summary>
		/// Binds buffers to be used in the compute shader.
		/// </summary>
		/// <param name="bufferOne">A buffer to bind.</param>
		/// <param name="bufferTwo">A buffer to bind.</param>
		public unsafe void BindComputeBuffers(
			GpuBuffer bufferOne,
			GpuBuffer bufferTwo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute buffers outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeBufferCount(2);
#endif

			var bufferPtrs = stackalloc IntPtr[2];
			bufferPtrs[0] = bufferOne.Handle;
			bufferPtrs[1] = bufferTwo.Handle;

			Refresh.Refresh_BindComputeBuffers(
				Device.Handle,
				Handle,
				(IntPtr) bufferPtrs
			);
		}

		/// <summary>
		/// Binds buffers to be used in the compute shader.
		/// </summary>
		/// <param name="bufferOne">A buffer to bind.</param>
		/// <param name="bufferTwo">A buffer to bind.</param>
		/// <param name="bufferThree">A buffer to bind.</param>
		public unsafe void BindComputeBuffers(
			GpuBuffer bufferOne,
			GpuBuffer bufferTwo,
			GpuBuffer bufferThree
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute buffers outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeBufferCount(3);
#endif

			var bufferPtrs = stackalloc IntPtr[3];
			bufferPtrs[0] = bufferOne.Handle;
			bufferPtrs[1] = bufferTwo.Handle;
			bufferPtrs[2] = bufferThree.Handle;

			Refresh.Refresh_BindComputeBuffers(
				Device.Handle,
				Handle,
				(IntPtr) bufferPtrs
			);
		}

		/// <summary>
		/// Binds buffers to be used in the compute shader.
		/// </summary>
		/// <param name="bufferOne">A buffer to bind.</param>
		/// <param name="bufferTwo">A buffer to bind.</param>
		/// <param name="bufferThree">A buffer to bind.</param>
		/// <param name="bufferFour">A buffer to bind.</param>
		public unsafe void BindComputeBuffers(
			GpuBuffer bufferOne,
			GpuBuffer bufferTwo,
			GpuBuffer bufferThree,
			GpuBuffer bufferFour
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute buffers outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeBufferCount(4);
#endif

			var bufferPtrs = stackalloc IntPtr[4];
			bufferPtrs[0] = bufferOne.Handle;
			bufferPtrs[1] = bufferTwo.Handle;
			bufferPtrs[2] = bufferThree.Handle;
			bufferPtrs[3] = bufferFour.Handle;

			Refresh.Refresh_BindComputeBuffers(
				Device.Handle,
				Handle,
				(IntPtr) bufferPtrs
			);
		}

		/// <summary>
		/// Binds buffers to be used in the compute shader.
		/// </summary>
		/// <param name="buffers">A Span of buffers to bind.</param>
		public unsafe void BindComputeBuffers(
			in Span<GpuBuffer> buffers
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute buffers outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeBufferCount(buffers.Length);
#endif

			var bufferPtrs = stackalloc IntPtr[buffers.Length];

			for (var i = 0; i < buffers.Length; i += 1)
			{
				bufferPtrs[i] = buffers[i].Handle;
			}

			Refresh.Refresh_BindComputeBuffers(
				Device.Handle,
				Handle,
				(IntPtr) bufferPtrs
			);
		}

		/// <summary>
		/// Binds a texture to be used in the compute shader.
		/// </summary>
		/// <param name="slice">A texture slice to bind.</param>
		public unsafe void BindComputeTextures(
			TextureSlice slice
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute textures outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeTextureCount(1);
#endif

			var textureSlicePtrs = stackalloc Refresh.TextureSlice[1];
			textureSlicePtrs[0] = slice.ToRefreshTextureSlice();

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) textureSlicePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="sliceOne">A texture-level pair to bind.</param>
		/// <param name="sliceTwo">A texture-level pair to bind.</param>
		public unsafe void BindComputeTextures(
			TextureSlice sliceOne,
			TextureSlice sliceTwo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute textures outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeTextureCount(2);
#endif

			var textureSlicePtrs = stackalloc Refresh.TextureSlice[2];
			textureSlicePtrs[0] = sliceOne.ToRefreshTextureSlice();
			textureSlicePtrs[1] = sliceTwo.ToRefreshTextureSlice();

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) textureSlicePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="sliceOne">A texture-level pair to bind.</param>
		/// <param name="sliceTwo">A texture-level pair to bind.</param>
		/// <param name="sliceThree">A texture-level pair to bind.</param>
		public unsafe void BindComputeTextures(
			TextureSlice sliceOne,
			TextureSlice sliceTwo,
			TextureSlice sliceThree
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute textures outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeTextureCount(3);
#endif

			var textureSlicePtrs = stackalloc Refresh.TextureSlice[3];
			textureSlicePtrs[0] = sliceOne.ToRefreshTextureSlice();
			textureSlicePtrs[1] = sliceTwo.ToRefreshTextureSlice();
			textureSlicePtrs[2] = sliceThree.ToRefreshTextureSlice();

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) textureSlicePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="sliceOne">A texture-level pair to bind.</param>
		/// <param name="sliceTwo">A texture-level pair to bind.</param>
		/// <param name="sliceThree">A texture-level pair to bind.</param>
		/// <param name="sliceFour">A texture-level pair to bind.</param>
		public unsafe void BindComputeTextures(
			TextureSlice sliceOne,
			TextureSlice sliceTwo,
			TextureSlice sliceThree,
			TextureSlice sliceFour
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute textures outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeTextureCount(4);
#endif

			var textureSlicePtrs = stackalloc Refresh.TextureSlice[4];
			textureSlicePtrs[0] = sliceOne.ToRefreshTextureSlice();
			textureSlicePtrs[1] = sliceTwo.ToRefreshTextureSlice();
			textureSlicePtrs[2] = sliceThree.ToRefreshTextureSlice();
			textureSlicePtrs[3] = sliceFour.ToRefreshTextureSlice();

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) textureSlicePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="slices">A set of texture-level pairs to bind.</param>
		public unsafe void BindComputeTextures(
			in Span<TextureSlice> slices
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot bind compute textures outside of compute pass!");
			AssertComputePipelineBound();
			AssertComputeTextureCount(slices.Length);
#endif

			Refresh.TextureSlice* textureSlicePtrs = (Refresh.TextureSlice*) NativeMemory.Alloc(
				(nuint) (Marshal.SizeOf<Refresh.TextureSlice>() * slices.Length)
			);

			for (var i = 0; i < slices.Length; i += 1)
			{
				textureSlicePtrs[i] = slices[i].ToRefreshTextureSlice();
			}

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) textureSlicePtrs
			);

			NativeMemory.Free(textureSlicePtrs);
		}

		/// <summary>
		/// Pushes compute shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with dispatch calls.</returns>
		public unsafe void PushComputeShaderUniforms(
			void* uniformsPtr,
			uint size
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertComputePipelineBound();

			if (currentComputePipeline.ComputeShaderInfo.UniformBufferSize == 0)
			{
				throw new System.InvalidOperationException("The current compute shader does not take a uniform buffer!");
			}

			if (currentComputePipeline.ComputeShaderInfo.UniformBufferSize != size)
			{
				throw new InvalidOperationException("Compute uniform data size mismatch!");
			}
#endif

			Refresh.Refresh_PushComputeShaderUniforms(
				Device.Handle,
				Handle,
				(IntPtr) uniformsPtr,
				size
			);
		}

		/// <summary>
		/// Pushes compute shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with dispatch calls.</returns>
		public unsafe void PushComputeShaderUniforms<T>(
			in T uniforms
		) where T : unmanaged
		{
			fixed (T* uniformsPtr = &uniforms)
			{
				PushComputeShaderUniforms(uniformsPtr, (uint) Marshal.SizeOf<T>());
			}
		}

		/// <summary>
		/// Dispatches compute work.
		/// </summary>
		/// <param name="groupCountX"></param>
		/// <param name="groupCountY"></param>
		/// <param name="groupCountZ"></param>
		/// <param name="computeParamOffset"></param>
		public void DispatchCompute(
			uint groupCountX,
			uint groupCountY,
			uint groupCountZ
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInComputePass("Cannot dispatch compute outside of compute pass!");
			AssertComputePipelineBound();

			if (groupCountX < 1 || groupCountY < 1 || groupCountZ < 1)
			{
				throw new ArgumentException("All dimensions for the compute work group must be >= 1!");
			}
#endif

			Refresh.Refresh_DispatchCompute(
				Device.Handle,
				Handle,
				groupCountX,
				groupCountY,
				groupCountZ
			);
		}

		public void EndComputePass()
		{
#if DEBUG
			AssertInComputePass("Cannot end compute pass while not in a compute pass!");
			computePassActive = false;
#endif

			Refresh.Refresh_EndComputePass(
				Device.Handle,
				Handle
			);
		}

		// Copy Pass

		/// <summary>
		/// Begins a copy pass.
		/// All copy commands must be made within a copy pass.
		/// It is an error to call this during any kind of pass.
		/// </summary>
		public void BeginCopyPass()
		{
#if DEBUG
			AssertNotSubmitted();
			AssertNotInPass("Cannot begin copy pass while in another pass!");
			copyPassActive = true;
#endif

			Refresh.Refresh_BeginCopyPass(
				Device.Handle,
				Handle
			);
		}

		/// <summary>
		/// Uploads data from a TransferBuffer to a TextureSlice.
		/// This copy occurs on the GPU timeline.
		///
		/// Overwriting the contents of the TransferBuffer before the command buffer
		/// has finished execution will cause undefined behavior.
		///
		/// You MAY assume that the copy has finished for subsequent commands.
		/// </summary>
		public void UploadToTexture(
			TransferBuffer transferBuffer,
			in TextureRegion textureRegion,
			in BufferImageCopy copyParams,
			CopyOptions option
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot upload to texture outside of copy pass!");
			AssertBufferBoundsCheck(transferBuffer.Size, copyParams.BufferOffset, textureRegion.Size);
#endif

			Refresh.Refresh_UploadToTexture(
				Device.Handle,
				Handle,
				transferBuffer.Handle,
				textureRegion.ToRefreshTextureRegion(),
				copyParams.ToRefresh(),
				(Refresh.CopyOptions) option
			);
		}

		/// <summary>
		/// Uploads the contents of an entire buffer to a texture with no mips.
		/// </summary>
		public void UploadToTexture(
			TransferBuffer transferBuffer,
			Texture texture,
			CopyOptions option
		) {
			UploadToTexture(
				transferBuffer,
				new TextureRegion(texture),
				new BufferImageCopy(0, 0, 0),
				option
			);
		}

		/// <summary>
		/// Uploads data from a TransferBuffer to a GpuBuffer.
		/// This copy occurs on the GPU timeline.
		///
		/// Overwriting the contents of the TransferBuffer before the command buffer
		/// has finished execution will cause undefined behavior.
		///
		/// You MAY assume that the copy has finished for subsequent commands.
		/// </summary>
		public void UploadToBuffer(
			TransferBuffer transferBuffer,
			GpuBuffer gpuBuffer,
			in BufferCopy copyParams,
			CopyOptions option
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot upload to texture outside of copy pass!");
			AssertBufferBoundsCheck(transferBuffer.Size, copyParams.SrcOffset, copyParams.Size);
			AssertBufferBoundsCheck(gpuBuffer.Size, copyParams.DstOffset, copyParams.Size);
#endif

			Refresh.Refresh_UploadToBuffer(
				Device.Handle,
				Handle,
				transferBuffer.Handle,
				gpuBuffer.Handle,
				copyParams.ToRefresh(),
				(Refresh.CopyOptions) option
			);
		}

		/// <summary>
		/// Copies the entire contents of a TransferBuffer to a GpuBuffer.
		/// </summary>
		public void UploadToBuffer(
			TransferBuffer transferBuffer,
			GpuBuffer gpuBuffer,
			CopyOptions option
		) {
			UploadToBuffer(
				transferBuffer,
				gpuBuffer,
				new BufferCopy(0, 0, transferBuffer.Size),
				option
			);
		}

		/// <summary>
		/// Copies data element-wise into from a TransferBuffer to a GpuBuffer.
		/// </summary>
		public void UploadToBuffer<T>(
			TransferBuffer transferBuffer,
			GpuBuffer gpuBuffer,
			uint sourceStartElement,
			uint destinationStartElement,
			uint numElements,
			CopyOptions option
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * numElements);
			var srcOffsetInBytes = (uint) (elementSize * sourceStartElement);
			var dstOffsetInBytes = (uint) (elementSize * destinationStartElement);

			UploadToBuffer(
				transferBuffer,
				gpuBuffer,
				new BufferCopy(
					srcOffsetInBytes,
					dstOffsetInBytes,
					dataLengthInBytes
				),
				option
			);
		}

		/// <summary>
		/// Downloads data from a Texture to a TransferBuffer.
		/// This copy occurs on the GPU timeline.
		///
		/// You MAY NOT assume that the data in the TransferBuffer is
		/// fully copied until the command buffer has finished execution.
		/// </summary>
		public void DownloadFromTexture(
			in TextureRegion textureRegion,
			TransferBuffer transferBuffer,
			in BufferImageCopy copyParams,
			TransferOptions option
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot download from texture outside of copy pass!");
			AssertBufferBoundsCheck(transferBuffer.Size, copyParams.BufferOffset, textureRegion.Size);
#endif

			Refresh.Refresh_DownloadFromTexture(
				Device.Handle,
				Handle,
				textureRegion.ToRefreshTextureRegion(),
				transferBuffer.Handle,
				copyParams.ToRefresh(),
				(Refresh.TransferOptions) option
			);
		}

		/// <summary>
		/// Downloads the contents of a Texture with no mips into a TransferBuffer.
		/// </summary>
		public void DownloadFromTexture(
			Texture texture,
			TransferBuffer transferBuffer,
			TransferOptions option
		) {
			DownloadFromTexture(
				new TextureRegion(texture),
				transferBuffer,
				new BufferImageCopy(0, 0, 0),
				option
			);
		}

		/// <summary>
		/// Downloads data from a GpuBuffer to a TransferBuffer.
		/// This copy occurs on the GPU timeline.
		///
		/// You MAY NOT assume that the data in the TransferBuffer is
		/// fully copied until the command buffer has finished execution.
		/// </summary>
		public void DownloadFromBuffer(
			GpuBuffer gpuBuffer,
			TransferBuffer transferBuffer,
			in BufferCopy copyParams,
			TransferOptions option
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot download from texture outside of copy pass!");
			AssertBufferBoundsCheck(transferBuffer.Size, copyParams.DstOffset, copyParams.Size);
#endif

			Refresh.Refresh_DownloadFromBuffer(
				Device.Handle,
				Handle,
				gpuBuffer.Handle,
				transferBuffer.Handle,
				copyParams.ToRefresh(),
				(Refresh.TransferOptions) option
			);
		}

		/// <summary>
		/// Downloads data from a GpuBuffer to a TransferBuffer.
		/// This copy occurs on the GPU timeline.
		///
		/// You MAY NOT assume that the data in the TransferBuffer is
		/// fully copied until the command buffer has finished execution.
		/// </summary>
		public void DownloadFromBuffer(
			GpuBuffer gpuBuffer,
			TransferBuffer transferBuffer,
			TransferOptions option
		) {
			DownloadFromBuffer(
				gpuBuffer,
				transferBuffer,
				new BufferCopy(0, 0, gpuBuffer.Size),
				option
			);
		}

		/// <summary>
		/// Copies the contents of a TextureSlice to another TextureSlice.
		/// The slices must have the same dimensions.
		/// This copy occurs on the GPU timeline.
		///
		/// You MAY assume that the copy has finished in subsequent commands.
		/// </summary>
		public void CopyTextureToTexture(
			in TextureRegion source,
			in TextureRegion destination,
			CopyOptions option
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot download from texture outside of copy pass!");
			AssertTextureBoundsCheck(destination.Size, source.Size);
#endif

			Refresh.Refresh_CopyTextureToTexture(
				Device.Handle,
				Handle,
				source.ToRefreshTextureRegion(),
				destination.ToRefreshTextureRegion(),
				(Refresh.CopyOptions) option
			);
		}

		/// <summary>
		/// Copies the contents of an entire Texture with no mips to another Texture with no mips.
		/// The textures must have the same dimensions.
		/// </summary>
		public void CopyTextureToTexture(
			Texture source,
			Texture destination,
			CopyOptions option
		) {
			CopyTextureToTexture(
				new TextureRegion(source),
				new TextureRegion(destination),
				option
			);
		}

		/// <summary>
		/// Copies the contents of a Texture to a GpuBuffer.
		/// This copy occurs on the GPU timeline.
		///
		/// You MAY assume that the copy has finished in subsequent commands.
		/// </summary>
		public void CopyTextureToBuffer(
			in TextureRegion textureRegion,
			GpuBuffer buffer,
			in BufferImageCopy copyParams,
			CopyOptions option
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot download from texture outside of copy pass!");
			AssertBufferBoundsCheck(buffer.Size, copyParams.BufferOffset, textureRegion.Size);
#endif

			Refresh.Refresh_CopyTextureToBuffer(
				Device.Handle,
				Handle,
				textureRegion.ToRefreshTextureRegion(),
				buffer.Handle,
				copyParams.ToRefresh(),
				(Refresh.CopyOptions) option
			);
		}

		/// <summary>
		/// Copies the entire contents of a Texture to a GpuBuffer.
		/// </summary>
		public void CopyTextureToBuffer(
			Texture texture,
			GpuBuffer buffer,
			CopyOptions option
		) {
			CopyTextureToBuffer(
				new TextureRegion(texture),
				buffer,
				new BufferImageCopy(0, 0, 0),
				option
			);
		}

		/// <summary>
		/// Copies the contents of a GpuBuffer to a Texture.
		/// This copy occurs on the GPU timeline.
		///
		/// You MAY assume that the copy has finished in subsequent commands.
		/// </summary>
		public void CopyBufferToTexture(
			GpuBuffer gpuBuffer,
			in TextureRegion textureRegion,
			in BufferImageCopy copyParams,
			CopyOptions option
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot download from texture outside of copy pass!");
			AssertBufferBoundsCheck(gpuBuffer.Size, copyParams.BufferOffset, textureRegion.Size);
#endif

			Refresh.Refresh_CopyBufferToTexture(
				Device.Handle,
				Handle,
				gpuBuffer.Handle,
				textureRegion.ToRefreshTextureRegion(),
				copyParams.ToRefresh(),
				(Refresh.CopyOptions) option
			);
		}

		/// <summary>
		/// Copies the entire contents of a Texture with no mips to a GpuBuffer.
		/// </summary>
		public void CopyBufferToTexture(
			GpuBuffer buffer,
			Texture texture,
			CopyOptions option
		) {
			CopyBufferToTexture(
				buffer,
				new TextureRegion(texture),
				new BufferImageCopy(0, 0, 0),
				option
			);
		}

		/// <summary>
		/// Copies data from a GpuBuffer to another GpuBuffer.
		/// This copy occurs on the GPU timeline.
		///
		/// You MAY assume that the copy has finished in subsequent commands.
		/// </summary>
		public void CopyBufferToBuffer(
			GpuBuffer source,
			GpuBuffer destination,
			in BufferCopy copyParams,
			CopyOptions option
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot download from texture outside of copy pass!");
			AssertBufferBoundsCheck(source.Size, copyParams.SrcOffset, copyParams.Size);
			AssertBufferBoundsCheck(destination.Size, copyParams.DstOffset, copyParams.Size);
#endif

			Refresh.Refresh_CopyBufferToBuffer(
				Device.Handle,
				Handle,
				source.Handle,
				destination.Handle,
				copyParams.ToRefresh(),
				(Refresh.CopyOptions) option
			);
		}

		/// <summary>
		/// Copies the entire contents of a GpuBuffer to another GpuBuffer.
		/// </summary>
		public void CopyBufferToBuffer(
			GpuBuffer source,
			GpuBuffer destination,
			CopyOptions option
		) {
			CopyBufferToBuffer(
				source,
				destination,
				new BufferCopy(0, 0, source.Size),
				option
			);
		}

		public void EndCopyPass()
		{
#if DEBUG
			AssertNotSubmitted();
			AssertInCopyPass("Cannot end copy pass while not in a copy pass!");
			copyPassActive = false;
#endif

			Refresh.Refresh_EndCopyPass(
				Device.Handle,
				Handle
			);
		}

#if DEBUG
		private void AssertRenderPassActive(string message = "No active render pass!")
		{
			if (!renderPassActive)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertRenderPassInactive(string message = "Render pass is active!")
		{
			if (renderPassActive)
			{
				throw new System.InvalidCastException(message);
			}
		}

		private void AssertGraphicsPipelineBound(string message = "No graphics pipeline is bound!")
		{
			if (currentGraphicsPipeline == null)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertRenderPassPipelineFormatMatch(GraphicsPipeline graphicsPipeline)
		{
			for (var i = 0; i < graphicsPipeline.AttachmentInfo.ColorAttachmentDescriptions.Length; i += 1)
			{
				TextureFormat format;
				if (i == 0)
				{
					format = colorFormatOne;
				}
				else if (i == 1)
				{
					format = colorFormatTwo;
				}
				else if (i == 2)
				{
					format = colorFormatThree;
				}
				else
				{
					format = colorFormatFour;
				}

				var pipelineFormat = graphicsPipeline.AttachmentInfo.ColorAttachmentDescriptions[i].Format;
				if (pipelineFormat != format)
				{
					throw new System.InvalidOperationException($"Color texture format mismatch! Pipeline expects {pipelineFormat}, render pass attachment is {format}");
				}
			}

			if (graphicsPipeline.AttachmentInfo.HasDepthStencilAttachment)
			{
				var pipelineDepthFormat = graphicsPipeline.AttachmentInfo.DepthStencilFormat;

				if (!hasDepthStencilAttachment)
				{
					throw new System.InvalidOperationException("Pipeline expects depth attachment!");
				}

				if (pipelineDepthFormat != depthStencilFormat)
				{
					throw new System.InvalidOperationException($"Depth texture format mismatch! Pipeline expects {pipelineDepthFormat}, render pass attachment is {depthStencilFormat}");
				}
			}
		}

		private void AssertVertexSamplerCount(int count)
		{
			if (currentGraphicsPipeline.VertexShaderInfo.SamplerBindingCount != count)
			{
				throw new System.InvalidOperationException($"Vertex sampler expected {currentGraphicsPipeline.VertexShaderInfo.SamplerBindingCount} samplers, but received {count}");
			}
		}

		private void AssertFragmentSamplerCount(int count)
		{
			if (currentGraphicsPipeline.FragmentShaderInfo.SamplerBindingCount != count)
			{
				throw new System.InvalidOperationException($"Fragment sampler expected {currentGraphicsPipeline.FragmentShaderInfo.SamplerBindingCount} samplers, but received {count}");
			}
		}

		private void AssertComputePipelineBound(string message = "No compute pipeline is bound!")
		{
			if (currentComputePipeline == null)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertComputeBufferCount(int count)
		{
			if (currentComputePipeline.ComputeShaderInfo.BufferBindingCount != count)
			{
				throw new System.InvalidOperationException($"Compute pipeline expects {currentComputePipeline.ComputeShaderInfo.BufferBindingCount} buffers, but received {count}");
			}
		}

		private void AssertComputeTextureCount(int count)
		{
			if (currentComputePipeline.ComputeShaderInfo.ImageBindingCount != count)
			{
				throw new System.InvalidOperationException($"Compute pipeline expects {currentComputePipeline.ComputeShaderInfo.ImageBindingCount} textures, but received {count}");
			}
		}

		private void AssertTextureNotNull(ColorAttachmentInfo colorAttachmentInfo)
		{
			if (colorAttachmentInfo.TextureSlice.Texture == null || colorAttachmentInfo.TextureSlice.Texture.Handle == IntPtr.Zero)
			{
				throw new System.ArgumentException("Render pass color attachment Texture cannot be null!");
			}
		}

		private void AssertColorTarget(ColorAttachmentInfo colorAttachmentInfo)
		{
			if ((colorAttachmentInfo.TextureSlice.Texture.UsageFlags & TextureUsageFlags.ColorTarget) == 0)
			{
				throw new System.ArgumentException("Render pass color attachment UsageFlags must include TextureUsageFlags.ColorTarget!");
			}
		}

		private void AssertSameSampleCount(Texture a, Texture b)
		{
			if (a.SampleCount != b.SampleCount)
			{
				throw new System.ArgumentException("All attachments in a render pass must have the same SampleCount!");
			}
		}

		private void AssertValidDepthAttachment(DepthStencilAttachmentInfo depthStencilAttachmentInfo)
		{
			if (depthStencilAttachmentInfo.TextureSlice.Texture == null ||
				depthStencilAttachmentInfo.TextureSlice.Texture.Handle == IntPtr.Zero)
			{
				throw new System.ArgumentException("Render pass depth stencil attachment Texture cannot be null!");
			}

			if ((depthStencilAttachmentInfo.TextureSlice.Texture.UsageFlags & TextureUsageFlags.DepthStencilTarget) == 0)
			{
				throw new System.ArgumentException("Render pass depth stencil attachment UsageFlags must include TextureUsageFlags.DepthStencilTarget!");
			}
		}

		private void AssertTextureSamplerBindingNonNull(in TextureSamplerBinding binding)
		{
			if (binding.Texture == null || binding.Texture.Handle == IntPtr.Zero)
			{
				throw new NullReferenceException("Texture binding must not be null!");
			}

			if (binding.Sampler == null || binding.Sampler.Handle == IntPtr.Zero)
			{
				throw new NullReferenceException("Sampler binding must not be null!");
			}
		}

		private void AssertTextureBindingUsageFlags(Texture texture)
		{
			if ((texture.UsageFlags & TextureUsageFlags.Sampler) == 0)
			{
				throw new System.ArgumentException("The bound Texture's UsageFlags must include TextureUsageFlags.Sampler!");
			}
		}

		private void AssertNonEmptyCopy(uint dataLengthInBytes)
		{
			if (dataLengthInBytes == 0)
			{
				throw new System.InvalidOperationException("SetBufferData must have a length greater than 0 bytes!");
			}
		}

		private void AssertBufferBoundsCheck(uint bufferLengthInBytes, uint offsetInBytes, uint copyLengthInBytes)
		{
			if (copyLengthInBytes > bufferLengthInBytes + offsetInBytes)
			{
				throw new System.InvalidOperationException($"SetBufferData overflow! buffer length {bufferLengthInBytes}, offset {offsetInBytes}, copy length {copyLengthInBytes}");
			}
		}

		private void AssertTextureBoundsCheck(uint textureSizeInBytes, uint dataLengthInBytes)
		{
			if (dataLengthInBytes > textureSizeInBytes)
			{
				throw new System.InvalidOperationException($"SetTextureData overflow! texture size {textureSizeInBytes}, data size {dataLengthInBytes}");
			}
		}

		private void AssertNotInPass(string message)
		{
			if (renderPassActive || copyPassActive || computePassActive)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertInRenderPass(string message)
		{
			if (!renderPassActive)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertInCopyPass(string message)
		{
			if (!copyPassActive)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertInComputePass(string message)
		{
			if (!computePassActive)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertNotSubmitted()
		{
			if (Submitted)
			{
				throw new System.InvalidOperationException("Cannot add commands to a submitted command buffer!");
			}
		}
#endif
	}
}
