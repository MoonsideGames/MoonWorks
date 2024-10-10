using System;

namespace MoonWorks.Graphics;

public static class VertexStructs
{
	public record struct Int(int X);
	public record struct Int2(int X, int Y);
	public record struct Int3(int X, int Y, int Z);
	public record struct Int4(int X, int Y, int Z, int W);
	public record struct Uint(uint X);
	public record struct Uint2(uint X, uint Y);
	public record struct Uint3(uint X, uint Y, uint Z);
	public record struct Uint4(uint X, uint Y, uint Z, uint W);
	public record struct Float(float X);
	public record struct Float2(float X, float Y);
	public record struct Float3(float X, float Y, float Z);
	public record struct Float4(float X, float Y, float Z, float W);
	public record struct Byte2(sbyte X, sbyte Y);
	public record struct Byte4(sbyte X, sbyte Y, sbyte Z, sbyte W);
	public record struct Ubyte2(byte X, byte Y);
	public record struct Ubyte4(byte X, byte Y, byte Z, byte W);

	public record struct Byte2Norm(sbyte X, sbyte Y)
	{
		public Byte2Norm(float x, float y) : this(
			SByteNormalize(x),
			SByteNormalize(y)
		) { }
	}

	public record struct Byte4Norm(sbyte X, sbyte Y, sbyte Z, sbyte W)
	{
		public Byte4Norm(float x, float y, float z, float w) : this(
			SByteNormalize(x),
			SByteNormalize(y),
			SByteNormalize(z),
			SByteNormalize(w)
		) { }
	}

	public record struct Ubyte2Norm(byte X, byte Y)
	{
		public Ubyte2Norm(float x, float y) : this(
			ByteNormalize(x),
			ByteNormalize(y)
		) { }
	}

	public record struct Ubyte4Norm(byte X, byte Y, byte Z, byte W)
	{
		public Ubyte4Norm(float x, float y, float z, float w) : this(
			ByteNormalize(x),
			ByteNormalize(y),
			ByteNormalize(z),
			ByteNormalize(w)
		) { }
	}

	public record struct Short2(short X, short Y);
	public record struct Short4(short X, short Y, short Z, short W);
	public record struct Ushort2(ushort X, ushort Y);
	public record struct Ushort4(ushort X, ushort Y, ushort Z, ushort W);

	public record struct Short2Norm(short X, short Y)
	{
		public Short2Norm(float x, float y) : this(
			ShortNormalize(x),
			ShortNormalize(y)
		) { }
	}

	public record struct Short4Norm(short X, short Y, short Z, short W)
	{
		public Short4Norm(float x, float y, float z, float w) : this(
			ShortNormalize(x),
			ShortNormalize(y),
			ShortNormalize(z),
			ShortNormalize(w)
		) { }
	}

	public record struct Ushort2Norm(ushort X, ushort Y)
	{
		public Ushort2Norm(float x, float y) : this(
			UShortNormalize(x),
			UShortNormalize(y)
		) { }
	}

	public record struct Ushort4Norm(ushort X, ushort Y, ushort Z, ushort W)
	{
		public Ushort4Norm(float x, float y, float z, float w) : this(
			UShortNormalize(x),
			UShortNormalize(y),
			UShortNormalize(z),
			UShortNormalize(w)
		) { }
	}

	public record struct Half2(Half X, Half Y);
	public record struct Half4(Half X, Half Y, Half Z, Half W);

	private static sbyte SByteNormalize(float x) => (sbyte) (System.Math.Clamp(x, -1, 1) * sbyte.MaxValue);
	private static byte ByteNormalize(float x) => (byte) (System.Math.Clamp(x, 0, 1) * byte.MaxValue);
	private static short ShortNormalize(float x) => (short) (System.Math.Clamp(x, -1, 1) * short.MaxValue);
	private static ushort UShortNormalize(float x) => (ushort) (System.Math.Clamp(x, 0, 1) * ushort.MaxValue);
}
