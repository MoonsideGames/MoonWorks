using System.Collections.Generic;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;

namespace MoonWorks
{
	/// <summary>
	/// Conversion utilities for interop.
	/// </summary>
	public static class Conversions
	{
		private readonly static Dictionary<VertexElementFormat, uint> Sizes = new()
		{
			{ VertexElementFormat.Int,         (uint) Marshal.SizeOf<VertexStructs.Int>() },
			{ VertexElementFormat.Int2,        (uint) Marshal.SizeOf<VertexStructs.Int2>() },
			{ VertexElementFormat.Int3,        (uint) Marshal.SizeOf<VertexStructs.Int3>() },
			{ VertexElementFormat.Int4,        (uint) Marshal.SizeOf<VertexStructs.Int4>() },
			{ VertexElementFormat.Uint,        (uint) Marshal.SizeOf<VertexStructs.Uint>() },
			{ VertexElementFormat.Uint2,       (uint) Marshal.SizeOf<VertexStructs.Uint2>() },
			{ VertexElementFormat.Uint3,       (uint) Marshal.SizeOf<VertexStructs.Uint3>() },
			{ VertexElementFormat.Uint4,       (uint) Marshal.SizeOf<VertexStructs.Uint4>() },
			{ VertexElementFormat.Float,       (uint) Marshal.SizeOf<VertexStructs.Float>() },
			{ VertexElementFormat.Float2,      (uint) Marshal.SizeOf<VertexStructs.Float2>() },
			{ VertexElementFormat.Float3,      (uint) Marshal.SizeOf<VertexStructs.Float3>() },
			{ VertexElementFormat.Float4,      (uint) Marshal.SizeOf<VertexStructs.Float4>() },
			{ VertexElementFormat.Byte2,       (uint) Marshal.SizeOf<VertexStructs.Byte2>() },
			{ VertexElementFormat.Byte4,       (uint) Marshal.SizeOf<VertexStructs.Byte4>() },
			{ VertexElementFormat.Ubyte2,      (uint) Marshal.SizeOf<VertexStructs.Ubyte2>() },
			{ VertexElementFormat.Ubyte4,      (uint) Marshal.SizeOf<VertexStructs.Ubyte4>() },
			{ VertexElementFormat.Byte2Norm,   (uint) Marshal.SizeOf<VertexStructs.Byte2Norm>() },
			{ VertexElementFormat.Byte4Norm,   (uint) Marshal.SizeOf<VertexStructs.Byte4Norm>() },
			{ VertexElementFormat.Ubyte2Norm,  (uint) Marshal.SizeOf<VertexStructs.Ubyte2Norm> () },
			{ VertexElementFormat.Ubyte4Norm,  (uint) Marshal.SizeOf<VertexStructs.Ubyte4Norm> () },
			{ VertexElementFormat.Short2,      (uint) Marshal.SizeOf<VertexStructs.Short2>() },
			{ VertexElementFormat.Short4,      (uint) Marshal.SizeOf<VertexStructs.Short4>() },
			{ VertexElementFormat.Ushort2,     (uint) Marshal.SizeOf<VertexStructs.Ushort2>() },
			{ VertexElementFormat.Ushort4,     (uint) Marshal.SizeOf<VertexStructs.Ushort4>() },
			{ VertexElementFormat.Short2Norm,  (uint) Marshal.SizeOf<VertexStructs.Short2Norm>() },
			{ VertexElementFormat.Short4Norm,  (uint) Marshal.SizeOf<VertexStructs.Short4Norm>() },
			{ VertexElementFormat.Ushort2Norm, (uint) Marshal.SizeOf<VertexStructs.Ushort2Norm>() },
			{ VertexElementFormat.Ushort4Norm, (uint) Marshal.SizeOf<VertexStructs.Ushort4Norm>() },
			{ VertexElementFormat.Half2,       (uint) Marshal.SizeOf<VertexStructs.Half2>() },
			{ VertexElementFormat.Half4,       (uint) Marshal.SizeOf<VertexStructs.Half4>() }
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
