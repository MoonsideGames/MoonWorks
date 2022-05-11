using System.Collections.Generic;
using MoonWorks.Math.Fixed;

namespace MoonWorks.Collision.Fixed
{
	/// <summary>
	/// A rectangle is a shape defined by a width and height. The origin is the center of the rectangle.
	/// </summary>
	public struct Rectangle : IShape2D, System.IEquatable<Rectangle>
	{
		public AABB2D AABB { get; }
		public Fix64 Width { get; }
		public Fix64 Height { get; }

		public Fix64 Right { get; }
		public Fix64 Left { get; }
		public Fix64 Top { get; }
		public Fix64 Bottom { get; }
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

		public Rectangle(Fix64 left, Fix64 top, Fix64 width, Fix64 height)
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

		public Rectangle(int left, int top, int width, int height)
		{
			Width = (Fix64) width;
			Height = (Fix64) height;
			Left = (Fix64) left;
			Right = (Fix64) (left + width);
			Top = (Fix64) top;
			Bottom = (Fix64) (top + height);
			AABB = new AABB2D(Left, Top, Right, Bottom);
			TopLeft = new Vector2(Left, Top);
			BottomRight = new Vector2(Right, Bottom);
			Min = AABB.Min;
			Max = AABB.Max;
		}

		private Vector2 Support(Vector2 direction)
		{
			if (direction.X >= Fix64.Zero && direction.Y >= Fix64.Zero)
			{
				return Max;
			}
			else if (direction.X >= Fix64.Zero && direction.Y < Fix64.Zero)
			{
				return new Vector2(Max.X, Min.Y);
			}
			else if (direction.X < Fix64.Zero && direction.Y >= Fix64.Zero)
			{
				return new Vector2(Min.X, Max.Y);
			}
			else if (direction.X < Fix64.Zero && direction.Y < Fix64.Zero)
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
