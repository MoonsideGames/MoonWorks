using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    /// <summary>
    /// A render target is a structure that wraps a texture so that it can be rendered to.
    /// </summary>
    public class RenderTarget : GraphicsResource
    {
        public TextureSlice TextureSlice { get; }
        public TextureFormat Format => TextureSlice.Texture.Format;

        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyRenderTarget;

        /// <summary>
        /// Creates a render target backed by a texture.
        /// </summary>
        /// <param name="device">An initialized GraphicsDevice.</param>
        /// <param name="width">The width of the render target.</param>
        /// <param name="height">The height of the render target.</param>
        /// <param name="format">The format of the render target.</param>
        /// <param name="canBeSampled">Whether the render target can be used by a sampler.</param>
        /// <param name="sampleCount">The multisample count of the render target.</param>
        /// <param name="levelCount">The mip level of the render target.</param>
        /// <returns></returns>
        public static RenderTarget CreateBackedRenderTarget(
            GraphicsDevice device,
            uint width,
            uint height,
            TextureFormat format,
            bool canBeSampled,
            SampleCount sampleCount = SampleCount.One,
            uint levelCount = 1
        ) {
            TextureUsageFlags flags = 0;

            if (
                format == TextureFormat.D16 ||
                format == TextureFormat.D32 ||
                format == TextureFormat.D16S8 ||
                format == TextureFormat.D32S8
            ) {
                flags |= TextureUsageFlags.DepthStencilTarget;
            }
            else
            {
                flags |= TextureUsageFlags.ColorTarget;
            }

            if (canBeSampled)
            {
                flags |= TextureUsageFlags.Sampler;
            }

            var texture = Texture.CreateTexture2D(
                device,
                width,
                height,
                format,
                flags,
                sampleCount,
                levelCount
            );

            return new RenderTarget(device, new TextureSlice(texture), sampleCount);
        }

        /// <summary>
        /// Creates a render target using a texture slice and an optional sample count.
        /// </summary>
        /// <param name="device">An initialized GraphicsDevice.</param>
        /// <param name="textureSlice">The texture slice that will be rendered to.</param>
        /// <param name="sampleCount">The desired multisample count of the render target.</param>
        public RenderTarget(
            GraphicsDevice device,
            in TextureSlice textureSlice,
            SampleCount sampleCount = SampleCount.One
        ) : base(device)
        {
            Handle = Refresh.Refresh_CreateRenderTarget(
                device.Handle,
                textureSlice.ToRefreshTextureSlice(),
                (Refresh.SampleCount) sampleCount
            );
            TextureSlice = textureSlice;
        }
    }
}
