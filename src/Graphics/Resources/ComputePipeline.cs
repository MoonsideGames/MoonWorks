using SDL2_gpuCS;
using System;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Compute pipelines perform arbitrary parallel processing on input data.
	/// </summary>
	public class ComputePipeline : SDL_GpuResource
	{
		protected override Action<IntPtr, IntPtr> ReleaseFunction => SDL_Gpu.SDL_GpuReleaseComputePipeline;

		public ComputePipelineResourceInfo ResourceInfo { get; }

		public unsafe ComputePipeline(
			GraphicsDevice device,
			Shader computeShader,
			ComputePipelineResourceInfo resourceInfo
		) : base(device)
		{
			var sdlComputePipelineCreateInfo = new SDL_Gpu.ComputePipelineCreateInfo
			{
				ComputeShader = computeShader.Handle,
				PipelineResourceInfo = resourceInfo.ToSDL()
			};

			Handle = SDL_Gpu.SDL_GpuCreateComputePipeline(
				device.Handle,
				sdlComputePipelineCreateInfo
			);

			if (Handle == IntPtr.Zero)
			{
				throw new Exception("Could not create compute pipeline!");
			}

			ResourceInfo = resourceInfo;
		}
	}
}
