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

#region Using Statements
using System;
using MoonWorks.Math;
#endregion

namespace MoonWorks.Graphics
{
	public struct HalfSingle : IPackedVector<ushort>, IEquatable<HalfSingle>, IPackedVector
	{
		#region Public Properties

		public ushort PackedValue
		{
			get
			{
				return packedValue;
			}
			set
			{
				packedValue = value;
			}
		}

		#endregion

		#region Private Variables

		private ushort packedValue;

		#endregion

		#region Public Constructors

		public HalfSingle(float single)
		{
			packedValue = HalfTypeHelper.Convert(single);
		}

		#endregion

		#region Public Methods

		public float ToSingle()
		{
			return HalfTypeHelper.Convert(packedValue);
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = HalfTypeHelper.Convert(vector.X);
		}

		Vector4 IPackedVector.ToVector4()
		{
			return new Vector4(ToSingle(), 0.0f, 0.0f, 1.0f);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override bool Equals(object obj)
		{
			return (obj is HalfSingle) && Equals((HalfSingle) obj);
		}

		public bool Equals(HalfSingle other)
		{
			return packedValue == other.packedValue;
		}

		public override string ToString()
		{
			return packedValue.ToString("X");
		}

		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public static bool operator ==(HalfSingle lhs, HalfSingle rhs)
		{
			return lhs.packedValue == rhs.packedValue;
		}

		public static bool operator !=(HalfSingle lhs, HalfSingle rhs)
		{
			return lhs.packedValue != rhs.packedValue;
		}

		#endregion
	}
}
