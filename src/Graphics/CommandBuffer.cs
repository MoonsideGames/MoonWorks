﻿using System;
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

			Submitted = false;
		}
#endif

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="colorAttachmentInfo">The color attachment to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			in ColorAttachmentInfo colorAttachmentInfo
		) {
#if DEBUG
			AssertNotSubmitted();
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
			colorAttachmentSampleCount = colorAttachmentInfo.Texture.SampleCount;
			colorAttachmentCount = 1;
			colorFormatOne = colorAttachmentInfo.Texture.Format;
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

			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoTwo.Texture);
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
			colorAttachmentSampleCount = colorAttachmentInfoOne.Texture.SampleCount;
			colorAttachmentCount = 2;
			colorFormatOne = colorAttachmentInfoOne.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.Texture.Format;
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

			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoTwo.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoThree.Texture);
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
			colorAttachmentSampleCount = colorAttachmentInfoOne.Texture.SampleCount;
			colorAttachmentCount = 3;
			colorFormatOne = colorAttachmentInfoOne.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.Texture.Format;
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

			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoTwo.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoThree.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoFour.Texture);
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
			colorAttachmentSampleCount = colorAttachmentInfoOne.Texture.SampleCount;
			colorAttachmentCount = 4;
			colorFormatOne = colorAttachmentInfoOne.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.Texture.Format;
			colorFormatFour = colorAttachmentInfoFour.Texture.Format;
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
			depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.Texture.SampleCount;
			depthStencilFormat = depthStencilAttachmentInfo.Texture.Format;
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
			AssertSameSampleCount(colorAttachmentInfo.Texture, depthStencilAttachmentInfo.Texture);
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
			colorAttachmentSampleCount = colorAttachmentInfo.Texture.SampleCount;
			colorAttachmentCount = 1;
			depthStencilAttachmentSampleCount = depthStencilAttachmentInfo.Texture.SampleCount;
			colorFormatOne = colorAttachmentInfo.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.Texture.Format;
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

			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoTwo.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, depthStencilAttachmentInfo.Texture);
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
			colorAttachmentSampleCount = colorAttachmentInfoOne.Texture.SampleCount;
			colorAttachmentCount = 2;
			colorFormatOne = colorAttachmentInfoOne.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.Texture.Format;
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

			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoTwo.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoTwo.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, depthStencilAttachmentInfo.Texture);
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
			colorAttachmentSampleCount = colorAttachmentInfoOne.Texture.SampleCount;
			colorAttachmentCount = 3;
			colorFormatOne = colorAttachmentInfoOne.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.Texture.Format;
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

			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoTwo.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoThree.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, colorAttachmentInfoFour.Texture);
			AssertSameSampleCount(colorAttachmentInfoOne.Texture, depthStencilAttachmentInfo.Texture);
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
			colorAttachmentSampleCount = colorAttachmentInfoOne.Texture.SampleCount;
			colorAttachmentCount = 4;
			colorFormatOne = colorAttachmentInfoOne.Texture.Format;
			colorFormatTwo = colorAttachmentInfoTwo.Texture.Format;
			colorFormatThree = colorAttachmentInfoThree.Texture.Format;
			colorFormatFour = colorAttachmentInfoFour.Texture.Format;
			depthStencilFormat = depthStencilAttachmentInfo.Texture.Format;
#endif
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
			Buffer buffer
		) {
#if DEBUG
			AssertNotSubmitted();
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
			Buffer bufferOne,
			Buffer bufferTwo
		) {
#if DEBUG
			AssertNotSubmitted();
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
			Buffer bufferOne,
			Buffer bufferTwo,
			Buffer bufferThree
		) {
#if DEBUG
			AssertNotSubmitted();
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
			Buffer bufferOne,
			Buffer bufferTwo,
			Buffer bufferThree,
			Buffer bufferFour
		) {
#if DEBUG
			AssertNotSubmitted();
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
			in Span<Buffer> buffers
		) {
#if DEBUG
			AssertNotSubmitted();
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
		/// <param name="texture">A texture to bind.</param>
		public unsafe void BindComputeTextures(
			Texture texture
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertComputePipelineBound();
			AssertComputeTextureCount(1);
#endif

			var texturePtrs = stackalloc IntPtr[1];
			texturePtrs[0] = texture.Handle;

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="textureOne">A texture to bind.</param>
		/// <param name="textureTwo">A texture to bind.</param>
		public unsafe void BindComputeTextures(
			Texture textureOne,
			Texture textureTwo
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertComputePipelineBound();
			AssertComputeTextureCount(2);
#endif

			var texturePtrs = stackalloc IntPtr[2];
			texturePtrs[0] = textureOne.Handle;
			texturePtrs[1] = textureTwo.Handle;

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="textureOne">A texture to bind.</param>
		/// <param name="textureTwo">A texture to bind.</param>
		/// <param name="textureThree">A texture to bind.</param>
		public unsafe void BindComputeTextures(
			Texture textureOne,
			Texture textureTwo,
			Texture textureThree
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertComputePipelineBound();
			AssertComputeTextureCount(3);
#endif

			var texturePtrs = stackalloc IntPtr[3];
			texturePtrs[0] = textureOne.Handle;
			texturePtrs[1] = textureTwo.Handle;
			texturePtrs[2] = textureThree.Handle;

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="textureOne">A texture to bind.</param>
		/// <param name="textureTwo">A texture to bind.</param>
		/// <param name="textureThree">A texture to bind.</param>
		/// <param name="textureFour">A texture to bind.</param>
		public unsafe void BindComputeTextures(
			Texture textureOne,
			Texture textureTwo,
			Texture textureThree,
			Texture textureFour
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertComputePipelineBound();
			AssertComputeTextureCount(4);
#endif

			var texturePtrs = stackalloc IntPtr[4];
			texturePtrs[0] = textureOne.Handle;
			texturePtrs[1] = textureTwo.Handle;
			texturePtrs[2] = textureThree.Handle;
			texturePtrs[3] = textureFour.Handle;

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs
			);
		}

		/// <summary>
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="textures">A set of textures to bind.</param>
		public unsafe void BindComputeTextures(
			in Span<Texture> textures
		) {
#if DEBUG
			AssertNotSubmitted();
			AssertComputePipelineBound();
			AssertComputeTextureCount(textures.Length);
#endif

			var texturePtrs = stackalloc IntPtr[textures.Length];

			for (var i = 0; i < textures.Length; i += 1)
			{
				texturePtrs[i] = textures[i].Handle;
			}

			Refresh.Refresh_BindComputeTextures(
				Device.Handle,
				Handle,
				(IntPtr) texturePtrs
			);
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
			uint groupCountZ,
			uint computeParamOffset
		) {
#if DEBUG
			AssertNotSubmitted();
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
				groupCountZ,
				computeParamOffset
			);
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
			Buffer indexBuffer,
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
		public unsafe uint PushVertexShaderUniforms(
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
#endif

			return Refresh.Refresh_PushVertexShaderUniforms(
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
		public unsafe uint PushVertexShaderUniforms<T>(
			in T uniforms
		) where T : unmanaged
		{
			fixed (T* uniformsPtr = &uniforms)
			{
				return PushVertexShaderUniforms(uniformsPtr, (uint) Marshal.SizeOf<T>());
			}
		}

		/// <summary>
		/// Pushes fragment shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with draw calls.</returns>
		public unsafe uint PushFragmentShaderUniforms(
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
#endif

			return Refresh.Refresh_PushFragmentShaderUniforms(
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
		public unsafe uint PushFragmentShaderUniforms<T>(
			in T uniforms
		) where T : unmanaged
		{
			fixed (T* uniformsPtr = &uniforms)
			{
				return PushFragmentShaderUniforms(uniformsPtr, (uint) Marshal.SizeOf<T>());
			}
		}

		/// <summary>
		/// Pushes compute shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with dispatch calls.</returns>
		public unsafe uint PushComputeShaderUniforms(
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
#endif

			return Refresh.Refresh_PushComputeShaderUniforms(
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
		public unsafe uint PushComputeShaderUniforms<T>(
			in T uniforms
		) where T : unmanaged
		{
			fixed (T* uniformsPtr = &uniforms)
			{
				return PushComputeShaderUniforms(uniformsPtr, (uint) Marshal.SizeOf<T>());
			}
		}

		/// <summary>
		/// Draws using instanced rendering.
		/// </summary>
		/// <param name="baseVertex">The starting index offset for the vertex buffer.</param>
		/// <param name="startIndex">The starting index offset for the index buffer.</param>
		/// <param name="primitiveCount">The number of primitives to draw.</param>
		/// <param name="instanceCount">The number of instances to draw.</param>
		/// <param name="vertexParamOffset">An offset value obtained from PushVertexShaderUniforms. If no uniforms are required then use 0.</param>
		/// <param name="fragmentParamOffset">An offset value obtained from PushFragmentShaderUniforms. If no uniforms are required the use 0.</param>
		public void DrawInstancedPrimitives(
			uint baseVertex,
			uint startIndex,
			uint primitiveCount,
			uint instanceCount,
			uint vertexParamOffset,
			uint fragmentParamOffset
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
				instanceCount,
				vertexParamOffset,
				fragmentParamOffset
			);
		}

		/// <summary>
		/// Draws using a vertex buffer and an index buffer.
		/// </summary>
		/// <param name="baseVertex">The starting index offset for the vertex buffer.</param>
		/// <param name="startIndex">The starting index offset for the index buffer.</param>
		/// <param name="primitiveCount">The number of primitives to draw.</param>
		/// <param name="vertexParamOffset">An offset value obtained from PushVertexShaderUniforms. If no uniforms are required then use 0.</param>
		/// <param name="fragmentParamOffset">An offset value obtained from PushFragmentShaderUniforms. If no uniforms are required the use 0.</param>
		public void DrawIndexedPrimitives(
			uint baseVertex,
			uint startIndex,
			uint primitiveCount,
			uint vertexParamOffset,
			uint fragmentParamOffset
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
				primitiveCount,
				vertexParamOffset,
				fragmentParamOffset
			);
		}

		/// <summary>
		/// Draws using a vertex buffer.
		/// </summary>
		/// <param name="vertexStart"></param>
		/// <param name="primitiveCount"></param>
		/// <param name="vertexParamOffset"></param>
		/// <param name="fragmentParamOffset"></param>
		public void DrawPrimitives(
			uint vertexStart,
			uint primitiveCount,
			uint vertexParamOffset,
			uint fragmentParamOffset
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
				primitiveCount,
				vertexParamOffset,
				fragmentParamOffset
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
			Buffer buffer,
			uint offsetInBytes,
			uint drawCount,
			uint stride,
			uint vertexParamOffset,
			uint fragmentParamOffset
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
				stride,
				vertexParamOffset,
				fragmentParamOffset
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
		/// Copies array data into a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to copy to.</param>
		/// <param name="data">The array to copy from.</param>
		/// <param name="bufferOffsetInBytes">Specifies where in the buffer to start copying.</param>
		/// <param name="setDataOption">Specifies whether the buffer should be copied in immediate or deferred mode. When in doubt, use deferred.</param>
		public unsafe void SetBufferData<T>(
			Buffer buffer,
			Span<T> data,
			uint bufferOffsetInBytes = 0
		) where T : unmanaged
		{
			SetBufferData(
				buffer,
				data,
				bufferOffsetInBytes,
				0,
				(uint) data.Length
			);
		}

		/// <summary>
		/// Copies array data into a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to copy to.</param>
		/// <param name="data">The array to copy from.</param>
		/// <param name="bufferOffsetInBytes">Specifies where in the buffer to start copying.</param>
		/// <param name="setDataOption">Specifies whether the buffer should be copied in immediate or deferred mode. When in doubt, use deferred.</param>
		public unsafe void SetBufferData<T>(
			Buffer buffer,
			T[] data,
			uint bufferOffsetInBytes = 0
		) where T : unmanaged
		{
			SetBufferData(
				buffer,
				new Span<T>(data),
				bufferOffsetInBytes,
				0,
				(uint) data.Length
			);
		}

		/// <summary>
		/// Copies arbitrary data into a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to copy into.</param>
		/// <param name="dataPtr">Pointer to the data to copy into the buffer.</param>
		/// <param name="bufferOffsetInBytes">Specifies where in the buffer to copy data.</param>
		/// <param name="dataLengthInBytes">The length of data that should be copied.</param>
		/// <param name="setDataOption">Specifies whether the buffer should be copied in immediate or deferred mode. When in doubt, use deferred.</param>
		public void SetBufferData(
			Buffer buffer,
			IntPtr dataPtr,
			uint bufferOffsetInBytes,
			uint dataLengthInBytes
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
			AssertNonEmptyCopy(dataLengthInBytes);
			AssertBufferBoundsCheck(buffer.Size, bufferOffsetInBytes, dataLengthInBytes);
#endif

			Refresh.Refresh_SetBufferData(
				Device.Handle,
				Handle,
				buffer.Handle,
				bufferOffsetInBytes,
				dataPtr,
				dataLengthInBytes
			);
		}

		/// <summary>
		/// Copies array data into a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to copy to.</param>
		/// <param name="data">The span to copy from.</param>
		/// <param name="bufferOffsetInBytes">Specifies where in the buffer to start copying.</param>
		/// <param name="startElement">The index of the first element to copy from the array.</param>
		/// <param name="numElements">How many elements to copy.</param>
		/// <param name="setDataOption">Specifies whether the buffer should be copied in immediate or deferred mode. When in doubt, use deferred.</param>
		public unsafe void SetBufferData<T>(
			Buffer buffer,
			Span<T> data,
			uint bufferOffsetInBytes,
			uint startElement,
			uint numElements
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * numElements);
			var dataOffsetInBytes = (int) startElement * elementSize;

#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
			AssertNonEmptyCopy(dataLengthInBytes);
			AssertBufferBoundsCheck(buffer.Size, bufferOffsetInBytes, dataLengthInBytes);
#endif

			fixed (T* ptr = data)
			{
				Refresh.Refresh_SetBufferData(
					Device.Handle,
					Handle,
					buffer.Handle,
					bufferOffsetInBytes,
					(IntPtr) ptr + dataOffsetInBytes,
					dataLengthInBytes
				);
			}
		}

		/// <summary>
		/// Copies array data into a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to copy to.</param>
		/// <param name="data">The span to copy from.</param>
		/// <param name="bufferOffsetInBytes">Specifies where in the buffer to start copying.</param>
		/// <param name="startElement">The index of the first element to copy from the array.</param>
		/// <param name="numElements">How many elements to copy.</param>
		/// <param name="setDataOption">Specifies whether the buffer should be copied in immediate or deferred mode. When in doubt, use deferred.</param>
		public unsafe void SetBufferData<T>(
			Buffer buffer,
			T[] data,
			uint bufferOffsetInBytes,
			uint startElement,
			uint numElements
		) where T : unmanaged
		{
			SetBufferData<T>(buffer, new Span<T>(data), bufferOffsetInBytes, startElement, numElements);
		}

		public unsafe void SetBufferData<T>(
			Buffer buffer,
			IntPtr dataPtr,
			uint bufferOffsetInElements,
			uint numElements
		) where T : unmanaged
		{
			var elementSize = Marshal.SizeOf<T>();
			var dataLengthInBytes = (uint) (elementSize * numElements);
			var offsetLengthInBytes = (uint) elementSize * bufferOffsetInElements;

#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
			AssertNonEmptyCopy((uint) (elementSize * numElements));
			AssertBufferBoundsCheck(buffer.Size, offsetLengthInBytes, dataLengthInBytes);
#endif

			Refresh.Refresh_SetBufferData(
				Device.Handle,
				Handle,
				buffer.Handle,
				offsetLengthInBytes,
				dataPtr,
				dataLengthInBytes
			);
		}

		/// <summary>
		/// Asynchronously copies data into a texture.
		/// </summary>
		/// <param name="data">A span of data to copy into the texture.</param>
		public unsafe void SetTextureData<T>(Texture texture, Span<T> data) where T : unmanaged
		{
			SetTextureData(new TextureSlice(texture), data);
		}

		/// <summary>
		/// Asynchronously copies data into a texture.
		/// </summary>
		/// <param name="data">An array of data to copy into the texture.</param>
		public unsafe void SetTextureData<T>(Texture texture, T[] data) where T : unmanaged
		{
			SetTextureData(new TextureSlice(texture), new Span<T>(data));
		}

		/// <summary>
		/// Asynchronously copies data into a texture slice.
		/// </summary>
		/// <param name="textureSlice">The texture slice to copy into.</param>
		/// <param name="data">A span of data to copy into the texture.</param>
		public unsafe void SetTextureData<T>(in TextureSlice textureSlice, Span<T> data) where T : unmanaged
		{
			var dataLengthInBytes = (uint) (data.Length * Marshal.SizeOf<T>());

#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
			AssertTextureBoundsCheck(textureSlice.Size, dataLengthInBytes);
#endif

			fixed (T* ptr = data)
			{
				Refresh.Refresh_SetTextureData(
					Device.Handle,
					Handle,
					textureSlice.ToRefreshTextureSlice(),
					(IntPtr) ptr,
					dataLengthInBytes
				);
			}
		}

		/// <summary>
		/// Asynchronously copies data into a texture slice.
		/// </summary>
		/// <param name="textureSlice">The texture slice to copy into.</param>
		/// <param name="data">An array of data to copy into the texture.</param>
		public unsafe void SetTextureData<T>(in TextureSlice textureSlice, T[] data) where T : unmanaged
		{
			SetTextureData(textureSlice, new Span<T>(data));
		}

		/// <summary>
		/// Asynchronously copies data into a texture slice.
		/// </summary>
		/// <param name="textureSlice">The texture slice to copy into.</param>
		/// <param name="dataPtr">A pointer to an array of data to copy from.</param>
		/// <param name="dataLengthInBytes">The amount of data to copy from the array.</param>
		public void SetTextureData(in TextureSlice textureSlice, IntPtr dataPtr, uint dataLengthInBytes)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
			AssertTextureBoundsCheck(textureSlice.Size, dataLengthInBytes);
#endif

			Refresh.Refresh_SetTextureData(
				Device.Handle,
				Handle,
				textureSlice.ToRefreshTextureSlice(),
				dataPtr,
				dataLengthInBytes
			);
		}

		/// <summary>
		/// Asynchronously copies data into a texture.
		/// </summary>
		/// <param name="dataPtr">A pointer to an array of data to copy from.</param>
		/// <param name="dataLengthInBytes">The amount of data to copy from the array.</param>
		public void SetTextureData(Texture texture, IntPtr dataPtr, uint dataLengthInBytes)
		{
			SetTextureData(new TextureSlice(texture), dataPtr, dataLengthInBytes);
		}

		/// <summary>
		/// Asynchronously copies YUV data into three textures. Use with compressed video.
		/// </summary>
		public void SetTextureDataYUV(
			Texture yTexture,
			Texture uTexture,
			Texture vTexture,
			IntPtr yDataPtr,
			IntPtr uDataPtr,
			IntPtr vDataPtr,
			uint yDataLengthInBytes,
			uint uvDataLengthInBytes,
			uint yStride,
			uint uvStride)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
#endif

			Refresh.Refresh_SetTextureDataYUV(
				Device.Handle,
				Handle,
				yTexture.Handle,
				uTexture.Handle,
				vTexture.Handle,
				yTexture.Width,
				yTexture.Height,
				uTexture.Width,
				uTexture.Height,
				yDataPtr,
				uDataPtr,
				vDataPtr,
				yDataLengthInBytes,
				uvDataLengthInBytes,
				yStride,
				uvStride
			);
		}

		/// <summary>
		/// Performs an asynchronous texture-to-texture copy on the GPU.
		/// </summary>
		/// <param name="sourceTextureSlice">The texture slice to copy from.</param>
		/// <param name="destinationTextureSlice">The texture slice to copy to.</param>
		/// <param name="filter">The filter to use if the sizes of the texture slices differ.</param>
		public void CopyTextureToTexture(
			in TextureSlice sourceTextureSlice,
			in TextureSlice destinationTextureSlice,
			Filter filter
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
#endif

			var sourceRefreshTextureSlice = sourceTextureSlice.ToRefreshTextureSlice();
			var destRefreshTextureSlice = destinationTextureSlice.ToRefreshTextureSlice();

			Refresh.Refresh_CopyTextureToTexture(
				Device.Handle,
				Handle,
				sourceRefreshTextureSlice,
				destRefreshTextureSlice,
				(Refresh.Filter) filter
			);
		}

		/// <summary>
		/// Performs an asynchronous texture-to-buffer copy.
		/// Note that the buffer is not guaranteed to be filled until you call GraphicsDevice.Wait()
		/// </summary>
		/// <param name="textureSlice"></param>
		/// <param name="buffer"></param>
		public void CopyTextureToBuffer(
			in TextureSlice textureSlice,
			Buffer buffer
		)
		{
#if DEBUG
			AssertNotSubmitted();
			AssertRenderPassInactive("Cannot copy during render pass!");
			AssertBufferBoundsCheck(buffer.Size, 0, textureSlice.Size);
#endif

			var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();

			Refresh.Refresh_CopyTextureToBuffer(
				Device.Handle,
				Handle,
				refreshTextureSlice,
				buffer.Handle
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
			if (colorAttachmentInfo.Texture == null || colorAttachmentInfo.Texture.Handle == IntPtr.Zero)
			{
				throw new System.ArgumentException("Render pass color attachment Texture cannot be null!");
			}
		}

		private void AssertColorTarget(ColorAttachmentInfo colorAttachmentInfo)
		{
			if ((colorAttachmentInfo.Texture.UsageFlags & TextureUsageFlags.ColorTarget) == 0)
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
			if (depthStencilAttachmentInfo.Texture == null ||
				depthStencilAttachmentInfo.Texture.Handle == IntPtr.Zero)
			{
				throw new System.ArgumentException("Render pass depth stencil attachment Texture cannot be null!");
			}

			if ((depthStencilAttachmentInfo.Texture.UsageFlags & TextureUsageFlags.DepthStencilTarget) == 0)
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
