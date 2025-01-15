using System.Runtime.InteropServices;
using System.Numerics;

namespace MoonWorks.Graphics.Font
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertexType
	{
		public Vector2 Position;
		public Vector2 TexCoord;
		public uint ChunkIndex;

		public static VertexElementFormat[] Formats { get; } =
		[
			VertexElementFormat.Float2,
			VertexElementFormat.Float2,
			VertexElementFormat.Uint
		];

		public static uint[] Offsets { get; } =
		[
			0,
			8,
			16
		];
	}
}
