using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SDL = MoonWorks.Graphics.SDL_GPU;

namespace MoonWorks.Graphics;

/// <summary>
/// Compute pipelines perform arbitrary parallel processing on input data.
/// </summary>
public class ComputePipeline : RefreshResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL.SDL_ReleaseGPUComputePipeline;

	public uint NumSamplers { get; private init; }
	public uint NumReadOnlyStorageTextures { get; private init; }
	public uint NumReadOnlyStorageBuffers { get; private init; }
	public uint NumReadWriteStorageTextures { get; private init; }
	public uint NumReadWriteStorageBuffers { get; private init; }
	public uint NumUniformBuffers { get; private init; }

	private ComputePipeline(GraphicsDevice device) : base(device) { }

	public static unsafe ComputePipeline Create(
		GraphicsDevice device,
		string filePath,
		string entryPoint,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		return Create(device, stream, entryPoint, computePipelineCreateInfo);
	}

	public static unsafe ComputePipeline Create(
		GraphicsDevice device,
		Stream stream,
		string entryPoint,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		var bytecodeBuffer = NativeMemory.Alloc((nuint) stream.Length);
		var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
		stream.ReadExactly(bytecodeSpan);

		var entryPointLength = Encoding.UTF8.GetByteCount(entryPoint);
		var entryPointBuffer = NativeMemory.Alloc((nuint) entryPointLength);
		Encoding.UTF8.GetString((byte*) entryPointBuffer, entryPointLength);

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
}
