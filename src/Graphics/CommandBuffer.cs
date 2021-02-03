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
            in Rect renderArea,
            in DepthStencilValue depthStencilClearValue,
            params Color[] clearColors
        ) {
            fixed (Color* clearColorPtr = &clearColors[0])
            {
                Refresh.Refresh_BeginRenderPass(
                    Device.Handle,
                    Handle,
                    renderPass.Handle,
                    framebuffer.Handle,
                    renderArea.ToRefresh(),
                    (IntPtr) clearColorPtr,
                    (uint)clearColors.Length,
                    depthStencilClearValue.ToRefresh()
                );
            }
        }

        public unsafe void BeginRenderPass(
            RenderPass renderPass,
            Framebuffer framebuffer,
            in Rect renderArea,
            params Color[] clearColors
        ) {
            fixed (Color* clearColorPtr = &clearColors[0])
            {
                Refresh.Refresh_BeginRenderPass(
                    Device.Handle,
                    Handle,
                    renderPass.Handle,
                    framebuffer.Handle,
                    renderArea.ToRefresh(),
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

        public unsafe uint PushComputeShaderUniforms<T>(
            params T[] uniforms
        ) where T : unmanaged
        {
            fixed (T* ptr = &uniforms[0])
            {
                return Refresh.Refresh_PushComputeShaderUniforms(
                    Device.Handle,
                    Handle,
                    (IntPtr) ptr,
                    (uint) (uniforms.Length * Marshal.SizeOf<T>())
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

        public unsafe uint PushVertexShaderUniforms<T>(
            params T[] uniforms
        ) where T : unmanaged
        {
            fixed (T* ptr = &uniforms[0])
            {
                return Refresh.Refresh_PushVertexShaderUniforms(
                    Device.Handle,
                    Handle,
                    (IntPtr) ptr,
                    (uint) (uniforms.Length * Marshal.SizeOf<T>())
                );
            }
        }

        public unsafe uint PushFragmentShaderUniforms<T>(
            params T[] uniforms
        ) where T : unmanaged
        {
            fixed (T* ptr = &uniforms[0])
            {
                return Refresh.Refresh_PushFragmentShaderUniforms(
                    Device.Handle,
                    Handle,
                    (IntPtr) ptr,
                    (uint) (uniforms.Length * Marshal.SizeOf<T>())
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

        public unsafe void BindVertexSamplers(
            params TextureSamplerBinding[] textureSamplerBindings
        ) {
            var texturePtrs = stackalloc IntPtr[textureSamplerBindings.Length];
            var samplerPtrs = stackalloc IntPtr[textureSamplerBindings.Length];

            for (var i = 0; i < textureSamplerBindings.Length; i += 1)
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

        public unsafe void Clear(
            in Rect clearRect,
            Refresh.ClearOptionsFlags clearOptions,
            in DepthStencilValue depthStencilClearValue,
            params Color[] clearColors
        ) {
            Refresh.Color* colors = stackalloc Refresh.Color[clearColors.Length];
            Refresh.Refresh_Clear(
                Device.Handle,
                Handle,
                clearRect.ToRefresh(),
                clearOptions,
                (IntPtr) colors,
                (uint) clearColors.Length,
                depthStencilClearValue.ToRefresh()
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
            in Texture texture,
            in Rect destinationRectangle,
            Filter filter
        )
        {
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
