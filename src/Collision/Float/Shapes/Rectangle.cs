using System.Collections.Generic;
using MoonWorks.Math.Float;

namespace MoonWorks.Collision.Float
{
	/// <summary>
	/// A rectangle is a shape defined by a width and height. The origin is the center of the rectangle.
	/// </summary>
	public struct Rectangle : IShape2D, System.IEquatable<Rectangle>
	{
		public AABB2D AABB { get; }
		public float Width { get; }
		public float Height { get; }

		public float Right { get; }
		public float Left { get; }
		public float Top { get; }
		public float Bottom { get; }
		public Vector2 TopLeft { get; }
		public Vector2 BottomRight { get; }

		public Vector2 Min { get; }
		public Vector2 Max { get; }

		public IEnumerable<IShape2D> Shapes
        {
            get
            {
				yield return this;
			}
        }

		public Rectangle(float left, float top, float width, float height)
		{
			Width = width;
			Height = height;
			Left = left;
			Right = left + width;
			Top = top;
			Bottom = top + height;
			AABB = new AABB2D(left, top, Right, Bottom);
			TopLeft = new Vector2(Left, Top);
			BottomRight = new Vector2(Right, Bottom);
			Min = AABB.Min;
			Max = AABB.Max;
		}

		private Vector2 Support(Vector2 direction)
		{
			if (direction.X >= 0 && direction.Y >= 0)
			{
				return Max;
			}
			else if (direction.X >= 0 && direction.Y < 0)
			{
				return new Vector2(Max.X, Min.Y);
			}
			else if (direction.X < 0 && direction.Y >= 0)
			{
				return new Vector2(Min.X, Max.Y);
			}
			else if (direction.X < 0 && direction.Y < 0)
			{
				return new Vector2(Min.X, Min.Y);
			}
			else
			{
				throw new System.ArgumentException("Support vector direction cannot be zero.");
			}
		}

		public Vector2 Support(Vector2 direction, Transform2D transform)
		{
			Matrix3x2 inverseTransform;
			Matrix3x2.Invert(transform.TransformMatrix, out inverseTransform);
			var inverseDirection = Vector2.TransformNormal(direction, inverseTransform);
			return Vector2.Transform(Support(inverseDirection), transform.TransformMatrix);
		}

		public AABB2D TransformedAABB(Transform2D transform)
		{
			return AABB2D.Transformed(AABB, transform);
		}

		public override bool Equals(object obj)
		{
			return obj is IShape2D other && Equals(other);
		}

		public bool Equals(IShape2D other)
		{
			return (other is Rectangle rectangle && Equals(rectangle));
		}

		public bool Equals(Rectangle other)
		{
			return Min == other.Min && Max == other.Max;
		}

		public override int GetHashCode()
		{
			return System.HashCode.Combine(Min, Max);
		}

		public static bool operator ==(Rectangle a, Rectangle b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Rectangle a, Rectangle b)
		{
			return !(a == b);
		}
	}
}
