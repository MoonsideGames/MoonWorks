using System;
using System.Collections.Generic;

namespace MoonWorks.Audio
{
	internal class AudioTweenManager
	{
		private AudioTweenPool AudioTweenPool = new AudioTweenPool();
		private readonly Dictionary<(WeakReference, AudioTweenProperty), AudioTween> AudioTweens = new Dictionary<(WeakReference, AudioTweenProperty), AudioTween>();
		private readonly List<AudioTween> DelayedAudioTweens = new List<AudioTween>();

		public void Update(float elapsedSeconds)
		{
			for (var i = DelayedAudioTweens.Count - 1; i >= 0; i--)
			{
				var audioTween = DelayedAudioTweens[i];
				if (audioTween.SoundInstanceReference.Target is SoundInstance soundInstance)
				{
					audioTween.Time += elapsedSeconds;

					if (audioTween.Time >= audioTween.DelayTime)
					{
						// set the tween start value to the current value of the property
						switch (audioTween.Property)
						{
							case AudioTweenProperty.Pan:
								audioTween.StartValue = soundInstance.Pan;
								break;

							case AudioTweenProperty.Pitch:
								audioTween.StartValue = soundInstance.Pitch;
								break;

							case AudioTweenProperty.Volume:
								audioTween.StartValue = soundInstance.Volume;
								break;

							case AudioTweenProperty.FilterFrequency:
								audioTween.StartValue = soundInstance.FilterFrequency;
								break;

							case AudioTweenProperty.Reverb:
								audioTween.StartValue = soundInstance.Reverb;
								break;
						}

						audioTween.Time = 0;
						DelayedAudioTweens.RemoveAt(i);

						AddTween(audioTween);
					}
				}
				else
				{
					AudioTweenPool.Free(audioTween);
					DelayedAudioTweens.RemoveAt(i);
				}
			}

			foreach (var (key, audioTween) in AudioTweens)
			{
				bool finished = true;
				if (audioTween.SoundInstanceReference.Target is SoundInstance soundInstance)
				{
					finished = UpdateAudioTween(audioTween, soundInstance, elapsedSeconds);
				}

				if (finished)
				{
					AudioTweenPool.Free(audioTween);
					AudioTweens.Remove(key);
				}
			}
		}

		public void CreateTween(
			SoundInstance soundInstance,
			AudioTweenProperty property,
			System.Func<float, float> easingFunction,
			float start,
			float end,
			float duration,
			float delayTime
		) {
			var tween = AudioTweenPool.Obtain();
			tween.SoundInstanceReference = soundInstance.weakReference;
			tween.Property = property;
			tween.EasingFunction = easingFunction;
			tween.StartValue = start;
			tween.EndValue = end;
			tween.Duration = duration;
			tween.Time = 0;
			tween.DelayTime = delayTime;

			if (delayTime == 0)
			{
				AddTween(tween);
			}
			else
			{
				DelayedAudioTweens.Add(tween);
			}
		}

		public void ClearTweens(WeakReference soundReference, AudioTweenProperty property)
		{
			AudioTweens.Remove((soundReference, property));
		}

		private void AddTween(
			AudioTween audioTween
		) {
			// if a tween with the same sound and property already exists, get rid of it
			if (AudioTweens.TryGetValue((audioTween.SoundInstanceReference, audioTween.Property), out var currentTween))
			{
				AudioTweenPool.Free(currentTween);
			}

			AudioTweens[(audioTween.SoundInstanceReference, audioTween.Property)] = audioTween;
		}

		private static bool UpdateAudioTween(AudioTween audioTween, SoundInstance soundInstance, float delta)
		{
			float value;
			audioTween.Time += delta;

			var finished = audioTween.Time >= audioTween.Duration;
			if (finished)
			{
				value = audioTween.EndValue;
			}
			else
			{
				value = MoonWorks.Math.Easing.Interp(
					audioTween.StartValue,
					audioTween.EndValue,
					audioTween.Time,
					audioTween.Duration,
					audioTween.EasingFunction
				);
			}

			switch (audioTween.Property)
			{
				case AudioTweenProperty.Pan:
					soundInstance.Pan = value;
					break;

				case AudioTweenProperty.Pitch:
					soundInstance.Pitch = value;
					break;

				case AudioTweenProperty.Volume:
					soundInstance.Volume = value;
					break;

				case AudioTweenProperty.FilterFrequency:
					soundInstance.FilterFrequency = value;
					break;

				case AudioTweenProperty.Reverb:
					soundInstance.Reverb = value;
					break;
			}

			return finished;
		}
	}
}
