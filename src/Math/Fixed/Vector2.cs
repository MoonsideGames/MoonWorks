/* MoonWorks - Game Development Framework
 * Copyright 2021-2024 Evan Hemsley
 */

/* Derived from code by Ethan Lee (Copyright 2009-2021).
 * Released under the Microsoft Public License.
 * See fna.LICENSE for details.

 * Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MoonWorks.Math.Fixed
{
	/// <summary>
	/// Describes a fixed point 2D-vector.
	/// </summary>
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Vector2 : IEquatable<Vector2>
	{
		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, 0.
		/// </summary>
		public static Vector2 Zero
		{
			get => new Vector2(0, 0);
		}

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 1, 1.
		/// </summary>
		public static Vector2 One
		{
			get => new Vector2(1, 1);
		}

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 1, 0.
		/// </summary>
		public static Vector2 UnitX
		{
			get => new Vector2(1, 0);
		}

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, 1.
		/// </summary>
		public static Vector2 UnitY
		{
			get => new Vector2(0, 1);
		}

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					X.ToString(), " ",
					Y.ToString()
				);
			}
		}

		/// <summary>
		/// The x coordinate of this <see cref="Vector2"/>.
		/// </summary>
		public Fix64 X;

		/// <summary>
		/// The y coordinate of this <see cref="Vector2"/>.
		/// </summary>
		public Fix64 Y;

		/// <summary>
		/// Constructs a 2d vector with X and Y from two values.
		/// </summary>
		/// <param name="x">The x coordinate in 2d-space.</param>
		/// <param name="y">The y coordinate in 2d-space.</param>
		public Vector2(Fix64 x, Fix64 y)
		{
			this.X = x;
			this.Y = y;
		}

		/// <summary>
		/// Constructs a 2d vector with X and Y set to the same value.
		/// </summary>
		/// <param name="value">The x and y coordinates in 2d-space.</param>
		public Vector2(Fix64 value)
		{
			this.X = value;
			this.Y = value;
		}

		public Vector2(int x, int y)
		{
			this.X = new Fix64(x);
			this.Y = new Fix64(y);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return obj is Vector2 fixVector && Equals(fixVector);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Vector2"/>.
		/// </summary>
		/// <param name="other">The <see cref="Vector2"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Vector2 other)
		{
			return (X == other.X &&
					Y == other.Y);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Vector2"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Vector2"/>.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode();
		}

		/// <summary>
		/// Returns the length of this <see cref="Vector2"/>.
		/// </summary>
		/// <returns>The length of this <see cref="Vector2"/>.</returns>
		public Fix64 Length()
		{
			return Fix64.Sqrt((X * X) + (Y * Y));
		}

		/// <summary>
		/// Returns the squared length of this <see cref="Vector2"/>.
		/// </summary>
		/// <returns>The squared length of this <see cref="Vector2"/>.</returns>
		public Fix64 LengthSquared()
		{
			return (X * X) + (Y * Y);
		}

		/// <summary>
		/// Turns this <see cref="Vector2"/> to an angle in radians.
		/// </summary>
		public Fix64 Angle()
		{
			return Fix64.Atan2(Y, X);
		}

		/// <summary>
		/// Returns this Vector2 with the fractional components cut off.
		/// </summary>
		public Vector2 Truncated()
		{
			return new Vector2((int) X, (int) Y);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <returns>The result of linear interpolation of the specified vectors.</returns>
		public static Vector2 Lerp(Vector2 value1, Vector2 value2, Fix64 amount)
		{
			return new Vector2(
				Fix64.Lerp(value1.X, value2.X, amount),
				Fix64.Lerp(value1.Y, value2.Y, amount)
			);
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Vector2"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>]}
		/// </summary>
		/// <returns>A <see cref="String"/> representation of this <see cref="Vector2"/>.</returns>
		public override string ToString()
		{
			return (
				"{X:" + X.ToString() +
				" Y:" + Y.ToString() +
				"}"
			);
		}

		/// <summary>
		/// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
		/// </summary>
		/// <param name="value1">The first vector to add.</param>
		/// <param name="value2">The second vector to add.</param>
		/// <returns>The result of the vector addition.</returns>
		public static Vector2 Add(Vector2 value1, Vector2 value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			return value1;
		}

		/// <summary>
		/// Clamps the specified value within a range.
		/// </summary>
		/// <param name="value1">The value to clamp.</param>
		/// <param name="min">The min value.</param>
		/// <param name="max">The max value.</param>
		/// <returns>The clamped value.</returns>
		public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
		{
			return new Vector2(
				Fix64.Clamp(value1.X, min.X, max.X),
				Fix64.Clamp(value1.Y, min.Y, max.Y)
			);
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The distance between two vectors.</returns>
		public static Fix64 Distance(Vector2 value1, Vector2 value2)
		{
			Fix64 v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			return Fix64.Sqrt((v1 * v1) + (v2 * v2));
		}

		/// <summary>
		/// Returns the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The squared distance between two vectors.</returns>
		public static Fix64 DistanceSquared(Vector2 value1, Vector2 value2)
		{
			Fix64 v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			return (v1 * v1) + (v2 * v2);
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2"/> by the components of another <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/>.</param>
		/// <param name="value2">Divisor <see cref="Vector2"/>.</param>
		/// <returns>The result of dividing the vectors.</returns>
		public static Vector2 Divide(Vector2 value1, Vector2 value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2"/> by a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <returns>The result of dividing a vector by a scalar.</returns>
		public static Vector2 Divide(Vector2 value1, Fix64 divider)
		{
			Fix64 factor = Fix64.One / divider;
			value1.X *= factor;
			value1.Y *= factor;
			return value1;
		}

		/// <summary>
		/// Returns a dot product of two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The dot product of two vectors.</returns>
		public static Fix64 Dot(Vector2 value1, Vector2 value2)
		{
			return (value1.X * value2.X) + (value1.Y * value2.Y);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a maximal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The <see cref="Vector2"/> with maximal values from the two vectors.</returns>
		public static Vector2 Max(Vector2 value1, Vector2 value2)
		{
			return new Vector2(
				value1.X > value2.X ? value1.X : value2.X,
				value1.Y > value2.Y ? value1.Y : value2.Y
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a minimal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The <see cref="Vector2"/> with minimal values from the two vectors.</returns>
		public static Vector2 Min(Vector2 value1, Vector2 value2)
		{
			return new Vector2(
				value1.X < value2.X ? value1.X : value2.X,
				value1.Y < value2.Y ? value1.Y : value2.Y
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a multiplication of two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/>.</param>
		/// <param name="value2">Source <see cref="Vector2"/>.</param>
		/// <returns>The result of the vector multiplication.</returns>
		public static Vector2 Multiply(Vector2 value1, Vector2 value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a multiplication of <see cref="Vector2"/> and a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>The result of the vector multiplication with a scalar.</returns>
		public static Vector2 Multiply(Vector2 value1, Fix64 scaleFactor)
		{
			value1.X *= scaleFactor;
			value1.Y *= scaleFactor;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains the specified vector inversion.
		/// direction of <paramref name="value"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <returns>The result of the vector inversion.</returns>
		public static Vector2 Negate(Vector2 value)
		{
			value.X = -value.X;
			value.Y = -value.Y;
			return value;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a normalized values from another vector.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <returns>Unit vector.</returns>
		public static Vector2 Normalize(Vector2 value)
		{
			Fix64 lengthSquared = (value.X * value.X) + (value.Y * value.Y);

			if (lengthSquared == Fix64.Zero)
			{
				return Zero;
			}

			Fix64 val = Fix64.One / Fix64.Sqrt(lengthSquared);
			value.X *= val;
			value.Y *= val;
			return value;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains reflect vector of the given vector and normal.
		/// </summary>
		/// <param name="vector">Source <see cref="Vector2"/>.</param>
		/// <param name="normal">Reflection normal.</param>
		/// <returns>Reflected vector.</returns>
		public static Vector2 Reflect(Vector2 vector, Vector2 normal)
		{
			Vector2 result;
			Fix64 val = new Fix64(2) * ((vector.X * normal.X) + (vector.Y * normal.Y));
			result.X = vector.X - (normal.X * val);
			result.Y = vector.Y - (normal.Y * val);
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains subtraction of on <see cref="Vector2"/> from a another.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/>.</param>
		/// <param name="value2">Source <see cref="Vector2"/>.</param>
		/// <returns>The result of the vector subtraction.</returns>
		public static Vector2 Subtract(Vector2 value1, Vector2 value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			return value1;
		}

		/// <summary>
		/// Rotates a Vector2 by an angle.
		/// </summary>
		/// <param name="vector">The vector to rotate.</param>
		/// <param name="angle">The angle in radians.</param>
		public static Vector2 Rotate(Vector2 vector, Fix64 angle)
		{
			return new Vector2(
				vector.X * Fix64.Cos(angle) - vector.Y * Fix64.Sin(angle),
				vector.X * Fix64.Sin(angle) + vector.Y * Fix64.Cos(angle)
			);
		}

		/// <summary>
		/// Inverts values in the specified <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Vector2 operator -(Vector2 value)
		{
			value.X = -value.X;
			value.Y = -value.Y;
			return value;
		}

		/// <summary>
		/// Compares whether two <see cref="Vector2"/> instances are equal.
		/// </summary>
		/// <param name="value1"><see cref="Vector2"/> instance on the left of the equal sign.</param>
		/// <param name="value2"><see cref="Vector2"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Vector2 value1, Vector2 value2)
		{
			return (value1.X == value2.X &&
					value1.Y == value2.Y);
		}

		/// <summary>
		/// Compares whether two <see cref="Vector2"/> instances are equal.
		/// </summary>
		/// <param name="value1"><see cref="Vector2"/> instance on the left of the equal sign.</param>
		/// <param name="value2"><see cref="Vector2"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(Vector2 value1, Vector2 value2)
		{
			return !(value1 == value2);
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/> on the left of the add sign.</param>
		/// <param name="value2">Source <see cref="Vector2"/> on the right of the add sign.</param>
		/// <returns>Sum of the vectors.</returns>
		public static Vector2 operator +(Vector2 value1, Vector2 value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			return value1;
		}

		/// <summary>
		/// Subtracts a <see cref="Vector2"/> from a <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/> on the left of the sub sign.</param>
		/// <param name="value2">Source <see cref="Vector2"/> on the right of the sub sign.</param>
		/// <returns>Result of the vector subtraction.</returns>
		public static Vector2 operator -(Vector2 value1, Vector2 value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			return value1;
		}

		/// <summary>
		/// Multiplies the components of two vectors by each other.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/> on the left of the mul sign.</param>
		/// <param name="value2">Source <see cref="Vector2"/> on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication.</returns>
		public static Vector2 operator *(Vector2 value1, Vector2 value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			return value1;
		}

		/// <summary>
		/// Multiplies the components of vector by a scalar.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication with a scalar.</returns>
		public static Vector2 operator *(Vector2 value, Fix64 scaleFactor)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			return value;
		}

		/// <summary>
		/// Multiplies the components of vector by a scalar.
		/// </summary>
		/// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
		/// <param name="value">Source <see cref="Vector2"/> on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication with a scalar.</returns>
		public static Vector2 operator *(Fix64 scaleFactor, Vector2 value)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			return value;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2"/> by the components of another <see cref="Vector2"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/> on the left of the div sign.</param>
		/// <param name="value2">Divisor <see cref="Vector2"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the vectors.</returns>
		public static Vector2 operator /(Vector2 value1, Vector2 value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2"/> by a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing a vector by a scalar.</returns>
		public static Vector2 operator /(Vector2 value1, Fix64 divider)
		{
			Fix64 factor = Fix64.One / divider;
			value1.X *= factor;
			value1.Y *= factor;
			return value1;
		}
	}
}
