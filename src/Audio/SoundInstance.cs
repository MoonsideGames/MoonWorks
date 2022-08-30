using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public abstract class SoundInstance : AudioResource
	{
		internal IntPtr Handle;
		internal FAudio.FAudioWaveFormatEx Format;

		protected FAudio.F3DAUDIO_DSP_SETTINGS dspSettings;

		public bool Is3D { get; protected set; }

		public virtual SoundState State { get; protected set; }

		private float pan = 0;
		public float Pan
		{
			get => pan;
			set
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
					Device.MasteringVoice,
					dspSettings.SrcChannelCount,
					dspSettings.DstChannelCount,
					dspSettings.pMatrixCoefficients,
					0
				);
			}
		}

		private float pitch = 0;
		public float Pitch
		{
			get => pitch;
			set
			{
				pitch = Math.MathHelper.Clamp(value, -1f, 1f);
				UpdatePitch();
			}
		}

		private float volume = 1;
		public float Volume
		{
			get => volume;
			set
			{
				volume = value;
				FAudio.FAudioVoice_SetVolume(Handle, volume, 0);
			}
		}

		private float reverb;
		public unsafe float Reverb
		{
			get => reverb;
			set
			{
				reverb = value;

				float* outputMatrix = (float*) dspSettings.pMatrixCoefficients;
				outputMatrix[0] = reverb;
				if (dspSettings.SrcChannelCount == 2)
				{
					outputMatrix[1] = reverb;
				}

				FAudio.FAudioVoice_SetOutputMatrix(
					Handle,
					Device.ReverbVoice,
					dspSettings.SrcChannelCount,
					1,
					dspSettings.pMatrixCoefficients,
					0
				);
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

		private float FilterFrequency
		{
			get => filterParameters.Frequency;
			set
			{
				value = System.Math.Clamp(value, 0.01f, MAX_FILTER_FREQUENCY);
				filterParameters.Frequency = value;

				FAudio.FAudioVoice_SetFilterParameters(
					Handle,
					ref filterParameters,
					0
				);
			}
		}

		private float FilterOneOverQ
		{
			get => filterParameters.OneOverQ;
			set
			{
				value = System.Math.Clamp(value, 0.01f, MAX_FILTER_ONEOVERQ);
				filterParameters.OneOverQ = value;

				FAudio.FAudioVoice_SetFilterParameters(
					Handle,
					ref filterParameters,
					0
				);
			}
		}

		private FilterType filterType;
		public FilterType FilterType
		{
			get => filterType;
			set
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
						break;

					case FilterType.BandPass:
						filterParameters.Type = FAudio.FAudioFilterType.FAudioBandPassFilter;
						break;

					case FilterType.HighPass:
						filterParameters.Type = FAudio.FAudioFilterType.FAudioHighPassFilter;
						break;
				}

				FAudio.FAudioVoice_SetFilterParameters(
					Handle,
					ref filterParameters,
					0
				);
			}
		}

		public SoundInstance(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond
		) : base(device)
		{
			var format = new FAudio.FAudioWaveFormatEx
			{
				wFormatTag = formatTag,
				wBitsPerSample = bitsPerSample,
				nChannels = channels,
				nBlockAlign = blockAlign,
				nSamplesPerSec = samplesPerSecond,
				nAvgBytesPerSec = blockAlign * samplesPerSecond
			};

			Format = format;

			FAudio.FAudio_CreateSourceVoice(
				Device.Handle,
				out Handle,
				ref Format,
				FAudio.FAUDIO_VOICE_USEFILTER,
				FAudio.FAUDIO_DEFAULT_FREQ_RATIO,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero
			);

			if (Handle == IntPtr.Zero)
			{
				Logger.LogError("SoundInstance failed to initialize!");
				return;
			}

			InitDSPSettings(Format.nChannels);

			// FIXME: not everything should be running through reverb...
			/*
			FAudio.FAudioVoice_SetOutputVoices(
				Handle,
				ref Device.ReverbSends
			);
			*/

			State = SoundState.Stopped;
		}

		public void Apply3D(AudioListener listener, AudioEmitter emitter)
		{
			Is3D = true;

			emitter.emitterData.CurveDistanceScaler = Device.CurveDistanceScalar;
			emitter.emitterData.ChannelCount = dspSettings.SrcChannelCount;

			FAudio.F3DAudioCalculate(
				Device.Handle3D,
				ref listener.listenerData,
				ref emitter.emitterData,
				FAudio.F3DAUDIO_CALCULATE_MATRIX | FAudio.F3DAUDIO_CALCULATE_DOPPLER,
				ref dspSettings
			);

			UpdatePitch();
			FAudio.FAudioVoice_SetOutputMatrix(
				Handle,
				Device.MasteringVoice,
				dspSettings.SrcChannelCount,
				dspSettings.DstChannelCount,
				dspSettings.pMatrixCoefficients,
				0
			);
		}

		public abstract void Play();
		public abstract void Pause();
		public abstract void Stop();
		public abstract void StopImmediate();

		private void InitDSPSettings(uint srcChannels)
		{
			dspSettings = new FAudio.F3DAUDIO_DSP_SETTINGS();
			dspSettings.DopplerFactor = 1f;
			dspSettings.SrcChannelCount = srcChannels;
			dspSettings.DstChannelCount = Device.DeviceDetails.OutputFormat.Format.nChannels;

			int memsize = (
				4 *
				(int) dspSettings.SrcChannelCount *
				(int) dspSettings.DstChannelCount
			);

			dspSettings.pMatrixCoefficients = Marshal.AllocHGlobal(memsize);
			unsafe
			{
				byte* memPtr = (byte*) dspSettings.pMatrixCoefficients;
				for (int i = 0; i < memsize; i += 1)
				{
					memPtr[i] = 0;
				}
			}
			SetPanMatrixCoefficients();
		}

		private void UpdatePitch()
		{
			float doppler;
			float dopplerScale = Device.DopplerScale;
			if (!Is3D || dopplerScale == 0.0f)
			{
				doppler = 1.0f;
			}
			else
			{
				doppler = dspSettings.DopplerFactor * dopplerScale;
			}

			FAudio.FAudioSourceVoice_SetFrequencyRatio(
				Handle,
				(float) System.Math.Pow(2.0, pitch) * doppler,
				0
			);
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
			float* outputMatrix = (float*) dspSettings.pMatrixCoefficients;
			if (dspSettings.SrcChannelCount == 1)
			{
				if (dspSettings.DstChannelCount == 1)
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
				if (dspSettings.DstChannelCount == 1)
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

		protected override void Destroy()
		{
			StopImmediate();
			FAudio.FAudioVoice_DestroyVoice(Handle);
			Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
		}
	}
}
