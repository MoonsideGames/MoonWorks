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
	public struct NormalizedShort2 : IPackedVector<uint>, IEquatable<NormalizedShort2>
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

		public NormalizedShort2(Vector2 vector)
		{
			packedValue = Pack(vector.X, vector.Y);
		}

		public NormalizedShort2(float x, float y)
		{
			packedValue = Pack(x, y);
		}

		#endregion

		#region Public Methods

		public Vector2 ToVector2()
		{
			const float maxVal = 0x7FFF;

			return new Vector2(
				(short) (packedValue & 0xFFFF) / maxVal,
				(short) (packedValue >> 0x10) / maxVal
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

		public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b)
		{
			return a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return (obj is NormalizedShort2) && Equals((NormalizedShort2) obj);
		}

		public bool Equals(NormalizedShort2 other)
		{
			return packedValue.Equals(other.packedValue);
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
			const float max = 0x7FFF;
			const float min = -max;

			uint word2 = (uint) (
				(int) MathHelper.Clamp(
					(float) System.Math.Round(x * max),
					min,
					max
				) & 0xFFFF
			);
			uint word1 = (uint) ((
				(int) MathHelper.Clamp(
					(float) System.Math.Round(y * max),
					min,
					max
				) & 0xFFFF
			) << 0x10);

			return (word2 | word1);
		}

		#endregion
	}
}
