using System;
using System.Runtime.InteropServices;

namespace MoonWorks;

internal ref struct ByteSpanStream
{
	public ReadOnlySpan<byte> Span;
	public int Index;

	public ByteSpanStream(ReadOnlySpan<byte> span)
	{
		Span = span;
		Index = 0;
	}

	public unsafe T Read<T>() where T : unmanaged
	{
		var result = MemoryMarshal.Read<T>(Span[Index..]);
		Index += sizeof(T);
		return result;
	}

	public void Advance(int offset)
	{
		Index += offset;
	}

	public ReadOnlySpan<byte> SliceRemainder() => Span[Index..];
}
