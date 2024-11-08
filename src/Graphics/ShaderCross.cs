using System.IO;
using SDL3;

namespace MoonWorks.Graphics;

/// <summary>
/// A static class used for loading cross-platform shaders with SDL_gpu_shadercross.
/// </summary>
public static class ShaderCross
{
	public enum ShaderFormat
	{
		Invalid,
		SPIRV,
		HLSL
	}

	public enum HLSLShaderModel
	{
		Invalid,
		Five,
		Six
	}

	public readonly record struct ShaderResourceInfo(
		uint NumSamplers,
		uint NumStorageTextures,
		uint NumStorageBuffers,
		uint NumUniformBuffers
	) {
		internal SDL_ShaderCross.ShaderResourceInfo ToNative() => new(
			NumSamplers,
			NumStorageTextures,
			NumStorageBuffers,
			NumUniformBuffers);
	}

	public readonly record struct ComputeResourceInfo(
		uint NumSamplers,
		uint NumReadOnlyStorageTextures,
		uint NumReadOnlyStorageBuffers,
		uint NumReadWriteStorageTextures,
		uint NumReadWriteStorageBuffers,
		uint NumUniformBuffers,
		uint ThreadCountX,
		uint ThreadCountY,
		uint ThreadCountZ
	) {
		internal SDL_ShaderCross.ComputeResourceInfo ToNative() => new(
			NumSamplers,
			NumReadOnlyStorageTextures,
			NumReadOnlyStorageBuffers,
			NumReadWriteStorageTextures,
			NumReadWriteStorageBuffers,
			NumUniformBuffers,
			ThreadCountX,
			ThreadCountY,
			ThreadCountZ);
	}

	public static Graphics.ShaderFormat SPIRVDestinationFormats => SDL_ShaderCross.SDL_ShaderCross_GetSPIRVShaderFormats();
	public static Graphics.ShaderFormat HLSLDestinationFormats => SDL_ShaderCross.SDL_ShaderCross_GetHLSLShaderFormats();

	internal static bool Initialized;

	/// <summary>
	/// You must call this before using any ShaderCross compilation functions.
	/// </summary>
	public static bool Initialize()
	{
		if (!SDL_ShaderCross.SDL_ShaderCross_Init())
		{
			Logger.LogError("Failed to initialize ShaderCross!");
			return false;
		}

		Initialized = true;
		return true;
	}

	public static Shader Create(
		GraphicsDevice device,
		string filepath,
		string entrypoint,
		ShaderFormat shaderFormat,
		ShaderStage shaderStage,
		in ShaderResourceInfo resourceInfo = new ShaderResourceInfo()
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		return Create(
			device,
			stream,
			entrypoint,
			shaderFormat,
			shaderStage,
			resourceInfo
		);
	}

	public static Shader Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		ShaderFormat shaderFormat,
		ShaderStage shaderStage,
		in ShaderResourceInfo resourceInfo = new ShaderResourceInfo()
	) {
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			return Shader.CreateFromSPIRV(device, stream, entrypoint, shaderStage, resourceInfo);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			return Shader.CreateFromHLSL(device, stream, entrypoint, shaderStage, resourceInfo);
		}
		else
		{
			Logger.LogError("Invalid shader format!");
			return null;
		}
	}

	public static ComputePipeline Create(
		GraphicsDevice device,
		string filepath,
		string entrypoint,
		ShaderFormat shaderFormat,
		in ComputeResourceInfo resourceInfo
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		return Create(
			device,
			stream,
			entrypoint,
			shaderFormat,
			resourceInfo
		);
	}

	public static ComputePipeline Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		ShaderFormat shaderFormat,
		in ComputeResourceInfo resourceInfo
	) {
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			return ComputePipeline.CreateFromSPIRV(device, stream, entrypoint, resourceInfo);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			return ComputePipeline.CreateFromHLSL(device, stream, entrypoint, resourceInfo);
		}
		else
		{
			Logger.LogError("Invalid shader format!");
			return null;
		}
	}

	public static void Quit()
	{
		if (Initialized)
		{
			SDL_ShaderCross.SDL_ShaderCross_Quit();
		}
		Initialized = false;
	}
}
