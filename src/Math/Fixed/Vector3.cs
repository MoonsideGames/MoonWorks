#region License

/* MoonWorks - Game Development Framework
 * Copyright 2022 Evan Hemsley
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace MoonWorks.Math.Fixed
{
	/// <summary>
	/// Describes a fixed point 3D-vector.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	[StructLayout(LayoutKind.Explicit)]
	public struct Vector3 : IEquatable<Vector3>
	{
		#region Public Static Properties

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, 0, 0.
		/// </summary>
		public static Vector3 Zero
		{
			get
			{
				return zero;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 1, 1, 1.
		/// </summary>
		public static Vector3 One
		{
			get
			{
				return one;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 1, 0, 0.
		/// </summary>
		public static Vector3 UnitX
		{
			get
			{
				return unitX;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, 1, 0.
		/// </summary>
		public static Vector3 UnitY
		{
			get
			{
				return unitY;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, 0, 1.
		/// </summary>
		public static Vector3 UnitZ
		{
			get
			{
				return unitZ;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, 1, 0.
		/// </summary>
		public static Vector3 Up
		{
			get
			{
				return up;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, -1, 0.
		/// </summary>
		public static Vector3 Down
		{
			get
			{
				return down;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 1, 0, 0.
		/// </summary>
		public static Vector3 Right
		{
			get
			{
				return right;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components -1, 0, 0.
		/// </summary>
		public static Vector3 Left
		{
			get
			{
				return left;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, 0, -1.
		/// </summary>
		public static Vector3 Forward
		{
			get
			{
				return forward;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector3"/> with components 0, 0, 1.
		/// </summary>
		public static Vector3 Backward
		{
			get
			{
				return backward;
			}
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					X.ToString(), " ",
					Y.ToString(), " ",
					Z.ToString()
				);
			}
		}

		#endregion

		#region Private Static Fields

		private static Vector3 zero = new Vector3(0, 0, 0); // Not readonly for performance -flibit
		private static readonly Vector3 one = new Vector3(1, 1, 1);
		private static readonly Vector3 unitX = new Vector3(1, 0, 0);
		private static readonly Vector3 unitY = new Vector3(0, 1, 0);
		private static readonly Vector3 unitZ = new Vector3(0, 0, 1);
		private static readonly Vector3 up = new Vector3(0, 1, 0);
		private static readonly Vector3 down = new Vector3(0, -1, 0);
		private static readonly Vector3 right = new Vector3(1, 0, 0);
		private static readonly Vector3 left = new Vector3(-1, 0, 0);
		private static readonly Vector3 forward = new Vector3(0, 0, -1);
		private static readonly Vector3 backward = new Vector3(0, 0, 1);

		#endregion

		#region Public Fields

		/// <summary>
		/// The x coordinate of this <see cref="Vector3"/>.
		/// </summary>
		[FieldOffset(0)]
		public Fix64 X;

		/// <summary>
		/// The y coordinate of this <see cref="Vector3"/>.
		/// </summary>
		[FieldOffset(8)]
		public Fix64 Y;

		/// <summary>
		/// The z coordinate of this <see cref="Vector3"/>.
		/// </summary>
		[FieldOffset(16)]
		public Fix64 Z;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Constructs a 3d vector with X, Y and Z from three values.
		/// </summary>
		/// <param name="x">The x coordinate in 3d-space.</param>
		/// <param name="y">The y coordinate in 3d-space.</param>
		/// <param name="z">The z coordinate in 3d-space.</param>
		public Vector3(Fix64 x, Fix64 y, Fix64 z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

        public Vector3(int x, int y, int z)
        {
			this.X = new Fix64(x);
			this.Y = new Fix64(y);
			this.Z = new Fix64(z);
		}

		/// <summary>
		/// Constructs a 3d vector with X, Y and Z set to the same value.
		/// </summary>
		/// <param name="value">The x, y and z coordinates in 3d-space.</param>
		public Vector3(Fix64 value)
		{
			this.X = value;
			this.Y = value;
			this.Z = value;
		}

		/// <summary>
		/// Constructs a 3d vector with X, Y from <see cref="Vector2"/> and Z from a scalar.
		/// </summary>
		/// <param name="value">The x and y coordinates in 3d-space.</param>
		/// <param name="z">The z coordinate in 3d-space.</param>
		public Vector3(Vector2 value, Fix64 z)
		{
			this.X = value.X;
			this.Y = value.Y;
			this.Z = z;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return (obj is Vector3) && Equals((Vector3) obj);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Vector3"/>.
		/// </summary>
		/// <param name="other">The <see cref="Vector3"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Vector3 other)
		{
			return (X == other.X &&
					Y == other.Y &&
					Z == other.Z);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Vector3"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Vector3"/>.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
		}

		/// <summary>
		/// Returns the length of this <see cref="Vector3"/>.
		/// </summary>
		/// <returns>The length of this <see cref="Vector3"/>.</returns>
		public Fix64 Length()
		{
			return Fix64.Sqrt((X * X) + (Y * Y) + (Z * Z));
		}

		/// <summary>
		/// Returns the squared length of this <see cref="Vector3"/>.
		/// </summary>
		/// <returns>The squared length of this <see cref="Vector3"/>.</returns>
		public Fix64 LengthSquared()
		{
			return (X * X) + (Y * Y) + (Z * Z);
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Vector3"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Z:[<see cref="Z"/>]}
		/// </summary>
		/// <returns>A <see cref="String"/> representation of this <see cref="Vector3"/>.</returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(32);
			sb.Append("{X:");
			sb.Append(this.X);
			sb.Append(" Y:");
			sb.Append(this.Y);
			sb.Append(" Z:");
			sb.Append(this.Z);
			sb.Append("}");
			return sb.ToString();
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
		/// </summary>
		/// <param name="value1">The first vector to add.</param>
		/// <param name="value2">The second vector to add.</param>
		/// <returns>The result of the vector addition.</returns>
		public static Vector3 Add(Vector3 value1, Vector3 value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			value1.Z += value2.Z;
			return value1;
		}

		/// <summary>
		/// Performs vector addition on <paramref name="value1"/> and
		/// <paramref name="value2"/>, storing the result of the
		/// addition in <paramref name="result"/>.
		/// </summary>
		/// <param name="value1">The first vector to add.</param>
		/// <param name="value2">The second vector to add.</param>
		/// <param name="result">The result of the vector addition.</param>
		public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
		{
			result.X = value1.X + value2.X;
			result.Y = value1.Y + value2.Y;
			result.Z = value1.Z + value2.Z;
		}

		/// <summary>
		/// Clamps the specified value within a range.
		/// </summary>
		/// <param name="value1">The value to clamp.</param>
		/// <param name="min">The min value.</param>
		/// <param name="max">The max value.</param>
		/// <returns>The clamped value.</returns>
		public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
		{
			return new Vector3(
				Fix64.Clamp(value1.X, min.X, max.X),
				Fix64.Clamp(value1.Y, min.Y, max.Y),
				Fix64.Clamp(value1.Z, min.Z, max.Z)
			);
		}

		/// <summary>
		/// Clamps the specified value within a range.
		/// </summary>
		/// <param name="value1">The value to clamp.</param>
		/// <param name="min">The min value.</param>
		/// <param name="max">The max value.</param>
		/// <param name="result">The clamped value as an output parameter.</param>
		public static void Clamp(
			ref Vector3 value1,
			ref Vector3 min,
			ref Vector3 max,
			out Vector3 result
		)
		{
			result.X = Fix64.Clamp(value1.X, min.X, max.X);
			result.Y = Fix64.Clamp(value1.Y, min.Y, max.Y);
			result.Z = Fix64.Clamp(value1.Z, min.Z, max.Z);
		}

		/// <summary>
		/// Clamps the magnitude of the specified vector.
		/// </summary>
		/// <param name="value">The vector to clamp.</param>
		/// <param name="maxLength">The maximum length of the vector.</param>
		/// <returns></returns>
		public static Vector3 ClampMagnitude(
			Vector3 value,
			Fix64 maxLength
		)
		{
			return (value.LengthSquared() > maxLength * maxLength) ? (Vector3.Normalize(value) * maxLength) : value;
		}

		/// <summary>
		/// Computes the cross product of two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>The cross product of two vectors.</returns>
		public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
		{
			Cross(ref vector1, ref vector2, out vector1);
			return vector1;
		}

		/// <summary>
		/// Computes the cross product of two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <param name="result">The cross product of two vectors as an output parameter.</param>
		public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
		{
			Fix64 x = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
			Fix64 y = -(vector1.X * vector2.Z - vector2.X * vector1.Z);
			Fix64 z = vector1.X * vector2.Y - vector2.X * vector1.Y;
			result.X = x;
			result.Y = y;
			result.Z = z;
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The distance between two vectors.</returns>
		public static Fix64 Distance(Vector3 vector1, Vector3 vector2)
		{
			Fix64 result;
			DistanceSquared(ref vector1, ref vector2, out result);
			return Fix64.Sqrt(result);
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The distance between two vectors as an output parameter.</param>
		public static void Distance(ref Vector3 value1, ref Vector3 value2, out Fix64 result)
		{
			DistanceSquared(ref value1, ref value2, out result);
			result = Fix64.Sqrt(result);
		}

		/// <summary>
		/// Returns the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The squared distance between two vectors.</returns>
		public static Fix64 DistanceSquared(Vector3 value1, Vector3 value2)
		{
			return (
				(value1.X - value2.X) * (value1.X - value2.X) +
				(value1.Y - value2.Y) * (value1.Y - value2.Y) +
				(value1.Z - value2.Z) * (value1.Z - value2.Z)
			);
		}

		/// <summary>
		/// Returns the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The squared distance between two vectors as an output parameter.</param>
		public static void DistanceSquared(
			ref Vector3 value1,
			ref Vector3 value2,
			out Fix64 result
		)
		{
			result = (
				(value1.X - value2.X) * (value1.X - value2.X) +
				(value1.Y - value2.Y) * (value1.Y - value2.Y) +
				(value1.Z - value2.Z) * (value1.Z - value2.Z)
			);
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector3"/> by the components of another <see cref="Vector3"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Divisor <see cref="Vector3"/>.</param>
		/// <returns>The result of dividing the vectors.</returns>
		public static Vector3 Divide(Vector3 value1, Vector3 value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			value1.Z /= value2.Z;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector3"/> by the components of another <see cref="Vector3"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Divisor <see cref="Vector3"/>.</param>
		/// <param name="result">The result of dividing the vectors as an output parameter.</param>
		public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
		{
			result.X = value1.X / value2.X;
			result.Y = value1.Y / value2.Y;
			result.Z = value1.Z / value2.Z;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector3"/> by a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Divisor scalar.</param>
		/// <returns>The result of dividing a vector by a scalar.</returns>
		public static Vector3 Divide(Vector3 value1, Fix64 value2)
		{
			Fix64 factor = Fix64.One / value2;
			value1.X *= factor;
			value1.Y *= factor;
			value1.Z *= factor;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector3"/> by a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Divisor scalar.</param>
		/// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
		public static void Divide(ref Vector3 value1, Fix64 value2, out Vector3 result)
		{
			Fix64 factor = Fix64.One / value2;
			result.X = value1.X * factor;
			result.Y = value1.Y * factor;
			result.Z = value1.Z * factor;
		}

		/// <summary>
		/// Returns a dot product of two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>The dot product of two vectors.</returns>
		public static Fix64 Dot(Vector3 vector1, Vector3 vector2)
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
		}

		/// <summary>
		/// Returns a dot product of two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <param name="result">The dot product of two vectors as an output parameter.</param>
		public static void Dot(ref Vector3 vector1, ref Vector3 vector2, out Fix64 result)
		{
			result = (
				(vector1.X * vector2.X) +
				(vector1.Y * vector2.Y) +
				(vector1.Z * vector2.Z)
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a maximal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The <see cref="Vector3"/> with maximal values from the two vectors.</returns>
		public static Vector3 Max(Vector3 value1, Vector3 value2)
		{
			return new Vector3(
				Fix64.Max(value1.X, value2.X),
				Fix64.Max(value1.Y, value2.Y),
				Fix64.Max(value1.Z, value2.Z)
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a maximal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The <see cref="Vector3"/> with maximal values from the two vectors as an output parameter.</param>
		public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
		{
			result.X = Fix64.Max(value1.X, value2.X);
			result.Y = Fix64.Max(value1.Y, value2.Y);
			result.Z = Fix64.Max(value1.Z, value2.Z);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a minimal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The <see cref="Vector3"/> with minimal values from the two vectors.</returns>
		public static Vector3 Min(Vector3 value1, Vector3 value2)
		{
			return new Vector3(
				Fix64.Min(value1.X, value2.X),
				Fix64.Min(value1.Y, value2.Y),
				Fix64.Min(value1.Z, value2.Z)
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a minimal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The <see cref="Vector3"/> with minimal values from the two vectors as an output parameter.</param>
		public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
		{
			result.X = Fix64.Min(value1.X, value2.X);
			result.Y = Fix64.Min(value1.Y, value2.Y);
			result.Z = Fix64.Min(value1.Z, value2.Z);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a multiplication of two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Source <see cref="Vector3"/>.</param>
		/// <returns>The result of the vector multiplication.</returns>
		public static Vector3 Multiply(Vector3 value1, Vector3 value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			value1.Z *= value2.Z;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a multiplication of <see cref="Vector3"/> and a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>The result of the vector multiplication with a scalar.</returns>
		public static Vector3 Multiply(Vector3 value1, Fix64 scaleFactor)
		{
			value1.X *= scaleFactor;
			value1.Y *= scaleFactor;
			value1.Z *= scaleFactor;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a multiplication of <see cref="Vector3"/> and a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
		public static void Multiply(ref Vector3 value1, Fix64 scaleFactor, out Vector3 result)
		{
			result.X = value1.X * scaleFactor;
			result.Y = value1.Y * scaleFactor;
			result.Z = value1.Z * scaleFactor;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a multiplication of two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Source <see cref="Vector3"/>.</param>
		/// <param name="result">The result of the vector multiplication as an output parameter.</param>
		public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
		{
			result.X = value1.X * value2.X;
			result.Y = value1.Y * value2.Y;
			result.Z = value1.Z * value2.Z;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains the specified vector inversion.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <returns>The result of the vector inversion.</returns>
		public static Vector3 Negate(Vector3 value)
		{
			value = new Vector3(-value.X, -value.Y, -value.Z);
			return value;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains the specified vector inversion.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <param name="result">The result of the vector inversion as an output parameter.</param>
		public static void Negate(ref Vector3 value, out Vector3 result)
		{
			result.X = -value.X;
			result.Y = -value.Y;
			result.Z = -value.Z;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains a normalized values from another vector.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/>.</param>
		/// <returns>Unit vector.</returns>
		public static Vector3 Normalize(Vector3 value)
		{
			Fix64 lengthSquared = (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z);

			if (lengthSquared == Fix64.Zero)
			{
				return Zero;
			}

			Fix64 factor = Fix64.One / Fix64.Sqrt(lengthSquared);
			return new Vector3(
				value.X * factor,
				value.Y * factor,
				value.Z * factor
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains reflect vector of the given vector and normal.
		/// </summary>
		/// <param name="vector">Source <see cref="Vector3"/>.</param>
		/// <param name="normal">Reflection normal.</param>
		/// <returns>Reflected vector.</returns>
		public static Vector3 Reflect(Vector3 vector, Vector3 normal)
		{
			/* I is the original array.
			 * N is the normal of the incident plane.
			 * R = I - (2 * N * ( DotProduct[ I,N] ))
			 */
			Vector3 reflectedVector;
			Fix64 two = new Fix64(2);
			// Inline the dotProduct here instead of calling method
			Fix64 dotProduct = ((vector.X * normal.X) + (vector.Y * normal.Y)) +
						(vector.Z * normal.Z);
			reflectedVector.X = vector.X - (two * normal.X) * dotProduct;
			reflectedVector.Y = vector.Y - (two * normal.Y) * dotProduct;
			reflectedVector.Z = vector.Z - (two * normal.Z) * dotProduct;

			return reflectedVector;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains subtraction of on <see cref="Vector3"/> from a another.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Source <see cref="Vector3"/>.</param>
		/// <returns>The result of the vector subtraction.</returns>
		public static Vector3 Subtract(Vector3 value1, Vector3 value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			value1.Z -= value2.Z;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector3"/> that contains subtraction of on <see cref="Vector3"/> from a another.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/>.</param>
		/// <param name="value2">Source <see cref="Vector3"/>.</param>
		/// <param name="result">The result of the vector subtraction as an output parameter.</param>
		public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
		{
			result.X = value1.X - value2.X;
			result.Y = value1.Y - value2.Y;
			result.Z = value1.Z - value2.Z;
		}

		#endregion

		#region Public Static Operators

		/// <summary>
		/// Compares whether two <see cref="Vector3"/> instances are equal.
		/// </summary>
		/// <param name="value1"><see cref="Vector3"/> instance on the left of the equal sign.</param>
		/// <param name="value2"><see cref="Vector3"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Vector3 value1, Vector3 value2)
		{
			return (value1.X == value2.X &&
					value1.Y == value2.Y &&
					value1.Z == value2.Z);
		}

		/// <summary>
		/// Compares whether two <see cref="Vector3"/> instances are not equal.
		/// </summary>
		/// <param name="value1"><see cref="Vector3"/> instance on the left of the not equal sign.</param>
		/// <param name="value2"><see cref="Vector3"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(Vector3 value1, Vector3 value2)
		{
			return !(value1 == value2);
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/> on the left of the add sign.</param>
		/// <param name="value2">Source <see cref="Vector3"/> on the right of the add sign.</param>
		/// <returns>Sum of the vectors.</returns>
		public static Vector3 operator +(Vector3 value1, Vector3 value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			value1.Z += value2.Z;
			return value1;
		}

		/// <summary>
		/// Inverts values in the specified <see cref="Vector3"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Vector3 operator -(Vector3 value)
		{
			value = new Vector3(-value.X, -value.Y, -value.Z);
			return value;
		}

		/// <summary>
		/// Subtracts a <see cref="Vector3"/> from a <see cref="Vector3"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/> on the left of the sub sign.</param>
		/// <param name="value2">Source <see cref="Vector3"/> on the right of the sub sign.</param>
		/// <returns>Result of the vector subtraction.</returns>
		public static Vector3 operator -(Vector3 value1, Vector3 value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			value1.Z -= value2.Z;
			return value1;
		}

		/// <summary>
		/// Multiplies the components of two vectors by each other.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/> on the left of the mul sign.</param>
		/// <param name="value2">Source <see cref="Vector3"/> on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication.</returns>
		public static Vector3 operator *(Vector3 value1, Vector3 value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			value1.Z *= value2.Z;
			return value1;
		}

		/// <summary>
		/// Multiplies the components of vector by a scalar.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication with a scalar.</returns>
		public static Vector3 operator *(Vector3 value, Fix64 scaleFactor)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			value.Z *= scaleFactor;
			return value;
		}

		/// <summary>
		/// Multiplies the components of vector by a scalar.
		/// </summary>
		/// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
		/// <param name="value">Source <see cref="Vector3"/> on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication with a scalar.</returns>
		public static Vector3 operator *(Fix64 scaleFactor, Vector3 value)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			value.Z *= scaleFactor;
			return value;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector3"/> by the components of another <see cref="Vector3"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector3"/> on the left of the div sign.</param>
		/// <param name="value2">Divisor <see cref="Vector3"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the vectors.</returns>
		public static Vector3 operator /(Vector3 value1, Vector3 value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			value1.Z /= value2.Z;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector3"/> by a scalar.
		/// </summary>
		/// <param name="value">Source <see cref="Vector3"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing a vector by a scalar.</returns>
		public static Vector3 operator /(Vector3 value, Fix64 divider)
		{
			Fix64 factor = Fix64.One / divider;
			value.X *= factor;
			value.Y *= factor;
			value.Z *= factor;
			return value;
		}

		#endregion
	}
}
