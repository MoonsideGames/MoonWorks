using System;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
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

        public unsafe void BeginRenderPass(
            RenderPass renderPass,
            Framebuffer framebuffer,
            ref Rect renderArea,
            ref DepthStencilValue depthStencilClearValue,
            params Color[] clearColors
        ) {
            var refreshRenderArea = renderArea.ToRefresh();
            var refreshDepthStencilClearValue = depthStencilClearValue.ToRefresh();

            fixed (Color* clearColorPtr = &clearColors[0])
            {
                Refresh.Refresh_BeginRenderPass(
                    Device.Handle,
                    Handle,
                    renderPass.Handle,
                    framebuffer.Handle,
                    ref refreshRenderArea,
                    (IntPtr) clearColorPtr,
                    (uint)clearColors.Length,
                    ref refreshDepthStencilClearValue
                );
            }
        }

        public unsafe void BeginRenderPass(
            RenderPass renderPass,
            Framebuffer framebuffer,
            ref Rect renderArea,
            params Color[] clearColors
        ) {
            var refreshRenderArea = renderArea.ToRefresh();

            fixed (Color* clearColorPtr = &clearColors[0])
            {
                Refresh.Refresh_BeginRenderPass(
                    Device.Handle,
                    Handle,
                    renderPass.Handle,
                    framebuffer.Handle,
                    ref refreshRenderArea,
                    (IntPtr) clearColorPtr,
                    (uint) clearColors.Length,
                    IntPtr.Zero
                );
            }
        }

        public void BindComputePipeline(
            ComputePipeline computePipeline
        ) {
            Refresh.Refresh_BindComputePipeline(
                Device.Handle,
                Handle,
                computePipeline.Handle
            );
        }

        public unsafe uint PushComputeShaderParams<T>(
            params T[] uniforms
        ) where T : unmanaged
        {
            fixed (T* ptr = &uniforms[0])
            {
                return Refresh.Refresh_PushComputeShaderParams(
                    Device.Handle,
                    Handle,
                    (IntPtr) ptr,
                    (uint) uniforms.Length
                );
            }
        }

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

        public void BindGraphicsPipeline(
            GraphicsPipeline graphicsPipeline
        ) {
            Refresh.Refresh_BindGraphicsPipeline(
                Device.Handle,
                Handle,
                graphicsPipeline.Handle
            );
        }

        public unsafe uint PushVertexShaderParams<T>(
            params T[] uniforms
        ) where T : unmanaged
        {
            fixed (T* ptr = &uniforms[0])
            {
                return Refresh.Refresh_PushVertexShaderParams(
                    Device.Handle,
                    Handle,
                    (IntPtr) ptr,
                    (uint) uniforms.Length
                );
            }
        }

        public unsafe uint PushFragmentShaderParams<T>(
            params T[] uniforms
        ) where T : unmanaged
        {
            fixed (T* ptr = &uniforms[0])
            {
                return Refresh.Refresh_PushFragmentShaderParams(
                    Device.Handle,
                    Handle,
                    (IntPtr) ptr,
                    (uint) uniforms.Length
                );
            }
        }

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

        public void BindIndexBuffer(
            Buffer indexBuffer,
            uint offset,
            Refresh.IndexElementSize indexElementSize
        ) {
            Refresh.Refresh_BindIndexBuffer(
                Device.Handle,
                Handle,
                indexBuffer.Handle,
                offset,
                indexElementSize
            );
        }

        public unsafe void BindVertexSamplers(
            Texture[] textures,
            Sampler[] samplers
        ) {
            var texturePtrs = stackalloc IntPtr[textures.Length];
            var samplerPtrs = stackalloc IntPtr[samplers.Length];

            for (var i = 0; i < textures.Length; i += 1)
            {
                texturePtrs[i] = textures[i].Handle;
            }

            for (var i = 0; i < samplers.Length; i += 1)
            {
                samplerPtrs[i] = samplers[i].Handle;
            }

            Refresh.Refresh_BindVertexSamplers(
                Device.Handle,
                Handle,
                (IntPtr) texturePtrs,
                (IntPtr) samplerPtrs
            );
        }

        public unsafe void BindFragmentSamplers(
            params TextureSamplerBinding[] textureSamplerBindings
        ) {
            var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Length];
            var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Length];

            for (var i = 0; i < textureSamplerBindings.Length; i += 1)
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

        public void Clear(
            ref Refresh.Rect clearRect,
            Refresh.ClearOptionsFlags clearOptions,
            ref Refresh.Color[] colors,
            float depth,
            int stencil
        ) {
            Refresh.Refresh_Clear(
                Device.Handle,
                Handle,
                ref clearRect,
                clearOptions,
                ref colors,
                (uint) colors.Length,
                depth,
                stencil
            );
        }

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

        public void EndRenderPass()
        {
            Refresh.Refresh_EndRenderPass(
                Device.Handle,
                Handle
            );
        }

        public void QueuePresent(
            ref TextureSlice textureSlice,
            ref Rect destinationRectangle,
            Filter filter
        ) {
            var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();
            var refreshRect = destinationRectangle.ToRefresh();

            Refresh.Refresh_QueuePresent(
                Device.Handle,
                Handle,
                ref refreshTextureSlice,
                ref refreshRect,
                (Refresh.Filter) filter
            );
        }

        public void QueuePresent(
            ref TextureSlice textureSlice,
            Filter filter
        ) {
            var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();

            Refresh.Refresh_QueuePresent(
                Device.Handle,
                Handle,
                ref refreshTextureSlice,
                IntPtr.Zero,
                (Refresh.Filter) filter
            );
        }

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
                ref refreshTextureSlice,
                IntPtr.Zero,
                (Refresh.Filter) filter
            );
        }

        public void CopyTextureToTexture(
            ref TextureSlice sourceTextureSlice,
            ref TextureSlice destinationTextureSlice,
            Filter filter
        ) {
            var sourceRefreshTextureSlice = sourceTextureSlice.ToRefreshTextureSlice();
            var destRefreshTextureSlice = destinationTextureSlice.ToRefreshTextureSlice();

            Refresh.Refresh_CopyTextureToTexture(
                Device.Handle,
                Handle,
                ref sourceRefreshTextureSlice,
                ref destRefreshTextureSlice,
                (Refresh.Filter) filter
            );
        }

        public void CopyTextureToBuffer(
            ref TextureSlice textureSlice,
            Buffer buffer
        ) {
            var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();

            Refresh.Refresh_CopyTextureToBuffer(
                Device.Handle,
                Handle,
                ref refreshTextureSlice,
                buffer.Handle
            );
        }
    }
}
