using RefreshCS;

namespace MoonWorks.Graphics
{
    public struct SamplerState
    {
        public Refresh.Filter MinFilter;
        public Refresh.Filter MagFilter;
        public Refresh.SamplerMipmapMode MipmapMode;
        public Refresh.SamplerAddressMode AddressModeU;
        public Refresh.SamplerAddressMode AddressModeV;
        public Refresh.SamplerAddressMode AddressModeW;
        public float MipLodBias;
        public bool AnisotropyEnable;
        public float MaxAnisotropy;
        public bool CompareEnable;
        public Refresh.CompareOp CompareOp;
        public float MinLod;
        public float MaxLod;
        public Refresh.BorderColor BorderColor;

        public static readonly SamplerState AnisotropicClamp = new SamplerState
        {
            MinFilter = Refresh.Filter.Linear,
            MagFilter = Refresh.Filter.Linear,
            MipmapMode = Refresh.SamplerMipmapMode.Linear,
            AddressModeU = Refresh.SamplerAddressMode.ClampToEdge,
            AddressModeV = Refresh.SamplerAddressMode.ClampToEdge,
            AddressModeW = Refresh.SamplerAddressMode.ClampToEdge,
            CompareEnable = false,
            AnisotropyEnable = true,
            MaxAnisotropy = 4,
            MipLodBias = 0f,
            MinLod = 0,
            MaxLod = 1000 /* VK_LOD_CLAMP_NONE */
        };

        public static readonly SamplerState AnisotropicWrap = new SamplerState
        {
            MinFilter = Refresh.Filter.Linear,
            MagFilter = Refresh.Filter.Linear,
            MipmapMode = Refresh.SamplerMipmapMode.Linear,
            AddressModeU = Refresh.SamplerAddressMode.Repeat,
            AddressModeV = Refresh.SamplerAddressMode.Repeat,
            AddressModeW = Refresh.SamplerAddressMode.Repeat,
            CompareEnable = false,
            AnisotropyEnable = true,
            MaxAnisotropy = 4,
            MipLodBias = 0f,
            MinLod = 0,
            MaxLod = 1000 /* VK_LOD_CLAMP_NONE */
        };

        public static readonly SamplerState LinearClamp = new SamplerState
        {
            MinFilter = Refresh.Filter.Linear,
            MagFilter = Refresh.Filter.Linear,
            MipmapMode = Refresh.SamplerMipmapMode.Linear,
            AddressModeU = Refresh.SamplerAddressMode.ClampToEdge,
            AddressModeV = Refresh.SamplerAddressMode.ClampToEdge,
            AddressModeW = Refresh.SamplerAddressMode.ClampToEdge,
            CompareEnable = false,
            AnisotropyEnable = false,
            MipLodBias = 0f,
            MinLod = 0,
            MaxLod = 1000
        };

        public static readonly SamplerState LinearWrap = new SamplerState
        {
            MinFilter = Refresh.Filter.Linear,
            MagFilter = Refresh.Filter.Linear,
            MipmapMode = Refresh.SamplerMipmapMode.Linear,
            AddressModeU = Refresh.SamplerAddressMode.Repeat,
            AddressModeV = Refresh.SamplerAddressMode.Repeat,
            AddressModeW = Refresh.SamplerAddressMode.Repeat,
            CompareEnable = false,
            AnisotropyEnable = false,
            MipLodBias = 0f,
            MinLod = 0,
            MaxLod = 1000
        };

        public static readonly SamplerState PointClamp = new SamplerState
        {
            MinFilter = Refresh.Filter.Nearest,
            MagFilter = Refresh.Filter.Nearest,
            MipmapMode = Refresh.SamplerMipmapMode.Nearest,
            AddressModeU = Refresh.SamplerAddressMode.ClampToEdge,
            AddressModeV = Refresh.SamplerAddressMode.ClampToEdge,
            AddressModeW = Refresh.SamplerAddressMode.ClampToEdge,
            CompareEnable = false,
            AnisotropyEnable = false,
            MipLodBias = 0f,
            MinLod = 0,
            MaxLod = 1000
        };

        public static readonly SamplerState PointWrap = new SamplerState
        {
            MinFilter = Refresh.Filter.Nearest,
            MagFilter = Refresh.Filter.Nearest,
            MipmapMode = Refresh.SamplerMipmapMode.Nearest,
            AddressModeU = Refresh.SamplerAddressMode.Repeat,
            AddressModeV = Refresh.SamplerAddressMode.Repeat,
            AddressModeW = Refresh.SamplerAddressMode.Repeat,
            CompareEnable = false,
            AnisotropyEnable = false,
            MipLodBias = 0f,
            MinLod = 0,
            MaxLod = 1000
        };

        public Refresh.SamplerStateCreateInfo ToRefreshSamplerStateCreateInfo()
        {
            return new Refresh.SamplerStateCreateInfo
            {
                minFilter = MinFilter,
                magFilter = MagFilter,
                mipmapMode = MipmapMode,
                addressModeU = AddressModeU,
                addressModeV = AddressModeV,
                addressModeW = AddressModeW,
                mipLodBias = MipLodBias,
                anisotropyEnable = Conversions.BoolToByte(AnisotropyEnable),
                maxAnisotropy = MaxAnisotropy,
                compareEnable = Conversions.BoolToByte(CompareEnable),
                compareOp = CompareOp,
                minLod = MinLod,
                maxLod = MaxLod,
                borderColor = BorderColor
            };
        }
    }
}
