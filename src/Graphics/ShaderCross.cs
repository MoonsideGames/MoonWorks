using System;
using System.IO;

namespace MoonWorks.Graphics;

/// <summary>
/// A static class used for loading cross-platform shaders with SDL_shadercross.
/// It is recommended to only call these functions after SDL_Init has succeeded.
/// </summary>
public static class ShaderCross
{
	public enum ShaderFormat
	{
		Invalid,
		SPIRV,
		HLSL
	}

	public readonly record struct HLSLDefine(string Name, string Value);

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
			Logger.LogError(SDL3.SDL.SDL_GetError());
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
		bool enableDebug = false,
		string name = null,
		string includeDir = null,       // Only used by HLSL
		params Span<HLSLDefine> defines // Only used by HLSL
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		name ??= Path.GetFileName(filepath); // if name is not provided, just use the filename
		return Create(
			device,
			stream,
			entrypoint,
			shaderFormat,
			shaderStage,
			enableDebug,
			name,
			includeDir,
			defines
		);
	}

	public static Shader Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		ShaderFormat shaderFormat,
		ShaderStage shaderStage,
		bool enableDebug = false,
		string name = null,
		string includeDir = null,        // Only used for HLSL
		params Span<HLSLDefine> defines  // Only used by HLSL
	) {
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			return Shader.CreateFromSPIRV(
				device,
				stream,
				entrypoint,
				shaderStage,
				enableDebug,
				name
			);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			return Shader.CreateFromHLSL(
				device,
				stream,
				entrypoint,
				includeDir,
				shaderStage,
				enableDebug,
				name,
				defines
			);
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
		bool enableDebug = false,
		string name = null,
		string includeDir = null,       // Only used for HLSL
		params Span<HLSLDefine> defines // Only used by HLSL
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		name ??= Path.GetFileName(filepath); // if name not provided, just use filename
		return Create(
			device,
			stream,
			entrypoint,
			shaderFormat,
			enableDebug,
			name,
			includeDir,
			defines
		);
	}

	public static ComputePipeline Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		ShaderFormat shaderFormat,
		bool enableDebug = false,
		string name = null,
		string includeDir = null,       // Only used by HLSL
		params Span<HLSLDefine> defines // Only used by HLSL
	) {
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			return ComputePipeline.CreateFromSPIRV(
				device,
				name,
				stream,
				entrypoint,
				enableDebug);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			return ComputePipeline.CreateFromHLSL(
				device,
				name,
				stream,
				entrypoint,
				includeDir,
				enableDebug,
				defines);
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
