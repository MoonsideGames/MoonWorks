using RefreshCS;
using System;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Compute pipelines perform arbitrary parallel processing on input data.
	/// </summary>
	public class ComputePipeline : GraphicsResource
	{
		protected override Action<IntPtr, IntPtr> QueueDestroyFunction => Refresh.Refresh_QueueDestroyComputePipeline;

		public ComputeShaderInfo ComputeShaderInfo { get; }

		public unsafe ComputePipeline(
			GraphicsDevice device,
			ComputeShaderInfo computeShaderInfo
		) : base(device)
		{
			var refreshComputeShaderInfo = new Refresh.ComputeShaderInfo
			{
				entryPointName = computeShaderInfo.EntryPointName,
				shaderModule = computeShaderInfo.ShaderModule.Handle,
				uniformBufferSize = computeShaderInfo.UniformBufferSize,
				bufferBindingCount = computeShaderInfo.BufferBindingCount,
				imageBindingCount = computeShaderInfo.ImageBindingCount
			};

			Handle = Refresh.Refresh_CreateComputePipeline(
				device.Handle,
				refreshComputeShaderInfo
			);
			if (Handle == IntPtr.Zero)
			{
				throw new Exception("Could not create compute pipeline!");
			}

			ComputeShaderInfo = computeShaderInfo;
		}
	}
}
