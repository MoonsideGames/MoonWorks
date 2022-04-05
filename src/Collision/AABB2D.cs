using System.Collections.Generic;
using MoonWorks.Math;

namespace MoonWorks.Collision
{
	/// <summary>
	/// Axis-aligned bounding box.
	/// </summary>
	public struct AABB2D : System.IEquatable<AABB2D>
	{
		/// <summary>
		/// The top-left position of the AABB.
		/// </summary>
		/// <value></value>
		public Vector2 Min { get; private set; }

		/// <summary>
		/// The bottom-right position of the AABB.
		/// </summary>
		/// <value></value>
		public Vector2 Max { get; private set; }

		public float Width { get { return Max.X - Min.X; } }
		public float Height { get { return Max.Y - Min.Y; } }

		public float Right { get { return Max.X; } }
		public float Left { get { return Min.X; } }

		/// <summary>
		/// The top of the AABB. Assumes a downward-aligned Y axis, so this value will be smaller than Bottom.
		/// </summary>
		/// <value></value>
		public float Top { get { return Min.Y; } }

		/// <summary>
		/// The bottom of the AABB. Assumes a downward-aligned Y axis, so this value will be larger than Top.
		/// </summary>
		/// <value></value>
		public float Bottom { get { return Max.Y; } }

		public AABB2D(float minX, float minY, float maxX, float maxY)
		{
			Min = new Vector2(minX, minY);
			Max = new Vector2(maxX, maxY);
		}

		public AABB2D(Vector2 min, Vector2 max)
		{
			Min = min;
			Max = max;
		}

		private static Matrix3x2 AbsoluteMatrix(Matrix3x2 matrix)
		{
			return new Matrix3x2
			(
				System.Math.Abs(matrix.M11), System.Math.Abs(matrix.M12),
				System.Math.Abs(matrix.M21), System.Math.Abs(matrix.M22),
				System.Math.Abs(matrix.M31), System.Math.Abs(matrix.M32)
			);
		}

		/// <summary>
		/// Efficiently transforms the AABB by a Transform2D.
		/// </summary>
		/// <param name="aabb"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static AABB2D Transformed(AABB2D aabb, Transform2D transform)
		{
            var center = (aabb.Min + aabb.Max) / 2f;
            var extent = (aabb.Max - aabb.Min) / 2f;

            var newCenter = Vector2.Transform(center, transform.TransformMatrix);
            var newExtent = Vector2.TransformNormal(extent, AbsoluteMatrix(transform.TransformMatrix));

            return new AABB2D(newCenter - newExtent, newCenter + newExtent);
		}

		/// <summary>
		/// Creates an AABB for an arbitrary collection of positions.
		/// This is less efficient than defining a custom AABB method for most shapes, so avoid using this if possible.
		/// </summary>
		/// <param name="vertices"></param>
		/// <returns></returns>
		public static AABB2D FromVertices(IEnumerable<Vector2> vertices)
		{
			var minX = float.MaxValue;
			var minY = float.MaxValue;
			var maxX = float.MinValue;
			var maxY = float.MinValue;

			foreach (var vertex in vertices)
			{
				if (vertex.X < minX)
				{
					minX = vertex.X;
				}
				if (vertex.Y < minY)
				{
					minY = vertex.Y;
				}
				if (vertex.X > maxX)
				{
					maxX = vertex.X;
				}
				if (vertex.Y > maxY)
				{
					maxY = vertex.Y;
				}
			}

			return new AABB2D(minX, minY, maxX, maxY);
		}

		public static bool TestOverlap(AABB2D a, AABB2D b)
		{
			return a.Left <= b.Right && a.Right >= b.Left && a.Top <= b.Bottom && a.Bottom >= b.Top;
		}

		public override bool Equals(object obj)
		{
			return obj is AABB2D aabb && Equals(aabb);
		}

		public bool Equals(AABB2D other)
		{
			return Min == other.Min &&
				   Max == other.Max;
		}

		public override int GetHashCode()
		{
			return System.HashCode.Combine(Min, Max);
		}

		public static bool operator ==(AABB2D left, AABB2D right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AABB2D left, AABB2D right)
		{
			return !(left == right);
		}
	}
}
