using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MoonWorks;

public static class InteropUtilities
{
	/// <summary>
	/// Encodes a string into a native buffer representation.
	/// You MUST call NativeMemory.Free when this is no longer needed.
	/// You can cause leaks with this, be careful!
	/// </summary>
	/// <returns>A string as a buffer of UTF-8 bytes.</returns>
	public static unsafe byte* EncodeToUTF8Buffer(string s, out int length)
	{
		if (s == null)
		{
			length = 0;
			return null;
		}

		length = Encoding.UTF8.GetByteCount(s) + 1;
		var buffer = (byte*) NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, length - 1);
		var byteCount = Encoding.UTF8.GetBytes(s, span);
		buffer[byteCount] = 0;

		return buffer;
	}

	/// <summary>
	/// Encodes a string into a native buffer representation.
	/// You MUST call NativeMemory.Free when this is no longer needed.
	/// You can cause leaks with this, be careful!
	/// </summary>
	/// <returns>A string as a buffer of UTF-8 bytes.</returns>
	public static unsafe byte* EncodeToUTF8Buffer(string s) => EncodeToUTF8Buffer(s, out _);

	/// <summary>
	/// Decodes a native buffer into a string.
	/// </summary>
	public static unsafe string DecodeFromUTF8Buffer(byte* buffer, int bufferLength)
	{
		return Encoding.UTF8.GetString(buffer, bufferLength - 1);
	}
}
