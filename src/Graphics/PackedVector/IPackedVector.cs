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

using MoonWorks.Math;

namespace MoonWorks.Graphics
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.packedvector.ipackedvector.aspx
	public interface IPackedVector
	{
		void PackFromVector4(Vector4 vector);

		Vector4 ToVector4();
	}

	// PackedVector Generic interface
	// http://msdn.microsoft.com/en-us/library/bb197661.aspx
	public interface IPackedVector<TPacked> : IPackedVector
	{
		TPacked PackedValue
		{
			get;
			set;
		}
	}
}
