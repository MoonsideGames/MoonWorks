using MoonWorks.Math;

namespace MoonWorks.Collision
{
	/// <summary>
	/// A Minkowski difference between two shapes.
	/// </summary>
	public struct MinkowskiDifference : System.IEquatable<MinkowskiDifference>
	{
		private IShape2D ShapeA { get; }
		private Transform2D TransformA { get; }
		private IShape2D ShapeB { get; }
		private Transform2D TransformB { get; }

		public MinkowskiDifference(IShape2D shapeA, Transform2D transformA, IShape2D shapeB, Transform2D transformB)
		{
			ShapeA = shapeA;
			TransformA = transformA;
			ShapeB = shapeB;
			TransformB = transformB;
		}

		public Vector2 Support(Vector2 direction)
		{
			return ShapeA.Support(direction, TransformA) - ShapeB.Support(-direction, TransformB);
		}

		public override bool Equals(object other)
		{
			return other is MinkowskiDifference minkowskiDifference && Equals(minkowskiDifference);
		}

		public bool Equals(MinkowskiDifference other)
		{
			return
				ShapeA == other.ShapeA &&
				TransformA == other.TransformA &&
				ShapeB == other.ShapeB &&
				TransformB == other.TransformB;
		}

		public override int GetHashCode()
		{
			return System.HashCode.Combine(ShapeA, TransformA, ShapeB, TransformB);
		}

		public static bool operator ==(MinkowskiDifference a, MinkowskiDifference b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(MinkowskiDifference a, MinkowskiDifference b)
		{
			return !(a == b);
		}
	}
}
