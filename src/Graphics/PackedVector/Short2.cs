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
using MoonWorks.Math.Float;
#endregion

namespace MoonWorks.Graphics
{
	public struct Short2 : IPackedVector<uint>, IEquatable<Short2>
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

		public Short2(Vector2 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		public Short2(float x, float y)
		{
			packedValue = Pack(x, y);
		}

		#endregion

		#region Public Methods

		public Vector2 ToVector2()
		{
			return new Vector2(
				(short) (packedValue & 0xFFFF),
				(short) (packedValue >> 16)
			);
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		Vector4 IPackedVector.ToVector4()
		{
			return new Vector4(ToVector2(), 0.0f, 1.0f);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(Short2 a, Short2 b)
		{
			return a.packedValue != b.packedValue;
		}

		public static bool operator ==(Short2 a, Short2 b)
		{
			return a.packedValue == b.packedValue;
		}

		public override bool Equals(object obj)
		{
			return (obj is Short2) && Equals((Short2) obj);
		}

		public bool Equals(Short2 other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return packedValue.GetHashCode();
		}

		public override string ToString()
		{
			return packedValue.ToString("X");
		}

		#endregion

		#region Private Static Pack Method

		private static uint Pack(float x, float y)
		{
			return (uint) (
				((int) System.Math.Round(MathHelper.Clamp(x, -32768, 32767)) & 0x0000FFFF) |
				(((int) System.Math.Round(MathHelper.Clamp(y, -32768, 32767))) << 16)
			);
		}

		#endregion
	}
}
