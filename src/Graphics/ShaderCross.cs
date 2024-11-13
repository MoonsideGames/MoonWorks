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
		string includeDir = null // Only used for HLSL
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		return Create(
			device,
			stream,
			entrypoint,
			shaderFormat,
			shaderStage,
			includeDir
		);
	}

	public static Shader Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		ShaderFormat shaderFormat,
		ShaderStage shaderStage,
		string includeDir = null // Only used for HLSL
	) {
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			return Shader.CreateFromSPIRV(device, stream, entrypoint, shaderStage);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			return Shader.CreateFromHLSL(device, stream, entrypoint, includeDir, shaderStage);
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
		string includeDir = null // Only used for HLSL
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		return Create(
			device,
			stream,
			entrypoint,
			shaderFormat,
			includeDir
		);
	}

	public static ComputePipeline Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		ShaderFormat shaderFormat,
		string includeDir = null // Only used for HLSL
	) {
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			return ComputePipeline.CreateFromSPIRV(device, stream, entrypoint);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			return ComputePipeline.CreateFromHLSL(device, stream, entrypoint, includeDir);
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
