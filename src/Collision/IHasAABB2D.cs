using MoonWorks.Math;

namespace MoonWorks.Collision
{
	public interface IHasAABB2D
	{
		AABB2D AABB { get; }

		/// <summary>
		/// Returns a bounding box based on the shape.
		/// </summary>
		/// <param name="transform">A Transform for transforming the shape vertices.</param>
		/// <returns>Returns a bounding box based on the shape.</returns>
		AABB2D TransformedAABB(Transform2D transform);
	}
}
