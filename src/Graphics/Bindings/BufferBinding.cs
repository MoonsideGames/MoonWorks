namespace MoonWorks.Graphics
{
	/// <summary>
	/// A buffer-offset pair to be used when binding vertex buffers.
	/// </summary>
	public struct BufferBinding
	{
		public Buffer Buffer;
		public ulong Offset;

		public BufferBinding(Buffer buffer, ulong offset)
		{
			Buffer = buffer;
			Offset = offset;
		}
	}
}
