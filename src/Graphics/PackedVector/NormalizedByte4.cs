﻿#region License

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

namespace MoonWorks.Graphics.PackedVector
{
	public struct NormalizedByte4 : IPackedVector<uint>, IEquatable<NormalizedByte4>
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

		public NormalizedByte4(Vector4 vector)
		{
			packedValue = Pack(vector.X, vector.Y, vector.Z, vector.W);
		}

		public NormalizedByte4(float x, float y, float z, float w)
		{
			packedValue = Pack(x, y, z, w);
		}

		#endregion

		#region Public Methods

		public Vector4 ToVector4()
		{
			return new Vector4(
				((sbyte) (packedValue & 0xFF)) / 127.0f,
				((sbyte) ((packedValue >> 8) & 0xFF)) / 127.0f,
				((sbyte) ((packedValue >> 16) & 0xFF)) / 127.0f,
				((sbyte) ((packedValue >> 24) & 0xFF)) / 127.0f
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

		public static bool operator !=(NormalizedByte4 a, NormalizedByte4 b)
		{
			return a.packedValue != b.packedValue;
		}

		public static bool operator ==(NormalizedByte4 a, NormalizedByte4 b)
		{
			return a.packedValue == b.packedValue;
		}

		public override bool Equals(object obj)
		{
			return (obj is NormalizedByte4) && Equals((NormalizedByte4) obj);
		}

		public bool Equals(NormalizedByte4 other)
		{
			return packedValue == other.packedValue;
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

		private static uint Pack(float x, float y, float z, float w)
		{
			uint byte4 = (
				(uint) System.Math.Round(MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)
			) & 0x000000FF;
			uint byte3 = (
				(
					(uint) System.Math.Round(MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)
				) << 8
			) & 0x0000FF00;
			uint byte2 = (
				(
					(uint) System.Math.Round(MathHelper.Clamp(z, -1.0f, 1.0f) * 127.0f)
				) << 16
			) & 0x00FF0000;
			uint byte1 = (
				(
					(uint) System.Math.Round(MathHelper.Clamp(w, -1.0f, 1.0f) * 127.0f)
				) << 24
			) & 0xFF000000;

			return byte4 | byte3 | byte2 | byte1;
		}

		#endregion
	}
}
