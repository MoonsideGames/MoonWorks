using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Video;

/// <summary>
/// A collection of YUV buffers for decoding video data.
/// </summary>
internal class YUVFramebuffer : IDisposable
{
	public IntPtr YDataBuffer;
	public IntPtr UDataBuffer;
	public IntPtr VDataBuffer;
	public uint YDataBufferLength;
	public uint UVDataBufferLength;
	public uint YStride;
	public uint UVStride;
	public uint YWidth;
	public uint YHeight;
	public uint UVWidth;
	public uint UVHeight;

	private bool IsDisposed;

	public unsafe void SetBufferData(
		Span<byte> ySpan,
		Span<byte> uSpan,
		Span<byte> vSpan,
		uint yStride,
		uint uvStride,
		uint yWidth,
		uint yHeight,
		uint uvWidth,
		uint uvHeight
	) {
		if (YDataBufferLength < ySpan.Length)
		{
			YDataBuffer = (nint) NativeMemory.Realloc((void*) YDataBuffer, (nuint) ySpan.Length);
			YDataBufferLength = (uint) ySpan.Length;
		}

		if (UVDataBufferLength < uSpan.Length)
		{
			// U and V buffer are always same length
			UDataBuffer = (nint) NativeMemory.Realloc((void*) UDataBuffer, (nuint) uSpan.Length);
			VDataBuffer = (nint) NativeMemory.Realloc((void*) VDataBuffer, (nuint) uSpan.Length);

			UVDataBufferLength = (uint) uSpan.Length;
		}

		var allocatedYSpan = new Span<byte>((void*) YDataBuffer, (int) YDataBufferLength);
		var allocatedUSpan = new Span<byte>((void*) UDataBuffer, (int) UVDataBufferLength);
		var allocatedVSpan = new Span<byte>((void*) VDataBuffer, (int) UVDataBufferLength);

		ySpan.CopyTo(allocatedYSpan);
		uSpan.CopyTo(allocatedUSpan);
		vSpan.CopyTo(allocatedVSpan);

		YStride = yStride;
		UVStride = uvStride;

		YHeight = yHeight;
		YWidth = yWidth;
		UVWidth = uvWidth;
		UVHeight = uvHeight;
	}

	protected virtual unsafe void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			NativeMemory.Free((void*) YDataBuffer);
			NativeMemory.Free((void*) UDataBuffer);
			NativeMemory.Free((void*) VDataBuffer);
			IsDisposed = true;
		}
	}

	~YUVFramebuffer()
	{
	    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	    Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
