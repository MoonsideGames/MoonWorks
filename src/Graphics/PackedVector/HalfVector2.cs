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
	public struct HalfVector2 : IPackedVector<uint>, IPackedVector, IEquatable<HalfVector2>
	{
		#region Public Properties

		public uint PackedValue
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

		private uint packedValue;

		#endregion

		#region Public Constructors

		public HalfVector2(float x, float y)
		{
			packedValue = PackHelper(x, y);
		}

		public HalfVector2(Vector2 vector)
		{
			packedValue = PackHelper(vector.X, vector.Y);
		}

		#endregion

		#region Public Methods

		public Vector2 ToVector2()
		{
			Vector2 vector;
			vector.X = HalfTypeHelper.Convert((ushort) packedValue);
			vector.Y = HalfTypeHelper.Convert((ushort) (packedValue >> 0x10));
			return vector;
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = PackHelper(vector.X, vector.Y);
		}

		Vector4 IPackedVector.ToVector4()
		{
			return new Vector4(ToVector2(), 0.0f, 1.0f);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override string ToString()
		{
			return packedValue.ToString("X");
		}

		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return ((obj is HalfVector2) && Equals((HalfVector2) obj));
		}

		public bool Equals(HalfVector2 other)
		{
			return packedValue.Equals(other.packedValue);
		}

		public static bool operator ==(HalfVector2 a, HalfVector2 b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(HalfVector2 a, HalfVector2 b)
		{
			return !a.Equals(b);
		}

		#endregion

		#region Private Static Pack Method

		private static uint PackHelper(float vectorX, float vectorY)
		{
			return (uint) (
				HalfTypeHelper.Convert(vectorX) |
				((uint) (HalfTypeHelper.Convert(vectorY) << 0x10))
			);
		}

		#endregion
	}
}
