using RefreshCS;
using System;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Compute pipelines perform arbitrary parallel processing on input data.
	/// </summary>
	public class ComputePipeline : RefreshResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => Refresh.Refresh_ReleaseComputePipeline;

		public ComputePipelineResourceInfo ResourceInfo { get; }

		public unsafe ComputePipeline(
			GraphicsDevice device,
			Shader computeShader,
			ComputePipelineResourceInfo resourceInfo
		) : base(device)
		{
			var refreshComputePipelineCreateInfo = new Refresh.ComputePipelineCreateInfo
			{
				ComputeShader = computeShader.Handle,
				PipelineResourceInfo = resourceInfo.ToRefresh()
			};

			Handle = Refresh.Refresh_CreateComputePipeline(
				device.Handle,
				refreshComputePipelineCreateInfo
			);

			if (Handle == IntPtr.Zero)
			{
				throw new Exception("Could not create compute pipeline!");
			}

			ResourceInfo = resourceInfo;
		}
	}
}
