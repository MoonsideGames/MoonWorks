using System;
using System.Runtime.InteropServices;
using MoonWorks.Math;

namespace MoonWorks.Audio
{
	public abstract class SoundInstance : AudioResource
	{
		internal IntPtr Handle;
		internal FAudio.FAudioWaveFormatEx Format;
		public bool Loop { get; protected set; } = false;

		protected FAudio.F3DAUDIO_DSP_SETTINGS dspSettings;

		public bool Is3D { get; protected set; }

		public abstract SoundState State { get; protected set; }

		private float _pan = 0;
		public float Pan
		{
			get => _pan;
			set
			{
				_pan = value;

				if (_pan < -1f)
				{
					_pan = -1f;
				}
				if (_pan > 1f)
				{
					_pan = 1f;
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

		private float _pitch = 1;
		public float Pitch
		{
			get => _pitch;
			set
			{
				_pitch = MathHelper.Clamp(value, -1f, 1f);
				UpdatePitch();
			}
		}

		private float _volume = 1;
		public float Volume
		{
			get => _volume;
			set
			{
				_volume = value;
				FAudio.FAudioVoice_SetVolume(Handle, _volume, 0);
			}
		}

		private float _reverb;
		public unsafe float Reverb
		{
			get => _reverb;
			set
			{
				_reverb = value;

				float* outputMatrix = (float*) dspSettings.pMatrixCoefficients;
				outputMatrix[0] = _reverb;
				if (dspSettings.SrcChannelCount == 2)
				{
					outputMatrix[1] = _reverb;
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

		private float _lowPassFilter;
		public float LowPassFilter
		{
			get => _lowPassFilter;
			set
			{
				_lowPassFilter = value;

				FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters
				{
					Type = FAudio.FAudioFilterType.FAudioLowPassFilter,
					Frequency = _lowPassFilter,
					OneOverQ = 1f
				};
				FAudio.FAudioVoice_SetFilterParameters(
					Handle,
					ref p,
					0
				);
			}
		}

		private float _highPassFilter;
		public float HighPassFilter
		{
			get => _highPassFilter;
			set
			{
				_highPassFilter = value;

				FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters
				{
					Type = FAudio.FAudioFilterType.FAudioHighPassFilter,
					Frequency = _highPassFilter,
					OneOverQ = 1f
				};
				FAudio.FAudioVoice_SetFilterParameters(
					Handle,
					ref p,
					0
				);
			}
		}

		private float _bandPassFilter;
		public float BandPassFilter
		{
			get => _bandPassFilter;
			set
			{
				_bandPassFilter = value;

				FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters
				{
					Type = FAudio.FAudioFilterType.FAudioBandPassFilter,
					Frequency = _bandPassFilter,
					OneOverQ = 1f
				};
				FAudio.FAudioVoice_SetFilterParameters(
					Handle,
					ref p,
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

		public abstract void Play(bool loop);
		public abstract void Pause();
		public abstract void Stop(bool immediate);

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
				(float) System.Math.Pow(2.0, _pitch) * doppler,
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
					outputMatrix[0] = (_pan > 0.0f) ? (1.0f - _pan) : 1.0f;
					outputMatrix[1] = (_pan < 0.0f) ? (1.0f + _pan) : 1.0f;
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
					if (_pan <= 0.0f)
					{
						// Left speaker blends left/right channels
						outputMatrix[0] = 0.5f * _pan + 1.0f;
						outputMatrix[1] = 0.5f * -_pan;
						// Right speaker gets less of the right channel
						outputMatrix[2] = 0.0f;
						outputMatrix[3] = _pan + 1.0f;
					}
					else
					{
						// Left speaker gets less of the left channel
						outputMatrix[0] = -_pan + 1.0f;
						outputMatrix[1] = 0.0f;
						// Right speaker blends right/left channels
						outputMatrix[2] = 0.5f * _pan;
						outputMatrix[3] = 0.5f * -_pan + 1.0f;
					}
				}
			}
		}

		protected override void Destroy()
		{
			Stop(true);

			FAudio.FAudioVoice_DestroyVoice(Handle);
			Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
		}
	}
}
