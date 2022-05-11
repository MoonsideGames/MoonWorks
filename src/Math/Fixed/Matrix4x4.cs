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
	/// Represents the right-handed 4x4 fixed point matrix, which can store translation, scale and rotation information.
	/// This differs from XNA in one major way: projections are modified to give right handed NDC space.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	[StructLayout(LayoutKind.Sequential)]
	public struct Matrix4x4 : IEquatable<Matrix4x4>
	{
		#region Public Properties

		/// <summary>
		/// The backward vector formed from the third row M31, M32, M33 elements.
		/// </summary>
		public Vector3 Backward
		{
			get
			{
				return new Vector3(M31, M32, M33);
			}
			set
			{
				M31 = value.X;
				M32 = value.Y;
				M33 = value.Z;
			}
		}

		/// <summary>
		/// The down vector formed from the second row -M21, -M22, -M23 elements.
		/// </summary>
		public Vector3 Down
		{
			get
			{
				return new Vector3(-M21, -M22, -M23);
			}
			set
			{
				M21 = -value.X;
				M22 = -value.Y;
				M23 = -value.Z;
			}
		}

		/// <summary>
		/// The forward vector formed from the third row -M31, -M32, -M33 elements.
		/// </summary>
		public Vector3 Forward
		{
			get
			{
				return new Vector3(-M31, -M32, -M33);
			}
			set
			{
				M31 = -value.X;
				M32 = -value.Y;
				M33 = -value.Z;
			}
		}

		/// <summary>
		/// Returns the identity matrix.
		/// </summary>
		public static Matrix4x4 Identity
		{
			get
			{
				return identity;
			}
		}

		/// <summary>
		/// The left vector formed from the first row -M11, -M12, -M13 elements.
		/// </summary>
		public Vector3 Left
		{
			get
			{
				return new Vector3(-M11, -M12, -M13);
			}
			set
			{
				M11 = -value.X;
				M12 = -value.Y;
				M13 = -value.Z;
			}
		}

		/// <summary>
		/// The right vector formed from the first row M11, M12, M13 elements.
		/// </summary>
		public Vector3 Right
		{
			get
			{
				return new Vector3(M11, M12, M13);
			}
			set
			{
				M11 = value.X;
				M12 = value.Y;
				M13 = value.Z;
			}
		}

		/// <summary>
		/// Position stored in this matrix.
		/// </summary>
		public Vector3 Translation
		{
			get
			{
				return new Vector3(M41, M42, M43);
			}
			set
			{
				M41 = value.X;
				M42 = value.Y;
				M43 = value.Z;
			}
		}

		/// <summary>
		/// The upper vector formed from the second row M21, M22, M23 elements.
		/// </summary>
		public Vector3 Up
		{
			get
			{
				return new Vector3(M21, M22, M23);
			}
			set
			{
				M21 = value.X;
				M22 = value.Y;
				M23 = value.Z;
			}
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					"( ", M11.ToString(), " ",
					M12.ToString(), " ",
					M13.ToString(), " ",
					M14.ToString(), " ) \r\n",
					"( ", M21.ToString(), " ",
					M22.ToString(), " ",
					M23.ToString(), " ",
					M24.ToString(), " ) \r\n",
					"( ", M31.ToString(), " ",
					M32.ToString(), " ",
					M33.ToString(), " ",
					M34.ToString(), " ) \r\n",
					"( ", M41.ToString(), " ",
					M42.ToString(), " ",
					M43.ToString(), " ",
					M44.ToString(), " )"
				);
			}
		}

		#endregion

		#region Public Fields

		/// <summary>
		/// A first row and first column value.
		/// </summary>
		public Fix64 M11;

		/// <summary>
		/// A first row and second column value.
		/// </summary>
		public Fix64 M12;

		/// <summary>
		/// A first row and third column value.
		/// </summary>
		public Fix64 M13;

		/// <summary>
		/// A first row and fourth column value.
		/// </summary>
		public Fix64 M14;

		/// <summary>
		/// A second row and first column value.
		/// </summary>
		public Fix64 M21;

		/// <summary>
		/// A second row and second column value.
		/// </summary>
		public Fix64 M22;

		/// <summary>
		/// A second row and third column value.
		/// </summary>
		public Fix64 M23;

		/// <summary>
		/// A second row and fourth column value.
		/// </summary>
		public Fix64 M24;

		/// <summary>
		/// A third row and first column value.
		/// </summary>
		public Fix64 M31;

		/// <summary>
		/// A third row and second column value.
		/// </summary>
		public Fix64 M32;

		/// <summary>
		/// A third row and third column value.
		/// </summary>
		public Fix64 M33;

		/// <summary>
		/// A third row and fourth column value.
		/// </summary>
		public Fix64 M34;

		/// <summary>
		/// A fourth row and first column value.
		/// </summary>
		public Fix64 M41;

		/// <summary>
		/// A fourth row and second column value.
		/// </summary>
		public Fix64 M42;

		/// <summary>
		/// A fourth row and third column value.
		/// </summary>
		public Fix64 M43;

		/// <summary>
		/// A fourth row and fourth column value.
		/// </summary>
		public Fix64 M44;

		#endregion

		#region Private Static Variables

		private static Matrix4x4 identity = new Matrix4x4(
			1, 0, 0, 0,
			0, 1, 0, 0,
			0, 0, 1, 0,
			0, 0, 0, 1
		);

		#endregion

		#region Public Constructors

		/// <summary>
		/// Constructs a matrix.
		/// </summary>
		/// <param name="m11">A first row and first column value.</param>
		/// <param name="m12">A first row and second column value.</param>
		/// <param name="m13">A first row and third column value.</param>
		/// <param name="m14">A first row and fourth column value.</param>
		/// <param name="m21">A second row and first column value.</param>
		/// <param name="m22">A second row and second column value.</param>
		/// <param name="m23">A second row and third column value.</param>
		/// <param name="m24">A second row and fourth column value.</param>
		/// <param name="m31">A third row and first column value.</param>
		/// <param name="m32">A third row and second column value.</param>
		/// <param name="m33">A third row and third column value.</param>
		/// <param name="m34">A third row and fourth column value.</param>
		/// <param name="m41">A fourth row and first column value.</param>
		/// <param name="m42">A fourth row and second column value.</param>
		/// <param name="m43">A fourth row and third column value.</param>
		/// <param name="m44">A fourth row and fourth column value.</param>
		public Matrix4x4(
			Fix64 m11, Fix64 m12, Fix64 m13, Fix64 m14,
			Fix64 m21, Fix64 m22, Fix64 m23, Fix64 m24,
			Fix64 m31, Fix64 m32, Fix64 m33, Fix64 m34,
			Fix64 m41, Fix64 m42, Fix64 m43, Fix64 m44
		)
		{
			M11 = m11;
			M12 = m12;
			M13 = m13;
			M14 = m14;
			M21 = m21;
			M22 = m22;
			M23 = m23;
			M24 = m24;
			M31 = m31;
			M32 = m32;
			M33 = m33;
			M34 = m34;
			M41 = m41;
			M42 = m42;
			M43 = m43;
			M44 = m44;
		}

		/// <summary>
		/// Constructs a matrix.
		/// </summary>
		/// <param name="m11">A first row and first column value.</param>
		/// <param name="m12">A first row and second column value.</param>
		/// <param name="m13">A first row and third column value.</param>
		/// <param name="m14">A first row and fourth column value.</param>
		/// <param name="m21">A second row and first column value.</param>
		/// <param name="m22">A second row and second column value.</param>
		/// <param name="m23">A second row and third column value.</param>
		/// <param name="m24">A second row and fourth column value.</param>
		/// <param name="m31">A third row and first column value.</param>
		/// <param name="m32">A third row and second column value.</param>
		/// <param name="m33">A third row and third column value.</param>
		/// <param name="m34">A third row and fourth column value.</param>
		/// <param name="m41">A fourth row and first column value.</param>
		/// <param name="m42">A fourth row and second column value.</param>
		/// <param name="m43">A fourth row and third column value.</param>
		/// <param name="m44">A fourth row and fourth column value.</param>
        public Matrix4x4(
			int m11, int m12, int m13, int m14,
			int m21, int m22, int m23, int m24,
			int m31, int m32, int m33, int m34,
			int m41, int m42, int m43, int m44
		)
		{
			M11 = new Fix64(m11);
			M12 = new Fix64(m12);
			M13 = new Fix64(m13);
			M14 = new Fix64(m14);
			M21 = new Fix64(m21);
			M22 = new Fix64(m22);
			M23 = new Fix64(m23);
			M24 = new Fix64(m24);
			M31 = new Fix64(m31);
			M32 = new Fix64(m32);
			M33 = new Fix64(m33);
			M34 = new Fix64(m34);
			M41 = new Fix64(m41);
			M42 = new Fix64(m42);
			M43 = new Fix64(m43);
			M44 = new Fix64(m44);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a determinant of this <see cref="Matrix4x4"/>.
		/// </summary>
		/// <returns>Determinant of this <see cref="Matrix4x4"/></returns>
		/// <remarks>See more about determinant here - http://en.wikipedia.org/wiki/Determinant.
		/// </remarks>
		public Fix64 Determinant()
		{
			Fix64 num18 = (M33 * M44) - (M34 * M43);
			Fix64 num17 = (M32 * M44) - (M34 * M42);
			Fix64 num16 = (M32 * M43) - (M33 * M42);
			Fix64 num15 = (M31 * M44) - (M34 * M41);
			Fix64 num14 = (M31 * M43) - (M33 * M41);
			Fix64 num13 = (M31 * M42) - (M32 * M41);
			return (
				(
					(
						(M11 * (((M22 * num18) - (M23 * num17)) + (M24 * num16))) -
						(M12 * (((M21 * num18) - (M23 * num15)) + (M24 * num14)))
					) + (M13 * (((M21 * num17) - (M22 * num15)) + (M24 * num13)))
				) - (M14 * (((M21 * num16) - (M22 * num14)) + (M23 * num13)))
			);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Matrix4x4"/> without any tolerance.
		/// </summary>
		/// <param name="other">The <see cref="Matrix4x4"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Matrix4x4 other)
		{
			return (M11 == other.M11 &&
					M12 == other.M12 &&
					M13 == other.M13 &&
					M14 == other.M14 &&
					M21 == other.M21 &&
					M22 == other.M22 &&
					M23 == other.M23 &&
					M24 == other.M24 &&
					M31 == other.M31 &&
					M32 == other.M32 &&
					M33 == other.M33 &&
					M34 == other.M34 &&
					M41 == other.M41 &&
					M42 == other.M42 &&
					M43 == other.M43 &&
					M44 == other.M44);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/> without any tolerance.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			return (obj is Matrix4x4) && Equals((Matrix4x4) obj);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Matrix4x4"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Matrix4x4"/>.</returns>
		public override int GetHashCode()
		{
			return (
				M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
				M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
				M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
				M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode()
			);
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Matrix4x4"/> in the format:
		/// {M11:[<see cref="M11"/>] M12:[<see cref="M12"/>] M13:[<see cref="M13"/>] M14:[<see cref="M14"/>]}
		/// {M21:[<see cref="M21"/>] M12:[<see cref="M22"/>] M13:[<see cref="M23"/>] M14:[<see cref="M24"/>]}
		/// {M31:[<see cref="M31"/>] M32:[<see cref="M32"/>] M33:[<see cref="M33"/>] M34:[<see cref="M34"/>]}
		/// {M41:[<see cref="M41"/>] M42:[<see cref="M42"/>] M43:[<see cref="M43"/>] M44:[<see cref="M44"/>]}
		/// </summary>
		/// <returns>A <see cref="String"/> representation of this <see cref="Matrix4x4"/>.</returns>
		public override string ToString()
		{
			return (
				"{M11:" + M11.ToString() +
				" M12:" + M12.ToString() +
				" M13:" + M13.ToString() +
				" M14:" + M14.ToString() +
				"} {M21:" + M21.ToString() +
				" M22:" + M22.ToString() +
				" M23:" + M23.ToString() +
				" M24:" + M24.ToString() +
				"} {M31:" + M31.ToString() +
				" M32:" + M32.ToString() +
				" M33:" + M33.ToString() +
				" M34:" + M34.ToString() +
				"} {M41:" + M41.ToString() +
				" M42:" + M42.ToString() +
				" M43:" + M43.ToString() +
				" M44:" + M44.ToString() + "}"
			);
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <returns>The result of the matrix addition.</returns>
		public static Matrix4x4 Add(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			matrix1.M11 += matrix2.M11;
			matrix1.M12 += matrix2.M12;
			matrix1.M13 += matrix2.M13;
			matrix1.M14 += matrix2.M14;
			matrix1.M21 += matrix2.M21;
			matrix1.M22 += matrix2.M22;
			matrix1.M23 += matrix2.M23;
			matrix1.M24 += matrix2.M24;
			matrix1.M31 += matrix2.M31;
			matrix1.M32 += matrix2.M32;
			matrix1.M33 += matrix2.M33;
			matrix1.M34 += matrix2.M34;
			matrix1.M41 += matrix2.M41;
			matrix1.M42 += matrix2.M42;
			matrix1.M43 += matrix2.M43;
			matrix1.M44 += matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> which contains sum of two matrixes.
		/// </summary>
		/// <param name="matrix1">The first matrix to add.</param>
		/// <param name="matrix2">The second matrix to add.</param>
		/// <param name="result">The result of the matrix addition as an output parameter.</param>
		public static void Add(ref Matrix4x4 matrix1, ref Matrix4x4 matrix2, out Matrix4x4 result)
		{
			result.M11 = matrix1.M11 + matrix2.M11;
			result.M12 = matrix1.M12 + matrix2.M12;
			result.M13 = matrix1.M13 + matrix2.M13;
			result.M14 = matrix1.M14 + matrix2.M14;
			result.M21 = matrix1.M21 + matrix2.M21;
			result.M22 = matrix1.M22 + matrix2.M22;
			result.M23 = matrix1.M23 + matrix2.M23;
			result.M24 = matrix1.M24 + matrix2.M24;
			result.M31 = matrix1.M31 + matrix2.M31;
			result.M32 = matrix1.M32 + matrix2.M32;
			result.M33 = matrix1.M33 + matrix2.M33;
			result.M34 = matrix1.M34 + matrix2.M34;
			result.M41 = matrix1.M41 + matrix2.M41;
			result.M42 = matrix1.M42 + matrix2.M42;
			result.M43 = matrix1.M43 + matrix2.M43;
			result.M44 = matrix1.M44 + matrix2.M44;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> which contains the rotation moment around specified axis.
		/// </summary>
		/// <param name="axis">The axis of rotation.</param>
		/// <param name="angle">The angle of rotation in radians.</param>
		/// <returns>The rotation <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateFromAxisAngle(Vector3 axis, Fix64 angle)
		{
			Matrix4x4 result;
			CreateFromAxisAngle(ref axis, angle, out result);
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> which contains the rotation moment around specified axis.
		/// </summary>
		/// <param name="axis">The axis of rotation.</param>
		/// <param name="angle">The angle of rotation in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateFromAxisAngle(
			ref Vector3 axis,
			Fix64 angle,
			out Matrix4x4 result
		)
		{
			Fix64 x = axis.X;
			Fix64 y = axis.Y;
			Fix64 z = axis.Z;
			Fix64 num2 = (Fix64) System.Math.Sin((double) angle);
			Fix64 num = (Fix64) System.Math.Cos((double) angle);
			Fix64 num11 = x * x;
			Fix64 num10 = y * y;
			Fix64 num9 = z * z;
			Fix64 num8 = x * y;
			Fix64 num7 = x * z;
			Fix64 num6 = y * z;
			result.M11 = num11 + (num * (Fix64.One - num11));
			result.M12 = (num8 - (num * num8)) + (num2 * z);
			result.M13 = (num7 - (num * num7)) - (num2 * y);
			result.M14 = Fix64.Zero;
			result.M21 = (num8 - (num * num8)) - (num2 * z);
			result.M22 = num10 + (num * (Fix64.One - num10));
			result.M23 = (num6 - (num * num6)) + (num2 * x);
			result.M24 = Fix64.Zero;
			result.M31 = (num7 - (num * num7)) + (num2 * y);
			result.M32 = (num6 - (num * num6)) - (num2 * x);
			result.M33 = num9 + (num * (Fix64.One - num9));
			result.M34 = Fix64.Zero;
			result.M41 = Fix64.Zero;
			result.M42 = Fix64.Zero;
			result.M43 = Fix64.Zero;
			result.M44 = Fix64.One;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> from a <see cref="FixQuaternion"/>.
		/// </summary>
		/// <param name="quaternion"><see cref="FixQuaternion"/> of rotation moment.</param>
		/// <returns>The rotation <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateFromQuaternion(Quaternion quaternion)
		{
			Matrix4x4 result;
			CreateFromQuaternion(ref quaternion, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> from a <see cref="FixQuaternion"/>.
		/// </summary>
		/// <param name="quaternion"><see cref="FixQuaternion"/> of rotation moment.</param>
		/// <param name="result">The rotation <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix4x4 result)
		{
			Fix64 two = new Fix64(2);
			Fix64 num9 = quaternion.X * quaternion.X;
			Fix64 num8 = quaternion.Y * quaternion.Y;
			Fix64 num7 = quaternion.Z * quaternion.Z;
			Fix64 num6 = quaternion.X * quaternion.Y;
			Fix64 num5 = quaternion.Z * quaternion.W;
			Fix64 num4 = quaternion.Z * quaternion.X;
			Fix64 num3 = quaternion.Y * quaternion.W;
			Fix64 num2 = quaternion.Y * quaternion.Z;
			Fix64 num = quaternion.X * quaternion.W;
			result.M11 = Fix64.One - (two * (num8 + num7));
			result.M12 = two * (num6 + num5);
			result.M13 = two * (num4 - num3);
			result.M14 = Fix64.Zero;
			result.M21 = two * (num6 - num5);
			result.M22 = Fix64.One - (two * (num7 + num9));
			result.M23 = two * (num2 + num);
			result.M24 = Fix64.Zero;
			result.M31 = two * (num4 + num3);
			result.M32 = two * (num2 - num);
			result.M33 = Fix64.One - (two * (num8 + num9));
			result.M34 = Fix64.Zero;
			result.M41 = Fix64.Zero;
			result.M42 = Fix64.Zero;
			result.M43 = Fix64.Zero;
			result.M44 = Fix64.One;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> from the specified yaw, pitch and roll values.
		/// </summary>
		/// <param name="yaw">The yaw rotation value in radians.</param>
		/// <param name="pitch">The pitch rotation value in radians.</param>
		/// <param name="roll">The roll rotation value in radians.</param>
		/// <returns>The rotation <see cref="Matrix4x4"/>.</returns>
		/// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
		/// </remarks>
		public static Matrix4x4 CreateFromYawPitchRoll(Fix64 yaw, Fix64 pitch, Fix64 roll)
		{
			Matrix4x4 matrix;
			CreateFromYawPitchRoll(yaw, pitch, roll, out matrix);
			return matrix;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> from the specified yaw, pitch and roll values.
		/// </summary>
		/// <param name="yaw">The yaw rotation value in radians.</param>
		/// <param name="pitch">The pitch rotation value in radians.</param>
		/// <param name="roll">The roll rotation value in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4x4"/> as an output parameter.</param>
		/// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
		/// </remarks>
		public static void CreateFromYawPitchRoll(
			Fix64 yaw,
			Fix64 pitch,
			Fix64 roll,
			out Matrix4x4 result
		)
		{
			Quaternion quaternion;
			Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out quaternion);
			CreateFromQuaternion(ref quaternion, out result);
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> around X axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix4x4"/> around X axis.</returns>
		public static Matrix4x4 CreateRotationX(Fix64 radians)
		{
			Matrix4x4 result;
			CreateRotationX(radians, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> around X axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4x4"/> around X axis as an output parameter.</param>
		public static void CreateRotationX(Fix64 radians, out Matrix4x4 result)
		{
			result = Matrix4x4.Identity;

			Fix64 val1 = Fix64.Cos(radians);
			Fix64 val2 = Fix64.Sin(radians);

			result.M22 = val1;
			result.M23 = val2;
			result.M32 = -val2;
			result.M33 = val1;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> around Y axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix4x4"/> around Y axis.</returns>
		public static Matrix4x4 CreateRotationY(Fix64 radians)
		{
			Matrix4x4 result;
			CreateRotationY(radians, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> around Y axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4x4"/> around Y axis as an output parameter.</param>
		public static void CreateRotationY(Fix64 radians, out Matrix4x4 result)
		{
			result = Matrix4x4.Identity;

			Fix64 val1 = Fix64.Cos(radians);
			Fix64 val2 = Fix64.Sin(radians);

			result.M11 = val1;
			result.M13 = -val2;
			result.M31 = val2;
			result.M33 = val1;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <returns>The rotation <see cref="Matrix4x4"/> around Z axis.</returns>
		public static Matrix4x4 CreateRotationZ(Fix64 radians)
		{
			Matrix4x4 result;
			CreateRotationZ(radians, out result);
			return result;
		}

		/// <summary>
		/// Creates a new rotation <see cref="Matrix4x4"/> around Z axis.
		/// </summary>
		/// <param name="radians">Angle in radians.</param>
		/// <param name="result">The rotation <see cref="Matrix4x4"/> around Z axis as an output parameter.</param>
		public static void CreateRotationZ(Fix64 radians, out Matrix4x4 result)
		{
			result = Matrix4x4.Identity;

			Fix64 val1 = Fix64.Cos(radians);
			Fix64 val2 = Fix64.Sin(radians);

			result.M11 = val1;
			result.M12 = val2;
			result.M21 = -val2;
			result.M22 = val1;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="scale">Scale value for all three axises.</param>
		/// <returns>The scaling <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateScale(Fix64 scale)
		{
			Matrix4x4 result;
			CreateScale(scale, scale, scale, out result);
			return result;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="scale">Scale value for all three axises.</param>
		/// <param name="result">The scaling <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateScale(Fix64 scale, out Matrix4x4 result)
		{
			CreateScale(scale, scale, scale, out result);
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <param name="zScale">Scale value for Z axis.</param>
		/// <returns>The scaling <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateScale(Fix64 xScale, Fix64 yScale, Fix64 zScale)
		{
			Matrix4x4 result;
			CreateScale(xScale, yScale, zScale, out result);
			return result;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="xScale">Scale value for X axis.</param>
		/// <param name="yScale">Scale value for Y axis.</param>
		/// <param name="zScale">Scale value for Z axis.</param>
		/// <param name="result">The scaling <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateScale(
			Fix64 xScale,
			Fix64 yScale,
			Fix64 zScale,
			out Matrix4x4 result
		)
		{
			result.M11 = xScale;
			result.M12 = Fix64.Zero;
			result.M13 = Fix64.Zero;
			result.M14 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = yScale;
			result.M23 = Fix64.Zero;
			result.M24 = Fix64.Zero;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;
			result.M33 = zScale;
			result.M34 = Fix64.Zero;
			result.M41 = Fix64.Zero;
			result.M42 = Fix64.Zero;
			result.M43 = Fix64.Zero;
			result.M44 = Fix64.One;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="scales"><see cref="FixVector3"/> representing x,y and z scale values.</param>
		/// <returns>The scaling <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateScale(Vector3 scales)
		{
			Matrix4x4 result;
			CreateScale(ref scales, out result);
			return result;
		}

		/// <summary>
		/// Creates a new scaling <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="scales"><see cref="FixVector3"/> representing x,y and z scale values.</param>
		/// <param name="result">The scaling <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateScale(ref Vector3 scales, out Matrix4x4 result)
		{
			result.M11 = scales.X;
			result.M12 = Fix64.Zero;
			result.M13 = Fix64.Zero;
			result.M14 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = scales.Y;
			result.M23 = Fix64.Zero;
			result.M24 = Fix64.Zero;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;
			result.M33 = scales.Z;
			result.M34 = Fix64.Zero;
			result.M41 = Fix64.Zero;
			result.M42 = Fix64.Zero;
			result.M43 = Fix64.Zero;
			result.M44 = Fix64.One;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="zPosition">Z coordinate of translation.</param>
		/// <returns>The translation <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateTranslation(
			Fix64 xPosition,
			Fix64 yPosition,
			Fix64 zPosition
		)
		{
			Matrix4x4 result;
			CreateTranslation(xPosition, yPosition, zPosition, out result);
			return result;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <param name="result">The translation <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateTranslation(ref Vector3 position, out Matrix4x4 result)
		{
			result.M11 = Fix64.One;
			result.M12 = Fix64.Zero;
			result.M13 = Fix64.Zero;
			result.M14 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = Fix64.One;
			result.M23 = Fix64.Zero;
			result.M24 = Fix64.Zero;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;
			result.M33 = Fix64.One;
			result.M34 = Fix64.Zero;
			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
			result.M44 = Fix64.One;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <returns>The translation <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateTranslation(Vector3 position)
		{
			Matrix4x4 result;
			CreateTranslation(ref position, out result);
			return result;
		}

		/// <summary>
		/// Creates a new translation <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="zPosition">Z coordinate of translation.</param>
		/// <param name="result">The translation <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateTranslation(
			Fix64 xPosition,
			Fix64 yPosition,
			Fix64 zPosition,
			out Matrix4x4 result
		)
		{
			result.M11 = Fix64.One;
			result.M12 = Fix64.Zero;
			result.M13 = Fix64.Zero;
			result.M14 = Fix64.Zero;
			result.M21 = Fix64.Zero;
			result.M22 = Fix64.One;
			result.M23 = Fix64.Zero;
			result.M24 = Fix64.Zero;
			result.M31 = Fix64.Zero;
			result.M32 = Fix64.Zero;
			result.M33 = Fix64.One;
			result.M34 = Fix64.Zero;
			result.M41 = xPosition;
			result.M42 = yPosition;
			result.M43 = zPosition;
			result.M44 = Fix64.One;
		}

		/// <summary>
		/// Creates a new world <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="position">The position vector.</param>
		/// <param name="forward">The forward direction vector.</param>
		/// <param name="up">The upward direction vector. Usually <see cref="FixVector3.Up"/>.</param>
		/// <returns>The world <see cref="Matrix4x4"/>.</returns>
		public static Matrix4x4 CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
		{
			Matrix4x4 ret;
			CreateWorld(ref position, ref forward, ref up, out ret);
			return ret;
		}

		/// <summary>
		/// Creates a new world <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="position">The position vector.</param>
		/// <param name="forward">The forward direction vector.</param>
		/// <param name="up">The upward direction vector. Usually <see cref="FixVector3.Up"/>.</param>
		/// <param name="result">The world <see cref="Matrix4x4"/> as an output parameter.</param>
		public static void CreateWorld(
			ref Vector3 position,
			ref Vector3 forward,
			ref Vector3 up,
			out Matrix4x4 result
		)
		{
			Vector3 x, y, z;
			z = Vector3.Normalize(forward);
			Vector3.Cross(ref forward, ref up, out x);
			Vector3.Cross(ref x, ref forward, out y);
			x.Normalize();
			y.Normalize();

			result = new Matrix4x4();
			result.Right = x;
			result.Up = y;
			result.Forward = z;
			result.Translation = position;
			result.M44 = Fix64.One;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4x4"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix4x4"/>.</param>
		/// <returns>The result of dividing the matrix.</returns>
		public static Matrix4x4 Divide(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			matrix1.M11 = matrix1.M11 / matrix2.M11;
			matrix1.M12 = matrix1.M12 / matrix2.M12;
			matrix1.M13 = matrix1.M13 / matrix2.M13;
			matrix1.M14 = matrix1.M14 / matrix2.M14;
			matrix1.M21 = matrix1.M21 / matrix2.M21;
			matrix1.M22 = matrix1.M22 / matrix2.M22;
			matrix1.M23 = matrix1.M23 / matrix2.M23;
			matrix1.M24 = matrix1.M24 / matrix2.M24;
			matrix1.M31 = matrix1.M31 / matrix2.M31;
			matrix1.M32 = matrix1.M32 / matrix2.M32;
			matrix1.M33 = matrix1.M33 / matrix2.M33;
			matrix1.M34 = matrix1.M34 / matrix2.M34;
			matrix1.M41 = matrix1.M41 / matrix2.M41;
			matrix1.M42 = matrix1.M42 / matrix2.M42;
			matrix1.M43 = matrix1.M43 / matrix2.M43;
			matrix1.M44 = matrix1.M44 / matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4x4"/> by the elements of another matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix4x4"/>.</param>
		/// <param name="result">The result of dividing the matrix as an output parameter.</param>
		public static void Divide(ref Matrix4x4 matrix1, ref Matrix4x4 matrix2, out Matrix4x4 result)
		{
			result.M11 = matrix1.M11 / matrix2.M11;
			result.M12 = matrix1.M12 / matrix2.M12;
			result.M13 = matrix1.M13 / matrix2.M13;
			result.M14 = matrix1.M14 / matrix2.M14;
			result.M21 = matrix1.M21 / matrix2.M21;
			result.M22 = matrix1.M22 / matrix2.M22;
			result.M23 = matrix1.M23 / matrix2.M23;
			result.M24 = matrix1.M24 / matrix2.M24;
			result.M31 = matrix1.M31 / matrix2.M31;
			result.M32 = matrix1.M32 / matrix2.M32;
			result.M33 = matrix1.M33 / matrix2.M33;
			result.M34 = matrix1.M34 / matrix2.M34;
			result.M41 = matrix1.M41 / matrix2.M41;
			result.M42 = matrix1.M42 / matrix2.M42;
			result.M43 = matrix1.M43 / matrix2.M43;
			result.M44 = matrix1.M44 / matrix2.M44;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4x4"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		public static Matrix4x4 Divide(Matrix4x4 matrix1, Fix64 divider)
		{
			Fix64 num = Fix64.One / divider;
			matrix1.M11 = matrix1.M11 * num;
			matrix1.M12 = matrix1.M12 * num;
			matrix1.M13 = matrix1.M13 * num;
			matrix1.M14 = matrix1.M14 * num;
			matrix1.M21 = matrix1.M21 * num;
			matrix1.M22 = matrix1.M22 * num;
			matrix1.M23 = matrix1.M23 * num;
			matrix1.M24 = matrix1.M24 * num;
			matrix1.M31 = matrix1.M31 * num;
			matrix1.M32 = matrix1.M32 * num;
			matrix1.M33 = matrix1.M33 * num;
			matrix1.M34 = matrix1.M34 * num;
			matrix1.M41 = matrix1.M41 * num;
			matrix1.M42 = matrix1.M42 * num;
			matrix1.M43 = matrix1.M43 * num;
			matrix1.M44 = matrix1.M44 * num;
			return matrix1;
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4x4"/> by a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
		public static void Divide(ref Matrix4x4 matrix1, Fix64 divider, out Matrix4x4 result)
		{
			Fix64 num = Fix64.One / divider;
			result.M11 = matrix1.M11 * num;
			result.M12 = matrix1.M12 * num;
			result.M13 = matrix1.M13 * num;
			result.M14 = matrix1.M14 * num;
			result.M21 = matrix1.M21 * num;
			result.M22 = matrix1.M22 * num;
			result.M23 = matrix1.M23 * num;
			result.M24 = matrix1.M24 * num;
			result.M31 = matrix1.M31 * num;
			result.M32 = matrix1.M32 * num;
			result.M33 = matrix1.M33 * num;
			result.M34 = matrix1.M34 * num;
			result.M41 = matrix1.M41 * num;
			result.M42 = matrix1.M42 * num;
			result.M43 = matrix1.M43 * num;
			result.M44 = matrix1.M44 * num;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix4x4"/>.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		public static Matrix4x4 Multiply(
			Matrix4x4 matrix1,
			Matrix4x4 matrix2
		)
		{
			Fix64 m11 = (
				(matrix1.M11 * matrix2.M11) +
				(matrix1.M12 * matrix2.M21) +
				(matrix1.M13 * matrix2.M31) +
				(matrix1.M14 * matrix2.M41)
			);
			Fix64 m12 = (
				(matrix1.M11 * matrix2.M12) +
				(matrix1.M12 * matrix2.M22) +
				(matrix1.M13 * matrix2.M32) +
				(matrix1.M14 * matrix2.M42)
			);
			Fix64 m13 = (
				(matrix1.M11 * matrix2.M13) +
				(matrix1.M12 * matrix2.M23) +
				(matrix1.M13 * matrix2.M33) +
				(matrix1.M14 * matrix2.M43)
			);
			Fix64 m14 = (
				(matrix1.M11 * matrix2.M14) +
				(matrix1.M12 * matrix2.M24) +
				(matrix1.M13 * matrix2.M34) +
				(matrix1.M14 * matrix2.M44)
			);
			Fix64 m21 = (
				(matrix1.M21 * matrix2.M11) +
				(matrix1.M22 * matrix2.M21) +
				(matrix1.M23 * matrix2.M31) +
				(matrix1.M24 * matrix2.M41)
			);
			Fix64 m22 = (
				(matrix1.M21 * matrix2.M12) +
				(matrix1.M22 * matrix2.M22) +
				(matrix1.M23 * matrix2.M32) +
				(matrix1.M24 * matrix2.M42)
			);
			Fix64 m23 = (
				(matrix1.M21 * matrix2.M13) +
				(matrix1.M22 * matrix2.M23) +
				(matrix1.M23 * matrix2.M33) +
				(matrix1.M24 * matrix2.M43)
			);
			Fix64 m24 = (
				(matrix1.M21 * matrix2.M14) +
				(matrix1.M22 * matrix2.M24) +
				(matrix1.M23 * matrix2.M34) +
				(matrix1.M24 * matrix2.M44)
			);
			Fix64 m31 = (
				(matrix1.M31 * matrix2.M11) +
				(matrix1.M32 * matrix2.M21) +
				(matrix1.M33 * matrix2.M31) +
				(matrix1.M34 * matrix2.M41)
			);
			Fix64 m32 = (
				(matrix1.M31 * matrix2.M12) +
				(matrix1.M32 * matrix2.M22) +
				(matrix1.M33 * matrix2.M32) +
				(matrix1.M34 * matrix2.M42)
			);
			Fix64 m33 = (
				(matrix1.M31 * matrix2.M13) +
				(matrix1.M32 * matrix2.M23) +
				(matrix1.M33 * matrix2.M33) +
				(matrix1.M34 * matrix2.M43)
			);
			Fix64 m34 = (
				(matrix1.M31 * matrix2.M14) +
				(matrix1.M32 * matrix2.M24) +
				(matrix1.M33 * matrix2.M34) +
				(matrix1.M34 * matrix2.M44)
			);
			Fix64 m41 = (
				(matrix1.M41 * matrix2.M11) +
				(matrix1.M42 * matrix2.M21) +
				(matrix1.M43 * matrix2.M31) +
				(matrix1.M44 * matrix2.M41)
			);
			Fix64 m42 = (
				(matrix1.M41 * matrix2.M12) +
				(matrix1.M42 * matrix2.M22) +
				(matrix1.M43 * matrix2.M32) +
				(matrix1.M44 * matrix2.M42)
			);
			Fix64 m43 = (
				(matrix1.M41 * matrix2.M13) +
				(matrix1.M42 * matrix2.M23) +
				(matrix1.M43 * matrix2.M33) +
				(matrix1.M44 * matrix2.M43)
			);
			Fix64 m44 = (
				(matrix1.M41 * matrix2.M14) +
				(matrix1.M42 * matrix2.M24) +
				(matrix1.M43 * matrix2.M34) +
				(matrix1.M44 * matrix2.M44)
			);
			matrix1.M11 = m11;
			matrix1.M12 = m12;
			matrix1.M13 = m13;
			matrix1.M14 = m14;
			matrix1.M21 = m21;
			matrix1.M22 = m22;
			matrix1.M23 = m23;
			matrix1.M24 = m24;
			matrix1.M31 = m31;
			matrix1.M32 = m32;
			matrix1.M33 = m33;
			matrix1.M34 = m34;
			matrix1.M41 = m41;
			matrix1.M42 = m42;
			matrix1.M43 = m43;
			matrix1.M44 = m44;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="result">Result of the matrix multiplication as an output parameter.</param>
		public static void Multiply(ref Matrix4x4 matrix1, ref Matrix4x4 matrix2, out Matrix4x4 result)
		{
			Fix64 m11 = (
				(matrix1.M11 * matrix2.M11) +
				(matrix1.M12 * matrix2.M21) +
				(matrix1.M13 * matrix2.M31) +
				(matrix1.M14 * matrix2.M41)
			);
			Fix64 m12 = (
				(matrix1.M11 * matrix2.M12) +
				(matrix1.M12 * matrix2.M22) +
				(matrix1.M13 * matrix2.M32) +
				(matrix1.M14 * matrix2.M42)
			);
			Fix64 m13 = (
				(matrix1.M11 * matrix2.M13) +
				(matrix1.M12 * matrix2.M23) +
				(matrix1.M13 * matrix2.M33) +
				(matrix1.M14 * matrix2.M43)
			);
			Fix64 m14 = (
				(matrix1.M11 * matrix2.M14) +
				(matrix1.M12 * matrix2.M24) +
				(matrix1.M13 * matrix2.M34) +
				(matrix1.M14 * matrix2.M44)
			);
			Fix64 m21 = (
				(matrix1.M21 * matrix2.M11) +
				(matrix1.M22 * matrix2.M21) +
				(matrix1.M23 * matrix2.M31) +
				(matrix1.M24 * matrix2.M41)
			);
			Fix64 m22 = (
				(matrix1.M21 * matrix2.M12) +
				(matrix1.M22 * matrix2.M22) +
				(matrix1.M23 * matrix2.M32) +
				(matrix1.M24 * matrix2.M42)
			);
			Fix64 m23 = (
				(matrix1.M21 * matrix2.M13) +
				(matrix1.M22 * matrix2.M23) +
				(matrix1.M23 * matrix2.M33) +
				(matrix1.M24 * matrix2.M43)
				);
			Fix64 m24 = (
				(matrix1.M21 * matrix2.M14) +
				(matrix1.M22 * matrix2.M24) +
				(matrix1.M23 * matrix2.M34) +
				(matrix1.M24 * matrix2.M44)
			);
			Fix64 m31 = (
				(matrix1.M31 * matrix2.M11) +
				(matrix1.M32 * matrix2.M21) +
				(matrix1.M33 * matrix2.M31) +
				(matrix1.M34 * matrix2.M41)
			);
			Fix64 m32 = (
				(matrix1.M31 * matrix2.M12) +
				(matrix1.M32 * matrix2.M22) +
				(matrix1.M33 * matrix2.M32) +
				(matrix1.M34 * matrix2.M42)
			);
			Fix64 m33 = (
				(matrix1.M31 * matrix2.M13) +
				(matrix1.M32 * matrix2.M23) +
				(matrix1.M33 * matrix2.M33) +
				(matrix1.M34 * matrix2.M43)
			);
			Fix64 m34 = (
				(matrix1.M31 * matrix2.M14) +
				(matrix1.M32 * matrix2.M24) +
				(matrix1.M33 * matrix2.M34) +
				(matrix1.M34 * matrix2.M44)
			);
			Fix64 m41 = (
				(matrix1.M41 * matrix2.M11) +
				(matrix1.M42 * matrix2.M21) +
				(matrix1.M43 * matrix2.M31) +
				(matrix1.M44 * matrix2.M41)
			);
			Fix64 m42 = (
				(matrix1.M41 * matrix2.M12) +
				(matrix1.M42 * matrix2.M22) +
				(matrix1.M43 * matrix2.M32) +
				(matrix1.M44 * matrix2.M42)
			);
			Fix64 m43 = (
				(matrix1.M41 * matrix2.M13) +
				(matrix1.M42 * matrix2.M23) +
				(matrix1.M43 * matrix2.M33) +
				(matrix1.M44 * matrix2.M43)
			);
			Fix64 m44 = (
				(matrix1.M41 * matrix2.M14) +
				(matrix1.M42 * matrix2.M24) +
				(matrix1.M43 * matrix2.M34) +
				(matrix1.M44 * matrix2.M44)
			);
			result.M11 = m11;
			result.M12 = m12;
			result.M13 = m13;
			result.M14 = m14;
			result.M21 = m21;
			result.M22 = m22;
			result.M23 = m23;
			result.M24 = m24;
			result.M31 = m31;
			result.M32 = m32;
			result.M33 = m33;
			result.M34 = m34;
			result.M41 = m41;
			result.M42 = m42;
			result.M43 = m43;
			result.M44 = m44;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> that contains a multiplication of <see cref="Matrix4x4"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		public static Matrix4x4 Multiply(Matrix4x4 matrix1, Fix64 scaleFactor)
		{
			matrix1.M11 *= scaleFactor;
			matrix1.M12 *= scaleFactor;
			matrix1.M13 *= scaleFactor;
			matrix1.M14 *= scaleFactor;
			matrix1.M21 *= scaleFactor;
			matrix1.M22 *= scaleFactor;
			matrix1.M23 *= scaleFactor;
			matrix1.M24 *= scaleFactor;
			matrix1.M31 *= scaleFactor;
			matrix1.M32 *= scaleFactor;
			matrix1.M33 *= scaleFactor;
			matrix1.M34 *= scaleFactor;
			matrix1.M41 *= scaleFactor;
			matrix1.M42 *= scaleFactor;
			matrix1.M43 *= scaleFactor;
			matrix1.M44 *= scaleFactor;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> that contains a multiplication of <see cref="Matrix4x4"/> and a scalar.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
		public static void Multiply(ref Matrix4x4 matrix1, Fix64 scaleFactor, out Matrix4x4 result)
		{
			result.M11 = matrix1.M11 * scaleFactor;
			result.M12 = matrix1.M12 * scaleFactor;
			result.M13 = matrix1.M13 * scaleFactor;
			result.M14 = matrix1.M14 * scaleFactor;
			result.M21 = matrix1.M21 * scaleFactor;
			result.M22 = matrix1.M22 * scaleFactor;
			result.M23 = matrix1.M23 * scaleFactor;
			result.M24 = matrix1.M24 * scaleFactor;
			result.M31 = matrix1.M31 * scaleFactor;
			result.M32 = matrix1.M32 * scaleFactor;
			result.M33 = matrix1.M33 * scaleFactor;
			result.M34 = matrix1.M34 * scaleFactor;
			result.M41 = matrix1.M41 * scaleFactor;
			result.M42 = matrix1.M42 * scaleFactor;
			result.M43 = matrix1.M43 * scaleFactor;
			result.M44 = matrix1.M44 * scaleFactor;

		}

		/// <summary>
		/// Returns a matrix with the all values negated.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4x4"/>.</param>
		/// <returns>Result of the matrix negation.</returns>
		public static Matrix4x4 Negate(Matrix4x4 matrix)
		{
			matrix.M11 = -matrix.M11;
			matrix.M12 = -matrix.M12;
			matrix.M13 = -matrix.M13;
			matrix.M14 = -matrix.M14;
			matrix.M21 = -matrix.M21;
			matrix.M22 = -matrix.M22;
			matrix.M23 = -matrix.M23;
			matrix.M24 = -matrix.M24;
			matrix.M31 = -matrix.M31;
			matrix.M32 = -matrix.M32;
			matrix.M33 = -matrix.M33;
			matrix.M34 = -matrix.M34;
			matrix.M41 = -matrix.M41;
			matrix.M42 = -matrix.M42;
			matrix.M43 = -matrix.M43;
			matrix.M44 = -matrix.M44;
			return matrix;
		}

		/// <summary>
		/// Returns a matrix with the all values negated.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4x4"/>.</param>
		/// <param name="result">Result of the matrix negation as an output parameter.</param>
		public static void Negate(ref Matrix4x4 matrix, out Matrix4x4 result)
		{
			result.M11 = -matrix.M11;
			result.M12 = -matrix.M12;
			result.M13 = -matrix.M13;
			result.M14 = -matrix.M14;
			result.M21 = -matrix.M21;
			result.M22 = -matrix.M22;
			result.M23 = -matrix.M23;
			result.M24 = -matrix.M24;
			result.M31 = -matrix.M31;
			result.M32 = -matrix.M32;
			result.M33 = -matrix.M33;
			result.M34 = -matrix.M34;
			result.M41 = -matrix.M41;
			result.M42 = -matrix.M42;
			result.M43 = -matrix.M43;
			result.M44 = -matrix.M44;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix4x4"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix4x4"/>.</param>
		/// <returns>The result of the matrix subtraction.</returns>
		public static Matrix4x4 Subtract(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			matrix1.M11 -= matrix2.M11;
			matrix1.M12 -= matrix2.M12;
			matrix1.M13 -= matrix2.M13;
			matrix1.M14 -= matrix2.M14;
			matrix1.M21 -= matrix2.M21;
			matrix1.M22 -= matrix2.M22;
			matrix1.M23 -= matrix2.M23;
			matrix1.M24 -= matrix2.M24;
			matrix1.M31 -= matrix2.M31;
			matrix1.M32 -= matrix2.M32;
			matrix1.M33 -= matrix2.M33;
			matrix1.M34 -= matrix2.M34;
			matrix1.M41 -= matrix2.M41;
			matrix1.M42 -= matrix2.M42;
			matrix1.M43 -= matrix2.M43;
			matrix1.M44 -= matrix2.M44;
			return matrix1;
		}

		/// <summary>
		/// Creates a new <see cref="Matrix4x4"/> that contains subtraction of one matrix from another.
		/// </summary>
		/// <param name="matrix1">The first <see cref="Matrix4x4"/>.</param>
		/// <param name="matrix2">The second <see cref="Matrix4x4"/>.</param>
		/// <param name="result">The result of the matrix subtraction as an output parameter.</param>
		public static void Subtract(ref Matrix4x4 matrix1, ref Matrix4x4 matrix2, out Matrix4x4 result)
		{
			result.M11 = matrix1.M11 - matrix2.M11;
			result.M12 = matrix1.M12 - matrix2.M12;
			result.M13 = matrix1.M13 - matrix2.M13;
			result.M14 = matrix1.M14 - matrix2.M14;
			result.M21 = matrix1.M21 - matrix2.M21;
			result.M22 = matrix1.M22 - matrix2.M22;
			result.M23 = matrix1.M23 - matrix2.M23;
			result.M24 = matrix1.M24 - matrix2.M24;
			result.M31 = matrix1.M31 - matrix2.M31;
			result.M32 = matrix1.M32 - matrix2.M32;
			result.M33 = matrix1.M33 - matrix2.M33;
			result.M34 = matrix1.M34 - matrix2.M34;
			result.M41 = matrix1.M41 - matrix2.M41;
			result.M42 = matrix1.M42 - matrix2.M42;
			result.M43 = matrix1.M43 - matrix2.M43;
			result.M44 = matrix1.M44 - matrix2.M44;
		}

		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <returns>The new <see cref="Matrix4x4"/> which contains the transposing result.</returns>
		public static Matrix4x4 Transpose(Matrix4x4 matrix)
		{
			Matrix4x4 ret;
			Transpose(ref matrix, out ret);
			return ret;
		}

		/// <summary>
		/// Swap the matrix rows and columns.
		/// </summary>
		/// <param name="matrix">The matrix for transposing operation.</param>
		/// <param name="result">The new <see cref="Matrix4x4"/> which contains the transposing result as an output parameter.</param>
		public static void Transpose(ref Matrix4x4 matrix, out Matrix4x4 result)
		{
			Matrix4x4 ret;

			ret.M11 = matrix.M11;
			ret.M12 = matrix.M21;
			ret.M13 = matrix.M31;
			ret.M14 = matrix.M41;

			ret.M21 = matrix.M12;
			ret.M22 = matrix.M22;
			ret.M23 = matrix.M32;
			ret.M24 = matrix.M42;

			ret.M31 = matrix.M13;
			ret.M32 = matrix.M23;
			ret.M33 = matrix.M33;
			ret.M34 = matrix.M43;

			ret.M41 = matrix.M14;
			ret.M42 = matrix.M24;
			ret.M43 = matrix.M34;
			ret.M44 = matrix.M44;

			result = ret;
		}

		public static Matrix4x4 Transform(Matrix4x4 value, Quaternion rotation)
		{
			Matrix4x4 result;
			Transform(ref value, ref rotation, out result);
			return result;
		}

		public static void Transform(
			ref Matrix4x4 value,
			ref Quaternion rotation,
			out Matrix4x4 result
		)
		{
			Matrix4x4 rotMatrix = CreateFromQuaternion(rotation);
			Multiply(ref value, ref rotMatrix, out result);
		}

		#endregion

		#region Public Static Operator Overloads

		/// <summary>
		/// Adds two matrixes.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/> on the left of the add sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4x4"/> on the right of the add sign.</param>
		/// <returns>Sum of the matrixes.</returns>
		public static Matrix4x4 operator +(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			return Matrix4x4.Add(matrix1, matrix2);
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4x4"/> by the elements of another <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/> on the left of the div sign.</param>
		/// <param name="matrix2">Divisor <see cref="Matrix4x4"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the matrixes.</returns>
		public static Matrix4x4 operator /(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			return Matrix4x4.Divide(matrix1, matrix2);
		}

		/// <summary>
		/// Divides the elements of a <see cref="Matrix4x4"/> by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4x4"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing a matrix by a scalar.</returns>
		public static Matrix4x4 operator /(Matrix4x4 matrix, Fix64 divider)
		{
			return Matrix4x4.Divide(matrix, divider);
		}

		/// <summary>
		/// Compares whether two <see cref="Matrix4x4"/> instances are equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/> on the left of the equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4x4"/> on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			return matrix1.Equals(matrix2);
		}

		/// <summary>
		/// Compares whether two <see cref="Matrix4x4"/> instances are not equal without any tolerance.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/> on the left of the not equal sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4x4"/> on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			return !matrix1.Equals(matrix2);
		}

		/// <summary>
		/// Multiplies two matrixes.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/> on the left of the mul sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4x4"/> on the right of the mul sign.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		/// <remarks>
		/// Using matrix multiplication algorithm - see http://en.wikipedia.org/wiki/Matrix_multiplication.
		/// </remarks>
		public static Matrix4x4 operator *(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			return Multiply(matrix1, matrix2);
		}

		/// <summary>
		/// Multiplies the elements of matrix by a scalar.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4x4"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the matrix multiplication with a scalar.</returns>
		public static Matrix4x4 operator *(Matrix4x4 matrix, Fix64 scaleFactor)
		{
			return Multiply(matrix, scaleFactor);
		}

		/// <summary>
		/// Subtracts the values of one <see cref="Matrix4x4"/> from another <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix4x4"/> on the left of the sub sign.</param>
		/// <param name="matrix2">Source <see cref="Matrix4x4"/> on the right of the sub sign.</param>
		/// <returns>Result of the matrix subtraction.</returns>
		public static Matrix4x4 operator -(Matrix4x4 matrix1, Matrix4x4 matrix2)
		{
			return Subtract(matrix1, matrix2);
		}

		/// <summary>
		/// Inverts values in the specified <see cref="Matrix4x4"/>.
		/// </summary>
		/// <param name="matrix">Source <see cref="Matrix4x4"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Matrix4x4 operator -(Matrix4x4 matrix)
		{
			return Negate(matrix);
		}

		#endregion
	}
}
