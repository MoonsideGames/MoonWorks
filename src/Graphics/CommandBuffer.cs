using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Command buffers are used to apply render state and issue draw calls.
	/// NOTE: it is not recommended to hold references to command buffers long term.
	/// </summary>
	public struct CommandBuffer
	{
		public GraphicsDevice Device { get; }
		public IntPtr Handle { get; }

		// some state for debug validation
		GraphicsPipeline currentGraphicsPipeline;
		ComputePipeline currentComputePipeline;
		bool renderPassActive;
		SampleCount currentSampleCount;

		// called from RefreshDevice
		internal CommandBuffer(GraphicsDevice device, IntPtr handle)
		{
			Device = device;
			Handle = handle;
			currentGraphicsPipeline = null;
			currentComputePipeline = null;
			renderPassActive = false;
			currentSampleCount = SampleCount.One;
		}

		// FIXME: we can probably use the NativeMemory functions to not have to generate arrays here

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="colorAttachmentInfos">The color attachments to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			params ColorAttachmentInfo[] colorAttachmentInfos
		)
		{
#if DEBUG
			AssertValidColorAttachments(colorAttachmentInfos, true);
#endif

			var refreshColorAttachmentInfos = new Refresh.ColorAttachmentInfo[colorAttachmentInfos.Length];

			for (var i = 0; i < colorAttachmentInfos.Length; i += 1)
			{
				refreshColorAttachmentInfos[i] = colorAttachmentInfos[i].ToRefresh();
			}

			fixed (Refresh.ColorAttachmentInfo* pColorAttachmentInfos = refreshColorAttachmentInfos)
			{
				Refresh.Refresh_BeginRenderPass(
					Device.Handle,
					Handle,
					(IntPtr) pColorAttachmentInfos,
					(uint) colorAttachmentInfos.Length,
					IntPtr.Zero
				);
			}

			renderPassActive = true;
		}

		/// <summary>
		/// Begins a render pass.
		/// All render state, resource binding, and draw commands must be made within a render pass.
		/// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
		/// </summary>
		/// <param name="depthStencilAttachmentInfo">The depth stencil attachment to use in the render pass.</param>
		/// <param name="colorAttachmentInfos">The color attachments to use in the render pass.</param>
		public unsafe void BeginRenderPass(
			DepthStencilAttachmentInfo depthStencilAttachmentInfo,
			params ColorAttachmentInfo[] colorAttachmentInfos
		)
		{
#if DEBUG
			AssertValidDepthAttachment(depthStencilAttachmentInfo);
			AssertValidColorAttachments(colorAttachmentInfos, false);
#endif

			var refreshColorAttachmentInfos = new Refresh.ColorAttachmentInfo[colorAttachmentInfos.Length];

			for (var i = 0; i < colorAttachmentInfos.Length; i += 1)
			{
				refreshColorAttachmentInfos[i] = colorAttachmentInfos[i].ToRefresh();
			}

			var refreshDepthStencilAttachmentInfo = depthStencilAttachmentInfo.ToRefresh();

			fixed (Refresh.ColorAttachmentInfo* pColorAttachmentInfos = refreshColorAttachmentInfos)
			{
				Refresh.Refresh_BeginRenderPass(
					Device.Handle,
					Handle,
					pColorAttachmentInfos,
					(uint) colorAttachmentInfos.Length,
					&refreshDepthStencilAttachmentInfo
				);
			}

			renderPassActive = true;
		}

		/// <summary>
		/// Binds a compute pipeline so that compute work may be dispatched.
		/// </summary>
		/// <param name="computePipeline">The compute pipeline to bind.</param>
		public void BindComputePipeline(
			ComputePipeline computePipeline
		)
		{
			Refresh.Refresh_BindComputePipeline(
				Device.Handle,
				Handle,
				computePipeline.Handle
			);

			currentComputePipeline = computePipeline;
		}

		/// <summary>
		/// Binds buffers to be used in the compute shader.
		/// </summary>
		/// <param name="buffers">A set of buffers to bind.</param>
		public unsafe void BindComputeBuffers(
			params Buffer[] buffers
		)
		{
#if DEBUG
			AssertComputePipelineBound();

			if (currentComputePipeline.ComputeShaderInfo.BufferBindingCount == 0)
			{
				throw new System.InvalidOperationException("The current compute shader does not take any buffers!");
			}

			if (currentComputePipeline.ComputeShaderInfo.BufferBindingCount < buffers.Length)
			{
				throw new System.InvalidOperationException("Buffer count exceeds the amount used by the current compute shader!");
			}
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
		/// Binds textures to be used in the compute shader.
		/// </summary>
		/// <param name="textures">A set of textures to bind.</param>
		public unsafe void BindComputeTextures(
			params Texture[] textures
		)
		{
#if DEBUG
			AssertComputePipelineBound();

			if (currentComputePipeline.ComputeShaderInfo.ImageBindingCount == 0)
			{
				throw new System.InvalidOperationException("The current compute shader does not take any textures!");
			}

			if (currentComputePipeline.ComputeShaderInfo.ImageBindingCount < textures.Length)
			{
				throw new System.InvalidOperationException("Texture count exceeds the amount used by the current compute shader!");
			}
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
		)
		{
#if DEBUG
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
		)
		{
#if DEBUG
			AssertRenderPassActive();

			if (graphicsPipeline.SampleCount != currentSampleCount)
			{
				throw new System.ArgumentException("The sample count of the bound GraphicsPipeline must match the sample count of the current render pass!");
			}
#endif

			Refresh.Refresh_BindGraphicsPipeline(
				Device.Handle,
				Handle,
				graphicsPipeline.Handle
			);

			currentGraphicsPipeline = graphicsPipeline;
		}

		/// <summary>
		/// Sets the viewport. Only valid during a render pass.
		/// </summary>
		public void SetViewport(Viewport viewport)
		{
#if DEBUG
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
		public void SetScissor(Rect scissor)
		{
#if DEBUG
			AssertRenderPassActive();
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
		/// <param name="firstBinding">The index of the first buffer to bind.</param>
		/// <param name="bufferBindings">Buffers to bind and their associated offsets.</param>
		public unsafe void BindVertexBuffers(
			uint firstBinding,
			params BufferBinding[] bufferBindings
		)
		{
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
		/// Binds vertex buffers to be used by subsequent draw calls.
		/// </summary>
		/// <param name="buffers">The buffers to bind.</param>
		public unsafe void BindVertexBuffers(
			params Buffer[] buffers
		)
		{
			var bufferPtrs = stackalloc IntPtr[buffers.Length];
			var offsets = stackalloc ulong[buffers.Length];

			for (var i = 0; i < buffers.Length; i += 1)
			{
				bufferPtrs[i] = buffers[i].Handle;
				offsets[i] = 0;
			}

			Refresh.Refresh_BindVertexBuffers(
				Device.Handle,
				Handle,
				0,
				(uint) buffers.Length,
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
		/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
		public unsafe void BindVertexSamplers(
			ArraySegment<TextureSamplerBinding> textureSamplerBindings
		)
		{
#if DEBUG
			AssertGraphicsPipelineBound();

			if (currentGraphicsPipeline.VertexShaderInfo.SamplerBindingCount == 0)
			{
				throw new System.InvalidOperationException("The vertex shader of the current graphics pipeline does not take any samplers!");
			}

			if (currentGraphicsPipeline.VertexShaderInfo.SamplerBindingCount < textureSamplerBindings.Count)
			{
				throw new System.InvalidOperationException("Vertex sampler count exceeds the amount used by the vertex shader!");
			}
#endif

			var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Count];
			var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Count];

			for (var i = 0; i < textureSamplerBindings.Count; i += 1)
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
		/// Binds samplers to be used by the vertex shader.
		/// </summary>
		/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
		public unsafe void BindVertexSamplers(
			params TextureSamplerBinding[] textureSamplerBindings
		)
		{
			BindVertexSamplers(new ArraySegment<TextureSamplerBinding>(textureSamplerBindings));
		}

		/// <summary>
		/// Binds samplers to be used by the vertex shader.
		/// </summary>
		/// <param name="textureSamplerBindings">The texture-sampler pairs to bind.</param>
		public unsafe void BindFragmentSamplers(
			ArraySegment<TextureSamplerBinding> textureSamplerBindings
		)
		{
#if DEBUG
			AssertGraphicsPipelineBound();

			if (currentGraphicsPipeline.FragmentShaderInfo.SamplerBindingCount == 0)
			{
				throw new System.InvalidOperationException("The fragment shader of the current graphics pipeline does not take any samplers!");
			}

			if (currentGraphicsPipeline.FragmentShaderInfo.SamplerBindingCount < textureSamplerBindings.Count)
			{
				throw new System.InvalidOperationException("Fragment sampler count exceeds the amount used by the fragment shader!");
			}
#endif

			var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Count];
			var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Count];

			for (var i = 0; i < textureSamplerBindings.Count; i += 1)
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
		/// Binds samplers to be used by the fragment shader.
		/// </summary>
		/// <param name="textureSamplerBindings">An array of texture-sampler pairs to bind.</param>
		public unsafe void BindFragmentSamplers(
			params TextureSamplerBinding[] textureSamplerBindings
		)
		{
			BindFragmentSamplers(new ArraySegment<TextureSamplerBinding>(textureSamplerBindings));
		}

		/// <summary>
		/// Pushes vertex shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset value to be used with draw calls.</returns>
		public unsafe uint PushVertexShaderUniforms<T>(
			params T[] uniforms
		) where T : unmanaged
		{
#if DEBUG
			AssertGraphicsPipelineBound();

			if (currentGraphicsPipeline.VertexShaderInfo.UniformBufferSize == 0)
			{
				throw new InvalidOperationException("The current vertex shader does not take a uniform buffer!");
			}
#endif

			fixed (T* ptr = &uniforms[0])
			{
				return Refresh.Refresh_PushVertexShaderUniforms(
					Device.Handle,
					Handle,
					(IntPtr) ptr,
					(uint) (uniforms.Length * sizeof(T))
				);
			}
		}

		/// <summary>
		/// Pushes fragment shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with draw calls.</returns>
		public unsafe uint PushFragmentShaderUniforms<T>(
			params T[] uniforms
		) where T : unmanaged
		{
#if DEBUG
			AssertGraphicsPipelineBound();

			if (currentGraphicsPipeline.FragmentShaderInfo.UniformBufferSize == 0)
			{
				throw new InvalidOperationException("The current fragment shader does not take a uniform buffer!");
			}
#endif

			fixed (T* ptr = &uniforms[0])
			{
				return Refresh.Refresh_PushFragmentShaderUniforms(
					Device.Handle,
					Handle,
					(IntPtr) ptr,
					(uint) (uniforms.Length * sizeof(T))
				);
			}
		}

		/// <summary>
		/// Pushes compute shader uniforms to the device.
		/// </summary>
		/// <returns>A starting offset to be used with dispatch calls.</returns>
		public unsafe uint PushComputeShaderUniforms<T>(
			params T[] uniforms
		) where T : unmanaged
		{
#if DEBUG
			AssertComputePipelineBound();

			if (currentComputePipeline.ComputeShaderInfo.UniformBufferSize == 0)
			{
				throw new System.InvalidOperationException("The current compute shader does not take a uniform buffer!");
			}
#endif

			fixed (T* ptr = &uniforms[0])
			{
				return Refresh.Refresh_PushComputeShaderUniforms(
					Device.Handle,
					Handle,
					(IntPtr) ptr,
					(uint) (uniforms.Length * sizeof(T))
				);
			}
		}

		/// <summary>
		/// Draws using instanced rendering.
		/// It is an error to call this method unless two vertex buffers have been bound.
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
			Refresh.Refresh_EndRenderPass(
				Device.Handle,
				Handle
			);

			currentGraphicsPipeline = null;
			renderPassActive = false;
		}

		/// <summary>
		/// Acquires a swapchain texture.
		/// This texture will be presented to the given window when the command buffer is submitted.
		/// Can return null if the swapchain is unavailable. The user should ALWAYS handle the case where this occurs.
		/// If null is returned, presentation will not occur.
		/// It is an error to acquire two swapchain textures from the same window in one command buffer.
		/// </summary>
		public Texture? AcquireSwapchainTexture(
			Window window
		)
		{
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

			return new Texture(
				Device,
				texturePtr,
				window.SwapchainFormat,
				width,
				height
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
#if DEBUG
			AssertRenderPassInactive("Cannot copy during render pass!");
#endif

			SetBufferData(
				buffer,
				data,
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
			AssertRenderPassInactive("Cannot copy during render pass!");
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
		/// <param name="data">The array to copy from.</param>
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
#if DEBUG
			AssertRenderPassInactive("Cannot copy during render pass!");
#endif

			var elementSize = sizeof(T);

			fixed (T* ptr = &data[startElement])
			{
				Refresh.Refresh_SetBufferData(
					Device.Handle,
					Handle,
					buffer.Handle,
					bufferOffsetInBytes,
					(IntPtr) ptr,
					(uint) (numElements * elementSize)
				);
			}
		}

		public unsafe void SetBufferData<T>(
			Buffer buffer,
			IntPtr dataPtr,
			uint bufferOffsetInElements,
			uint numElements
		) where T : unmanaged {
#if DEBUG
			AssertRenderPassInactive("Cannot copy during render pass!");
#endif

			Refresh.Refresh_SetBufferData(
				Device.Handle,
				Handle,
				buffer.Handle,
				(uint) sizeof(T) * bufferOffsetInElements,
				dataPtr,
				(uint) sizeof(T) * numElements
			);
		}

		/// <summary>
		/// Asynchronously copies data into a texture.
		/// </summary>
		/// <param name="data">An array of data to copy into the texture.</param>
		public unsafe void SetTextureData<T>(Texture texture, T[] data) where T : unmanaged
		{
			SetTextureData(new TextureSlice(texture), data);
		}

		/// <summary>
		/// Asynchronously copies data into a texture slice.
		/// </summary>
		/// <param name="textureSlice">The texture slice to copy into.</param>
		/// <param name="data">An array of data to copy into the texture.</param>
		public unsafe void SetTextureData<T>(in TextureSlice textureSlice, T[] data) where T : unmanaged
		{
#if DEBUG
			AssertRenderPassInactive("Cannot copy during render pass!");
#endif

			var size = sizeof(T);

			fixed (T* ptr = &data[0])
			{
				Refresh.Refresh_SetTextureData(
					Device.Handle,
					Handle,
					textureSlice.ToRefreshTextureSlice(),
					(IntPtr) ptr,
					(uint) (data.Length * size)
				);
			}
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
			AssertRenderPassInactive("Cannot copy during render pass!");
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
		public void SetTextureDataYUV(Texture yTexture, Texture uTexture, Texture vTexture, IntPtr dataPtr, uint dataLengthInBytes)
		{
#if DEBUG
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
				dataPtr,
				dataLengthInBytes
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
			AssertRenderPassInactive("Cannot copy during render pass!");
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

		private void AssertComputePipelineBound(string message = "No compute pipeline is bound!")
		{
			if (currentComputePipeline == null)
			{
				throw new System.InvalidOperationException(message);
			}
		}

		private void AssertValidColorAttachments(ColorAttachmentInfo[] colorAttachmentInfos, bool atLeastOneRequired)
		{
			if (atLeastOneRequired && colorAttachmentInfos.Length == 0)
			{
				throw new System.ArgumentException("Render pass must contain at least one attachment!");
			}

			currentSampleCount = (colorAttachmentInfos.Length > 0) ? colorAttachmentInfos[0].SampleCount : SampleCount.One;

			if (colorAttachmentInfos.Length > 4)
			{
				throw new System.ArgumentException("Render pass cannot have more than 4 color attachments!");
			}

			for (int i = 0; i < colorAttachmentInfos.Length; i += 1)
			{
				if (colorAttachmentInfos[i].Texture == null ||
					colorAttachmentInfos[i].Texture.Handle == IntPtr.Zero)
				{
					throw new System.ArgumentException("Render pass color attachment Texture cannot be null!");
				}

				if ((colorAttachmentInfos[i].Texture.UsageFlags & TextureUsageFlags.ColorTarget) == 0)
				{
					throw new System.ArgumentException("Render pass color attachment UsageFlags must include TextureUsageFlags.ColorTarget!");
				}

				if (colorAttachmentInfos[i].SampleCount != currentSampleCount)
				{
					throw new System.ArgumentException("All color attachments in a render pass must have the same SampleCount!");
				}
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
#endif
	}
}
