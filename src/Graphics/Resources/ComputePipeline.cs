using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
		string filePath,
		string entryPoint,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		return Create(device, stream, entryPoint, computePipelineCreateInfo);
	}

	/// <summary>
	/// Creates a compute pipeline using a specified shader format.
	/// </summary>
	public static unsafe ComputePipeline Create(
		GraphicsDevice device,
		Stream stream,
		string entryPoint,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
		var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
		stream.ReadExactly(bytecodeSpan);

		var entryPointLength = Encoding.UTF8.GetByteCount(entryPoint) + 1;
		var entryPointBuffer = NativeMemory.Alloc((nuint) entryPointLength);
		var buffer = new Span<byte>(entryPointBuffer, entryPointLength);
		var byteCount = Encoding.UTF8.GetBytes(entryPoint, buffer);
		buffer[byteCount] = 0;

		INTERNAL_ComputePipelineCreateInfo pipelineCreateInfo;
		pipelineCreateInfo.CodeSize = (nuint) stream.Length;
		pipelineCreateInfo.Code = (byte*) bytecodeBuffer;
		pipelineCreateInfo.EntryPoint = (byte*) entryPointBuffer;
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

		NativeMemory.Free(bytecodeBuffer);
		NativeMemory.Free(entryPointBuffer);

		if (computePipelineHandle == nint.Zero)
		{
			throw new Exception("Could not create compute pipeline!");
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

	/// <summary>
	/// Creates a compute pipeline for any backend from SPIRV bytecode.
	/// </summary>
	internal static unsafe ComputePipeline CreateFromSPIRV(
		GraphicsDevice device,
		string name, // can be null
		Stream stream,
		string entryPoint,
		bool enableDebug
	) {
		var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
		var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
		stream.ReadExactly(bytecodeSpan);

		var entryPointBuffer = MarshalString(entryPoint);
		var nameBuffer = MarshalString(name);

		SDL_ShaderCross.INTERNAL_SPIRVInfo spirvInfo;
		spirvInfo.Bytecode = (byte*) bytecodeBuffer;
		spirvInfo.BytecodeSize = (nuint) stream.Length;
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

		NativeMemory.Free(bytecodeBuffer);
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

	/// <summary>
	/// Creates a compute pipeline for any backend from HLSL source.
	/// </summary>
	internal static unsafe ComputePipeline CreateFromHLSL(
		GraphicsDevice device,
		string name, // can be null
		Stream stream,
		string entryPoint,
		string includeDir,
		bool enableDebug,
		params Span<ShaderCross.HLSLDefine> defines
	) {
		byte* hlslBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length + 1);
		var hlslSpan = new Span<byte>(hlslBuffer, (int) stream.Length);
		stream.ReadExactly(hlslSpan);
		hlslBuffer[(int)stream.Length] = 0; // ensure null-terminated

		var entryPointBuffer = MarshalString(entryPoint);
		var includeDirBuffer = MarshalString(includeDir);
		var nameBuffer = MarshalString(name);

		SDL_ShaderCross.INTERNAL_HLSLDefine* definesBuffer = null;

		if (defines.Length > 0) {
			definesBuffer = (SDL_ShaderCross.INTERNAL_HLSLDefine*) NativeMemory.Alloc((nuint) (Marshal.SizeOf<SDL_ShaderCross.INTERNAL_HLSLDefine>() * (defines.Length + 1)));
			for (var i = 0; i < defines.Length; i += 1)
			{
				definesBuffer[i].Name = MarshalString(defines[i].Name);
				definesBuffer[i].Value = MarshalString(defines[i].Value);
			}
			// Null-terminate the array
			definesBuffer[defines.Length].Name = null;
			definesBuffer[defines.Length].Value = null;
		}

		SDL_ShaderCross.INTERNAL_HLSLInfo hlslInfo;
		hlslInfo.Source = hlslBuffer;
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

		NativeMemory.Free(hlslBuffer);
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

	// MUST call NativeMemory.Free on the result eventually!
	private static unsafe byte* MarshalString(string s)
	{
		if (s == null) { return null; }

		var length = Encoding.UTF8.GetByteCount(s) + 1;
		var buffer = (byte*) NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, length);
		var byteCount = Encoding.UTF8.GetBytes(s, span);
		span[byteCount] = 0;

		return buffer;
	}
}
