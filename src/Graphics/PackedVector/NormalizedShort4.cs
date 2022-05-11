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
	public struct NormalizedShort4 : IPackedVector<ulong>, IEquatable<NormalizedShort4>
	{
		#region Public Properties

		public ulong PackedValue
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

		private ulong packedValue;

		#endregion

		#region Public Constructors

		public NormalizedShort4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		public NormalizedShort4(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

		#endregion

		#region Public Methods

		public Vector4 ToVector4()
		{
			const float maxVal = 0x7FFF;

			return new Vector4(
				((short) (packedValue & 0xFFFF)) / maxVal,
				((short) ((packedValue >> 0x10) & 0xFFFF)) / maxVal,
				((short) ((packedValue >> 0x20) & 0xFFFF)) / maxVal,
				((short) ((packedValue >> 0x30) & 0xFFFF)) / maxVal
			);
		}

		#endregion

		#region IPackedVector Methods

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		#endregion

		#region Public Static Operators and Override Methods

		public static bool operator !=(NormalizedShort4 a, NormalizedShort4 b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(NormalizedShort4 a, NormalizedShort4 b)
		{
			return a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return (obj is NormalizedShort4) && Equals((NormalizedShort4) obj);
		}

		public bool Equals(NormalizedShort4 other)
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

		private static ulong Pack(float x, float y, float z, float w)
		{
			const float max = 0x7FFF;
			const float min = -max;

			ulong word4 = (
				(ulong) MathHelper.Clamp(
					(float) System.Math.Round(x * max),
					min,
					max
				) & 0xFFFF
			);
			ulong word3 = (
				(ulong) MathHelper.Clamp(
					(float) System.Math.Round(y * max),
					min,
					max
				) & 0xFFFF
			) << 0x10;
			ulong word2 = (
				(ulong) MathHelper.Clamp(
					(float) System.Math.Round(z * max),
					min,
					max
				) & 0xFFFF
			) << 0x20;
			ulong word1 = (
				(ulong) MathHelper.Clamp(
					(float) System.Math.Round(w * max),
					min,
					max
				) & 0xFFFF
			) << 0x30;

			return (word4 | word3 | word2 | word1);
		}

		#endregion
	}
}
