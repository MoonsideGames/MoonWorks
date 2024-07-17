using RefreshCS;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics;

/// <summary>
/// Compute pipelines perform arbitrary parallel processing on input data.
/// </summary>
public class ComputePipeline : RefreshResource
{
	protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseComputePipeline;

	public uint ReadOnlyStorageTextureCount { get; }
	public uint ReadOnlyStorageBufferCount { get; }
	public uint ReadWriteStorageTextureCount { get; }
	public uint ReadWriteStorageBufferCount { get; }
	public uint UniformBufferCount { get; }

	public ComputePipeline(
		GraphicsDevice device,
		string filePath,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) : base(device)
	{
		using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		Handle = CreateFromStream(device, stream, entryPointName, computePipelineCreateInfo);

		ReadOnlyStorageTextureCount = computePipelineCreateInfo.ReadOnlyStorageTextureCount;
		ReadOnlyStorageBufferCount = computePipelineCreateInfo.ReadOnlyStorageBufferCount;
		ReadWriteStorageTextureCount = computePipelineCreateInfo.ReadWriteStorageTextureCount;
		ReadWriteStorageBufferCount = computePipelineCreateInfo.ReadWriteStorageBufferCount;
		UniformBufferCount = computePipelineCreateInfo.UniformBufferCount;
	}

	public ComputePipeline(
		GraphicsDevice device,
		Stream stream,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) : base(device)
	{
		Handle = CreateFromStream(device, stream, entryPointName, computePipelineCreateInfo);

		ReadOnlyStorageTextureCount = computePipelineCreateInfo.ReadOnlyStorageTextureCount;
		ReadOnlyStorageBufferCount = computePipelineCreateInfo.ReadOnlyStorageBufferCount;
		ReadWriteStorageTextureCount = computePipelineCreateInfo.ReadWriteStorageTextureCount;
		ReadWriteStorageBufferCount = computePipelineCreateInfo.ReadWriteStorageBufferCount;
		UniformBufferCount = computePipelineCreateInfo.UniformBufferCount;
	}

	private static unsafe nint CreateFromStream(
		GraphicsDevice device,
		Stream stream,
		string entryPointName,
		in ComputePipelineCreateInfo computePipelineCreateInfo
	) {
		var bytecodeBuffer = (byte*) NativeMemory.Alloc((nuint) stream.Length);
		var bytecodeSpan = new Span<byte>(bytecodeBuffer, (int) stream.Length);
		stream.ReadExactly(bytecodeSpan);

		Refresh.ComputePipelineCreateInfo refreshPipelineCreateInfo;
		refreshPipelineCreateInfo.Code = bytecodeBuffer;
		refreshPipelineCreateInfo.CodeSize = (nuint) stream.Length;
		refreshPipelineCreateInfo.EntryPointName = entryPointName;
		refreshPipelineCreateInfo.Format = (Refresh.ShaderFormat) computePipelineCreateInfo.ShaderFormat;
		refreshPipelineCreateInfo.ReadOnlyStorageTextureCount = computePipelineCreateInfo.ReadOnlyStorageTextureCount;
		refreshPipelineCreateInfo.ReadOnlyStorageBufferCount = computePipelineCreateInfo.ReadOnlyStorageBufferCount;
		refreshPipelineCreateInfo.ReadWriteStorageTextureCount = computePipelineCreateInfo.ReadWriteStorageTextureCount;
		refreshPipelineCreateInfo.ReadWriteStorageBufferCount = computePipelineCreateInfo.ReadWriteStorageBufferCount;
		refreshPipelineCreateInfo.UniformBufferCount = computePipelineCreateInfo.UniformBufferCount;
		refreshPipelineCreateInfo.ThreadCountX = computePipelineCreateInfo.ThreadCountX;
		refreshPipelineCreateInfo.ThreadCountY = computePipelineCreateInfo.ThreadCountY;
		refreshPipelineCreateInfo.ThreadCountZ = computePipelineCreateInfo.ThreadCountZ;

		var computePipelineHandle = Refresh.Refresh_CreateComputePipeline(
			device.Handle,
			refreshPipelineCreateInfo
		);

		if (computePipelineHandle == nint.Zero)
		{
			throw new Exception("Could not create compute pipeline!");
		}

		NativeMemory.Free(bytecodeBuffer);
		return computePipelineHandle;
	}
}
