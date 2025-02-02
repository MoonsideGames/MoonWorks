using System;
using System.Runtime.InteropServices;
using System.Text;
using MoonWorks.Storage;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

/// <summary>
/// Compute pipelines perform arbitrary parallel processing on input data.
/// </summary>
public class ComputePipeline : SDLGPUResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUComputePipeline;

	public uint NumSamplers { get; init; }
	public uint NumReadOnlyStorageTextures { get; init; }
	public uint NumReadOnlyStorageBuffers { get; init; }
	public uint NumReadWriteStorageTextures { get; init; }
	public uint NumReadWriteStorageBuffers { get; init; }
	public uint NumUniformBuffers { get; init; }
	public uint ThreadCountX { get; init; }
	public uint ThreadCountY { get; init; }
	public uint ThreadCountZ { get; init; }

	private ComputePipeline(GraphicsDevice device) : base(device)
	{
		Name = "ComputePipeline";
	}

	/// <summary>
	/// Creates a compute pipeline using a specified shader format.
	/// </summary>
	public static unsafe ComputePipeline Create(
		GraphicsDevice device,
		TitleStorage storage,
		string filePath,
		string entryPoint,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		if (!storage.GetFileSize(filePath, out var size))
		{
			return null;
		}

		var buffer = NativeMemory.Alloc((nuint) size);
		var span = new Span<byte>(buffer, (int) size);
		if (!storage.ReadFile(filePath, span))
		{
			return null;
		}

		var pipeline = Create(device, span, entryPoint, computePipelineCreateInfo);
		NativeMemory.Free(buffer);
		return pipeline;
	}

	/// <summary>
	/// Creates a compute pipeline using a specified shader format.
	/// </summary>
	public static unsafe ComputePipeline Create(
		GraphicsDevice device,
		ReadOnlySpan<byte> span,
		string entryPoint,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		var entryPointBuffer = InteropUtilities.EncodeToUTF8Buffer(entryPoint);

		fixed (byte* spanPtr = span)
		{
			INTERNAL_ComputePipelineCreateInfo pipelineCreateInfo;
			pipelineCreateInfo.CodeSize = (nuint) span.Length;
			pipelineCreateInfo.Code = spanPtr;
			pipelineCreateInfo.EntryPoint = entryPointBuffer;
			pipelineCreateInfo.Format = computePipelineCreateInfo.Format;
			pipelineCreateInfo.NumSamplers = computePipelineCreateInfo.NumSamplers;
			pipelineCreateInfo.NumReadonlyStorageTextures = computePipelineCreateInfo.NumReadonlyStorageTextures;
			pipelineCreateInfo.NumReadonlyStorageBuffers = computePipelineCreateInfo.NumReadonlyStorageBuffers;
			pipelineCreateInfo.NumReadWriteStorageTextures = computePipelineCreateInfo.NumReadWriteStorageTextures;
			pipelineCreateInfo.NumReadWriteStorageBuffers = computePipelineCreateInfo.NumReadWriteStorageBuffers;
			pipelineCreateInfo.NumUniformBuffers = computePipelineCreateInfo.NumUniformBuffers;
			pipelineCreateInfo.ThreadCountX = computePipelineCreateInfo.ThreadCountX;
			pipelineCreateInfo.ThreadCountY = computePipelineCreateInfo.ThreadCountY;
			pipelineCreateInfo.ThreadCountZ = computePipelineCreateInfo.ThreadCountZ;
			pipelineCreateInfo.Props = computePipelineCreateInfo.Props;

			var cleanProps = false;
			if (computePipelineCreateInfo.Name != null)
			{
				if (pipelineCreateInfo.Props == 0)
				{
					pipelineCreateInfo.Props = SDL3.SDL.SDL_CreateProperties();
					cleanProps = true;
				}

				SDL3.SDL.SDL_SetStringProperty(pipelineCreateInfo.Props, SDL3.SDL.SDL_PROP_GPU_COMPUTEPIPELINE_CREATE_NAME_STRING, computePipelineCreateInfo.Name);
			}

			var computePipelineHandle = SDL.SDL_CreateGPUComputePipeline(
				device.Handle,
				pipelineCreateInfo
			);

			NativeMemory.Free(entryPointBuffer);

			if (computePipelineHandle == nint.Zero)
			{
				Logger.LogError($"Could not create compute pipeline: {SDL3.SDL.SDL_GetError()}");
				return null;
			}

			var computePipeline = new ComputePipeline(device)
			{
				Handle = computePipelineHandle,
				NumSamplers = computePipelineCreateInfo.NumSamplers,
				NumReadOnlyStorageTextures = computePipelineCreateInfo.NumReadonlyStorageTextures,
				NumReadOnlyStorageBuffers = computePipelineCreateInfo.NumReadonlyStorageBuffers,
				NumReadWriteStorageTextures = computePipelineCreateInfo.NumReadWriteStorageTextures,
				NumReadWriteStorageBuffers = computePipelineCreateInfo.NumReadWriteStorageBuffers,
				NumUniformBuffers = computePipelineCreateInfo.NumUniformBuffers,
				ThreadCountX = computePipelineCreateInfo.ThreadCountX,
				ThreadCountY = computePipelineCreateInfo.ThreadCountY,
				ThreadCountZ = computePipelineCreateInfo.ThreadCountZ,
				Name = SDL3.SDL.SDL_GetStringProperty(pipelineCreateInfo.Props, SDL3.SDL.SDL_PROP_GPU_COMPUTEPIPELINE_CREATE_NAME_STRING, "Compute Pipeline")
			};

			if (cleanProps)
			{
				SDL3.SDL.SDL_DestroyProperties(pipelineCreateInfo.Props);
			}

			return computePipeline;
		}
	}

	/// <summary>
	/// Creates a compute pipeline for any backend from SPIRV bytecode.
	/// </summary>
	internal static unsafe ComputePipeline CreateFromSPIRV(
		GraphicsDevice device,
		string name, // can be null
		ReadOnlySpan<byte> span,
		string entryPoint,
		bool enableDebug
	) {
		var entryPointBuffer = InteropUtilities.EncodeToUTF8Buffer(entryPoint);
		var nameBuffer = InteropUtilities.EncodeToUTF8Buffer(name);

		fixed (byte* spanPtr = span)
		{
			SDL_ShaderCross.INTERNAL_SPIRVInfo spirvInfo;
			spirvInfo.Bytecode = spanPtr;
			spirvInfo.BytecodeSize = (nuint) span.Length;
			spirvInfo.EntryPoint = entryPointBuffer;
			spirvInfo.ShaderStage = SDL_ShaderCross.ShaderStage.Compute;
			spirvInfo.EnableDebug = enableDebug;
			spirvInfo.Name = nameBuffer;
			spirvInfo.Props = 0;

			var computePipelineHandle = SDL_ShaderCross.SDL_ShaderCross_CompileComputePipelineFromSPIRV(
				device.Handle,
				spirvInfo,
				out var pipelineMetadata
			);

			NativeMemory.Free(entryPointBuffer);
			NativeMemory.Free(nameBuffer);

			if (computePipelineHandle == nint.Zero)
			{
				Logger.LogError("Failed to create compute pipeline!");
				Logger.LogError(SDL3.SDL.SDL_GetError());
				return null;
			}

			var computePipeline = new ComputePipeline(device)
			{
				Handle = computePipelineHandle,
				NumSamplers = pipelineMetadata.NumSamplers,
				NumReadOnlyStorageTextures = pipelineMetadata.NumReadOnlyStorageTextures,
				NumReadOnlyStorageBuffers = pipelineMetadata.NumReadOnlyStorageBuffers,
				NumReadWriteStorageTextures = pipelineMetadata.NumReadWriteStorageTextures,
				NumReadWriteStorageBuffers = pipelineMetadata.NumReadWriteStorageBuffers,
				NumUniformBuffers = pipelineMetadata.NumUniformBuffers,
				ThreadCountX = pipelineMetadata.ThreadCountX,
				ThreadCountY = pipelineMetadata.ThreadCountY,
				ThreadCountZ = pipelineMetadata.ThreadCountZ,
				Name = name ?? "ComputePipeline"
			};

			return computePipeline;
		}
	}

	/// <summary>
	/// Creates a compute pipeline for any backend from HLSL source.
	/// </summary>
	internal static unsafe ComputePipeline CreateFromHLSL(
		GraphicsDevice device,
		string name, // can be null
		ReadOnlySpan<byte> span,
		string entryPoint,
		string includeDir,
		bool enableDebug,
		params Span<ShaderCross.HLSLDefine> defines
	) {
		var entryPointBuffer = InteropUtilities.EncodeToUTF8Buffer(entryPoint);
		var includeDirBuffer = InteropUtilities.EncodeToUTF8Buffer(includeDir);
		var nameBuffer = InteropUtilities.EncodeToUTF8Buffer(name);

		fixed (byte* spanPtr = span)
		{

			SDL_ShaderCross.INTERNAL_HLSLDefine* definesBuffer = null;

			if (defines.Length > 0) {
				definesBuffer = (SDL_ShaderCross.INTERNAL_HLSLDefine*) NativeMemory.Alloc((nuint) (Marshal.SizeOf<SDL_ShaderCross.INTERNAL_HLSLDefine>() * (defines.Length + 1)));
				for (var i = 0; i < defines.Length; i += 1)
				{
					definesBuffer[i].Name = InteropUtilities.EncodeToUTF8Buffer(defines[i].Name);
					definesBuffer[i].Value = InteropUtilities.EncodeToUTF8Buffer(defines[i].Value);
				}
				// Null-terminate the array
				definesBuffer[defines.Length].Name = null;
				definesBuffer[defines.Length].Value = null;
			}

			SDL_ShaderCross.INTERNAL_HLSLInfo hlslInfo;
			hlslInfo.Source = spanPtr;
			hlslInfo.EntryPoint = entryPointBuffer;
			hlslInfo.IncludeDir = includeDirBuffer;
			hlslInfo.Defines = definesBuffer;
			hlslInfo.ShaderStage = SDL_ShaderCross.ShaderStage.Compute;
			hlslInfo.EnableDebug = enableDebug;
			hlslInfo.Name = nameBuffer;
			hlslInfo.Props = 0;

			var computePipelineHandle = SDL_ShaderCross.SDL_ShaderCross_CompileComputePipelineFromHLSL(
				device.Handle,
				hlslInfo,
				out var pipelineMetadata
			);

			NativeMemory.Free(entryPointBuffer);
			NativeMemory.Free(includeDirBuffer);
			for (var i = 0; i < defines.Length; i += 1)
			{
				NativeMemory.Free(definesBuffer[i].Name);
				NativeMemory.Free(definesBuffer[i].Value);
			}
			NativeMemory.Free(definesBuffer);
			NativeMemory.Free(nameBuffer);

			if (computePipelineHandle == nint.Zero)
			{
				Logger.LogError("Failed to create compute pipeline!");
				Logger.LogError(SDL3.SDL.SDL_GetError());
				return null;
			}

			var computePipeline = new ComputePipeline(device)
			{
				Handle = computePipelineHandle,
				NumSamplers = pipelineMetadata.NumSamplers,
				NumReadOnlyStorageTextures = pipelineMetadata.NumReadOnlyStorageTextures,
				NumReadOnlyStorageBuffers = pipelineMetadata.NumReadOnlyStorageBuffers,
				NumReadWriteStorageTextures = pipelineMetadata.NumReadWriteStorageTextures,
				NumReadWriteStorageBuffers = pipelineMetadata.NumReadWriteStorageBuffers,
				NumUniformBuffers = pipelineMetadata.NumUniformBuffers,
				ThreadCountX = pipelineMetadata.ThreadCountX,
				ThreadCountY = pipelineMetadata.ThreadCountY,
				ThreadCountZ = pipelineMetadata.ThreadCountZ,
				Name = name ?? "ComputePipeline"
			};

			return computePipeline;
		}
	}
}
