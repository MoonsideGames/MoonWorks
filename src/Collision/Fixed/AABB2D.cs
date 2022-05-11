using System.Collections.Generic;
using MoonWorks.Math.Fixed;

namespace MoonWorks.Collision.Fixed
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

		public Fix64 Width { get { return Max.X - Min.X; } }
		public Fix64 Height { get { return Max.Y - Min.Y; } }

		public Fix64 Right { get { return Max.X; } }
		public Fix64 Left { get { return Min.X; } }

		/// <summary>
		/// The top of the AABB. Assumes a downward-aligned Y axis, so this value will be smaller than Bottom.
		/// </summary>
		/// <value></value>
		public Fix64 Top { get { return Min.Y; } }

		/// <summary>
		/// The bottom of the AABB. Assumes a downward-aligned Y axis, so this value will be larger than Top.
		/// </summary>
		/// <value></value>
		public Fix64 Bottom { get { return Max.Y; } }

		public AABB2D(Fix64 minX, Fix64 minY, Fix64 maxX, Fix64 maxY)
		{
			Min = new Vector2(minX, minY);
			Max = new Vector2(maxX, maxY);
		}

		public AABB2D(int minX, int minY, int maxX, int maxY)
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
				Fix64.Abs(matrix.M11), Fix64.Abs(matrix.M12),
				Fix64.Abs(matrix.M21), Fix64.Abs(matrix.M22),
				Fix64.Abs(matrix.M31), Fix64.Abs(matrix.M32)
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
			var two = new Fix64(2);
			var center = (aabb.Min + aabb.Max) / two;
            var extent = (aabb.Max - aabb.Min) / two;

            var newCenter = Vector2.Transform(center, transform.TransformMatrix);
            var newExtent = Vector2.TransformNormal(extent, AbsoluteMatrix(transform.TransformMatrix));

            return new AABB2D(newCenter - newExtent, newCenter + newExtent);
		}

		public AABB2D Compose(AABB2D aabb)
		{
			Fix64 left = Left;
			Fix64 top = Top;
			Fix64 right = Right;
			Fix64 bottom = Bottom;

			if (aabb.Left < left)
			{
				left = aabb.Left;
			}
			if (aabb.Right > right)
			{
				right = aabb.Right;
			}
			if (aabb.Top < top)
			{
				top = aabb.Top;
			}
			if (aabb.Bottom > bottom)
			{
				bottom = aabb.Bottom;
			}

			return new AABB2D(left, top, right, bottom);
		}

		/// <summary>
		/// Creates an AABB for an arbitrary collection of positions.
		/// This is less efficient than defining a custom AABB method for most shapes, so avoid using this if possible.
		/// </summary>
		/// <param name="vertices"></param>
		/// <returns></returns>
		public static AABB2D FromVertices(IEnumerable<Vector2> vertices)
		{
			var minX = Fix64.MaxValue;
			var minY = Fix64.MaxValue;
			var maxX = Fix64.MinValue;
			var maxY = Fix64.MinValue;

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
			return a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
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
