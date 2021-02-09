using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class RenderTarget : GraphicsResource
    {
        public TextureSlice TextureSlice { get; }
        public TextureFormat Format => TextureSlice.Texture.Format;

        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyRenderTarget;

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

        public RenderTarget(GraphicsDevice device, in TextureSlice textureSlice, SampleCount sampleCount = SampleCount.One) : base(device)
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
