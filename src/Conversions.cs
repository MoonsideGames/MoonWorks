using System.Collections.Generic;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Graphics.PackedVector;

namespace MoonWorks
{
	/// <summary>
	/// Conversion utilities for interop.
	/// </summary>
	public static class Conversions
	{
		private readonly static Dictionary<VertexElementFormat, uint> Sizes = new Dictionary<VertexElementFormat, uint>
		{
			{ VertexElementFormat.Byte4, (uint) Marshal.SizeOf<Byte4>() },
			{ VertexElementFormat.Color, (uint) Marshal.SizeOf<Color>() },
			{ VertexElementFormat.Float, (uint) Marshal.SizeOf<float>() },
			{ VertexElementFormat.HalfVector2, (uint) Marshal.SizeOf<HalfVector2>() },
			{ VertexElementFormat.HalfVector4, (uint) Marshal.SizeOf<HalfVector4>() },
			{ VertexElementFormat.NormalizedShort2, (uint) Marshal.SizeOf<NormalizedShort2>() },
			{ VertexElementFormat.NormalizedShort4, (uint) Marshal.SizeOf<NormalizedShort4>() },
			{ VertexElementFormat.Short2, (uint) Marshal.SizeOf<Short2>() },
			{ VertexElementFormat.Short4, (uint) Marshal.SizeOf<Short4>() },
			{ VertexElementFormat.Uint, (uint) Marshal.SizeOf<uint>() },
			{ VertexElementFormat.Vector2, (uint) Marshal.SizeOf<Math.Float.Vector2>() },
			{ VertexElementFormat.Vector3, (uint) Marshal.SizeOf<Math.Float.Vector3>() },
			{ VertexElementFormat.Vector4, (uint) Marshal.SizeOf<Math.Float.Vector4>() }
		};

		public static byte BoolToByte(bool b)
		{
			return (byte) (b ? 1 : 0);
		}

		public static bool ByteToBool(byte b)
		{
			return b != 0;
		}

		public static int BoolToInt(bool b)
		{
			return b ? 1 : 0;
		}

		public static bool IntToBool(int b)
		{
			return b != 0;
		}

		public static uint VertexElementFormatSize(VertexElementFormat format)
		{
			return Sizes[format];
		}
	}
}
