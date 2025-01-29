using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MoonWorks;

public static class InteropUtilities
{
	/// <summary>
	/// Marshals a string into a native buffer representation.
	/// You MUST call NativeMemory.Free when this is no longer needed.
	/// You can cause leaks with this, be careful!
	/// </summary>
	/// <returns>A string as a buffer of UTF-8 bytes.</returns>
	public static unsafe byte* MarshalString(string s, out int length)
	{
		if (s == null) {
			length = 0;
			return null;
		}

		length = Encoding.UTF8.GetByteCount(s) + 1;
		var buffer = (byte*) NativeMemory.Alloc((nuint) length);
		var span = new Span<byte>(buffer, length);
		var byteCount = Encoding.UTF8.GetBytes(s, span);
		span[byteCount] = 0;

		return buffer;
	}

	/// <summary>
	/// Marshals a string into a native buffer representation.
	/// You MUST call NativeMemory.Free when this is no longer needed.
	/// You can cause leaks with this, be careful!
	/// </summary>
	/// <returns>A string as a buffer of UTF-8 bytes.</returns>
	public static unsafe byte* MarshalString(string s) => MarshalString(s, out _);
}
