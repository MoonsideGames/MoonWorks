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

#endregion

namespace MoonWorks.Math.Fixed
{
	/// <summary>
	/// Describes a fixed point 2D-vector.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	[StructLayout(LayoutKind.Explicit)]
	public struct Vector2 : IEquatable<Vector2>
	{
		#region Public Static Properties

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, 0.
		/// </summary>
		public static Vector2 Zero
		{
			get
			{
				return zeroVector;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 1, 1.
		/// </summary>
		public static Vector2 One
		{
			get
			{
				return unitVector;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 1, 0.
		/// </summary>
		public static Vector2 UnitX
		{
			get
			{
				return unitXVector;
			}
		}

		/// <summary>
		/// Returns a <see cref="Vector2"/> with components 0, 1.
		/// </summary>
		public static Vector2 UnitY
		{
			get
			{
				return unitYVector;
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
					Y.ToString()
				);
			}
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// The x coordinate of this <see cref="Vector2"/>.
		/// </summary>
		[FieldOffset(0)]
		public Fix64 X;

		/// <summary>
		/// The y coordinate of this <see cref="Vector2"/>.
		/// </summary>
		[FieldOffset(8)]
		public Fix64 Y;

		#endregion

		#region Private Static Fields

		private static readonly Vector2 zeroVector = new Vector2(0, 0);
		private static readonly Vector2 unitVector = new Vector2(1, 1);
		private static readonly Vector2 unitXVector = new Vector2(1, 0);
		private static readonly Vector2 unitYVector = new Vector2(0, 1);

		#endregion

		#region Public Constructors

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

		#endregion

		#region Public Methods

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
		/// Turns this <see cref="Vector2"/> to a unit vector with the same direction.
		/// </summary>
		public void Normalize()
		{
			Fix64 val = Fix64.One / Fix64.Sqrt((X * X) + (Y * Y));
			X *= val;
			Y *= val;
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

		#endregion

		#region Public Static Methods

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
			Fix64 val = Fix64.One / Fix64.Sqrt((value.X * value.X) + (value.Y * value.Y));
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
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix4x4"/>.</param>
		/// <returns>Transformed <see cref="Vector2"/>.</returns>
		public static Vector2 Transform(Vector2 position, Matrix4x4 matrix)
		{
			return new Vector2(
				(position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
				(position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
		/// <returns>Transformed <see cref="Vector2"/>.</returns>
		public static Vector2 Transform(Vector2 value, Quaternion rotation)
		{
			Transform(ref value, ref rotation, out value);
			return value;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Quaternion"/>, representing the rotation.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2"/>.</param>
		/// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
		/// <param name="result">Transformed <see cref="Vector2"/> as an output parameter.</param>
		public static void Transform(
			ref Vector2 value,
			ref Quaternion rotation,
			out Vector2 result
		)
		{
			Fix64 two = new Fix64(2);
			Fix64 x = two * -(rotation.Z * value.Y);
			Fix64 y = two * (rotation.Z * value.X);
			Fix64 z = two * (rotation.X * value.Y - rotation.Y * value.X);

			result.X = value.X + x * rotation.W + (rotation.Y * z - rotation.Z * y);
			result.Y = value.Y + y * rotation.W + (rotation.Z * x - rotation.X * z);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Math.Matrix3x2"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Math.Matrix3x2"/>.</param>
		/// <returns>Transformed <see cref="Vector2"/>.</returns>
		public static Vector2 Transform(Vector2 position, Matrix3x2 matrix)
		{
			return new Vector2(
				(position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M31,
				(position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M32
			);
		}

		/// <summary>
		/// Apply transformation on all vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix4x4"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix4x4"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		public static void Transform(
			Vector2[] sourceArray,
			ref Matrix4x4 matrix,
			Vector2[] destinationArray
		)
		{
			Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
		}

		/// <summary>
		/// Apply transformation on vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix4x4"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix4x4"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2"/> should be written.</param>
		/// <param name="length">The number of vectors to be transformed.</param>
		public static void Transform(
			Vector2[] sourceArray,
			int sourceIndex,
			ref Matrix4x4 matrix,
			Vector2[] destinationArray,
			int destinationIndex,
			int length
		)
		{
			for (int x = 0; x < length; x += 1)
			{
				Vector2 position = sourceArray[sourceIndex + x];
				Vector2 destination = destinationArray[destinationIndex + x];
				destination.X = (position.X * matrix.M11) + (position.Y * matrix.M21)
						+ matrix.M41;
				destination.Y = (position.X * matrix.M12) + (position.Y * matrix.M22)
						+ matrix.M42;
				destinationArray[destinationIndex + x] = destination;
			}
		}

		/// <summary>
		/// Apply transformation on all vectors within array of <see cref="Vector2"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
		/// <param name="destinationArray">Destination array.</param>
		public static void Transform(
			Vector2[] sourceArray,
			ref Quaternion rotation,
			Vector2[] destinationArray
		)
		{
			Transform(
				sourceArray,
				0,
				ref rotation,
				destinationArray,
				0,
				sourceArray.Length
			);
		}

		/// <summary>
		/// Apply transformation on vectors within array of <see cref="Vector2"/> by the specified <see cref="Quaternion"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
		/// <param name="rotation">The <see cref="Quaternion"/> which contains rotation transformation.</param>
		/// <param name="destinationArray">Destination array.</param>
		/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2"/> should be written.</param>
		/// <param name="length">The number of vectors to be transformed.</param>
		public static void Transform(
			Vector2[] sourceArray,
			int sourceIndex,
			ref Quaternion rotation,
			Vector2[] destinationArray,
			int destinationIndex,
			int length
		)
		{
			for (int i = 0; i < length; i += 1)
			{
				Vector2 position = sourceArray[sourceIndex + i];
				Vector2 v;
				Transform(ref position, ref rotation, out v);
				destinationArray[destinationIndex + i] = v;
			}
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of the specified normal by the specified <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="normal">Source <see cref="Vector2"/> which represents a normal vector.</param>
		/// <param name="matrix">The transformation <see cref="Matrix4x4"/>.</param>
		/// <returns>Transformed normal.</returns>
		public static Vector2 TransformNormal(Vector2 normal, Matrix4x4 matrix)
		{
			return new Vector2(
				(normal.X * matrix.M11) + (normal.Y * matrix.M21),
				(normal.X * matrix.M12) + (normal.Y * matrix.M22)
			);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of the specified normal by the specified <see cref="Math.Matrix3x2"/>.
		/// </summary>
		/// <param name="normal">Source <see cref="Vector2"/> which represents a normal vector.</param>
		/// <param name="matrix">The transformation <see cref="Math.Matrix3x2"/>.</param>
		/// <returns>Transformed normal.</returns>
		public static Vector2 TransformNormal(Vector2 normal, Matrix3x2 matrix)
        {
            return new Vector2(
                normal.X * matrix.M11 + normal.Y * matrix.M21,
                normal.X * matrix.M12 + normal.Y * matrix.M22);
        }

		/// <summary>
		/// Apply transformation on all normals within array of <see cref="Vector2"/> by the specified <see cref="Matrix4x4"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix4x4"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		public static void TransformNormal(
			Vector2[] sourceArray,
			ref Matrix4x4 matrix,
			Vector2[] destinationArray
		)
		{
			TransformNormal(
				sourceArray,
				0,
				ref matrix,
				destinationArray,
				0,
				sourceArray.Length
			);
		}

		/// <summary>
		/// Apply transformation on normals within array of <see cref="Vector2"/> by the specified <see cref="Matrix4x4"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix4x4"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2"/> should be written.</param>
		/// <param name="length">The number of normals to be transformed.</param>
		public static void TransformNormal(
			Vector2[] sourceArray,
			int sourceIndex,
			ref Matrix4x4 matrix,
			Vector2[] destinationArray,
			int destinationIndex,
			int length
		)
		{
			for (int i = 0; i < length; i += 1)
			{
				Vector2 position = sourceArray[sourceIndex + i];
				Vector2 result;
				result.X = (position.X * matrix.M11) + (position.Y * matrix.M21);
				result.Y = (position.X * matrix.M12) + (position.Y * matrix.M22);
				destinationArray[destinationIndex + i] = result;
			}
		}

		#endregion

		#region Public Static Operators

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

		#endregion
	}
}
