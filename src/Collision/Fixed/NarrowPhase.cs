using MoonWorks.Math.Fixed;

namespace MoonWorks.Collision.Fixed
{
	public static class NarrowPhase
	{
		private struct Edge
		{
			public Fix64 Distance;
			public Vector2 Normal;
			public int Index;
		}

        public static bool TestCollision(ICollidable collidableA, Transform2D transformA, ICollidable collidableB, Transform2D transformB)
        {
            foreach (var shapeA in collidableA.Shapes)
            {
                foreach (var shapeB in collidableB.Shapes)
                {
					if (TestCollision(shapeA, transformA, shapeB, transformB))
                    {
						return true;
					}
				}
            }

			return false;
		}

		public static bool TestCollision(IShape2D shapeA, Transform2D transformA, IShape2D shapeB, Transform2D transformB)
		{
			// If we can use a fast path check, let's do that!
			if (shapeA is Rectangle rectangleA && shapeB is Rectangle rectangleB && transformA.IsAxisAligned && transformB.IsAxisAligned)
			{
				return TestRectangleOverlap(rectangleA, transformA, rectangleB, transformB);
			}
			else if (shapeA is Point && shapeB is Rectangle && transformB.IsAxisAligned)
			{
				return TestPointRectangleOverlap((Point) shapeA, transformA, (Rectangle) shapeB, transformB);
			}
			else if (shapeA is Rectangle && shapeB is Point && transformA.IsAxisAligned)
			{
				return TestPointRectangleOverlap((Point) shapeB, transformB, (Rectangle) shapeA, transformA);
			}
			else if (shapeA is Rectangle && shapeB is Circle && transformA.IsAxisAligned && transformB.IsUniformScale)
			{
				return TestCircleRectangleOverlap((Circle) shapeB, transformB, (Rectangle) shapeA, transformA);
			}
			else if (shapeA is Circle && shapeB is Rectangle && transformA.IsUniformScale && transformB.IsAxisAligned)
			{
				return TestCircleRectangleOverlap((Circle) shapeA, transformA, (Rectangle) shapeB, transformB);
			}
			else if (shapeA is Circle && shapeB is Point && transformA.IsUniformScale)
			{
				return TestCirclePointOverlap((Circle) shapeA, transformA, (Point) shapeB, transformB);
			}
			else if (shapeA is Point && shapeB is Circle && transformB.IsUniformScale)
			{
				return TestCirclePointOverlap((Circle) shapeB, transformB, (Point) shapeA, transformA);
			}
			else if (shapeA is Circle circleA && shapeB is Circle circleB && transformA.IsUniformScale && transformB.IsUniformScale)
			{
				return TestCircleOverlap(circleA, transformA, circleB, transformB);
			}

			// Sad, we can't do a fast path optimization. Time for a simplex reduction.
			return FindCollisionSimplex(shapeA, transformA, shapeB, transformB).Item1;
		}

		public static bool TestRectangleOverlap(Rectangle rectangleA, Transform2D transformA, Rectangle rectangleB, Transform2D transformB)
		{
			var firstAABB = rectangleA.TransformedAABB(transformA);
			var secondAABB = rectangleB.TransformedAABB(transformB);

			return firstAABB.Left < secondAABB.Right && firstAABB.Right > secondAABB.Left && firstAABB.Top < secondAABB.Bottom && firstAABB.Bottom > secondAABB.Top;
		}

		public static bool TestPointRectangleOverlap(Point point, Transform2D pointTransform, Rectangle rectangle, Transform2D rectangleTransform)
		{
			var transformedPoint = pointTransform.Position;
			var AABB = rectangle.TransformedAABB(rectangleTransform);

			return transformedPoint.X > AABB.Left && transformedPoint.X < AABB.Right && transformedPoint.Y < AABB.Bottom && transformedPoint.Y > AABB.Top;
		}

		public static bool TestCirclePointOverlap(Circle circle, Transform2D circleTransform, Point point, Transform2D pointTransform)
		{
			var circleCenter = circleTransform.Position;
			var circleRadius = circle.Radius * circleTransform.Scale.X;

			var distanceX = circleCenter.X - pointTransform.Position.X;
			var distanceY = circleCenter.Y - pointTransform.Position.Y;

			return (distanceX * distanceX) + (distanceY * distanceY) < (circleRadius * circleRadius);
		}

		/// <summary>
		/// NOTE: The rectangle must be axis aligned, and the scaling of the circle must be uniform.
		/// </summary>
		public static bool TestCircleRectangleOverlap(Circle circle, Transform2D circleTransform, Rectangle rectangle, Transform2D rectangleTransform)
		{
			var circleCenter = circleTransform.Position;
			var circleRadius = circle.Radius * circleTransform.Scale.X;
			var AABB = rectangle.TransformedAABB(rectangleTransform);

			var closestX = Fix64.Clamp(circleCenter.X, AABB.Left, AABB.Right);
			var closestY = Fix64.Clamp(circleCenter.Y, AABB.Top, AABB.Bottom);

			var distanceX = circleCenter.X - closestX;
			var distanceY = circleCenter.Y - closestY;

			var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
			return distanceSquared < (circleRadius * circleRadius);
		}

		public static bool TestCircleOverlap(Circle circleA, Transform2D transformA, Circle circleB, Transform2D transformB)
		{
			var radiusA = circleA.Radius * transformA.Scale.X;
			var radiusB = circleB.Radius * transformB.Scale.Y;

			var centerA = transformA.Position;
			var centerB = transformB.Position;

			var distanceSquared = (centerA - centerB).LengthSquared();
			var radiusSumSquared = (radiusA + radiusB) * (radiusA + radiusB);

			return distanceSquared < radiusSumSquared;
		}

		public static (bool, Simplex2D) FindCollisionSimplex(IShape2D shapeA, Transform2D transformA, IShape2D shapeB, Transform2D transformB)
		{
			var minkowskiDifference = new MinkowskiDifference(shapeA, transformA, shapeB, transformB);
			var c = minkowskiDifference.Support(Vector2.UnitX);
			var b = minkowskiDifference.Support(-Vector2.UnitX);
			return Check(minkowskiDifference, c, b);
		}

        public unsafe static Vector2 Intersect(IShape2D shapeA, Transform2D Transform2DA, IShape2D shapeB, Transform2D Transform2DB, Simplex2D simplex)
        {
            if (shapeA == null) { throw new System.ArgumentNullException(nameof(shapeA)); }
            if (shapeB == null) { throw new System.ArgumentNullException(nameof(shapeB)); }
            if (!simplex.TwoSimplex) { throw new System.ArgumentException("Simplex must be a 2-Simplex.", nameof(simplex)); }

			var epsilon = Fix64.FromFraction(1, 10000);

			var a = simplex.A;
            var b = simplex.B.Value;
            var c = simplex.C.Value;

            Vector2 intersection = default;

            for (var i = 0; i < 32; i++)
            {
                var edge = FindClosestEdge(simplex);
                var support = CalculateSupport(shapeA, Transform2DA, shapeB, Transform2DB, edge.Normal);
                var distance = Vector2.Dot(support, edge.Normal);

                intersection = edge.Normal;
                intersection *= distance;

                if (Fix64.Abs(distance - edge.Distance) <= epsilon)
                {
                    return intersection;
                }
                else
                {
					simplex.Insert(support, edge.Index);
                }
            }

            return intersection; // close enough
        }

        private static unsafe Edge FindClosestEdge(Simplex2D simplex)
        {
			var closestDistance = Fix64.MaxValue;
            var closestNormal = Vector2.Zero;
            var closestIndex = 0;

			for (var i = 0; i < 4; i += 1)
			{
				var j = (i + 1 == 3) ? 0 : i + 1;

				var a = simplex[i];
				var b = simplex[j];

				var e = b - a;

				var oa = a;

				var n = Vector2.Normalize(TripleProduct(e, oa, e));

				var d = Vector2.Dot(n, a);

				if (d < closestDistance)
				{
					closestDistance = d;
					closestNormal = n;
					closestIndex = j;
				}
			}

            return new Edge
			{
				Distance = closestDistance,
				Normal = closestNormal,
				Index = closestIndex
			};
        }

        private static Vector2 CalculateSupport(IShape2D shapeA, Transform2D Transform2DA, IShape2D shapeB, Transform2D Transform2DB, Vector2 direction)
        {
            return shapeA.Support(direction, Transform2DA) - shapeB.Support(-direction, Transform2DB);
        }

		private static (bool, Simplex2D) Check(MinkowskiDifference minkowskiDifference, Vector2 c, Vector2 b)
        {
            var cb = c - b;
            var c0 = -c;
            var d = Direction(cb, c0);
            return DoSimplex(minkowskiDifference, new Simplex2D(b, c), d);
        }

        private static (bool, Simplex2D) DoSimplex(MinkowskiDifference minkowskiDifference, Simplex2D simplex, Vector2 direction)
        {
            var a = minkowskiDifference.Support(direction);
            var notPastOrigin = Vector2.Dot(a, direction) < Fix64.Zero;
            var (intersects, newSimplex, newDirection) = EnclosesOrigin(a, simplex);

            if (notPastOrigin)
            {
                return (false, default(Simplex2D));
            }
            else if (intersects)
            {
                return (true, new Simplex2D(simplex.A, simplex.B.Value, a));
            }
            else
            {
                return DoSimplex(minkowskiDifference, newSimplex, newDirection);
            }
        }

        private static (bool, Simplex2D, Vector2) EnclosesOrigin(Vector2 a, Simplex2D simplex)
        {
            if (simplex.ZeroSimplex)
            {
                return HandleZeroSimplex(a, simplex.A);
            }
            else if (simplex.OneSimplex)
            {
                return HandleOneSimplex(a, simplex.A, simplex.B.Value);
            }
            else
            {
                return (false, simplex, Vector2.Zero);
            }
        }

        private static (bool, Simplex2D, Vector2) HandleZeroSimplex(Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var a0 = -a;
            var (newSimplex, newDirection) = SameDirection(ab, a0) ? (new Simplex2D(a, b), Perpendicular(ab, a0)) : (new Simplex2D(a), a0);
            return (false, newSimplex, newDirection);
        }

        private static (bool, Simplex2D, Vector2) HandleOneSimplex(Vector2 a, Vector2 b, Vector2 c)
        {
            var a0 = -a;
            var ab = b - a;
            var ac = c - a;
            var abp = Perpendicular(ab, -ac);
            var acp = Perpendicular(ac, -ab);

            if (SameDirection(abp, a0))
            {
                if (SameDirection(ab, a0))
                {
                    return (false, new Simplex2D(a, b), abp);
                }
                else
                {
                    return (false, new Simplex2D(a), a0);
                }
            }
            else if (SameDirection(acp, a0))
            {
                if (SameDirection(ac, a0))
                {
                    return (false, new Simplex2D(a, c), acp);
                }
                else
                {
                    return (false, new Simplex2D(a), a0);
                }
            }
            else
            {
                return (true, new Simplex2D(b, c), a0);
            }
        }

        private static Vector2 TripleProduct(Vector2 a, Vector2 b, Vector2 c)
        {
            var A = new Vector3(a.X, a.Y, Fix64.Zero);
            var B = new Vector3(b.X, b.Y, Fix64.Zero);
            var C = new Vector3(c.X, c.Y, Fix64.Zero);

            var first = Vector3.Cross(A, B);
            var second = Vector3.Cross(first, C);

            return new Vector2(second.X, second.Y);
        }

        private static Vector2 Direction(Vector2 a, Vector2 b)
        {
            var d = TripleProduct(a, b, a);
            var collinear = d == Vector2.Zero;
            return collinear ? new Vector2(a.Y, -a.X) : d;
        }

        private static bool SameDirection(Vector2 a, Vector2 b)
        {
            return Vector2.Dot(a, b) > Fix64.Zero;
        }

        private static Vector2 Perpendicular(Vector2 a, Vector2 b)
        {
            return TripleProduct(a, b, a);
        }
	}
}
