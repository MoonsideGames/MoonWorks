using MoonWorks.Math;

namespace MoonWorks.Collision
{
	public interface IShape2D : ICollidable, System.IEquatable<IShape2D>
	{
		/// <summary>
		/// A Minkowski support function. Gives the farthest point on the edge of a shape along the given direction.
		/// </summary>
		/// <param name="direction">A normalized Vector2.</param>
		/// <param name="transform">A Transform for transforming the shape vertices.</param>
		/// <returns>The farthest point on the edge of the shape along the given direction.</returns>
		Vector2 Support(Vector2 direction, Transform2D transform);
	}
}
