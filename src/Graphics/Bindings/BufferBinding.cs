namespace MoonWorks.Graphics
{
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
