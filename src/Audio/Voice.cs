using System;
using System.Runtime.InteropServices;
using EasingFunction = System.Func<float, float>;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Handles audio playback from audio buffer data. Can be configured with a variety of parameters.
	/// </summary>
	public abstract unsafe class Voice : AudioResource
	{
		protected IntPtr handle;
		public IntPtr Handle => handle;

		public uint SourceChannelCount { get; }
		public uint DestinationChannelCount { get; }

		protected SubmixVoice OutputVoice;
		private ReverbEffect ReverbEffect;

		protected byte* pMatrixCoefficients;

		public bool Is3D { get; protected set; }

		private float dopplerFactor;
		/// <summary>
		/// The strength of the doppler effect on this voice.
		/// </summary>
		public float DopplerFactor
		{
			get => dopplerFactor;
			set
			{
				if (dopplerFactor != value)
				{
					dopplerFactor = value;
					UpdatePitch();
				}
			}
		}

		private float volume = 1;
		/// <summary>
		/// The overall volume level for the voice.
		/// </summary>
		public float Volume
		{
			get => volume;
			internal set
			{
				value = Math.MathHelper.Max(0, value);
				if (volume != value)
				{
					volume = value;
					FAudio.FAudioVoice_SetVolume(Handle, volume, 0);
				}
			}
		}

		private float pitch = 0;
		/// <summary>
		/// The pitch of the voice.
		/// </summary>
		public float Pitch
		{
			get => pitch;
			internal set
			{
				value = Math.MathHelper.Clamp(value, -1f, 1f);
				if (pitch != value)
				{
					pitch = value;
					UpdatePitch();
				}
			}
		}

		private const float MAX_FILTER_FREQUENCY = 1f;
		private const float MAX_FILTER_ONEOVERQ = 1.5f;

		private FAudio.FAudioFilterParameters filterParameters = new FAudio.FAudioFilterParameters
		{
			Type = FAudio.FAudioFilterType.FAudioLowPassFilter,
			Frequency = 1f,
			OneOverQ = 1f
		};

		/// <summary>
		/// The frequency cutoff on the voice filter.
		/// </summary>
		public float FilterFrequency
		{
			get => filterParameters.Frequency;
			internal set
			{
				value = System.Math.Clamp(value, 0.01f, MAX_FILTER_FREQUENCY);
				if (filterParameters.Frequency != value)
				{
					filterParameters.Frequency = value;

					FAudio.FAudioVoice_SetFilterParameters(
						Handle,
						ref filterParameters,
						0
					);
				}
			}
		}

		/// <summary>
		/// Reciprocal of Q factor.
		/// Controls how quickly frequencies beyond the filter frequency are dampened.
		/// </summary>
		public float FilterOneOverQ
		{
			get => filterParameters.OneOverQ;
			internal set
			{
				value = System.Math.Clamp(value, 0.01f, MAX_FILTER_ONEOVERQ);
				if (filterParameters.OneOverQ != value)
				{
					filterParameters.OneOverQ = value;

					FAudio.FAudioVoice_SetFilterParameters(
						Handle,
						ref filterParameters,
						0
					);
				}
			}
		}

		private FilterType filterType;
		/// <summary>
		/// The frequency filter that is applied to the voice.
		/// </summary>
		public FilterType FilterType
		{
			get => filterType;
			set
			{
				if (filterType != value)
				{
					filterType = value;

					switch (filterType)
					{
						case FilterType.None:
							filterParameters = new FAudio.FAudioFilterParameters
							{
								Type = FAudio.FAudioFilterType.FAudioLowPassFilter,
								Frequency = 1f,
								OneOverQ = 1f
							};
							break;

						case FilterType.LowPass:
							filterParameters.Type = FAudio.FAudioFilterType.FAudioLowPassFilter;
							filterParameters.Frequency = 1f;
							break;

						case FilterType.BandPass:
							filterParameters.Type = FAudio.FAudioFilterType.FAudioBandPassFilter;
							break;

						case FilterType.HighPass:
							filterParameters.Type = FAudio.FAudioFilterType.FAudioHighPassFilter;
							filterParameters.Frequency = 0f;
							break;
					}

					FAudio.FAudioVoice_SetFilterParameters(
						Handle,
						ref filterParameters,
						0
					);
				}
			}
		}

		protected float pan = 0;
		/// <summary>
		/// Left-right panning. -1 is hard left pan, 1 is hard right pan.
		/// </summary>
		public float Pan
		{
			get => pan;
			internal set
			{
				value = Math.MathHelper.Clamp(value, -1f, 1f);
				if (pan != value)
				{
					pan = value;

					if (pan < -1f)
					{
						pan = -1f;
					}
					if (pan > 1f)
					{
						pan = 1f;
					}

					if (Is3D) { return; }

					SetPanMatrixCoefficients();
					FAudio.FAudioVoice_SetOutputMatrix(
						Handle,
						OutputVoice.Handle,
						SourceChannelCount,
						DestinationChannelCount,
						(nint) pMatrixCoefficients,
						0
					);
				}
			}
		}

		private float reverb;
		/// <summary>
		/// The wet-dry mix of the reverb effect.
		/// Has no effect if SetReverbEffectChain has not been called.
		/// </summary>
		public unsafe float Reverb
		{
			get => reverb;
			internal set
			{
				if (ReverbEffect != null)
				{
					value = MathF.Max(0, value);
					if (reverb != value)
					{
						reverb = value;

						float* outputMatrix = (float*) pMatrixCoefficients;
						outputMatrix[0] = reverb;
						if (SourceChannelCount == 2)
						{
							outputMatrix[1] = reverb;
						}

						FAudio.FAudioVoice_SetOutputMatrix(
							Handle,
							ReverbEffect.Handle,
							SourceChannelCount,
							1,
							(nint) pMatrixCoefficients,
							0
						);
					}
				}

				#if DEBUG
				if (ReverbEffect == null)
				{
					Logger.LogWarn("Tried to set reverb value before applying a reverb effect");
				}
				#endif
			}
		}

		public Voice(AudioDevice device, uint sourceChannelCount, uint destinationChannelCount) : base(device)
		{
			SourceChannelCount = sourceChannelCount;
			DestinationChannelCount = destinationChannelCount;
			nuint memsize = 4 * sourceChannelCount * destinationChannelCount;
			pMatrixCoefficients = (byte*) NativeMemory.AllocZeroed(memsize);
			SetPanMatrixCoefficients();
		}

		/// <summary>
		/// Sets the pitch of the voice. Valid input range is -1f to 1f.
		/// </summary>
		public void SetPitch(float targetValue)
		{
			Pitch = targetValue;
			Device.ClearTweens(this, AudioTweenProperty.Pitch);
		}

		/// <summary>
		/// Sets the pitch of the voice over a time duration in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public void SetPitch(float targetValue, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Pitch, easingFunction, Pitch, targetValue, duration, 0);
		}

		/// <summary>
		/// Sets the pitch of the voice over a time duration in seconds after a delay in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public void SetPitch(float targetValue, float delayTime, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Pitch, easingFunction, Pitch, targetValue, duration, delayTime);
		}

		/// <summary>
		/// Sets the volume of the voice. Minimum value is 0f.
		/// </summary>
		public void SetVolume(float targetValue)
		{
			Volume = targetValue;
			Device.ClearTweens(this, AudioTweenProperty.Volume);
		}

		/// <summary>
		/// Sets the volume of the voice over a time duration in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public void SetVolume(float targetValue, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Volume, easingFunction, Volume, targetValue, duration, 0);
		}

		/// <summary>
		/// Sets the volume of the voice over a time duration in seconds after a delay in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public void SetVolume(float targetValue, float delayTime, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Volume, easingFunction, Volume, targetValue, duration, delayTime);
		}

		/// <summary>
		/// Sets the frequency cutoff on the voice filter. Valid range is 0.01f to 1f.
		/// </summary>
		public void SetFilterFrequency(float targetValue)
		{
			FilterFrequency = targetValue;
			Device.ClearTweens(this, AudioTweenProperty.FilterFrequency);
		}

		/// <summary>
		/// Sets the frequency cutoff on the voice filter over a time duration in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public void SetFilterFrequency(float targetValue, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.FilterFrequency, easingFunction, FilterFrequency, targetValue, duration, 0);
		}

		/// <summary>
		/// Sets the frequency cutoff on the voice filter over a time duration in seconds after a delay in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public void SetFilterFrequency(float targetValue, float delayTime, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.FilterFrequency, easingFunction, FilterFrequency, targetValue, duration, delayTime);
		}

		/// <summary>
		/// Sets reciprocal of Q factor on the frequency filter.
		/// Controls how quickly frequencies beyond the filter frequency are dampened.
		/// </summary>
		public void SetFilterOneOverQ(float targetValue)
		{
			FilterOneOverQ = targetValue;
		}

		/// <summary>
		/// Sets a left-right panning value. -1f is hard left pan, 1f is hard right pan.
		/// </summary>
		public virtual void SetPan(float targetValue)
		{
			Pan = targetValue;
			Device.ClearTweens(this, AudioTweenProperty.Pan);
		}

		/// <summary>
		/// Sets a left-right panning value over a time duration in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public virtual void SetPan(float targetValue, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Pan, easingFunction, Pan, targetValue, duration, 0);
		}

		/// <summary>
		/// Sets a left-right panning value over a time duration in seconds after a delay in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public virtual void SetPan(float targetValue, float delayTime, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Pan, easingFunction, Pan, targetValue, duration, delayTime);
		}

		/// <summary>
		/// Sets the wet-dry mix value of the reverb effect. Minimum value is 0f.
		/// </summary>
		public virtual void SetReverb(float targetValue)
		{
			Reverb = targetValue;
			Device.ClearTweens(this, AudioTweenProperty.Reverb);
		}

		/// <summary>
		/// Sets the wet-dry mix value of the reverb effect over a time duration in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public virtual void SetReverb(float targetValue, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Reverb, easingFunction, Volume, targetValue, duration, 0);
		}

		/// <summary>
		/// Sets the wet-dry mix value of the reverb effect over a time duration in seconds after a delay in seconds.
		/// </summary>
		/// <param name="easingFunction">An easing function. See MoonWorks.Math.Easing.Function.Float</param>
		public virtual void SetReverb(float targetValue, float delayTime, float duration, EasingFunction easingFunction)
		{
			Device.CreateTween(this, AudioTweenProperty.Reverb, easingFunction, Volume, targetValue, duration, delayTime);
		}

		/// <summary>
		/// Sets the output voice for this voice.
		/// </summary>
		/// <param name="send">Where the output should be sent.</param>
		public unsafe void SetOutputVoice(SubmixVoice send)
		{
			OutputVoice = send;

			if (ReverbEffect != null)
			{
				SetReverbEffectChain(ReverbEffect);
			}
			else
			{
				FAudio.FAudioSendDescriptor* sendDesc = stackalloc FAudio.FAudioSendDescriptor[1];
				sendDesc[0].Flags = 0;
				sendDesc[0].pOutputVoice = send.Handle;

				var sends = new FAudio.FAudioVoiceSends();
				sends.SendCount = 1;
				sends.pSends = (nint) sendDesc;

				FAudio.FAudioVoice_SetOutputVoices(
					Handle,
					ref sends
				);
			}
		}

		/// <summary>
		/// Applies a reverb effect chain to this voice.
		/// </summary>
		public unsafe void SetReverbEffectChain(ReverbEffect reverbEffect)
		{
			var sendDesc = stackalloc FAudio.FAudioSendDescriptor[2];
			sendDesc[0].Flags = 0;
			sendDesc[0].pOutputVoice = OutputVoice.Handle;
			sendDesc[1].Flags = 0;
			sendDesc[1].pOutputVoice = reverbEffect.Handle;

			var sends = new FAudio.FAudioVoiceSends();
			sends.SendCount = 2;
			sends.pSends = (nint) sendDesc;

			FAudio.FAudioVoice_SetOutputVoices(
				Handle,
				ref sends
			);

			ReverbEffect = reverbEffect;
		}

		/// <summary>
		/// Removes the reverb effect chain from this voice.
		/// </summary>
		public void RemoveReverbEffectChain()
		{
			if (ReverbEffect != null)
			{
				ReverbEffect = null;
				reverb = 0;
				SetOutputVoice(OutputVoice);
			}
		}

		/// <summary>
		/// Resets all voice parameters to defaults.
		/// </summary>
		public virtual void Reset()
		{
			RemoveReverbEffectChain();
			Volume = 1;
			Pan = 0;
			Pitch = 0;
			FilterType = FilterType.None;
			SetOutputVoice(Device.MasteringVoice);
		}

		// Taken from https://github.com/FNA-XNA/FNA/blob/master/src/Audio/SoundEffectInstance.cs
		private unsafe void SetPanMatrixCoefficients()
		{
			/* Two major things to notice:
			 * 1. The spec assumes any speaker count >= 2 has Front Left/Right.
			 * 2. Stereo panning is WAY more complicated than you think.
			 *    The main thing is that hard panning does NOT eliminate an
			 *    entire channel; the two channels are blended on each side.
			 * -flibit
			 */
			float* outputMatrix = (float*) pMatrixCoefficients;
			if (SourceChannelCount == 1)
			{
				if (DestinationChannelCount == 1)
				{
					outputMatrix[0] = 1.0f;
				}
				else
				{
					outputMatrix[0] = (pan > 0.0f) ? (1.0f - pan) : 1.0f;
					outputMatrix[1] = (pan < 0.0f) ? (1.0f + pan) : 1.0f;
				}
			}
			else
			{
				if (DestinationChannelCount == 1)
				{
					outputMatrix[0] = 1.0f;
					outputMatrix[1] = 1.0f;
				}
				else
				{
					if (pan <= 0.0f)
					{
						// Left speaker blends left/right channels
						outputMatrix[0] = 0.5f * pan + 1.0f;
						outputMatrix[1] = 0.5f * -pan;
						// Right speaker gets less of the right channel
						outputMatrix[2] = 0.0f;
						outputMatrix[3] = pan + 1.0f;
					}
					else
					{
						// Left speaker gets less of the left channel
						outputMatrix[0] = -pan + 1.0f;
						outputMatrix[1] = 0.0f;
						// Right speaker blends right/left channels
						outputMatrix[2] = 0.5f * pan;
						outputMatrix[3] = 0.5f * -pan + 1.0f;
					}
				}
			}
		}

		protected void UpdatePitch()
		{
			float doppler;
			float dopplerScale = Device.DopplerScale;
			if (!Is3D || dopplerScale == 0.0f)
			{
				doppler = 1.0f;
			}
			else
			{
				doppler = DopplerFactor * dopplerScale;
			}

			FAudio.FAudioSourceVoice_SetFrequencyRatio(
				Handle,
				(float) System.Math.Pow(2.0, pitch) * doppler,
				0
			);
		}

		protected unsafe override void Destroy()
		{
			NativeMemory.Free(pMatrixCoefficients);
			FAudio.FAudioVoice_DestroyVoice(Handle);
		}
	}
}
