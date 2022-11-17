using MoonWorks.Math.Fixed;
using EasingFunctionFloat = System.Func<float, float, float, float, float>;
using EasingFunctionFixed = System.Func<MoonWorks.Math.Fixed.Fix64, MoonWorks.Math.Fixed.Fix64, MoonWorks.Math.Fixed.Fix64, MoonWorks.Math.Fixed.Fix64, MoonWorks.Math.Fixed.Fix64>;
using System.Collections.Generic;

namespace MoonWorks.Math
{
	public static class Easing
	{
		private static float OutIn(
			EasingFunctionFloat outFunc,
			EasingFunctionFloat inFunc,
			float start,
			float end,
			float time,
			float duration
		) {
			if (time < duration / 2)
			{
				return outFunc(start, end / 2, time * 2, duration);
			}
			else
			{
				return inFunc(start + (end / 2), end / 2, (time * 2) - duration, duration);
			}
		}

		private static Fix64 OutIn(
			EasingFunctionFixed outFunc,
			EasingFunctionFixed inFunc,
			Fix64 start,
			Fix64 end,
			Fix64 time,
			Fix64 duration
		) {
			if (time < duration / 2)
			{
				return outFunc(start, end / 2, time * 2, duration);
			}
			else
			{
				return inFunc(start + (end / 2), end / 2, (time * 2) - duration, duration);
			}
		}

		/* GENERAL-USE FUNCTIONS */

		public static float AttackHoldRelease(
			float start,
			float hold,
			float end,
			float time,
			float attackDuration,
			EasingFunctionFloat attackEasingFunction,
			float holdDuration,
			float releaseDuration,
			EasingFunctionFloat releaseEasingFunction
		) {
			if (time < attackDuration)
			{
				return attackEasingFunction.Invoke(start, hold, time, attackDuration);
			}
			else if (time >= attackDuration && time < holdDuration)
			{
				return hold;
			}
			else // time >= attackDuration + holdDuration
			{
				return releaseEasingFunction.Invoke(hold, end, time - holdDuration - attackDuration, releaseDuration);
			}
		}

		public static Fix64 AttackHoldRelease(
			Fix64 start,
			Fix64 hold,
			Fix64 end,
			Fix64 time,
			Fix64 attackDuration,
			EasingFunctionFixed attackEasingFunction,
			Fix64 holdDuration,
			Fix64 releaseDuration,
			EasingFunctionFixed releaseEasingFunction
		) {
			if (time < attackDuration)
			{
				return attackEasingFunction.Invoke(start, hold, time, attackDuration);
			}
			else if (time >= attackDuration && time < holdDuration)
			{
				return hold;
			}
			else // time >= attackDuration + holdDuration
			{
				return releaseEasingFunction.Invoke(hold, end, time - holdDuration - attackDuration, releaseDuration);
			}
		}

		/* EASING FUNCTIONS */

		// LINEAR

		public static float Linear(float start, float end, float time, float duration)
		{
			return (end * (time / duration)) + start;
		}

		public static Fix64 Linear(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			return (end * (time / duration)) + start;
		}

		// QUADRATIC

		public static float InQuad(float start, float end, float time, float duration)
		{
			time /= duration;
			return (end * (time * time)) + start;
		}

		public static float OutQuad(float start, float end, float time, float duration)
		{
			time /= duration;
			return (-end * time * (time - 2)) + start;
		}

		public static float InOutQuad(float start, float end, float time, float duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time)) + start;
			}
			else
			{
				return (-end / 2 * (((time - 1) * (time - 3)) - 1)) + start;
			}
		}

		public static float OutInQuad(float start, float end, float time, float duration) => OutIn(OutQuad, InQuad, time, start, end, duration);

		public static Fix64 InQuad(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time /= duration;
			return (end * (time * time)) + start;
		}

		public static Fix64 OutQuad(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time /= duration;
			return (-end * time * (time - 2)) + start;
		}

		public static Fix64 InOutQuad(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time)) + start;
			}
			else
			{
				return (-end / 2 * (((time - 1) * (time - 3)) - 1)) + start;
			}
		}

		public static Fix64 OutInQuad(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutIn(OutQuad, InQuad, time, start, end, duration);


		// CUBIC

		public static float InCubic(float start, float end, float time, float duration)
		{
			time /= duration;
			return (end * (time * time * time)) + start;
		}

		public static float OutCubic(float start, float end, float time, float duration)
		{
			time = (time / duration) - 1;
			return (end * ((time * time * time) + 1)) + start;
		}

		public static float InOutCubic(float start, float end, float time, float duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * time * time * time) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * ((time * time * time) + 2)) + start;
			}
		}

		public static float OutInCubic(float start, float end, float time, float duration) => OutIn(OutCubic, InCubic, start, end, time, duration);

		public static Fix64 InCubic(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time /= duration;
			return (end * (time * time * time)) + start;
		}

		public static Fix64 OutCubic(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = (time / duration) - 1;
			return (end * ((time * time * time) + 1)) + start;
		}

		public static Fix64 InOutCubic(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * time * time * time) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * ((time * time * time) + 2)) + start;
			}
		}

		public static Fix64 OutInCubic(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutIn(OutCubic, InCubic, start, end, time, duration);

		// QUARTIC

		public static float InQuart(float start, float end, float time, float duration)
		{
			time /= duration;
			return (end * (time * time * time * time)) + start;
		}

		public static float OutQuart(float start, float end, float time, float duration)
		{
			time = (time / duration) - 1;
			return (-end * ((time * time * time * time) - 1)) + start;
		}

		public static float InOutQuart(float start, float end, float time, float duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time * time * time)) + start;
			}
			else
			{
				time -= 2;
				return (-end / 2 * ((time * time * time * time) - 2)) + start;
			}
		}

		public static float OutInQuart(float start, float end, float time, float duration) => OutIn(OutQuart, InQuart, time, start, end, duration);

		public static Fix64 InQuart(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time /= duration;
			return (end * (time * time * time * time)) + start;
		}

		public static Fix64 OutQuart(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = (time / duration) - 1;
			return (-end * ((time * time * time * time) - 1)) + start;
		}

		public static Fix64 InOutQuart(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time * time * time)) + start;
			}
			else
			{
				time -= 2;
				return (-end / 2 * ((time * time * time * time) - 2)) + start;
			}
		}

		public static Fix64 OutInQuart(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutIn(OutQuart, InQuart, time, start, end, duration);

		// QUINTIC

		public static float InQuint(float start, float end, float time, float duration)
		{
			time /= duration;
			return (end * (time * time * time * time * time)) + start;
		}

		public static float OutQuint(float start, float end, float time, float duration)
		{
			time = (time / duration) - 1;
			return (end * ((time * time * time * time * time) + 1)) + start;
		}

		public static float InOutQuint(float start, float end, float time, float duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time * time * time * time)) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * ((time * time * time * time * time) + 2)) + start;
			}
		}

		public static float OutInQuint(float start, float end, float time, float duration) => OutIn(OutQuint, InQuint, time, start, end, duration);

		public static Fix64 InQuint(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time /= duration;
			return (end * (time * time * time * time * time)) + start;
		}

		public static Fix64 OutQuint(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = (time / duration) - 1;
			return (end * ((time * time * time * time * time) + 1)) + start;
		}

		public static Fix64 InOutQuint(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time * time * time * time)) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * ((time * time * time * time * time) + 2)) + start;
			}
		}

		public static Fix64 OutInQuint(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutIn(OutQuint, InQuint, time, start, end, duration);


		// SINE

		public static float InSine(float start, float end, float time, float duration)
		{
			return (-end * System.MathF.Cos(time / duration * (System.MathF.PI / 2))) + end + start;
		}

		public static float OutSine(float start, float end, float time, float duration)
		{
			return (end * System.MathF.Sin(time / duration * (System.MathF.PI / 2))) + start;
		}

		public static float InOutSine(float start, float end, float time, float duration)
		{
			return (-end / 2 * (System.MathF.Cos(System.MathF.PI * time / duration) - 1)) + start;
		}

		public static float OutInSine(float start, float end, float time, float duration) => OutIn(OutSine, InSine, time, start, end, duration);

		public static Fix64 InSine(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			return (-end * Fix64.Cos((time / duration) * Fix64.PiOver2)) + end + start;
		}

		public static Fix64 OutSine(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			return (end * Fix64.Sin((time / duration) * Fix64.PiOver2)) + start;
		}

		public static Fix64 InOutSine(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			return (-end / 2 * (Fix64.Cos(Fix64.Pi * time / duration) - 1)) + start;
		}

		public static Fix64 OutInSine(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutIn(OutSine, InSine, time, start, end, duration);

		// EXPONENTIAL

		public static float InExpo(float start, float end, float time, float duration)
		{
			if (time == 0)
			{
				return start;
			}
			else
			{
				return (end * System.MathF.Pow(2, 10 * ((time / duration) - 1))) + start - (end * 0.001f);
			}
		}

		public static float OutExpo(float start, float end, float time, float duration)
		{
			if (time == duration)
			{
				return start + end;
			}
			else
			{
				return (end * 1.001f * (-System.MathF.Pow(2, -10 * time / duration) + 1)) + start;
			}
		}

		public static float InOutExpo(float start, float end, float time, float duration)
		{
			if (time == 0) { return start; }
			if (time == duration) { return start + end; }
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * System.MathF.Pow(2, 10 * (time - 1))) + start - (end * 0.0005f);
			}
			else
			{
				time--;
				return (end / 2 * 1.0005f * (-System.MathF.Pow(2, -10 * time) + 2)) + start;
			}
		}

		public static float OutInExpo(float start, float end, float time, float duration) => OutIn(OutExpo, InExpo, time, start, end, duration);

		// TODO: need Fix64 power function for Expo

		// CIRCULAR

		public static float InCirc(float start, float end, float time, float duration)
		{
			time /= duration;
			return (-end * (System.MathF.Sqrt(1 - (time * time)) - 1)) + start;
		}

		public static float OutCirc(float start, float end, float time, float duration)
		{
			time = (time / duration) - 1;
			return (end * System.MathF.Sqrt(1 - (time * time))) + start;
		}

		public static float InOutCirc(float start, float end, float time, float duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (-end / 2 * (System.MathF.Sqrt(1 - (time * time)) - 1)) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * (System.MathF.Sqrt(1 - (time * time)) + 1)) + start;
			}
		}

		public static float OutInCirc(float start, float end, float time, float duration) => OutIn(OutCirc, InCirc, time, start, end, duration);

		public static Fix64 InCirc(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time /= duration;
			return (-end * (Fix64.Sqrt(1 - (time * time)) - 1)) + start;
		}

		public static Fix64 OutCirc(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = (time / duration) - 1;
			return (end * Fix64.Sqrt(1 - (time * time))) + start;
		}

		public static Fix64 InOutCirc(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time = time / duration * 2;
			if (time < 1)
			{
				return (-end / 2 * (Fix64.Sqrt(1 - (time * time)) - 1)) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * (Fix64.Sqrt(1 - (time * time)) + 1)) + start;
			}
		}

		public static Fix64 OutInCirc(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutIn(OutCirc, InCirc, time, start, end, duration);

		// ELASTIC

		public static float InElastic(float start, float end, float time, float duration, float a, float p)
		{
			if (time == 0) { return start; }

			time /= duration;

			if (time == 1) { return start + end; }

			float s;
			if (a < System.MathF.Abs(end))
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / (2 * System.MathF.PI) * System.MathF.Asin(end / a);
			}

			time--;

			return -(a * System.MathF.Pow(2, 10 * time) * System.MathF.Sin(((time * duration) - s) * (2 * System.MathF.PI) / p)) + start;
		}

		public static float InElastic(float start, float end, float time, float duration) => InElastic(start, end, time, duration, end, duration * 0.3f);

		public static float OutElastic(float start, float end, float time, float duration, float a, float p)
		{
			if (time == 0) { return start; }

			time /= duration;

			if (time == 1) { return start + end; }

			float s;

			if (a < System.MathF.Abs(end))
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / (2 * System.MathF.PI) * System.MathF.Asin(end / a);
			}

			return (a * System.MathF.Pow(2, -10 * time) * System.MathF.Sin(((time * duration) - s) * (2 * System.MathF.PI) / p)) + end + start;
		}

		public static float OutElastic(float start, float end, float time, float duration) => OutElastic(start, end, time, duration, end, duration * 0.3f);

		public static float InOutElastic(float start, float end, float time, float duration, float a, float p)
		{
			if (time == 0) { return start; }

			time = time / duration * 2;

			if (time == 2) { return start + end; }

			float s;

			if (a < System.MathF.Abs(end))
			{
				a = end;
				s = p / 4;
			}
			else
			{
				s = p / (2 * System.MathF.PI) * System.MathF.Asin(end / a);
			}

			if (time < 1)
			{
				time--;
				return (-0.5f * (a * System.MathF.Pow(2, 10 * time) * System.MathF.Sin(((time * duration) - s) * (2 * System.MathF.PI) / p))) + start;
			}
			else
			{
				time--;
				return (a * System.MathF.Pow(2, -10 * time) * System.MathF.Sin(((time * duration) - s) * (2 * System.MathF.PI) / p) * 0.5f) + end + start;
			}
		}

		public static float InOutElastic(float start, float end, float time, float duration) => InOutElastic(start, end, time, duration, end, duration * 0.3f * 1.5f);

		public static float OutInElastic(float start, float end, float time, float duration, float a, float p)
		{
			if (time < duration / 2)
			{
				return OutElastic(time * 2, start, end / 2, duration, a, p);
			}
			else
			{
				return InElastic((time * 2) - duration, start + (end / 2), end / 2, duration, a, p);
			}
		}

		public static float OutInElastic(float start, float end, float time, float duration) => OutInElastic(start, end, time, duration, end, duration * 0.3f);

		// TODO: Need Fix64 Asin for elastic

		// BACK

		public static float InBack(float start, float end, float time, float duration, float s = 1.70158f)
		{
			time /= duration;
			return (end * time * time * (((s + 1) * time) - s)) + start;
		}

		public static float InBack(float start, float end, float time, float duration) => InBack(start, end, time, duration, 1.70158f);

		public static float OutBack(float start, float end, float time, float duration, float s = 1.70158f)
		{
			time = (time / duration) - 1;
			return (end * ((time * time * (((s + 1) * time) + s)) + 1)) + start;
		}

		public static float OutBack(float start, float end, float time, float duration) => OutBack(start, end, time, duration, 1.70158f);

		public static float InOutBack(float start, float end, float time, float duration, float s = 1.70158f)
		{
			s *= 1.525f;
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time * (((s + 1) * time) - s))) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * ((time * time * (((s + 1) * time) + s)) + 2)) + start;
			}
		}

		public static float InOutBack(float start, float end, float time, float duration) => InOutBack(start, end, time, duration, 1.70158f);

		public static float OutInBack(float start, float end, float time, float duration, float s = 1.70158f)
		{
			if (time < duration / 2)
			{
				return OutBack(time * 2, start, end / 2, duration, s);
			}
			else
			{
				return InBack((time * 2) - duration, start + (end / 2), end / 2, duration, s);
			}
		}

		public static float OutInBack(float start, float end, float time, float duration) => OutInBack(start, end, time, duration, 1.70158f);

		private static readonly Fix64 S_DEFAULT = Fix64.FromFraction(170158, 100000);
		private static readonly Fix64 S_MULTIPLIER = Fix64.FromFraction(61, 40);

		public static Fix64 InBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration, Fix64 s)
		{
			time /= duration;
			return (end * time * time * (((s + 1) * time) - s)) + start;
		}

		public static Fix64 InBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => InBack(start, end, time, duration, S_DEFAULT);

		public static Fix64 OutBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration, Fix64 s)
		{
			time = (time / duration) - 1;
			return (end * ((time * time * (((s + 1) * time) + s)) + 1)) + start;
		}

		public static Fix64 OutBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutBack(start, end, time, duration, S_DEFAULT);

		public static Fix64 InOutBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration, Fix64 s)
		{
			s *= S_MULTIPLIER;
			time = time / duration * 2;
			if (time < 1)
			{
				return (end / 2 * (time * time * (((s + 1) * time) - s))) + start;
			}
			else
			{
				time -= 2;
				return (end / 2 * ((time * time * (((s + 1) * time) + s)) + 2)) + start;
			}
		}

		public static Fix64 InOutBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => InOutBack(start, end, time, duration, S_DEFAULT);

		public static Fix64 OutInBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration, Fix64 s)
		{
			if (time < duration / 2)
			{
				return OutBack(time * 2, start, end / 2, duration, s);
			}
			else
			{
				return InBack((time * 2) - duration, start + (end / 2), end / 2, duration, s);
			}
		}

		public static Fix64 OutInBack(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutInBack(start, end, time, duration, S_DEFAULT);

		// BOUNCE

		public static float InBounce(float start, float end, float time, float duration)
		{
			return end - OutBounce(duration - time, 0, end, duration) + start;
		}

		public static float OutBounce(float start, float end, float time, float duration)
		{
			time /= duration;
			if (time < 1 / 2.75f)
			{
				return (end * (7.5625f * time * time)) + start;
			}
			else if (time < 2 / 2.75f)
			{
				time -= (1.5f / 2.75f);
				return (end * ((7.5625f * time * time) + 0.75f)) + start;
			}
			else if (time < 2.5 / 2.75)
			{
				time -= (2.25f / 2.75f);
				return (end * ((7.5625f * time * time) + 0.9375f)) + start;
			}
			else
			{
				time -= (2.625f / 2.75f);
				return (end * ((7.5625f * time * time) + 0.984375f)) + start;
			}
		}

		public static float InOutBounce(float start, float end, float time, float duration)
		{
			if (time < duration / 2)
			{
				return (InBounce(time * 2, 0, end, duration) * 0.5f) + start;
			}
			else
			{
				return (OutBounce((time * 2) - duration, 0, end, duration) * 0.5f) + (end * 0.5f) + start;
			}
		}

		public static float OutInBounce(float start, float end, float time, float duration) => OutIn(OutBounce, InBounce, time, start, end, duration);

		private static readonly Fix64 BOUNCE_MULTIPLIER = Fix64.FromFraction(121, 16);

		public static Fix64 InBounce(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			return end - OutBounce(duration - time, Fix64.Zero, end, duration) + start;
		}

		// FIXME: these constants are kinda gnarly, could maybe define them as static readonlys
		public static Fix64 OutBounce(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			time /= duration;
			if (time < Fix64.FromFraction(4, 11))
			{
				return (end * (BOUNCE_MULTIPLIER * time * time)) + start;
			}
			else if (time < Fix64.FromFraction(8, 11))
			{
				time -= Fix64.FromFraction(6, 11);
				return (end * ((BOUNCE_MULTIPLIER * time * time) + Fix64.FromFraction(3, 4))) + start;
			}
			else if (time < Fix64.FromFraction(10, 11))
			{
				time -= Fix64.FromFraction(9, 11);
				return (end * ((BOUNCE_MULTIPLIER * time * time) + Fix64.FromFraction(15, 16))) + start;
			}
			else
			{
				time -= Fix64.FromFraction(21, 22);
				return (end * ((BOUNCE_MULTIPLIER * time * time) + Fix64.FromFraction(63, 64))) + start;
			}
		}

		public static Fix64 InOutBounce(Fix64 start, Fix64 end, Fix64 time, Fix64 duration)
		{
			if (time < duration / 2)
			{
				return (InBounce(time * 2, Fix64.Zero, end, duration) * Fix64.FromFraction(1, 2)) + start;
			}
			else
			{
				return (OutBounce((time * 2) - duration, Fix64.Zero, end, duration) * Fix64.FromFraction(1, 2)) + (end * Fix64.FromFraction(1, 2)) + start;
			}
		}

		public static Fix64 OutInBounce(Fix64 start, Fix64 end, Fix64 time, Fix64 duration) => OutIn(OutBounce, InBounce, time, start, end, duration);

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

			private static Dictionary<Float, EasingFunctionFloat> FloatLookup = new Dictionary<Float, EasingFunctionFloat>
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

			private static Dictionary<Fixed, EasingFunctionFixed> FixedLookup = new Dictionary<Fixed, EasingFunctionFixed>
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

			public static EasingFunctionFloat Get(Float functionEnum)
			{
				return FloatLookup[functionEnum];
			}

			public static EasingFunctionFixed Get(Fixed functionEnum)
			{
				return FixedLookup[functionEnum];
			}
		}
	}
}
