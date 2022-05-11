using System.Collections.Generic;
using MoonWorks.Math.Float;

namespace MoonWorks.Collision.Float
{
	/// <summary>
	/// A Point is "that which has no part".
	/// All points by themselves are identical.
	/// </summary>
	public struct Point : IShape2D, System.IEquatable<Point>
	{
		public AABB2D AABB { get; }
		public IEnumerable<IShape2D> Shapes
        {
            get
            {
				yield return this;
			}
        }

		public AABB2D TransformedAABB(Transform2D transform)
		{
			return AABB2D.Transformed(AABB, transform);
		}

		public Vector2 Support(Vector2 direction, Transform2D transform)
		{
			return Vector2.Transform(Vector2.Zero, transform.TransformMatrix);
		}

		public override bool Equals(object obj)
		{
			return obj is IShape2D other && Equals(other);
		}

		public bool Equals(IShape2D other)
		{
			return other is Point otherPoint && Equals(otherPoint);
		}

		public bool Equals(Point other)
		{
			return true;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public static bool operator ==(Point a, Point b)
		{
			return true;
		}

		public static bool operator !=(Point a, Point b)
		{
			return false;
		}
	}
}
