// This source is heavily borrowed from https://github.com/asik/FixedMath.Net

using System;
using System.Runtime.CompilerServices;

namespace MoonWorks.Math.Fixed
{
	public struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>
	{
		private readonly long RawValue;

		const long MAX_VALUE = long.MaxValue;
		const long MIN_VALUE = long.MinValue;
		const int FRACTIONAL_PLACES = 32;
		const int NUM_BITS = 64;
		const long ONE = 1L << FRACTIONAL_PLACES;
		const long PI_TIMES_2 = 0x6487ED511;
		const long PI = 0x3243F6A88;
		const long PI_OVER_2 = 0x1921FB544;

		public static readonly Fix64 MaxValue = new Fix64(MAX_VALUE);
		public static readonly Fix64 MinValue = new Fix64(MIN_VALUE);
		public static readonly Fix64 One = new Fix64(ONE);
		public static readonly Fix64 Zero = new Fix64(0);

		public static readonly Fix64 Pi = new Fix64(PI);
		public static readonly Fix64 PiOver2 = new Fix64(PI_OVER_2);
		public static readonly Fix64 PiOver4 = PiOver2 / new Fix64(2);
		public static readonly Fix64 PiTimes2 = new Fix64(PI_TIMES_2);

		const int LUT_SIZE = (int)(PI_OVER_2 >> 15);
		static readonly Fix64 LutInterval = (Fix64)(LUT_SIZE - 1) / PiOver2;

		public bool IsFractional => (RawValue & 0x00000000FFFFFFFF) != 0;
		public bool IsIntegral => (RawValue & 0x00000000FFFFFFFF) == 0;

		private Fix64(long value)
		{
			RawValue = value;
		}

		public Fix64(int value)
		{
			RawValue = value * ONE;
		}

		/// <summary>
		/// Create a fractional Fix64 number of the value (numerator / denominator).
		/// </summary>
		public static Fix64 FromFraction(int numerator, int denominator)
		{
			return new Fix64(numerator) / new Fix64(denominator);
		}

		/// <summary>
		/// Gets the fractional component of this Fix64 value.
		/// </summary>
		public static Fix64 Fractional(Fix64 number)
		{
			return new Fix64(number.RawValue & 0x00000000FFFFFFFF);
		}

		public static Fix64 Random(System.Random random, int max)
		{
			var fractional = random.Next();
			var integral = random.Next(max);
			long rawValue = (integral << FRACTIONAL_PLACES) + fractional;

			return new Fix64(rawValue);
		}

		// Max should be between 0.0 and 1.0.
		public static Fix64 RandomFraction(System.Random random, Fix64 max)
		{
			long fractionalPart = (max.RawValue & 0x00000000FFFFFFFF);
			long fractional = random.NextInt64(fractionalPart);

			return new Fix64(fractional);
		}

		/// <summary>
		/// Returns an int indicating the sign of a Fix64 number.
		/// </summary>
		/// <returns>1 if the value is positive, 0 if it is 0, and -1 if it is negative.</returns>
		public static int Sign(Fix64 value)
		{
			return
				value.RawValue < 0 ? -1 :
				value.RawValue > 0 ? 1 :
				0;
		}

		/// <summary>
		/// Returns the absolute value of a Fix64 number.
		/// </summary>
		public static Fix64 Abs(Fix64 value)
		{
			if (value.RawValue == MIN_VALUE)
			{
				return MaxValue;
			}

			return FastAbs(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Fix64 FastAbs(Fix64 value)
		{
			// branchless implementation, see http://www.strchr.com/optimized_abs_function
			var mask = value.RawValue >> 63;
			return new Fix64((value.RawValue + mask) ^ mask);
		}

        /// <summary>
        /// Returns the largest integral value less than or equal to the specified number.
        /// </summary>
		public static Fix64 Floor(Fix64 value)
		{
			// Zero out the fractional part.
			return new Fix64((long)((ulong)value.RawValue & 0xFFFFFFFF00000000));
		}

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
		public static Fix64 Ceiling(Fix64 value)
		{
			return value.IsFractional ? Floor(value) + One : value;
		}

        /// <summary>
        /// Rounds to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
		public static Fix64 Round(Fix64 value)
		{
			var fractionalPart = value.RawValue & 0x00000000FFFFFFFF;
			var integralPart = Floor(value);
			if (fractionalPart < 0x80000000)
			{
				return integralPart;
			}
			if (fractionalPart > 0x80000000)
			{
				return integralPart + One;
			}
			// if number is halfway between two values, round to the nearest even number
			// this is the method used by System.Math.Round().
			return (integralPart.RawValue & ONE) == 0
					   ? integralPart
					   : integralPart + One;
		}

		/// <summary>
		/// Returns a remainder value as defined by the IEEE remainder method.
		/// </summary>
		/// <returns></returns>
		public static Fix64 IEEERemainder(Fix64 dividend, Fix64 divisor)
		{
			//Formula taken from https://docs.microsoft.com/en-us/dotnet/api/system.math.ieeeremainder?view=net-6.0
			return dividend - (divisor * Round(dividend / divisor));
		}

		/// <summary>
		/// Returns the minimum of two given Fix64 values.
		/// </summary>
		public static Fix64 Min(Fix64 x, Fix64 y)
		{
			return (x < y) ? x : y;
		}

		/// <summary>
		/// Returns the maximum of two given Fix64 values.
		/// </summary>
		public static Fix64 Max(Fix64 x, Fix64 y)
		{
			return (x > y) ? x : y;
		}

		/// <summary>
		/// Returns a value that is neither greater than nor less than a given min and max value.
		/// </summary>
		public static Fix64 Clamp(Fix64 value, Fix64 min, Fix64 max)
		{
			return Fix64.Min(Fix64.Max(value, min), max);
		}

		public static Fix64 Lerp(Fix64 value1, Fix64 value2, Fix64 amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		/// <summary>
		/// Rescales a value within a given range to a new range.
		/// </summary>
		public static Fix64 Normalize(Fix64 value, Fix64 min, Fix64 max, Fix64 newMin, Fix64 newMax)
		{
			return ((value - min) * (newMax - newMin)) / (max - min) + newMin;
		}

		// Trigonometry functions

		/// <summary>
		/// Returns the square root of the given Fix64 value.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Throws if x is less than zero.</exception>
		public static Fix64 Sqrt(Fix64 x)
		{
			var xl = x.RawValue;
			if (xl < 0)
			{
				// We cannot represent infinities like Single and Double, and Sqrt is
				// mathematically undefined for x < 0. So we just throw an exception.
				throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
			}

			var num = (ulong)xl;
			var result = 0UL;

			// second-to-top bit
			var bit = 1UL << (NUM_BITS - 2);

			while (bit > num)
			{
				bit >>= 2;
			}

			// The main part is executed twice, in order to avoid
			// using 128 bit values in computations.
			for (var i = 0; i < 2; ++i)
			{
				// First we get the top 48 bits of the answer.
				while (bit != 0)
				{
					if (num >= result + bit)
					{
						num -= result + bit;
						result = (result >> 1) + bit;
					}
					else
					{
						result = result >> 1;
					}
					bit >>= 2;
				}

				if (i == 0)
				{
					// Then process it again to get the lowest 16 bits.
					if (num > (1UL << (NUM_BITS / 2)) - 1)
					{
						// The remainder 'num' is too large to be shifted left
						// by 32, so we have to add 1 to result manually and
						// adjust 'num' accordingly.
						// num = a - (result + 0.5)^2
						//       = num + result^2 - (result + 0.5)^2
						//       = num - result - 0.5
						num -= result;
						num = (num << (NUM_BITS / 2)) - 0x80000000UL;
						result = (result << (NUM_BITS / 2)) + 0x80000000UL;
					}
					else
					{
						num <<= (NUM_BITS / 2);
						result <<= (NUM_BITS / 2);
					}

					bit = 1UL << (NUM_BITS / 2 - 2);
				}
			}
			// Finally, if next bit would have been 1, round the result upwards.
			if (num > result)
			{
				++result;
			}
			return new Fix64((long)result);
		}

		private static long ClampSinValue(long angle, out bool flipHorizontal, out bool flipVertical)
		{
			var largePI = 7244019458077122842;
			// Obtained from ((Fix64)1686629713.065252369824872831112M).m_rawValue
			// This is (2^29)*PI, where 29 is the largest N such that (2^N)*PI < MaxValue.
			// The idea is that this number contains way more precision than PI_TIMES_2,
			// and (((x % (2^29*PI)) % (2^28*PI)) % ... (2^1*PI) = x % (2 * PI)
			// In practice this gives us an error of about 1,25e-9 in the worst case scenario (Sin(MaxValue))
			// Whereas simply doing x % PI_TIMES_2 is the 2e-3 range.

			var clamped2Pi = angle;
			for (int i = 0; i < 29; ++i)
			{
				clamped2Pi %= (largePI >> i);
			}
			if (angle < 0)
			{
				clamped2Pi += PI_TIMES_2;
			}

			// The LUT contains values for 0 - PiOver2; every other value must be obtained by
			// vertical or horizontal mirroring
			flipVertical = clamped2Pi >= PI;
			// obtain (angle % PI) from (angle % 2PI) - much faster than doing another modulo
			var clampedPi = clamped2Pi;
			while (clampedPi >= PI)
			{
				clampedPi -= PI;
			}
			flipHorizontal = clampedPi >= PI_OVER_2;
			// obtain (angle % PI_OVER_2) from (angle % PI) - much faster than doing another modulo
			var clampedPiOver2 = clampedPi;
			if (clampedPiOver2 >= PI_OVER_2)
			{
				clampedPiOver2 -= PI_OVER_2;
			}
			return clampedPiOver2;
		}

		/// <summary>
		/// Returns the sine of the specified angle.
		/// </summary>
		public static Fix64 Sin(Fix64 x)
		{
			var clampedL = ClampSinValue(x.RawValue, out var flipHorizontal, out var flipVertical);
			var clamped = new Fix64(clampedL);

			// Find the two closest values in the LUT and perform linear interpolation
			// This is what kills the performance of this function on x86 - x64 is fine though
			var rawIndex = FastMul(clamped, LutInterval);
			var roundedIndex = Round(rawIndex);
			var indexError = FastSub(rawIndex, roundedIndex);

			var nearestValue = new Fix64(Fix64Lut.Sin[flipHorizontal ?
				Fix64Lut.Sin.Length - 1 - (int)roundedIndex :
				(int)roundedIndex]);
			var secondNearestValue = new Fix64(Fix64Lut.Sin[flipHorizontal ?
				Fix64Lut.Sin.Length - 1 - (int)roundedIndex - Sign(indexError) :
				(int)roundedIndex + Sign(indexError)]);

			var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue))).RawValue;
			var interpolatedValue = nearestValue.RawValue + (flipHorizontal ? -delta : delta);
			var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;
			return new Fix64(finalValue);
		}

		/// <summary>
		/// Returns the cosine of the specified angle.
		/// </summary>
		public static Fix64 Cos(Fix64 x)
		{
			var xl = x.RawValue;
			var rawAngle = xl + (xl > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
			return Sin(new Fix64(rawAngle));
		}

		/// <summary>
		/// Returns the tangent of the specified angle.
		/// </summary>
		public static Fix64 Tan(Fix64 x)
		{
			var clampedPi = x.RawValue % PI;
			var flip = false;
			if (clampedPi < 0)
			{
				clampedPi = -clampedPi;
				flip = true;
			}
			if (clampedPi > PI_OVER_2)
			{
				flip = !flip;
				clampedPi = PI_OVER_2 - (clampedPi - PI_OVER_2);
			}

			var clamped = new Fix64(clampedPi);

			// Find the two closest values in the LUT and perform linear interpolation
			var rawIndex = FastMul(clamped, LutInterval);
			var roundedIndex = Round(rawIndex);
			var indexError = FastSub(rawIndex, roundedIndex);

			var nearestValue = new Fix64(Fix64Lut.Tan[(int)roundedIndex]);
			var secondNearestValue = new Fix64(Fix64Lut.Tan[(int)roundedIndex + Sign(indexError)]);

			var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue))).RawValue;
			var interpolatedValue = nearestValue.RawValue + delta;
			var finalValue = flip ? -interpolatedValue : interpolatedValue;
			return new Fix64(finalValue);
		}

		/// <summary>
		/// Returns the angle whose tangent is the specified number.
		/// </summary>
		public static Fix64 Atan(Fix64 z)
		{
			if (z.RawValue == 0) return Zero;

			// Force positive values for argument
			// Atan(-z) = -Atan(z).
			var neg = z.RawValue < 0;
			if (neg)
			{
				z = -z;
			}

			Fix64 result;
			var two = (Fix64)2;
			var three = (Fix64)3;

			bool invert = z > One;
			if (invert) z = One / z;

			result = One;
			var term = One;

			var zSq = z * z;
			var zSq2 = zSq * two;
			var zSqPlusOne = zSq + One;
			var zSq12 = zSqPlusOne * two;
			var dividend = zSq2;
			var divisor = zSqPlusOne * three;

			for (var i = 2; i < 30; ++i)
			{
				term *= dividend / divisor;
				result += term;

				dividend += zSq2;
				divisor += zSq12;

				if (term.RawValue == 0) break;
			}

			result = result * z / zSqPlusOne;

			if (invert)
			{
				result = PiOver2 - result;
			}

			if (neg)
			{
				result = -result;
			}
			return result;
		}

		/// <summary>
		/// Returns the angle whose tangent is the quotient of two specified numbers.
		/// </summary>
		public static Fix64 Atan2(Fix64 y, Fix64 x)
		{
			var yl = y.RawValue;
			var xl = x.RawValue;
			if (xl == 0)
			{
				if (yl > 0)
				{
					return PiOver2;
				}
				if (yl == 0)
				{
					return Zero;
				}
				return -PiOver2;
			}
			Fix64 atan;
			var z = y / x;

			// Deal with overflow
			if (One + (Fix64)0.28M * z * z == MaxValue)
			{
				return y < Zero ? -PiOver2 : PiOver2;
			}

			if (Abs(z) < One)
			{
				atan = z / (One + (Fix64)0.28M * z * z);
				if (xl < 0)
				{
					if (yl < 0)
					{
						return atan - Pi;
					}
					return atan + Pi;
				}
			}
			else
			{
				atan = PiOver2 - z / (z * z + (Fix64)0.28M);
				if (yl < 0)
				{
					return atan - Pi;
				}
			}
			return atan;
		}

		// Operators

		public static Fix64 operator +(Fix64 x, Fix64 y)
		{
			var xl = x.RawValue;
			var yl = y.RawValue;
			var sum = xl + yl;
			// if signs of operands are equal and signs of sum and x are different
			if (((~(xl ^ yl) & (xl ^ sum)) & MIN_VALUE) != 0)
			{
				sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
			}
			return new Fix64(sum);
		}

		public static Fix64 operator -(Fix64 x, Fix64 y)
		{
			var xl = x.RawValue;
			var yl = y.RawValue;
			var diff = xl - yl;
			// if signs of operands are different and signs of sum and x are different
			if ((((xl ^ yl) & (xl ^ diff)) & MIN_VALUE) != 0)
			{
				diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
			}
			return new Fix64(diff);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Fix64 FastSub(Fix64 x, Fix64 y)
		{
			return new Fix64(x.RawValue - y.RawValue);
		}

		private static long AddOverflowHelper(long x, long y, ref bool overflow)
		{
			var sum = x + y;
			// x + y overflows if sign(x) ^ sign(y) != sign(sum)
			overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
			return sum;
		}

		public static Fix64 operator *(Fix64 x, Fix64 y)
		{
			var xl = x.RawValue;
			var yl = y.RawValue;

			var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
			var xhi = xl >> FRACTIONAL_PLACES;
			var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
			var yhi = yl >> FRACTIONAL_PLACES;

			var lolo = xlo * ylo;
			var lohi = (long)xlo * yhi;
			var hilo = xhi * (long)ylo;
			var hihi = xhi * yhi;

			var loResult = lolo >> FRACTIONAL_PLACES;
			var midResult1 = lohi;
			var midResult2 = hilo;
			var hiResult = hihi << FRACTIONAL_PLACES;

			bool overflow = false;
			var sum = AddOverflowHelper((long)loResult, midResult1, ref overflow);
			sum = AddOverflowHelper(sum, midResult2, ref overflow);
			sum = AddOverflowHelper(sum, hiResult, ref overflow);

			bool opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

			// if signs of operands are equal and sign of result is negative,
			// then multiplication overflowed positively
			// the reverse is also true
			if (opSignsEqual)
			{
				if (sum < 0 || (overflow && xl > 0))
				{
					return MaxValue;
				}
			}
			else
			{
				if (sum > 0)
				{
					return MinValue;
				}
			}

			// if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
			// then this means the result overflowed.
			var topCarry = hihi >> FRACTIONAL_PLACES;
			if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/)
			{
				return opSignsEqual ? MaxValue : MinValue;
			}

			// If signs differ, both operands' magnitudes are greater than 1,
			// and the result is greater than the negative operand, then there was negative overflow.
			if (!opSignsEqual)
			{
				long posOp, negOp;
				if (xl > yl)
				{
					posOp = xl;
					negOp = yl;
				}
				else
				{
					posOp = yl;
					negOp = xl;
				}
				if (sum > negOp && negOp < -ONE && posOp > ONE)
				{
					return MinValue;
				}
			}

			return new Fix64(sum);
		}

		private static Fix64 FastMul(Fix64 x, Fix64 y)
		{
			var xl = x.RawValue;
			var yl = y.RawValue;

			var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
			var xhi = xl >> FRACTIONAL_PLACES;
			var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
			var yhi = yl >> FRACTIONAL_PLACES;

			var lolo = xlo * ylo;
			var lohi = (long)xlo * yhi;
			var hilo = xhi * (long)ylo;
			var hihi = xhi * yhi;

			var loResult = lolo >> FRACTIONAL_PLACES;
			var midResult1 = lohi;
			var midResult2 = hilo;
			var hiResult = hihi << FRACTIONAL_PLACES;

			var sum = (long)loResult + midResult1 + midResult2 + hiResult;
			return new Fix64(sum);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int CountLeadingZeroes(ulong x)
		{
			int result = 0;
			while ((x & 0xF000000000000000) == 0) { result += 4; x <<= 4; }
			while ((x & 0x8000000000000000) == 0) { result += 1; x <<= 1; }
			return result;
		}

		public static Fix64 operator /(Fix64 x, Fix64 y)
		{
			var xl = x.RawValue;
			var yl = y.RawValue;

			if (yl == 0)
			{
				throw new DivideByZeroException();
			}

			var remainder = (ulong)(xl >= 0 ? xl : -xl);
			var divider = (ulong)(yl >= 0 ? yl : -yl);
			var quotient = 0UL;
			var bitPos = NUM_BITS / 2 + 1;


			// If the divider is divisible by 2^n, take advantage of it.
			while ((divider & 0xF) == 0 && bitPos >= 4)
			{
				divider >>= 4;
				bitPos -= 4;
			}

			while (remainder != 0 && bitPos >= 0)
			{
				int shift = CountLeadingZeroes(remainder);
				if (shift > bitPos)
				{
					shift = bitPos;
				}
				remainder <<= shift;
				bitPos -= shift;

				var div = remainder / divider;
				remainder = remainder % divider;
				quotient += div << bitPos;

				// Detect overflow
				if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
				{
					return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
				}

				remainder <<= 1;
				--bitPos;
			}

			// rounding
			++quotient;
			var result = (long)(quotient >> 1);
			if (((xl ^ yl) & MIN_VALUE) != 0)
			{
				result = -result;
			}

			return new Fix64(result);
		}

		public static Fix64 operator %(Fix64 x, Fix64 y)
		{
			return new Fix64(
				x.RawValue == MIN_VALUE & y.RawValue == -1 ?
				0 :
				x.RawValue % y.RawValue);
		}

		public static Fix64 operator -(Fix64 x)
		{
			return x.RawValue == MIN_VALUE ? MaxValue : new Fix64(-x.RawValue);
		}

		public static bool operator ==(Fix64 x, Fix64 y)
		{
			return x.RawValue == y.RawValue;
		}

		public static bool operator !=(Fix64 x, Fix64 y)
		{
			return x.RawValue != y.RawValue;
		}

		public static bool operator >(Fix64 x, Fix64 y)
		{
			return x.RawValue > y.RawValue;
		}

		public static bool operator <(Fix64 x, Fix64 y)
		{
			return x.RawValue < y.RawValue;
		}

		public static bool operator >(Fix64 x, int y)
		{
			return x > ((Fix64) y);
		}

		public static bool operator <(Fix64 x, int y)
		{
			return x < ((Fix64) y);
		}

		public static bool operator >=(Fix64 x, Fix64 y)
		{
			return x.RawValue >= y.RawValue;
		}

		public static bool operator <=(Fix64 x, Fix64 y)
		{
			return x.RawValue <= y.RawValue;
		}

		public static bool operator >=(Fix64 x, int y)
		{
			return x >= ((Fix64) y);
		}

		public static bool operator <=(Fix64 x, int y)
		{
			return x <= ((Fix64) y);
		}

		// Casting

		public static explicit operator Fix64(long value)
		{
			return new Fix64(value * ONE);
		}

		public static explicit operator long(Fix64 value)
		{
			return value.RawValue >> FRACTIONAL_PLACES;
		}

		public static explicit operator Fix64(float value)
		{
			return new Fix64((long)(value * ONE));
		}

		public static explicit operator float(Fix64 value)
		{
			return (float)value.RawValue / ONE;
		}

		public static explicit operator Fix64(double value)
		{
			return new Fix64((long)(value * ONE));
		}

		public static explicit operator double(Fix64 value)
		{
			return (double)value.RawValue / ONE;
		}

		public static explicit operator Fix64(decimal value)
		{
			return new Fix64((long)(value * ONE));
		}

		public static explicit operator decimal(Fix64 value)
		{
			return (decimal)value.RawValue / ONE;
		}

		public int CompareTo(Fix64 other)
		{
			return RawValue.CompareTo(other.RawValue);
		}

		public override bool Equals(object obj)
		{
			return obj is Fix64 fix && RawValue == fix.RawValue;
		}

		public bool Equals(Fix64 other)
		{
			return RawValue == other.RawValue;
		}

		public override int GetHashCode()
		{
			return RawValue.GetHashCode();
		}

		// FIXME: can we avoid this cast?
		public override string ToString()
		{
			// Up to 10 decimal places
			return ((decimal)this).ToString("0.##########");
		}

		public string ToString(System.Globalization.CultureInfo ci)
		{
			return ((decimal) this).ToString("0.##########", ci);
		}
	}
}
