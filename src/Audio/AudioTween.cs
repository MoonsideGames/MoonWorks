using System.Collections.Generic;
using EasingFunction = System.Func<float, float>;

namespace MoonWorks.Audio
{
	internal enum AudioTweenProperty
	{
		Pan,
		Pitch,
		Volume,
		FilterFrequency,
		Reverb
	}

	internal class AudioTween
	{
		public SoundInstance SoundInstance;
		public AudioTweenProperty Property;
		public EasingFunction EasingFunction;
		public float Time;
		public float StartValue;
		public float EndValue;
		public float DelayTime;
		public float Duration;
	}

	internal class AudioTweenPool
	{
		private Queue<AudioTween> Tweens = new Queue<AudioTween>(16);

		public AudioTweenPool()
		{
			for (int i = 0; i < 16; i += 1)
			{
				Tweens.Enqueue(new AudioTween());
			}
		}

		public AudioTween Obtain()
		{
			if (Tweens.Count > 0)
			{
				var tween = Tweens.Dequeue();
				return tween;
			}
			else
			{
				return new AudioTween();
			}
		}

		public void Free(AudioTween tween)
		{
			tween.SoundInstance = null;
			Tweens.Enqueue(tween);
		}
	}
}
