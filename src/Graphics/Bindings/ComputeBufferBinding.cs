using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Binding specification to be used when binding buffers for compute shaders.
	/// </summary>
	/// <param name="GpuBuffer">The GpuBuffer to bind.</param>
	/// <param name="WriteOption">
	///   Specifies data dependency behavior when this buffer is written to in the shader. <br/>
	///
	///   Cycle:
	///     If this buffer has been used in commands that have not finished,
	///     the implementation may choose to prevent a dependency on those commands
	///     at the cost of increased memory usage.
	///     You may NOT assume that any of the previous data is retained.
	///     This may prevent stalls when frequently updating a resource.  <br />
	///
	///   SafeOverwrite:
	///     Overwrites the data safely using a GPU memory barrier.
	/// </param>
	public readonly record struct ComputeBufferBinding(
		GpuBuffer GpuBuffer,
		WriteOptions WriteOption
	) {
		public Refresh.ComputeBufferBinding ToRefresh()
		{
			return new Refresh.ComputeBufferBinding
			{
				gpuBuffer = GpuBuffer.Handle,
				writeOption = (Refresh.WriteOptions) WriteOption
			};
		}
	}
}
