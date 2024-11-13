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

	private ComputePipeline(GraphicsDevice device) : base(device) { }

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
			Handle = computePipelineHandle
		};

		return computePipeline;
	}

	/// <summary>
	/// Creates a compute pipeline for any backend from SPIRV bytecode.
	/// </summary>
	internal static unsafe ComputePipeline CreateFromSPIRV(
		GraphicsDevice device,
		Stream stream,
		string entryPoint
	) {
		var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
		var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
		stream.ReadExactly(bytecodeSpan);

		var computePipelineHandle = SDL_ShaderCross.SDL_ShaderCross_CompileComputePipelineFromSPIRV(
			device.Handle,
			bytecodeSpan,
			(nuint) stream.Length,
			entryPoint
		);

		NativeMemory.Free(bytecodeBuffer);

		if (computePipelineHandle == nint.Zero)
		{
			Logger.LogError("Failed to create compute pipeline!");
			return null;
		}

		var computePipeline = new ComputePipeline(device)
		{
			Handle = computePipelineHandle
		};

		return computePipeline;
	}

	/// <summary>
	/// Creates a compute pipeline for any backend from HLSL source.
	/// </summary>
	internal static unsafe ComputePipeline CreateFromHLSL(
		GraphicsDevice device,
		Stream stream,
		string entryPoint,
		string includeDir
	) {
		byte* hlslBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length + 1);
		var hlslSpan = new Span<byte>(hlslBuffer, (int) stream.Length);
		stream.ReadExactly(hlslSpan);
		hlslBuffer[(int)stream.Length] = 0; // ensure null-terminated

		var computePipelineHandle = SDL_ShaderCross.SDL_ShaderCross_CompileComputePipelineFromHLSL(
			device.Handle,
			hlslSpan,
			entryPoint,
			includeDir
		);

		NativeMemory.Free(hlslBuffer);

		if (computePipelineHandle == nint.Zero)
		{
			Logger.LogError("Failed to create compute pipeline!");
			return null;
		}

		var computePipeline = new ComputePipeline(device)
		{
			Handle = computePipelineHandle
		};

		return computePipeline;
	}
}
