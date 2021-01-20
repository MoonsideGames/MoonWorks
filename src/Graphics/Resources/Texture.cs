using System;
using System.IO;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class Texture : GraphicsResource
    {
        public uint Width { get; }
        public uint Height { get; }
        public ColorFormat Format { get; }

        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyTexture;

        public static Texture LoadPNG(GraphicsDevice device, FileInfo fileInfo)
        {
            var pixels = Refresh.Refresh_Image_Load(
                fileInfo.FullName,
                out var width,
                out var height,
                out var channels
            );

            TextureCreateInfo textureCreateInfo;
            textureCreateInfo.Width = (uint)width;
            textureCreateInfo.Height = (uint)height;
            textureCreateInfo.Depth = 1;
            textureCreateInfo.Format = ColorFormat.R8G8B8A8;
            textureCreateInfo.IsCube = false;
            textureCreateInfo.LevelCount = 1;
            textureCreateInfo.SampleCount = SampleCount.One;
            textureCreateInfo.UsageFlags = TextureUsageFlags.SamplerBit;

            var texture = new Texture(device, ref textureCreateInfo);

            texture.SetData(pixels, (uint)(width * height * 4));

            Refresh.Refresh_Image_Free(pixels);
            return texture;
        }

        public unsafe static void SavePNG(string path, int width, int height, byte[] pixels)
        {
            fixed (byte* ptr = &pixels[0])
            {
                Refresh.Refresh_Image_SavePNG(path, width, height, (IntPtr) ptr);
            }
        }

        public static Texture CreateTexture2D(
            GraphicsDevice device,
            uint width,
            uint height,
            ColorFormat format,
            TextureUsageFlags usageFlags,
            SampleCount sampleCount = SampleCount.One,
            uint levelCount = 1
        )
        {
            var textureCreateInfo = new TextureCreateInfo
            {
                Width = width,
                Height = height,
                Depth = 1,
                IsCube = false,
                SampleCount = sampleCount,
                LevelCount = levelCount,
                Format = format,
                UsageFlags = usageFlags
            };

            return new Texture(device, ref textureCreateInfo);
        }

        public static Texture CreateTexture3D(
            GraphicsDevice device,
            uint width,
            uint height,
            uint depth,
            ColorFormat format,
            TextureUsageFlags usageFlags,
            SampleCount sampleCount = SampleCount.One,
            uint levelCount = 1
        )
        {
            var textureCreateInfo = new TextureCreateInfo
            {
                Width = width,
                Height = height,
                Depth = depth,
                IsCube = false,
                SampleCount = sampleCount,
                LevelCount = levelCount,
                Format = format,
                UsageFlags = usageFlags
            };

            return new Texture(device, ref textureCreateInfo);
        }

        public static Texture CreateTextureCube(
            GraphicsDevice device,
            uint size,
            ColorFormat format,
            TextureUsageFlags usageFlags,
            SampleCount sampleCount = SampleCount.One,
            uint levelCount = 1
        )
        {
            var textureCreateInfo = new TextureCreateInfo
            {
                Width = size,
                Height = size,
                Depth = 1,
                IsCube = true,
                SampleCount = sampleCount,
                LevelCount = levelCount,
                Format = format,
                UsageFlags = usageFlags
            };

            return new Texture(device, ref textureCreateInfo);
        }

        public Texture(GraphicsDevice device, ref TextureCreateInfo textureCreateInfo) : base(device)
        {
            var refreshTextureCreateInfo = textureCreateInfo.ToRefreshTextureCreateInfo();

            Handle = Refresh.Refresh_CreateTexture(
                device.Handle,
                ref refreshTextureCreateInfo
            );

            Format = textureCreateInfo.Format;
            Width = textureCreateInfo.Width;
            Height = textureCreateInfo.Height;
        }

        public void SetData(IntPtr data, uint dataLengthInBytes)
        {
            Refresh.TextureSlice textureSlice;
            textureSlice.texture = Handle;
            textureSlice.rectangle.x = 0;
            textureSlice.rectangle.y = 0;
            textureSlice.rectangle.w = (int)Width;
            textureSlice.rectangle.h = (int)Height;
            textureSlice.level = 0;
            textureSlice.layer = 0;
            textureSlice.depth = 0;

            Refresh.Refresh_SetTextureData(
                Device.Handle,
                ref textureSlice,
                data,
                dataLengthInBytes
            );
        }

        public void SetData(ref TextureSlice textureSlice, IntPtr data, uint dataLengthInBytes)
        {
            var refreshTextureSlice = textureSlice.ToRefreshTextureSlice();

            Refresh.Refresh_SetTextureData(
                Device.Handle,
                ref refreshTextureSlice,
                data,
                dataLengthInBytes
            );
        }
    }
}
