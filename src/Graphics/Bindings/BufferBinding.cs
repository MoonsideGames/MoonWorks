namespace MoonWorks.Graphics
{
	/// <summary>
	/// A buffer-offset pair to be used when binding vertex buffers.
	/// </summary>
	public struct BufferBinding
	{
		public GpuBuffer Buffer;
		public ulong Offset;

		public BufferBinding(GpuBuffer buffer, ulong offset)
		{
			Buffer = buffer;
			Offset = offset;
		}
	}
}
