using System;
using System.Runtime.InteropServices;
using MoonWorks.Storage;

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

	public static unsafe Shader Create(
		GraphicsDevice device,
		TitleStorage storage,
		string filepath,
		string entrypoint,
		ShaderFormat shaderFormat,
		ShaderStage shaderStage,
		bool enableDebug = false,
		string name = null,
		string includeDir = null,       // Only used by HLSL
		params Span<HLSLDefine> defines // Only used by HLSL
	) {
		name ??= System.IO.Path.GetFileName(filepath); // if name not provided, just use filename

		Shader shader;
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			var buffer = storage.ReadFile(filepath, out var size);
			if (buffer == null)
			{
				return null;
			}

			var span = new ReadOnlySpan<byte>(buffer, (int) size);
			shader = Shader.CreateFromSPIRV(
				device,
				name,
				span,
				entrypoint,
				shaderStage,
				enableDebug
			);

			NativeMemory.Free(buffer);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			// HLSL data is a string so we need to add a null byte, lol
			if (!storage.GetFileSize(filepath, out var size))
			{
				return null;
			}

			var buffer = NativeMemory.Alloc((nuint) (size + 1));
			var fileSpan = new ReadOnlySpan<byte>(buffer, (int) size);

			if (!storage.ReadFile(filepath, fileSpan))
			{
				return null;
			}

			var bufferSpan = new Span<byte>(buffer, (int) (size + 1));
			bufferSpan[^1] = 0;

			shader = Shader.CreateFromHLSL(
				device,
				name,
				bufferSpan,
				entrypoint,
				includeDir,
				shaderStage,
				enableDebug,
				defines
			);

			NativeMemory.Free(buffer);
		}
		else
		{
			Logger.LogError("Invalid shader format!");
			return null;
		}

		return shader;
	}

	public static unsafe ComputePipeline Create(
		GraphicsDevice device,
		TitleStorage storage,
		string filepath,
		string entrypoint,
		ShaderFormat shaderFormat,
		bool enableDebug = false,
		string name = null,
		string includeDir = null,       // Only used for HLSL
		params Span<HLSLDefine> defines // Only used by HLSL
	) {
		name ??= System.IO.Path.GetFileName(filepath); // if name not provided, just use filename

		ComputePipeline pipeline;
		if (shaderFormat == ShaderFormat.SPIRV)
		{
			var buffer = storage.ReadFile(filepath, out var size);
			if (buffer == null)
			{
				return null;
			}

			var span = new ReadOnlySpan<byte>(buffer, (int) size);
			pipeline = ComputePipeline.CreateFromSPIRV(
				device,
				name,
				span,
				entrypoint,
				enableDebug
			);

			NativeMemory.Free(buffer);
		}
		else if (shaderFormat == ShaderFormat.HLSL)
		{
			// HLSL data is a string so we need to add a null byte, lol
			if (!storage.GetFileSize(filepath, out var size))
			{
				return null;
			}

			var buffer = NativeMemory.Alloc((nuint) (size + 1));
			var fileSpan = new ReadOnlySpan<byte>(buffer, (int) size);

			if (!storage.ReadFile(filepath, fileSpan))
			{
				return null;
			}

			var bufferSpan = new Span<byte>(buffer, (int) (size + 1));
			bufferSpan[^1] = 0;

			pipeline = ComputePipeline.CreateFromHLSL(
				device,
				name,
				bufferSpan,
				entrypoint,
				includeDir,
				enableDebug,
				defines
			);

			NativeMemory.Free(buffer);
		}
		else
		{
			Logger.LogError("Invalid shader format!");
			return null;
		}

		return pipeline;
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
