using System;
using MoonWorks.Math;
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

        // called from RefreshDevice
        internal CommandBuffer(GraphicsDevice device)
        {
            Device = device;
            Handle = IntPtr.Zero;
        }

        /// <summary>
        /// Begins a render pass.
        /// All render state, resource binding, and draw commands must be made within a render pass.
        /// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
        /// </summary>
        /// <param name="renderPass">The render pass object to begin.</param>
        /// <param name="framebuffer">The framebuffer used by the render pass.</param>
        /// <param name="renderArea">The screen area of the render pass.</param>
        /// <param name="depthStencilClearValue">Clear values for the depth/stencil buffer. This is ignored if the render pass does not clear.</param>
        public unsafe void BeginRenderPass(
            RenderPass renderPass,
            Framebuffer framebuffer,
            in Rect renderArea,
            in DepthStencilValue depthStencilClearValue
        ) {
            Refresh.Refresh_BeginRenderPass(
                Device.Handle,
                Handle,
                renderPass.Handle,
                framebuffer.Handle,
                renderArea.ToRefresh(),
                IntPtr.Zero,
                0,
                depthStencilClearValue.ToRefresh()
            );
        }

        /// <summary>
        /// Begins a render pass.
        /// All render state, resource binding, and draw commands must be made within a render pass.
        /// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
        /// </summary>
        /// <param name="renderPass">The render pass object to begin.</param>
        /// <param name="framebuffer">The framebuffer used by the render pass.</param>
        /// <param name="renderArea">The screen area of the render pass.</param>
        /// <param name="depthStencilClearValue">Clear values for the depth/stencil buffer. This is ignored if the render pass does not clear.</param>
        /// <param name="clearColors">Color clear values for each render target in the framebuffer.</param>
        public unsafe void BeginRenderPass(
            RenderPass renderPass,
            Framebuffer framebuffer,
            in Rect renderArea,
            in DepthStencilValue depthStencilClearValue,
            params Vector4[] clearColors
        ) {
            Refresh.Vec4* colors = stackalloc Refresh.Vec4[clearColors.Length];

            for (var i = 0; i < clearColors.Length; i++)
            {
                colors[i] = new Refresh.Vec4
                {
                    x = clearColors[i].X,
                    y = clearColors[i].Y,
                    z = clearColors[i].Z,
                    w = clearColors[i].W
                };
            }

            Refresh.Refresh_BeginRenderPass(
                Device.Handle,
                Handle,
                renderPass.Handle,
                framebuffer.Handle,
                renderArea.ToRefresh(),
                (IntPtr) colors,
                (uint)clearColors.Length,
                depthStencilClearValue.ToRefresh()
            );
        }

        /// <summary>
        /// Begins a render pass.
        /// All render state, resource binding, and draw commands must be made within a render pass.
        /// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
        /// </summary>
        /// <param name="renderPass">The render pass object to begin.</param>
        /// <param name="framebuffer">The framebuffer used by the render pass.</param>
        /// <param name="renderArea">The screen area of the render pass.</param>
        /// <param name="clearColors">Color clear values for each render target in the framebuffer.</param>
        public unsafe void BeginRenderPass(
            RenderPass renderPass,
            Framebuffer framebuffer,
            in Rect renderArea,
            params Vector4[] clearColors
        ) {
            Refresh.Vec4* colors = stackalloc Refresh.Vec4[clearColors.Length];

            for (var i = 0; i < clearColors.Length; i++)
            {
                colors[i] = new Refresh.Vec4
                {
                    x = clearColors[i].X,
                    y = clearColors[i].Y,
                    z = clearColors[i].Z,
                    w = clearColors[i].W
                };
            }

            Refresh.Refresh_BeginRenderPass(
                Device.Handle,
                Handle,
                renderPass.Handle,
                framebuffer.Handle,
                renderArea.ToRefresh(),
                (IntPtr) colors,
                (uint) clearColors.Length,
                IntPtr.Zero
            );
        }

        /// <summary>
        /// Begins a render pass.
        /// All render state, resource binding, and draw commands must be made within a render pass.
        /// It is an error to call this after calling BeginRenderPass but before calling EndRenderPass.
        /// </summary>
        /// <param name="renderPass">The render pass object to begin.</param>
        /// <param name="framebuffer">The framebuffer used by the render pass.</param>
        /// <param name="renderArea">The screen area of the render pass.</param>
        public unsafe void BeginRenderPass(
            RenderPass renderPass,
            Framebuffer framebuffer,
            in Rect renderArea
        ) {
            Refresh.Refresh_BeginRenderPass(
                Device.Handle,
                Handle,
                renderPass.Handle,
                framebuffer.Handle,
                renderArea.ToRefresh(),
                IntPtr.Zero,
                0,
                IntPtr.Zero
            );
        }

        /// <summary>
        /// Binds a compute pipeline so that compute work may be dispatched.
        /// </summary>
        /// <param name="computePipeline">The compute pipeline to bind.</param>
        public void BindComputePipeline(
            ComputePipeline computePipeline
        ) {
            Refresh.Refresh_BindComputePipeline(
                Device.Handle,
                Handle,
                computePipeline.Handle
            );
        }

        /// <summary>
        /// Binds buffers to be used in the compute shader.
        /// </summary>
        /// <param name="buffers">A set of buffers to bind.</param>
        public unsafe void BindComputeBuffers(
            params Buffer[] buffers
        ) {
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
        ) {
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
        /// Binds a graphics pipeline so that rendering work may be performed.
        /// </summary>
        /// <param name="graphicsPipeline">The graphics pipeline to bind.</param>
        public void BindGraphicsPipeline(
            GraphicsPipeline graphicsPipeline
        ) {
            Refresh.Refresh_BindGraphicsPipeline(
                Device.Handle,
                Handle,
                graphicsPipeline.Handle
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
        ) {
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
        ) {
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
        ) {
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
        /// <param name="textureSamplerBindings">An array of texture-sampler pairs to bind.</param>
        /// <param name="length">The number of texture-sampler pairs from the array to bind.</param>
        public unsafe void BindVertexSamplers(
            TextureSamplerBinding[] textureSamplerBindings,
            int length
        ) {
            var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Length];
            var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Length];

            for (var i = 0; i < length; i += 1)
            {
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
        ) {
            BindVertexSamplers(textureSamplerBindings, textureSamplerBindings.Length);
        }

        /// <summary>
        /// Binds samplers to be used by the fragment shader.
        /// </summary>
        /// <param name="textureSamplerBindings">An array of texture-sampler pairs to bind.</param>
        /// <param name="length">The number of texture-sampler pairs from the given array to bind.</param>
        public unsafe void BindFragmentSamplers(
            TextureSamplerBinding[] textureSamplerBindings,
            int length
        ) {
            var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Length];
            var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Length];

            for (var i = 0; i < length; i += 1)
            {
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
        ) {
            BindFragmentSamplers(textureSamplerBindings, textureSamplerBindings.Length);
        }

        /// <summary>
        /// Clears the render targets on the current framebuffer to a single color or depth/stencil value.
        /// NOTE: It is recommended that you clear when beginning render passes unless you have a good reason to clear mid-pass.
        /// </summary>
        /// <param name="clearRect">The area of the framebuffer to clear.</param>
        /// <param name="clearOptions">Whether to clear colors, depth, or stencil value, or multiple.</param>
        /// <param name="depthStencilClearValue">The depth/stencil clear values. Will be ignored if color is not provided in ClearOptions.</param>
        /// <param name="clearColors">The color clear values. Must provide one per render target. Can be omitted if depth/stencil is not cleared.</param>
        public unsafe void Clear(
            in Rect clearRect,
            ClearOptionsFlags clearOptions,
            in DepthStencilValue depthStencilClearValue,
            params Vector4[] clearColors
        ) {
            Refresh.Vec4* colors = stackalloc Refresh.Vec4[clearColors.Length];
            for (var i = 0; i < clearColors.Length; i++)
            {
                colors[i] = new Refresh.Vec4
                {
                    x = clearColors[i].X,
                    y = clearColors[i].Y,
                    z = clearColors[i].Z,
                    w = clearColors[i].W
                };
            }

            Refresh.Refresh_Clear(
                Device.Handle,
                Handle,
                clearRect.ToRefresh(),
                (Refresh.ClearOptionsFlags)clearOptions,
                (IntPtr) colors,
                (uint) clearColors.Length,
                depthStencilClearValue.ToRefresh()
            );
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
        ) {
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
        ) {
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
        ) {
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
        /// Ends the current render pass.
        /// This must be called before beginning another render pass or submitting the command buffer.
        /// </summary>
        public void EndRenderPass()
        {
            Refresh.Refresh_EndRenderPass(
                Device.Handle,
                Handle
            );
        }

        /// <summary>
        /// Prepares a texture to be presented to the screen.
        /// </summary>
        /// <param name="texture">The texture to present.</param>
        /// <param name="destinationRectangle">The area of the screen to present to.</param>
        /// <param name="filter">The filter to use when the texture size differs from the destination rectangle.</param>
        public void QueuePresent(
            in Texture texture,
            in Rect destinationRectangle,
            Filter filter
        ) {
            var refreshRect = destinationRectangle.ToRefresh();
            var refreshTextureSlice = new Refresh.TextureSlice
            {
                texture = texture.Handle,
                rectangle = new Refresh.Rect
                {
                    x = 0,
                    y = 0,
                    w = (int)texture.Width,
                    h = (int)texture.Height
                },
                layer = 0,
                level = 0,
                depth = 0
            };

            Refresh.Refresh_QueuePresent(
                Device.Handle,
                Handle,
                refreshTextureSlice,
                refreshRect,
                (Refresh.Filter)filter
            );
        }

        /// <summary>
        /// Prepares a texture slice to be presented to the screen.
        /// </summary>
        /// <param name="textureSlice">The texture slice to present.</param>
        /// <param name="destinationRectangle">The area of the screen to present to.</param>
        /// <param name="filter">The filter to use when the texture size differs from the destination rectangle.</param>
        public void QueuePresent(
            in TextureSlice textureSlice,
            in Rect destinationRectangle,
            Filter filter
        ) {
            var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();
            var refreshRect = destinationRectangle.ToRefresh();

            Refresh.Refresh_QueuePresent(
                Device.Handle,
                Handle,
                refreshTextureSlice,
                refreshRect,
                (Refresh.Filter) filter
            );
        }

        /// <summary>
        /// Prepares a texture slice to be presented to the screen.
        /// This particular variant of this method will present to the entire window area.
        /// </summary>
        /// <param name="textureSlice">The texture slice to present.</param>
        /// <param name="filter">The filter to use when the texture size differs from the window size.</param>
        public void QueuePresent(
            in TextureSlice textureSlice,
            Filter filter
        ) {
            Refresh.Refresh_QueuePresent(
                Device.Handle,
                Handle,
                textureSlice.ToRefreshTextureSlice(),
                IntPtr.Zero,
                (Refresh.Filter) filter
            );
        }

        /// <summary>
        /// Prepares a texture to be presented to the screen.
        /// This particular variant of this method will present to the entire window area.
        /// </summary>
        /// <param name="texture">The texture to present.</param>
        /// <param name="filter">The filter to use when the texture size differs from the window size.</param>
        public void QueuePresent(
            Texture texture,
            Filter filter
        ) {
            var refreshTextureSlice = new Refresh.TextureSlice
            {
                texture = texture.Handle,
                rectangle = new Refresh.Rect
                {
                    x = 0,
                    y = 0,
                    w = (int) texture.Width,
                    h = (int) texture.Height
                },
                layer = 0,
                level = 0,
                depth = 0
            };

            Refresh.Refresh_QueuePresent(
                Device.Handle,
                Handle,
                refreshTextureSlice,
                IntPtr.Zero,
                (Refresh.Filter) filter
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
        ) {
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
        ) {
            var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();

            Refresh.Refresh_CopyTextureToBuffer(
                Device.Handle,
                Handle,
                refreshTextureSlice,
                buffer.Handle
            );
        }
    }
}
