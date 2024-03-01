using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// A buffer-offset pair to be used when binding vertex or index buffers.
	/// </summary>
	public readonly record struct BufferBinding(
		GpuBuffer Buffer,
		uint Offset
	) {
		public Refresh.BufferBinding ToRefresh()
		{
			return new Refresh.BufferBinding
			{
				gpuBuffer = Buffer.Handle,
				offset = Offset
			};
		}
	}
}
