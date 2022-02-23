#region License

/* MoonWorks - Game Development Framework
 * Copyright 2021 Evan Hemsley
 */

/* Derived from code by Ethan Lee (Copyright 2009-2021).
 * Released under the Microsoft Public License.
 * See fna.LICENSE for details.

 * Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */

#endregion

namespace MoonWorks.Math
{
	/// <summary>
	/// Defines the intersection between a <see cref="Plane"/> and a bounding volume.
	/// </summary>
	public enum PlaneIntersectionType
	{
		/// <summary>
		/// There is no intersection, the bounding volume is in the negative half space of the plane.
		/// </summary>
		Front,
		/// <summary>
		/// There is no intersection, the bounding volume is in the positive half space of the plane.
		/// </summary>
		Back,
		/// <summary>
		/// The plane is intersected.
		/// </summary>
		Intersecting
	}
}
