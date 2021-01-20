using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class ColorTarget : GraphicsResource
    {
        public uint Width { get; }
        public uint Height { get; }

        public Texture Texture { get; }
        public Refresh.ColorFormat Format => Texture.Format;

        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyColorTarget;

        public static ColorTarget CreateBackedColorTarget2D(
            GraphicsDevice device,
            uint width,
            uint height,
            Refresh.ColorFormat format,
            bool canBeSampled,
            Refresh.SampleCount sampleCount = Refresh.SampleCount.One,
            uint levelCount = 1
        )
        {
            var flags = Refresh.TextureUsageFlags.ColorTargetBit;
            if (canBeSampled) { flags |= Refresh.TextureUsageFlags.SamplerBit; }

            var texture = Texture.CreateTexture2D(
                device,
                width,
                height,
                format,
                flags,
                sampleCount,
                levelCount
            );

            var textureSlice = new TextureSlice(texture);

            return new ColorTarget(device, sampleCount, ref textureSlice);
        }

        public ColorTarget(GraphicsDevice device, Refresh.SampleCount sampleCount, ref TextureSlice textureSlice) : base(device)
        {
            var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();
            Handle = Refresh.Refresh_CreateColorTarget(device.Handle, sampleCount, ref refreshTextureSlice);
        }
    }
}
