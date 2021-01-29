using System;
using System.IO;
using System.Runtime.InteropServices;
using RefreshCS;

namespace MoonWorks.Graphics
{
    public class Texture : GraphicsResource
    {
        public uint Width { get; }
        public uint Height { get; }
        public TextureFormat Format { get; }

        protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyTexture;

        public static Texture LoadPNG(GraphicsDevice device, string filePath)
        {
            var pixels = Refresh.Refresh_Image_Load(
                filePath,
                out var width,
                out var height,
                out var channels
            );

            TextureCreateInfo textureCreateInfo;
            textureCreateInfo.Width = (uint)width;
            textureCreateInfo.Height = (uint)height;
            textureCreateInfo.Depth = 1;
            textureCreateInfo.Format = TextureFormat.R8G8B8A8;
            textureCreateInfo.IsCube = false;
            textureCreateInfo.LevelCount = 1;
            textureCreateInfo.SampleCount = SampleCount.One;
            textureCreateInfo.UsageFlags = TextureUsageFlags.Sampler;

            var texture = new Texture(device, textureCreateInfo);

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
            TextureFormat format,
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

            return new Texture(device, textureCreateInfo);
        }

        public static Texture CreateTexture3D(
            GraphicsDevice device,
            uint width,
            uint height,
            uint depth,
            TextureFormat format,
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

            return new Texture(device, textureCreateInfo);
        }

        public static Texture CreateTextureCube(
            GraphicsDevice device,
            uint size,
            TextureFormat format,
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

            return new Texture(device, textureCreateInfo);
        }

        public Texture(
            GraphicsDevice device,
            in TextureCreateInfo textureCreateInfo
        ) : base(device)
        {
            Handle = Refresh.Refresh_CreateTexture(
                device.Handle,
                textureCreateInfo.ToRefreshTextureCreateInfo()
            );

            Format = textureCreateInfo.Format;
            Width = textureCreateInfo.Width;
            Height = textureCreateInfo.Height;
        }

        public void SetData(in TextureSlice textureSlice, IntPtr data, uint dataLengthInBytes)
        {
            Refresh.Refresh_SetTextureData(
                Device.Handle,
                textureSlice.ToRefreshTextureSlice(),
                data,
                dataLengthInBytes
            );
        }

        public void SetData(IntPtr data, uint dataLengthInBytes)
        {
            SetData(new TextureSlice(this), data, dataLengthInBytes);
        }

        public unsafe void SetData<T>(in TextureSlice textureSlice, T[] data) where T : unmanaged
        {
            var size = Marshal.SizeOf<T>();

            fixed (T* ptr = &data[0])
            {
                Refresh.Refresh_SetTextureData(
                    Device.Handle,
                    textureSlice.ToRefreshTextureSlice(),
                    (IntPtr) ptr,
                    (uint) (data.Length * size)
                );
            }
        }

        public unsafe void SetData<T>(T[] data) where T : unmanaged
        {
            SetData(new TextureSlice(this), data);
        }
    }
}
