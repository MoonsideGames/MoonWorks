using RefreshCS;

namespace MoonWorks.Graphics
{
	public struct SamplerCreateInfo
	{
		public Filter MinFilter;
		public Filter MagFilter;
		public SamplerMipmapMode MipmapMode;
		public SamplerAddressMode AddressModeU;
		public SamplerAddressMode AddressModeV;
		public SamplerAddressMode AddressModeW;
		public float MipLodBias;
		public bool AnisotropyEnable;
		public float MaxAnisotropy;
		public bool CompareEnable;
		public CompareOp CompareOp;
		public float MinLod;
		public float MaxLod;
		public BorderColor BorderColor;

		public static readonly SamplerCreateInfo AnisotropicClamp = new SamplerCreateInfo
		{
			MinFilter = Filter.Linear,
			MagFilter = Filter.Linear,
			MipmapMode = SamplerMipmapMode.Linear,
			AddressModeU = SamplerAddressMode.ClampToEdge,
			AddressModeV = SamplerAddressMode.ClampToEdge,
			AddressModeW = SamplerAddressMode.ClampToEdge,
			CompareEnable = false,
			AnisotropyEnable = true,
			MaxAnisotropy = 4,
			MipLodBias = 0f,
			MinLod = 0,
			MaxLod = 1000 /* VK_LOD_CLAMP_NONE */
		};

		public static readonly SamplerCreateInfo AnisotropicWrap = new SamplerCreateInfo
		{
			MinFilter = Filter.Linear,
			MagFilter = Filter.Linear,
			MipmapMode = SamplerMipmapMode.Linear,
			AddressModeU = SamplerAddressMode.Repeat,
			AddressModeV = SamplerAddressMode.Repeat,
			AddressModeW = SamplerAddressMode.Repeat,
			CompareEnable = false,
			AnisotropyEnable = true,
			MaxAnisotropy = 4,
			MipLodBias = 0f,
			MinLod = 0,
			MaxLod = 1000 /* VK_LOD_CLAMP_NONE */
		};

		public static readonly SamplerCreateInfo LinearClamp = new SamplerCreateInfo
		{
			MinFilter = Filter.Linear,
			MagFilter = Filter.Linear,
			MipmapMode = SamplerMipmapMode.Linear,
			AddressModeU = SamplerAddressMode.ClampToEdge,
			AddressModeV = SamplerAddressMode.ClampToEdge,
			AddressModeW = SamplerAddressMode.ClampToEdge,
			CompareEnable = false,
			AnisotropyEnable = false,
			MipLodBias = 0f,
			MinLod = 0,
			MaxLod = 1000
		};

		public static readonly SamplerCreateInfo LinearWrap = new SamplerCreateInfo
		{
			MinFilter = Filter.Linear,
			MagFilter = Filter.Linear,
			MipmapMode = SamplerMipmapMode.Linear,
			AddressModeU = SamplerAddressMode.Repeat,
			AddressModeV = SamplerAddressMode.Repeat,
			AddressModeW = SamplerAddressMode.Repeat,
			CompareEnable = false,
			AnisotropyEnable = false,
			MipLodBias = 0f,
			MinLod = 0,
			MaxLod = 1000
		};

		public static readonly SamplerCreateInfo PointClamp = new SamplerCreateInfo
		{
			MinFilter = Filter.Nearest,
			MagFilter = Filter.Nearest,
			MipmapMode = SamplerMipmapMode.Nearest,
			AddressModeU = SamplerAddressMode.ClampToEdge,
			AddressModeV = SamplerAddressMode.ClampToEdge,
			AddressModeW = SamplerAddressMode.ClampToEdge,
			CompareEnable = false,
			AnisotropyEnable = false,
			MipLodBias = 0f,
			MinLod = 0,
			MaxLod = 1000
		};

		public static readonly SamplerCreateInfo PointWrap = new SamplerCreateInfo
		{
			MinFilter = Filter.Nearest,
			MagFilter = Filter.Nearest,
			MipmapMode = SamplerMipmapMode.Nearest,
			AddressModeU = SamplerAddressMode.Repeat,
			AddressModeV = SamplerAddressMode.Repeat,
			AddressModeW = SamplerAddressMode.Repeat,
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
				minFilter = (Refresh.Filter) MinFilter,
				magFilter = (Refresh.Filter) MagFilter,
				mipmapMode = (Refresh.SamplerMipmapMode) MipmapMode,
				addressModeU = (Refresh.SamplerAddressMode) AddressModeU,
				addressModeV = (Refresh.SamplerAddressMode) AddressModeV,
				addressModeW = (Refresh.SamplerAddressMode) AddressModeW,
				mipLodBias = MipLodBias,
				anisotropyEnable = Conversions.BoolToByte(AnisotropyEnable),
				maxAnisotropy = MaxAnisotropy,
				compareEnable = Conversions.BoolToByte(CompareEnable),
				compareOp = (Refresh.CompareOp) CompareOp,
				minLod = MinLod,
				maxLod = MaxLod,
				borderColor = (Refresh.BorderColor) BorderColor
			};
		}
	}
}
