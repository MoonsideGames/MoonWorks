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
	/// Defines how the bounding volumes intersects or contain one another.
	/// </summary>
	public enum ContainmentType
	{
		/// <summary>
		/// Indicates that there is no overlap between two bounding volumes.
		/// </summary>
		Disjoint,
		/// <summary>
		/// Indicates that one bounding volume completely contains another volume.
		/// </summary>
		Contains,
		/// <summary>
		/// Indicates that bounding volumes partially overlap one another.
		/// </summary>
		Intersects
	}
}
