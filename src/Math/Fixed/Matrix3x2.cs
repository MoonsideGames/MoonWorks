/* MoonWorks - Game Development Framework
 * Copyright 2022 Evan Hemsley
 */

/* Derived from code by Microsoft.
 * Released under the MIT license.
 * See microsoft.LICENSE for details.
 */

using System;
using System.Globalization;

namespace MoonWorks.Math.Fixed
{
	/// <summary>
	/// A structure encapsulating a 3x2 fixed point matrix.
	/// </summary>
	public struct Matrix3x2 : IEquatable<Matrix3x2>
	{
		#region Public Fields
		/// <summary>
		/// The first element of the first row
		/// </summary>
		public Fix64 M11;
		/// <summary>
		/// The second element of the first row
		/// </summary>
		public Fix64 M12;
		/// <summary>
		/// The first element of the second row
		/// </summary>
		public Fix64 M21;
		/// <summary>
		/// The second element of the second row
		/// </summary>
		public Fix64 M22;
		/// <summary>
		/// The first element of the third row
		/// </summary>
		public Fix64 M31;
		/// <summary>
		/// The second element of the third row
		/// </summary>
		public Fix64 M32;
		#endregion Public Fields

		private static readonly Matrix3x2 _identity = new Matrix3x2
		(
			1, 0,
			0, 1,
			0, 0
		);

		private static readonly Fix64 RotationEpsilon = Fix64.FromFraction(1, 1000) * (Fix64.Pi / new Fix64(180));

		/// <summary>
		/// Returns the multiplicative identity matrix.
		/// </summary>
		public static Matrix3x2 Identity
		{
			get { return _identity; }
		}

		/// <summary>
		/// Returns whether the matrix is the identity matrix.
		/// </summary>
		public bool IsIdentity
		{
			get
			{
				return  M11 == Fix64.One && M22 == Fix64.One && // Check diagonal element first for early out.
                        M12 == Fix64.Zero &&
					    M21 == Fix64.Zero &&
					    M31 == Fix64.Zero && M32 == Fix64.Zero;
			}
		}

		/// <summary>
		/// Gets or sets the translation component of this matrix.
		/// </summary>
		public Vector2 Translation
		{
			get
			{
				return new Vector2(M31, M32);
			}

			set
			{
				M31 = value.X;
				M32 = value.Y;
			}
		}

		/// <summary>
		/// Constructs a FixMatrix3x2 from the given components.
		/// </summary>
		public Matrix3x2(Fix64 m11, Fix64 m12,
						 Fix64 m21, Fix64 m22,
						 Fix64 m31, Fix64 m32)
		{
			M11 = m11;
			M12 = m12;
			M21 = m21;
			M22 = m22;
			M31 = m31;
			M32 = m32;
		}

        public Matrix3x2(int m11, int m12, int m21, int m22, int m31, int m32)
        {
            M11 = new Fix64(m11);
			M12 = new Fix64(m12);
			M21 = new Fix64(m21);
			M22 = new Fix64(m22);
			M31 = new Fix64(m31);
			M32 = new Fix64(m32);
        }

		/// <summary>
		/// Creates a translation matrix from the given vector.
		/// </summary>
		/// <param name="position">The translation position.</param>
		/// <returns>A translation matrix.</returns>
		public static Matrix3x2 CreateTranslation(Vector2 position)
		{
			Matrix3x2 result;

			result.M11 = Fix64.One;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = Fix64.One;

			result.M31 = position.X;
			result.M32 = position.Y;

			return result;
		}

		/// <summary>
		/// Creates a translation matrix from the given X and Y components.
		/// </summary>
		/// <param name="xPosition">The X position.</param>
		/// <param name="yPosition">The Y position.</param>
		/// <returns>A translation matrix.</returns>
		public static Matrix3x2 CreateTranslation(Fix64 xPosition, Fix64 yPosition)
		{
			Matrix3x2 result;

			result.M11 = Fix64.One;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = Fix64.One;

			result.M31 = xPosition;
			result.M32 = yPosition;

			return result;
		}

		/// <summary>
		/// Creates a scale matrix from the given X and Y components.
		/// </summary>
		/// <param name="xScale">Value to scale by on the X-axis.</param>
		/// <param name="yScale">Value to scale by on the Y-axis.</param>
		/// <returns>A scaling matrix.</returns>
		public static Matrix3x2 CreateScale(Fix64 xScale, Fix64 yScale)
		{
			Matrix3x2 result;

			result.M11 = xScale;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = yScale;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;

			return result;
		}

		/// <summary>
		/// Creates a scale matrix that is offset by a given center point.
		/// </summary>
		/// <param name="xScale">Value to scale by on the X-axis.</param>
		/// <param name="yScale">Value to scale by on the Y-axis.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>A scaling matrix.</returns>
		public static Matrix3x2 CreateScale(Fix64 xScale, Fix64 yScale, Vector2 centerPoint)
		{
			Matrix3x2 result;

			Fix64 tx = centerPoint.X * (Fix64.One - xScale);
			Fix64 ty = centerPoint.Y * (Fix64.One - yScale);

			result.M11 = xScale;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = yScale;
			result.M31 = tx;
			result.M32 = ty;

			return result;
		}

		/// <summary>
		/// Creates a scale matrix from the given vector scale.
		/// </summary>
		/// <param name="scales">The scale to use.</param>
		/// <returns>A scaling matrix.</returns>
		public static Matrix3x2 CreateScale(Vector2 scales)
		{
			Matrix3x2 result;

			result.M11 = scales.X;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = scales.Y;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;

			return result;
		}

		/// <summary>
		/// Creates a scale matrix from the given vector scale with an offset from the given center point.
		/// </summary>
		/// <param name="scales">The scale to use.</param>
		/// <param name="centerPoint">The center offset.</param>
		/// <returns>A scaling matrix.</returns>
		public static Matrix3x2 CreateScale(Vector2 scales, Vector2 centerPoint)
		{
			Matrix3x2 result;

			Fix64 tx = centerPoint.X * (Fix64.One - scales.X);
			Fix64 ty = centerPoint.Y * (Fix64.One - scales.Y);

			result.M11 = scales.X;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = scales.Y;
			result.M31 = tx;
			result.M32 = ty;

			return result;
		}

		/// <summary>
		/// Creates a scale matrix that scales uniformly with the given scale.
		/// </summary>
		/// <param name="scale">The uniform scale to use.</param>
		/// <returns>A scaling matrix.</returns>
		public static Matrix3x2 CreateScale(Fix64 scale)
		{
			Matrix3x2 result;

			result.M11 = scale;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = scale;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;

			return result;
		}

		/// <summary>
		/// Creates a scale matrix that scales uniformly with the given scale with an offset from the given center.
		/// </summary>
		/// <param name="scale">The uniform scale to use.</param>
		/// <param name="centerPoint">The center offset.</param>
		/// <returns>A scaling matrix.</returns>
		public static Matrix3x2 CreateScale(Fix64 scale, Vector2 centerPoint)
		{
			Matrix3x2 result;

			Fix64 tx = centerPoint.X * (Fix64.One - scale);
			Fix64 ty = centerPoint.Y * (Fix64.One - scale);

			result.M11 = scale;
			result.M12 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = scale;
			result.M31 = tx;
			result.M32 = ty;

			return result;
		}

		/// <summary>
		/// Creates a skew matrix from the given angles in radians.
		/// </summary>
		/// <param name="radiansX">The X angle, in radians.</param>
		/// <param name="radiansY">The Y angle, in radians.</param>
		/// <returns>A skew matrix.</returns>
		public static Matrix3x2 CreateSkew(Fix64 radiansX, Fix64 radiansY)
		{
			Matrix3x2 result;

			Fix64 xTan = (Fix64) Fix64.Tan(radiansX);
			Fix64 yTan = (Fix64) Fix64.Tan(radiansY);

			result.M11 = Fix64.One;
			result.M12 = yTan;
			result.M21 = xTan;
			result.M22 = Fix64.One;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;

			return result;
		}

		/// <summary>
		/// Creates a skew matrix from the given angles in radians and a center point.
		/// </summary>
		/// <param name="radiansX">The X angle, in radians.</param>
		/// <param name="radiansY">The Y angle, in radians.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>A skew matrix.</returns>
		public static Matrix3x2 CreateSkew(Fix64 radiansX, Fix64 radiansY, Vector2 centerPoint)
		{
			Matrix3x2 result;

			Fix64 xTan = (Fix64) Fix64.Tan(radiansX);
			Fix64 yTan = (Fix64) Fix64.Tan(radiansY);

			Fix64 tx = -centerPoint.Y * xTan;
			Fix64 ty = -centerPoint.X * yTan;

			result.M11 = Fix64.One;
			result.M12 = yTan;
			result.M21 = xTan;
			result.M22 = Fix64.One;
			result.M31 = tx;
			result.M32 = ty;

			return result;
		}

		/// <summary>
		/// Creates a rotation matrix using the given rotation in radians.
		/// </summary>
		/// <param name="radians">The amount of rotation, in radians.</param>
		/// <returns>A rotation matrix.</returns>
		public static Matrix3x2 CreateRotation(Fix64 radians)
		{
			Matrix3x2 result;

			radians = Fix64.IEEERemainder(radians, Fix64.PiTimes2);

			Fix64 c, s;

			if (radians > -RotationEpsilon && radians < RotationEpsilon)
			{
				// Exact case for zero rotation.
				c = Fix64.One;
				s = Fix64.Zero;
			}
			else if (radians > Fix64.PiOver2 - RotationEpsilon && radians < Fix64.PiOver2 + RotationEpsilon)
			{
				// Exact case for 90 degree rotation.
				c = Fix64.Zero;
				s = Fix64.One;
			}
			else if (radians < -Fix64.Pi + RotationEpsilon || radians > Fix64.Pi - RotationEpsilon)
			{
				// Exact case for 180 degree rotation.
				c = -Fix64.One;
				s = Fix64.Zero;
			}
			else if (radians > -Fix64.PiOver2 - RotationEpsilon && radians < -Fix64.PiOver2 + RotationEpsilon)
			{
				// Exact case for 270 degree rotation.
				c = Fix64.Zero;
				s = -Fix64.One;
			}
			else
			{
				// Arbitrary rotation.
				c = Fix64.Cos(radians);
				s = Fix64.Sin(radians);
			}

			// [  c  s ]
			// [ -s  c ]
			// [  0  0 ]
			result.M11 = c;
			result.M12 = s;
			result.M21 = -s;
			result.M22 = c;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;

			return result;
		}

		/// <summary>
		/// Creates a rotation matrix using the given rotation in radians and a center point.
		/// </summary>
		/// <param name="radians">The amount of rotation, in radians.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>A rotation matrix.</returns>
		public static Matrix3x2 CreateRotation(Fix64 radians, Vector2 centerPoint)
		{
			Matrix3x2 result;

			radians = Fix64.IEEERemainder(radians, Fix64.PiTimes2);

			Fix64 c, s;

			if (radians > -RotationEpsilon && radians < RotationEpsilon)
			{
				// Exact case for zero rotation.
				c = Fix64.One;
				s = Fix64.Zero;
			}
			else if (radians > Fix64.PiOver2 - RotationEpsilon && radians < Fix64.PiOver2 + RotationEpsilon)
			{
				// Exact case for 90 degree rotation.
				c = Fix64.Zero;
				s = Fix64.One;
			}
			else if (radians < -Fix64.Pi + RotationEpsilon || radians > Fix64.Pi - RotationEpsilon)
			{
				// Exact case for 180 degree rotation.
				c = -Fix64.One;
				s = Fix64.Zero;
			}
			else if (radians > -Fix64.PiOver2 - RotationEpsilon && radians < -Fix64.PiOver2 + RotationEpsilon)
			{
				// Exact case for 270 degree rotation.
				c = Fix64.Zero;
				s = -Fix64.One;
			}
			else
			{
				// Arbitrary rotation.
				c = (Fix64) Fix64.Cos(radians);
				s = (Fix64) Fix64.Sin(radians);
			}

			Fix64 x = centerPoint.X * (Fix64.One - c) + centerPoint.Y * s;
			Fix64 y = centerPoint.Y * (Fix64.One - c) - centerPoint.X * s;

			// [  c  s ]
			// [ -s  c ]
			// [  x  y ]
			result.M11 = c;
			result.M12 = s;
			result.M21 = -s;
			result.M22 = c;
			result.M31 = x;
			result.M32 = y;

			return result;
		}

		/// <summary>
		/// Calculates the determinant for this matrix.
		/// The determinant is calculated by expanding the matrix with a third column whose values are (0,0,1).
		/// </summary>
		/// <returns>The determinant.</returns>
		public Fix64 GetDeterminant()
		{
			// There isn't actually any such thing as a determinant for a non-square matrix,
			// but this 3x2 type is really just an optimization of a 3x3 where we happen to
			// know the rightmost column is always (0, 0, 1). So we expand to 3x3 format:
			//
			//  [ M11, M12, 0 ]
			//  [ M21, M22, 0 ]
			//  [ M31, M32, 1 ]
			//
			// Sum the diagonal products:
			//  (M11 * M22 * 1) + (M12 * 0 * M31) + (0 * M21 * M32)
			//
			// Subtract the opposite diagonal products:
			//  (M31 * M22 * 0) + (M32 * 0 * M11) + (1 * M21 * M12)
			//
			// Collapse out the constants and oh look, this is just a 2x2 determinant!

			return (M11 * M22) - (M21 * M12);
		}

		/// <summary>
		/// Attempts to invert the given matrix. If the operation succeeds, the inverted matrix is stored in the result parameter.
		/// </summary>
		/// <param name="matrix">The source matrix.</param>
		/// <param name="result">The output matrix.</param>
		/// <returns>True if the operation succeeded, False otherwise.</returns>
		public static bool Invert(Matrix3x2 matrix, out Matrix3x2 result)
		{
			Fix64 det = (matrix.M11 * matrix.M22) - (matrix.M21 * matrix.M12);

			if (Fix64.Abs(det) == Fix64.Zero)
			{
				result = new Matrix3x2(Fix64.Zero, Fix64.Zero, Fix64.Zero, Fix64.Zero, Fix64.Zero, Fix64.Zero);
				return false;
			}

			Fix64 invDet = Fix64.One / det;

			result.M11 = matrix.M22 * invDet;
			result.M12 = -matrix.M12 * invDet;
			result.M21 = -matrix.M21 * invDet;
			result.M22 = matrix.M11 * invDet;
			result.M31 = (matrix.M21 * matrix.M32 - matrix.M31 * matrix.M22) * invDet;
			result.M32 = (matrix.M31 * matrix.M12 - matrix.M11 * matrix.M32) * invDet;

			return true;
		}

		/// <summary>
		/// Linearly interpolates from matrix1 to matrix2, based on the third parameter.
		/// </summary>
		/// <param name="matrix1">The first source matrix.</param>
		/// <param name="matrix2">The second source matrix.</param>
		/// <param name="amount">The relative weighting of matrix2.</param>
		/// <returns>The interpolated matrix.</returns>
		public static Matrix3x2 Lerp(Matrix3x2 matrix1, Matrix3x2 matrix2, Fix64 amount)
		{
			Matrix3x2 result;

			// First row
			result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
			result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;

			// Second row
			result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
			result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;

			// Third row
			result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
			result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;

			return result;
		}

		/// <summary>
		/// Negates the given matrix by multiplying all values by -1.
		/// </summary>
		/// <param name="value">The source matrix.</param>
		/// <returns>The negated matrix.</returns>
		public static Matrix3x2 Negate(Matrix3x2 value)
		{
			Matrix3x2 result;

			result.M11 = -value.M11;
			result.M12 = -value.M12;
			result.M21 = -value.M21;
			result.M22 = -value.M22;
			result.M31 = -value.M31;
			result.M32 = -value.M32;

			return result;
		}

		/// <summary>
		/// Adds each matrix element in value1 with its corresponding element in value2.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The matrix containing the summed values.</returns>
		public static Matrix3x2 Add(Matrix3x2 value1, Matrix3x2 value2)
		{
			Matrix3x2 result;

			result.M11 = value1.M11 + value2.M11;
			result.M12 = value1.M12 + value2.M12;
			result.M21 = value1.M21 + value2.M21;
			result.M22 = value1.M22 + value2.M22;
			result.M31 = value1.M31 + value2.M31;
			result.M32 = value1.M32 + value2.M32;

			return result;
		}

		/// <summary>
		/// Subtracts each matrix element in value2 from its corresponding element in value1.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The matrix containing the resulting values.</returns>
		public static Matrix3x2 Subtract(Matrix3x2 value1, Matrix3x2 value2)
		{
			Matrix3x2 result;

			result.M11 = value1.M11 - value2.M11;
			result.M12 = value1.M12 - value2.M12;
			result.M21 = value1.M21 - value2.M21;
			result.M22 = value1.M22 - value2.M22;
			result.M31 = value1.M31 - value2.M31;
			result.M32 = value1.M32 - value2.M32;

			return result;
		}

		/// <summary>
		/// Multiplies two matrices together and returns the resulting matrix.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The product matrix.</returns>
		public static Matrix3x2 Multiply(Matrix3x2 value1, Matrix3x2 value2)
		{
			Matrix3x2 result;

			// First row
			result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21;
			result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22;

			// Second row
			result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21;
			result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22;

			// Third row
			result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value2.M31;
			result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value2.M32;

			return result;
		}

		public Matrix4x4 ToMatrix4x4()
		{
			return new Matrix4x4(
				M11, M12, Fix64.Zero, Fix64.Zero,
				M21, M22, Fix64.Zero, Fix64.Zero,
				Fix64.Zero, Fix64.Zero, Fix64.One, Fix64.Zero,
				M31, M32, Fix64.Zero, Fix64.One
			);
		}

		/// <summary>
		/// Scales all elements in a matrix by the given scalar factor.
		/// </summary>
		/// <param name="value1">The source matrix.</param>
		/// <param name="value2">The scaling value to use.</param>
		/// <returns>The resulting matrix.</returns>
		public static Matrix3x2 Multiply(Matrix3x2 value1, Fix64 value2)
		{
			Matrix3x2 result;

			result.M11 = value1.M11 * value2;
			result.M12 = value1.M12 * value2;
			result.M21 = value1.M21 * value2;
			result.M22 = value1.M22 * value2;
			result.M31 = value1.M31 * value2;
			result.M32 = value1.M32 * value2;

			return result;
		}

		/// <summary>
		/// Negates the given matrix by multiplying all values by -1.
		/// </summary>
		/// <param name="value">The source matrix.</param>
		/// <returns>The negated matrix.</returns>
		public static Matrix3x2 operator -(Matrix3x2 value)
		{
			Matrix3x2 m;

			m.M11 = -value.M11;
			m.M12 = -value.M12;
			m.M21 = -value.M21;
			m.M22 = -value.M22;
			m.M31 = -value.M31;
			m.M32 = -value.M32;

			return m;
		}

		/// <summary>
		/// Adds each matrix element in value1 with its corresponding element in value2.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The matrix containing the summed values.</returns>
		public static Matrix3x2 operator +(Matrix3x2 value1, Matrix3x2 value2)
		{
			Matrix3x2 m;

			m.M11 = value1.M11 + value2.M11;
			m.M12 = value1.M12 + value2.M12;
			m.M21 = value1.M21 + value2.M21;
			m.M22 = value1.M22 + value2.M22;
			m.M31 = value1.M31 + value2.M31;
			m.M32 = value1.M32 + value2.M32;

			return m;
		}

		/// <summary>
		/// Subtracts each matrix element in value2 from its corresponding element in value1.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The matrix containing the resulting values.</returns>
		public static Matrix3x2 operator -(Matrix3x2 value1, Matrix3x2 value2)
		{
			Matrix3x2 m;

			m.M11 = value1.M11 - value2.M11;
			m.M12 = value1.M12 - value2.M12;
			m.M21 = value1.M21 - value2.M21;
			m.M22 = value1.M22 - value2.M22;
			m.M31 = value1.M31 - value2.M31;
			m.M32 = value1.M32 - value2.M32;

			return m;
		}

		/// <summary>
		/// Multiplies two matrices together and returns the resulting matrix.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The product matrix.</returns>
		public static Matrix3x2 operator *(Matrix3x2 value1, Matrix3x2 value2)
		{
			Matrix3x2 m;

			// First row
			m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21;
			m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22;

			// Second row
			m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21;
			m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22;

			// Third row
			m.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value2.M31;
			m.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value2.M32;

			return m;
		}

		/// <summary>
		/// Scales all elements in a matrix by the given scalar factor.
		/// </summary>
		/// <param name="value1">The source matrix.</param>
		/// <param name="value2">The scaling value to use.</param>
		/// <returns>The resulting matrix.</returns>
		public static Matrix3x2 operator *(Matrix3x2 value1, Fix64 value2)
		{
			Matrix3x2 m;

			m.M11 = value1.M11 * value2;
			m.M12 = value1.M12 * value2;
			m.M21 = value1.M21 * value2;
			m.M22 = value1.M22 * value2;
			m.M31 = value1.M31 * value2;
			m.M32 = value1.M32 * value2;

			return m;
		}

		/// <summary>
		/// Returns a boolean indicating whether the given matrices are equal.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>True if the matrices are equal; False otherwise.</returns>
		public static bool operator ==(Matrix3x2 value1, Matrix3x2 value2)
		{
			return (value1.M11 == value2.M11 && value1.M22 == value2.M22 && // Check diagonal element first for early out.
												value1.M12 == value2.M12 &&
					value1.M21 == value2.M21 &&
					value1.M31 == value2.M31 && value1.M32 == value2.M32);
		}

		/// <summary>
		/// Returns a boolean indicating whether the given matrices are not equal.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>True if the matrices are not equal; False if they are equal.</returns>
		public static bool operator !=(Matrix3x2 value1, Matrix3x2 value2)
		{
			return (value1.M11 != value2.M11 || value1.M12 != value2.M12 ||
					value1.M21 != value2.M21 || value1.M22 != value2.M22 ||
					value1.M31 != value2.M31 || value1.M32 != value2.M32);
		}

		/// <summary>
		/// Casts to floating point Matrix3x2.
		/// </summary>
		public static explicit operator Math.Float.Matrix3x2(Matrix3x2 matrix)
		{
			return new Math.Float.Matrix3x2(
				(float) matrix.M11, (float) matrix.M12,
				(float) matrix.M21, (float) matrix.M22,
				(float) matrix.M31, (float) matrix.M32
			);
		}

		/// <summary>
		/// Returns a boolean indicating whether the matrix is equal to the other given matrix.
		/// </summary>
		/// <param name="other">The other matrix to test equality against.</param>
		/// <returns>True if this matrix is equal to other; False otherwise.</returns>
		public bool Equals(Matrix3x2 other)
		{
			return (M11 == other.M11 && M22 == other.M22 && // Check diagonal element first for early out.
										M12 == other.M12 &&
					M21 == other.M21 &&
					M31 == other.M31 && M32 == other.M32);
		}

		/// <summary>
		/// Returns a boolean indicating whether the given Object is equal to this matrix instance.
		/// </summary>
		/// <param name="obj">The Object to compare against.</param>
		/// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Matrix3x2)
			{
				return Equals((Matrix3x2) obj);
			}

			return false;
		}

		/// <summary>
		/// Returns a String representing this matrix instance.
		/// </summary>
		/// <returns>The string representation.</returns>
		public override string ToString()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			return String.Format(ci, "{{ {{M11:{0} M12:{1}}} {{M21:{2} M22:{3}}} {{M31:{4} M32:{5}}} }}",
								 M11.ToString(ci), M12.ToString(ci),
								 M21.ToString(ci), M22.ToString(ci),
								 M31.ToString(ci), M32.ToString(ci));
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return M11.GetHashCode() + M12.GetHashCode() +
				   M21.GetHashCode() + M22.GetHashCode() +
				   M31.GetHashCode() + M32.GetHashCode();
		}
	}
}
