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

	public uint NumSamplers { get; private init; }
	public uint NumReadOnlyStorageTextures { get; private init; }
	public uint NumReadOnlyStorageBuffers { get; private init; }
	public uint NumReadWriteStorageTextures { get; private init; }
	public uint NumReadWriteStorageBuffers { get; private init; }
	public uint NumUniformBuffers { get; private init; }

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
			Handle = computePipelineHandle,
			NumSamplers = pipelineCreateInfo.NumSamplers,
			NumReadOnlyStorageTextures = pipelineCreateInfo.NumReadonlyStorageTextures,
			NumReadOnlyStorageBuffers = pipelineCreateInfo.NumReadonlyStorageBuffers,
			NumReadWriteStorageTextures = pipelineCreateInfo.NumReadWriteStorageTextures,
			NumReadWriteStorageBuffers = pipelineCreateInfo.NumReadWriteStorageBuffers,
			NumUniformBuffers = pipelineCreateInfo.NumUniformBuffers
		};

		return computePipeline;
	}

	/// <summary>
	/// Creates a compute pipeline for any backend from SPIRV bytecode.
	/// </summary>
	internal static unsafe ComputePipeline CreateFromSPIRV(
		GraphicsDevice device,
		Stream stream,
		string entryPoint,
		in ShaderCross.ComputePipelineCreateInfo createInfo
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
		pipelineCreateInfo.Format = ShaderFormat.Private; // this will be replaced
		pipelineCreateInfo.NumSamplers = createInfo.NumSamplers;
		pipelineCreateInfo.NumReadonlyStorageTextures = createInfo.NumReadonlyStorageTextures;
		pipelineCreateInfo.NumReadonlyStorageBuffers = createInfo.NumReadonlyStorageBuffers;
		pipelineCreateInfo.NumReadWriteStorageTextures = createInfo.NumReadWriteStorageTextures;
		pipelineCreateInfo.NumReadWriteStorageBuffers = createInfo.NumReadWriteStorageBuffers;
		pipelineCreateInfo.NumUniformBuffers = createInfo.NumUniformBuffers;
		pipelineCreateInfo.ThreadCountX = createInfo.ThreadCountX;
		pipelineCreateInfo.ThreadCountY = createInfo.ThreadCountY;
		pipelineCreateInfo.ThreadCountZ = createInfo.ThreadCountZ;
		pipelineCreateInfo.Props = createInfo.Props;

		var computePipelineHandle = SDL_ShaderCross.SDL_ShaderCross_CompileComputePipelineFromSPIRV(
			device.Handle,
			pipelineCreateInfo
		);

		NativeMemory.Free(bytecodeBuffer);
		NativeMemory.Free(entryPointBuffer);

		if (computePipelineHandle == nint.Zero)
		{
			Logger.LogError("Failed to create compute pipeline!");
			return null;
		}

		var computePipeline = new ComputePipeline(device)
		{
			Handle = computePipelineHandle,
			NumSamplers = pipelineCreateInfo.NumSamplers,
			NumReadOnlyStorageTextures = pipelineCreateInfo.NumReadonlyStorageTextures,
			NumReadOnlyStorageBuffers = pipelineCreateInfo.NumReadonlyStorageBuffers,
			NumReadWriteStorageTextures = pipelineCreateInfo.NumReadWriteStorageTextures,
			NumReadWriteStorageBuffers = pipelineCreateInfo.NumReadWriteStorageBuffers,
			NumUniformBuffers = pipelineCreateInfo.NumUniformBuffers
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
		in ShaderCross.ComputePipelineCreateInfo createInfo
	) {
		byte* bytecodeBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length + 1);
		var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
		stream.ReadExactly(bytecodeSpan);
		bytecodeBuffer[(int)stream.Length] = 0; // null-terminate

		var entryPointLength = Encoding.UTF8.GetByteCount(entryPoint) + 1;
		var entryPointBuffer = NativeMemory.Alloc((nuint) entryPointLength);
		var buffer = new Span<byte>(entryPointBuffer, entryPointLength);
		var byteCount = Encoding.UTF8.GetBytes(entryPoint, buffer);
		buffer[byteCount] = 0;

		INTERNAL_ComputePipelineCreateInfo pipelineCreateInfo;
		pipelineCreateInfo.CodeSize = (nuint) stream.Length;
		pipelineCreateInfo.Code = (byte*) bytecodeBuffer;
		pipelineCreateInfo.EntryPoint = (byte*) entryPointBuffer;
		pipelineCreateInfo.Format = ShaderFormat.Private; // this will be replaced
		pipelineCreateInfo.NumSamplers = createInfo.NumSamplers;
		pipelineCreateInfo.NumReadonlyStorageTextures = createInfo.NumReadonlyStorageTextures;
		pipelineCreateInfo.NumReadonlyStorageBuffers = createInfo.NumReadonlyStorageBuffers;
		pipelineCreateInfo.NumReadWriteStorageTextures = createInfo.NumReadWriteStorageTextures;
		pipelineCreateInfo.NumReadWriteStorageBuffers = createInfo.NumReadWriteStorageBuffers;
		pipelineCreateInfo.NumUniformBuffers = createInfo.NumUniformBuffers;
		pipelineCreateInfo.ThreadCountX = createInfo.ThreadCountX;
		pipelineCreateInfo.ThreadCountY = createInfo.ThreadCountY;
		pipelineCreateInfo.ThreadCountZ = createInfo.ThreadCountZ;
		pipelineCreateInfo.Props = createInfo.Props;

		var computePipelineHandle = SDL_ShaderCross.SDL_ShaderCross_CompileComputePipelineFromHLSL(
			device.Handle,
			pipelineCreateInfo,
			bytecodeSpan
		);

		NativeMemory.Free(bytecodeBuffer);
		NativeMemory.Free(entryPointBuffer);

		if (computePipelineHandle == nint.Zero)
		{
			Logger.LogError("Failed to create compute pipeline!");
			return null;
		}

		var computePipeline = new ComputePipeline(device)
		{
			Handle = computePipelineHandle,
			NumSamplers = pipelineCreateInfo.NumSamplers,
			NumReadOnlyStorageTextures = pipelineCreateInfo.NumReadonlyStorageTextures,
			NumReadOnlyStorageBuffers = pipelineCreateInfo.NumReadonlyStorageBuffers,
			NumReadWriteStorageTextures = pipelineCreateInfo.NumReadWriteStorageTextures,
			NumReadWriteStorageBuffers = pipelineCreateInfo.NumReadWriteStorageBuffers,
			NumUniformBuffers = pipelineCreateInfo.NumUniformBuffers
		};

		return computePipeline;
	}
}
