using System.Runtime.InteropServices;
using MoonWorks.Math.Float;

namespace MoonWorks.Graphics.Font
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex : IVertexType
	{
		public Vector3 Position;
		public Vector2 TexCoord;
		public Color Color;

		public static VertexElementFormat[] Formats { get; } =
		[
			VertexElementFormat.Vector3,
			VertexElementFormat.Vector2,
			VertexElementFormat.Color
		];

		public static uint[] Offsets { get; } =
		[
			0,
			12,
			20
		];
	}
}
