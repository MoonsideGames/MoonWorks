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

	public struct ShaderCreateInfo
	{
		public ShaderFormat Format;
		public ShaderStage Stage;
		public uint NumSamplers;
		public uint NumStorageTextures;
		public uint NumStorageBuffers;
		public uint NumUniformBuffers;
		public uint Props;
	}

	public struct ComputePipelineCreateInfo
	{
		public ShaderFormat Format;
		public uint NumSamplers;
		public uint NumReadonlyStorageTextures;
		public uint NumReadonlyStorageBuffers;
		public uint NumReadWriteStorageTextures;
		public uint NumReadWriteStorageBuffers;
		public uint NumUniformBuffers;
		public uint ThreadCountX;
		public uint ThreadCountY;
		public uint ThreadCountZ;
		public uint Props;
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
		in ShaderCreateInfo shaderCreateInfo
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		return Create(
			device,
			stream,
			entrypoint,
			shaderCreateInfo
		);
	}

	public static Shader Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		in ShaderCreateInfo shaderCreateInfo
	) {
		if (shaderCreateInfo.Format == ShaderFormat.SPIRV)
		{
			return Shader.CreateFromSPIRV(device, stream, entrypoint, shaderCreateInfo);
		}
		else if (shaderCreateInfo.Format == ShaderFormat.HLSL)
		{
			return Shader.CreateFromHLSL(device, stream, entrypoint, shaderCreateInfo);
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
		in ComputePipelineCreateInfo pipelineCreateInfo
	) {
		using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		return Create(
			device,
			stream,
			entrypoint,
			pipelineCreateInfo
		);
	}

	public static ComputePipeline Create(
		GraphicsDevice device,
		Stream stream,
		string entrypoint,
		in ComputePipelineCreateInfo pipelineCreateInfo
	) {
		if (pipelineCreateInfo.Format == ShaderFormat.SPIRV)
		{
			return ComputePipeline.CreateFromSPIRV(device, stream, entrypoint, pipelineCreateInfo);
		}
		else if (pipelineCreateInfo.Format == ShaderFormat.HLSL)
		{
			return ComputePipeline.CreateFromHLSL(device, stream, entrypoint, pipelineCreateInfo);
		}
		else
		{
			Logger.LogError("Invalid shader format!");
			return null;
		}
	}

	public static bool Quit()
	{
		if (Initialized)
		{
			if (!SDL_ShaderCross.SDL_ShaderCross_Quit())
			{
				Logger.LogError("Failed to quit ShaderCross!");
				return false;
			}
		}

		Initialized = false;
		return true;
	}
}
