using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class ColorTarget : GraphicsResource
    {
        public TextureSlice TextureSlice { get; }
        public ColorFormat Format => TextureSlice.Texture.Format;

        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyColorTarget;

        public static ColorTarget CreateBackedColorTarget2D(
            GraphicsDevice device,
            uint width,
            uint height,
            ColorFormat format,
            bool canBeSampled,
            SampleCount sampleCount = SampleCount.One,
            uint levelCount = 1
        )
        {
            var flags = TextureUsageFlags.ColorTargetBit;
            if (canBeSampled) { flags |= TextureUsageFlags.SamplerBit; }

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

        public ColorTarget(GraphicsDevice device, SampleCount sampleCount, ref TextureSlice textureSlice) : base(device)
        {
            Handle = Refresh.Refresh_CreateColorTarget(
                device.Handle, 
                (Refresh.SampleCount) sampleCount, 
                textureSlice.ToRefreshTextureSlice()
            );
            TextureSlice = textureSlice;
        }
    }
}
