using MoonWorks.Math.Fixed;
using System.Collections.Generic;

namespace MoonWorks.Math
{
	public static class Easing
	{
		private const float C1 = 1.70158f;
		private const float C2 = C1 * 1.525f;
		private const float C3 = C1 + 1;
		private const float C4 = (2 * System.MathF.PI) / 3;
		private const float C5 = (2 * System.MathF.PI) / 4.5f;

		private static readonly Fix64 HALF = Fix64.FromFraction(1, 2);
		private static readonly Fix64 FIXED_C1 = Fix64.FromFraction(170158, 100000);
		private static readonly Fix64 FIXED_C2 = FIXED_C1 * Fix64.FromFraction(61, 40);
		private static readonly Fix64 FIXED_C3 = FIXED_C1 + Fix64.One;
		private static readonly Fix64 FIXED_C4 = Fix64.PiTimes2 / new Fix64(3);
		private static readonly Fix64 FIXED_C5 = Fix64.PiTimes2 / Fix64.FromFraction(9, 2);

		private static readonly Fix64 FIXED_N1 = Fix64.FromFraction(121, 16);
		private static readonly Fix64 FIXED_D1 = Fix64.FromFraction(11, 4);

		private static float OutIn(
			System.Func<float, float> outFunc,
			System.Func<float, float> inFunc,
			float t
		) {
			if (t < 0.5f)
			{
				return outFunc(t);
			}
			else
			{
				return inFunc(t);
			}
		}

		private static Fix64 OutIn(
			System.Func<Fix64, Fix64> outFunc,
			System.Func<Fix64, Fix64> inFunc,
			Fix64 t
		) {
			if (t < HALF)
			{
				return outFunc(t);
			}
			else
			{
				return inFunc(t);
			}
		}

		/* GENERAL-USE FUNCTIONS */

		public static float AttackHoldRelease(
			float start,
			float hold,
			float end,
			float time,
			float attackDuration,
			Function.Float attackEasingFunction,
			float holdDuration,
			float releaseDuration,
			Function.Float releaseEasingFunction
		) {
			if (time < attackDuration)
			{
				return Interp(start, hold, time, attackDuration, Function.Get(attackEasingFunction));
			}
			else if (time >= attackDuration && time < attackDuration + holdDuration)
			{
				return hold;
			}
			else // time >= attackDuration + holdDuration
			{
				return Interp(hold, end, time - holdDuration - attackDuration, releaseDuration, Function.Get(releaseEasingFunction));
			}
		}

		public static Fix64 AttackHoldRelease(
			Fix64 start,
			Fix64 hold,
			Fix64 end,
			Fix64 time,
			Fix64 attackDuration,
			Function.Fixed attackEasingFunction,
			Fix64 holdDuration,
			Fix64 releaseDuration,
			Function.Fixed releaseEasingFunction
		) {
			if (time < attackDuration)
			{
				return Interp(start, hold, time, attackDuration, Function.Get(attackEasingFunction));
			}
			else if (time >= attackDuration && time < attackDuration + holdDuration)
			{
				return hold;
			}
			else // time >= attackDuration + holdDuration
			{
				return Interp(hold, end, time - holdDuration - attackDuration, releaseDuration, Function.Get(releaseEasingFunction));
			}
		}

		public static float Lerp(float start, float end, float time)
		{
			return (start + (end - start) * time);
		}

		public static float Interp(float start, float end, float time, float duration, System.Func<float, float> easingFunc)
		{
			return Lerp(start, end, easingFunc(time / duration));
		}

		public static float Interp(float start, float end, float time, float duration, MoonWorks.Math.Easing.Function.Float easingFunc)
		{
			return Interp(start, end, time, duration, Function.Get(easingFunc));
		}

		public static Fix64 Lerp(Fix64 start, Fix64 end, Fix64 time)
		{
			return (start + (end - start) * time);
		}

		public static Fix64 Interp(Fix64 start, Fix64 end, Fix64 time, Fix64 duration, System.Func<Fix64, Fix64> easingFunc)
		{
			return Lerp(start, end, easingFunc(time / duration));
		}

		public static Fix64 Interp(Fix64 start, Fix64 end, Fix64 time, Fix64 duration, MoonWorks.Math.Easing.Function.Fixed easingFunc)
		{
			return Interp(start, end, time, duration, Function.Get(easingFunc));
		}

		/* FLOAT EASING FUNCTIONS */

		// LINEAR

		public static float Linear(float t)
		{
			return t;
		}

		// QUADRATIC

		public static float InQuad(float t)
		{
			return t * t;
		}

		public static float OutQuad(float t)
		{
			return 1 - (1 - t) * (1 - t);
		}

		public static float InOutQuad(float t)
		{
			if (t < 0.5f)
			{
				return 2 * t * t;
			}
			else
			{
				var x = (-2 * t + 2);
				return 1 - ((x * x) / 2);
			}
		}

		public static float OutInQuad(float t) => OutIn(OutQuad, InQuad, t);

		// CUBIC

		public static float InCubic(float t)
		{
			return t * t * t;
		}

		public static float OutCubic(float t)
		{
			var x = 1 - t;
			return 1 - (x * x * x);
		}

		public static float InOutCubic(float t)
		{
			if (t < 0.5f)
			{
				return 4 * t * t * t;
			}
			else
			{
				var x = -2 * t + 2;
				return 1 - ((x * x * x) / 2);
			}
		}

		public static float OutInCubic(float t) => OutIn(OutCubic, InCubic, t);

		// QUARTIC

		public static float InQuart(float t)
		{
			return t * t * t * t;
		}

		public static float OutQuart(float t)
		{
			var x = 1 - t;
			return 1 - (x * x * x * x);
		}

		public static float InOutQuart(float t)
		{
			if (t < 0.5f)
			{
				return 8 * t * t * t * t;
			}
			else
			{
				var x = -2 * t + 2;
				return 1 - ((x * x * x * x) / 2);
			}
		}

		public static float OutInQuart(float t) => OutIn(OutQuart, InQuart, t);

		// QUINTIC

		public static float InQuint(float t)
		{
			return t * t * t * t * t;
		}

		public static float OutQuint(float t)
		{
			var x = 1 - t;
			return 1 - (x * x * x * x * x);
		}

		public static float InOutQuint(float t)
		{
			if (t < 0.5f)
			{
				return 16 * t * t * t * t * t;
			}
			else
			{
				var x = -2 * t + 2;
				return 1 - ((x * x * x * x * x) / 2);
			}
		}

		public static float OutInQuint(float t) => OutIn(OutQuint, InQuint, t);

		// SINE

		public static float InSine(float t)
		{
			return 1 - System.MathF.Cos((t * System.MathF.PI) / 2);
		}

		public static float OutSine(float t)
		{
			return System.MathF.Sin((t * System.MathF.PI) / 2);
		}

		public static float InOutSine(float t)
		{
			return -(System.MathF.Cos(System.MathF.PI * t) - 1) / 2;
		}

		public static float OutInSine(float t) => OutIn(OutSine, InSine, t);

		// EXPONENTIAL

		public static float InExpo(float t)
		{
			if (t == 0)
			{
				return 0;
			}
			else
			{
				return System.MathF.Pow(2, 10 * t - 10);
			}
		}

		public static float OutExpo(float t)
		{
			if (t == 1)
			{
				return 1;
			}
			else
			{
				return 1 - System.MathF.Pow(2, -10 * t);
			}
		}

		public static float InOutExpo(float t)
		{
			if (t == 0)
			{
				return 0;
			}
			else if (t == 1)
			{
				return 1;
			}
			else if (t < 0.5f)
			{
				return System.MathF.Pow(2, 20 * t - 10) / 2;
			}
			else
			{
				return (2 - System.MathF.Pow(2, -20 * t + 10)) / 2;
			}
		}

		public static float OutInExpo(float t) => OutIn(OutExpo, InExpo, t);

		// CIRCULAR

		public static float InCirc(float t)
		{
			return 1 - System.MathF.Sqrt(1 - (t * t));
		}

		public static float OutCirc(float t)
		{
			return System.MathF.Sqrt(1 - ((t - 1) * (t - 1)));
		}

		public static float InOutCirc(float t)
		{
			if (t < 0.5f)
			{
				return (1 - System.MathF.Sqrt(1 - ((2 * t) * (2 * t)))) / 2;
			}
			else
			{
				var x = -2 * t + 2;
				return (System.MathF.Sqrt(1 - (x * x)) + 1) / 2;
			}
		}

		public static float OutInCirc(float t) => OutIn(OutCirc, InCirc, t);

		// BACK

		public static float InBack(float t)
		{
			return C3 * t * t * t - C1 * t * t;
		}

		public static float OutBack(float t)
		{
			return 1 + C3 * (t - 1) * (t - 1) * (t - 1) + C1 * (t - 1) * (t - 1);
		}

		public static float InOutBack(float t)
		{
			if (t < 0.5f)
			{
				return ((2 * t) * (2 * t) * ((C2 + 1) * 2 * t - C2)) / 2;
			}
			else
			{
				var x = 2 * t - 2;
				return ((t * t) * ((C2 + 1) * (x) + C2) + 2) / 2;
			}
		}

		public static float OutInBack(float t) => OutIn(OutBack, InBack, t);

		// ELASTIC

		public static float InElastic(float t)
		{
			if (t == 0)
			{
				return 0;
			}
			else if (t == 1)
			{
				return 1;
			}
			else
			{
				return -System.MathF.Pow(2, 10 * t - 10) * System.MathF.Sin((t * 10 - 10.75f) * C4);
			}
		}

		public static float OutElastic(float t)
		{
			if (t == 0)
			{
				return 0;
			}
			else if (t == 1)
			{
				return 1;
			}
			else
			{
				return System.MathF.Pow(2, -10 * t) * System.MathF.Sin((t * 10 - 0.75f) * C4) + 1;
			}
		}

		public static float InOutElastic(float t)
		{
			if (t == 0)
			{
				return 0;
			}
			else if (t == 1)
			{
				return 1;
			}
			else if (t < 0.5f)
			{
				return -(System.MathF.Pow(2, 20 * t - 10) * System.MathF.Sin((20 * t - 11.125f) * C5)) / 2;
			}
			else
			{
				return (System.MathF.Pow(2, -20 * t + 10) * System.MathF.Sin((20 * t - 11.125f) * C5)) / 2 + 1;
			}
		}

		public static float OutInElastic(float t) => OutIn(OutElastic, InElastic, t);

		// BOUNCE

		public static float InBounce(float t)
		{
			return 1 - OutBounce(1 - t);
		}

		public static float OutBounce(float t)
		{
			const float N1 = 7.5625f;
			const float D1 = 2.75f;

			if (t < 1 / D1)
			{
				return N1 * t * t;
			}
			else if (t < 2 / D1) {
				return N1 * (t -= 1.5f / D1) * t + 0.75f;
			}
			else if (t < 2.5f / D1)
			{
				return N1 * (t -= 2.25f / D1) * t + 0.9375f;
			}
			else
			{
				return N1 * (t -= 2.625f / D1) * t + 0.984375f;
			}
		}

		public static float InOutBounce(float t)
		{
			if (t < 0.5f)
			{
				return (1 - OutBounce(1 - 2 * t)) / 2;
			}
			else
			{
				return (1 + OutBounce(2 * t - 1)) / 2;
			}
		}

		public static float OutInBounce(float t) => OutIn(OutBounce, InBounce, t);

		/* FIXED EASING FUNCTIONS */

		// LINEAR

		public static Fix64 Linear(Fix64 t)
		{
			return t;
		}

		// QUADRATIC

		public static Fix64 InQuad(Fix64 t)
		{
			return t * t;
		}

		public static Fix64 OutQuad(Fix64 t)
		{
			return 1 - (1 - t) * (1 - t);
		}

		public static Fix64 InOutQuad(Fix64 t)
		{
			if (t < HALF)
			{
				return 2 * t * t;
			}
			else
			{
				var x = (-2 * t + 2);
				return 1 - ((x * x) / 2);
			}
		}

		public static Fix64 OutInQuad(Fix64 t) => OutIn(OutQuad, InQuad, t);

		// CUBIC

		public static Fix64 InCubic(Fix64 t)
		{
			return t * t * t;
		}

		public static Fix64 OutCubic(Fix64 t)
		{
			var x = 1 - t;
			return 1 - (x * x * x);
		}

		public static Fix64 InOutCubic(Fix64 t)
		{
			if (t < HALF)
			{
				return 4 * t * t * t;
			}
			else
			{
				var x = -2 * t + 2;
				return 1 - ((x * x * x) / 2);
			}
		}

		public static Fix64 OutInCubic(Fix64 t) => OutIn(OutCubic, InCubic, t);

		// QUARTIC

		public static Fix64 InQuart(Fix64 t)
		{
			return t * t * t * t;
		}

		public static Fix64 OutQuart(Fix64 t)
		{
			var x = 1 - t;
			return 1 - (x * x * x * x);
		}

		public static Fix64 InOutQuart(Fix64 t)
		{
			if (t < HALF)
			{
				return 8 * t * t * t * t;
			}
			else
			{
				var x = -2 * t + 2;
				return 1 - ((x * x * x * x) / 2);
			}
		}

		public static Fix64 OutInQuart(Fix64 t) => OutIn(OutQuart, InQuart, t);

		// QUINTIC

		public static Fix64 InQuint(Fix64 t)
		{
			return t * t * t * t * t;
		}

		public static Fix64 OutQuint(Fix64 t)
		{
			var x = 1 - t;
			return 1 - (x * x * x * x * x);
		}

		public static Fix64 InOutQuint(Fix64 t)
		{
			if (t < HALF)
			{
				return 16 * t * t * t * t * t;
			}
			else
			{
				var x = -2 * t + 2;
				return 1 - ((x * x * x * x * x) / 2);
			}
		}

		public static Fix64 OutInQuint(Fix64 t) => OutIn(OutQuint, InQuint, t);

		// SINE

		public static Fix64 InSine(Fix64 t)
		{
			return 1 - Fix64.Cos((t * Fix64.Pi) / 2);
		}

		public static Fix64 OutSine(Fix64 t)
		{
			return Fix64.Sin((t * Fix64.Pi) / 2);
		}

		public static Fix64 InOutSine(Fix64 t)
		{
			return -(Fix64.Cos(Fix64.Pi * t) - 1) / 2;
		}

		public static Fix64 OutInSine(Fix64 t) => OutIn(OutSine, InSine, t);

		// CIRCULAR

		public static Fix64 InCirc(Fix64 t)
		{
			return 1 - Fix64.Sqrt(1 - (t * t));
		}

		public static Fix64 OutCirc(Fix64 t)
		{
			return Fix64.Sqrt(1 - ((t - 1) * (t - 1)));
		}

		public static Fix64 InOutCirc(Fix64 t)
		{
			if (t < HALF)
			{
				return (1 - Fix64.Sqrt(1 - ((2 * t) * (2 * t)))) / 2;
			}
			else
			{
				var x = -2 * t + 2;
				return (Fix64.Sqrt(1 - (x * x)) + 1) / 2;
			}
		}

		public static Fix64 OutInCirc(Fix64 t) => OutIn(OutCirc, InCirc, t);

		// BACK

		public static Fix64 InBack(Fix64 t)
		{
			return FIXED_C3 * t * t * t - FIXED_C1 * t * t;
		}

		public static Fix64 OutBack(Fix64 t)
		{
			return 1 + FIXED_C3 * (t - 1) * (t - 1) * (t - 1) + FIXED_C1 * (t - 1) * (t - 1);
		}

		public static Fix64 InOutBack(Fix64 t)
		{
			if (t < HALF)
			{
				return ((2 * t) * (2 * t) * ((FIXED_C2 + 1) * 2 * t - FIXED_C2)) / 2;
			}
			else
			{
				var x = 2 * t - 2;
				return ((t * t) * ((FIXED_C2 + 1) * (x) + FIXED_C2) + 2) / 2;
			}
		}

		public static Fix64 OutInBack(Fix64 t) => OutIn(OutBack, InBack, t);

		// BOUNCE

		public static Fix64 InBounce(Fix64 t)
		{
			return 1 - OutBounce(1 - t);
		}

		public static Fix64 OutBounce(Fix64 t)
		{
			if (t < 1 / FIXED_D1)
			{
				return FIXED_N1 * t * t;
			}
			else if (t < 2 / FIXED_D1) {
				return FIXED_N1 * (t -= Fix64.FromFraction(3, 2) / FIXED_D1) * t + Fix64.FromFraction(3, 4);
			}
			else if (t < Fix64.FromFraction(5, 2) / FIXED_D1)
			{
				return FIXED_N1 * (t -= Fix64.FromFraction(9, 4) / FIXED_D1) * t + Fix64.FromFraction(15, 16);
			}
			else
			{
				return FIXED_N1 * (t -= Fix64.FromFraction(181, 80) / FIXED_D1) * t + Fix64.FromFraction(63, 64);
			}
		}

		public static Fix64 InOutBounce(Fix64 t)
		{
			if (t < HALF)
			{
				return (1 - OutBounce(1 - 2 * t)) / 2;
			}
			else
			{
				return (1 + OutBounce(2 * t - 1)) / 2;
			}
		}

		public static Fix64 OutInBounce(Fix64 t) => OutIn(OutBounce, InBounce, t);

		public static class Function
		{
			public enum Float
			{
				Linear,
				InQuad,
				OutQuad,
				InOutQuad,
				OutInQuad,
				InCubic,
				OutCubic,
				InOutCubic,
				OutInCubic,
				InQuart,
				OutQuart,
				InOutQuart,
				OutInQuart,
				InQuint,
				OutQuint,
				InOutQuint,
				OutInQuint,
				InSine,
				OutSine,
				InOutSine,
				OutInSine,
				InExpo,
				OutExpo,
				InOutExpo,
				OutInExpo,
				InCirc,
				OutCirc,
				InOutCirc,
				OutInCirc,
				InElastic,
				OutElastic,
				InOutElastic,
				OutInElastic,
				InBack,
				OutBack,
				InOutBack,
				OutInBack,
				InBounce,
				OutBounce,
				InOutBounce,
				OutInBounce
			}

			public enum Fixed
			{
				Linear,
				InQuad,
				OutQuad,
				InOutQuad,
				OutInQuad,
				InCubic,
				OutCubic,
				InOutCubic,
				OutInCubic,
				InQuart,
				OutQuart,
				InOutQuart,
				OutInQuart,
				InQuint,
				OutQuint,
				InOutQuint,
				OutInQuint,
				InSine,
				OutSine,
				InOutSine,
				OutInSine,
				InCirc,
				OutCirc,
				InOutCirc,
				OutInCirc,
				InBack,
				OutBack,
				InOutBack,
				OutInBack,
				InBounce,
				OutBounce,
				InOutBounce,
				OutInBounce
			}

			private static Dictionary<Float, System.Func<float, float>> FloatLookup = new Dictionary<Float, System.Func<float, float>>
			{
				{ Float.Linear, Linear },
				{ Float.InQuad, InQuad },
				{ Float.OutQuad, OutQuad },
				{ Float.InOutQuad, InOutQuad },
				{ Float.OutInQuad, OutInQuad },
				{ Float.InCubic, InCubic },
				{ Float.OutCubic, OutCubic },
				{ Float.InOutCubic, InOutCubic },
				{ Float.OutInCubic, OutInCubic },
				{ Float.InQuart, InQuart },
				{ Float.OutQuart, OutQuart },
				{ Float.InOutQuart, InOutQuart },
				{ Float.OutInQuart, OutInQuart },
				{ Float.InQuint, InQuint },
				{ Float.OutQuint, OutQuint },
				{ Float.InOutQuint, InOutQuint },
				{ Float.OutInQuint, OutInQuint },
				{ Float.InSine, InSine },
				{ Float.OutSine, OutSine },
				{ Float.InOutSine, InOutSine },
				{ Float.OutInSine, OutInSine },
				{ Float.InExpo, InExpo },
				{ Float.OutExpo, OutExpo },
				{ Float.InOutExpo, InOutExpo },
				{ Float.OutInExpo, OutInExpo },
				{ Float.InCirc, InCirc },
				{ Float.OutCirc, OutCirc },
				{ Float.InOutCirc, InOutCirc },
				{ Float.OutInCirc, OutInCirc },
				{ Float.InElastic, InElastic },
				{ Float.OutElastic, OutElastic },
				{ Float.InOutElastic, InOutElastic },
				{ Float.OutInElastic, OutInElastic },
				{ Float.InBack, InBack },
				{ Float.OutBack, OutBack },
				{ Float.InOutBack, InOutBack },
				{ Float.OutInBack, OutInBack },
				{ Float.InBounce, InBounce },
				{ Float.OutBounce, OutBounce },
				{ Float.InOutBounce, InOutBounce },
				{ Float.OutInBounce, OutInBounce }
			};

			private static Dictionary<Fixed, System.Func<Fix64, Fix64>> FixedLookup = new Dictionary<Fixed, System.Func<Fix64, Fix64>>
			{
				{ Fixed.Linear, Linear },
				{ Fixed.InQuad, InQuad },
				{ Fixed.OutQuad, OutQuad },
				{ Fixed.InOutQuad, InOutQuad },
				{ Fixed.OutInQuad, OutInQuad },
				{ Fixed.InCubic, InCubic },
				{ Fixed.OutCubic, OutCubic },
				{ Fixed.InOutCubic, InOutCubic },
				{ Fixed.OutInCubic, OutInCubic },
				{ Fixed.InQuart, InQuart },
				{ Fixed.OutQuart, OutQuart },
				{ Fixed.InOutQuart, InOutQuart },
				{ Fixed.OutInQuart, OutInQuart },
				{ Fixed.InQuint, InQuint },
				{ Fixed.OutQuint, OutQuint },
				{ Fixed.InOutQuint, InOutQuint },
				{ Fixed.OutInQuint, OutInQuint },
				{ Fixed.InSine, InSine },
				{ Fixed.OutSine, OutSine },
				{ Fixed.InOutSine, InOutSine },
				{ Fixed.OutInSine, OutInSine },
				{ Fixed.InCirc, InCirc },
				{ Fixed.OutCirc, OutCirc },
				{ Fixed.InOutCirc, InOutCirc },
				{ Fixed.OutInCirc, OutInCirc },
				{ Fixed.InBack, InBack },
				{ Fixed.OutBack, OutBack },
				{ Fixed.InOutBack, InOutBack },
				{ Fixed.OutInBack, OutInBack },
				{ Fixed.InBounce, InBounce },
				{ Fixed.OutBounce, OutBounce },
				{ Fixed.InOutBounce, InOutBounce },
				{ Fixed.OutInBounce, OutInBounce }
			};

			public static System.Func<float, float> Get(Float functionEnum)
			{
				return FloatLookup[functionEnum];
			}

			public static System.Func<Fix64, Fix64> Get(Fixed functionEnum)
			{
				return FixedLookup[functionEnum];
			}
		}
	}
}
