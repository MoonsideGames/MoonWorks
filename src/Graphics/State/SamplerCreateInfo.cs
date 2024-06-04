﻿using SDL2_gpuCS;

namespace MoonWorks.Graphics;

/// <summary>
/// All of the information that is used to create a sampler.
/// </summary>
public struct SamplerCreateInfo
{
	/// <summary>
	/// Minification filter mode. Used when the image is downscaled.
	/// </summary>
	public Filter MinFilter;
	/// <summary>
	/// Magnification filter mode. Used when the image is upscaled.
	/// </summary>
	public Filter MagFilter;
	/// <summary>
	/// Filter mode applied to mipmap lookups.
	/// </summary>
	public SamplerMipmapMode MipmapMode;
	/// <summary>
	/// Horizontal addressing mode.
	/// </summary>
	public SamplerAddressMode AddressModeU;
	/// <summary>
	/// Vertical addressing mode.
	/// </summary>
	public SamplerAddressMode AddressModeV;
	/// <summary>
	/// Depth addressing mode.
	/// </summary>
	public SamplerAddressMode AddressModeW;
	/// <summary>
	/// Bias value added to mipmap level of detail calculation.
	/// </summary>
	public float MipLodBias;
	/// <summary>
	/// Enables anisotropic filtering.
	/// </summary>
	public bool AnisotropyEnable;
	/// <summary>
	/// Maximum anisotropy value.
	/// </summary>
	public float MaxAnisotropy;
	public bool CompareEnable;
	public CompareOp CompareOp;
	/// <summary>
	/// Clamps the LOD value to a specified minimum.
	/// </summary>
	public float MinLod;
	/// <summary>
	/// Clamps the LOD value to a specified maximum.
	/// </summary>
	public float MaxLod;
	/// <summary>
	/// If an address mode is set to ClampToBorder, will replace color with this color when samples are outside the [0, 1) range.
	/// </summary>
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

	public SDL_Gpu.SamplerCreateInfo ToSDL()
	{
		return new SDL_Gpu.SamplerCreateInfo
		{
			MinFilter = (SDL_Gpu.Filter) MinFilter,
			MagFilter = (SDL_Gpu.Filter) MagFilter,
			MipmapMode = (SDL_Gpu.SamplerMipmapMode) MipmapMode,
			AddressModeU = (SDL_Gpu.SamplerAddressMode) AddressModeU,
			AddressModeV = (SDL_Gpu.SamplerAddressMode) AddressModeV,
			AddressModeW = (SDL_Gpu.SamplerAddressMode) AddressModeW,
			MipLodBias = MipLodBias,
			AnisotropyEnable = Conversions.BoolToByte(AnisotropyEnable),
			MaxAnisotropy = MaxAnisotropy,
			CompareEnable = Conversions.BoolToByte(CompareEnable),
			CompareOp = (SDL_Gpu.CompareOp) CompareOp,
			MinLod = MinLod,
			MaxLod = MaxLod,
			BorderColor = (SDL_Gpu.BorderColor) BorderColor
		};
	}
}
