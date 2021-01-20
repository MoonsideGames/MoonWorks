using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
    public abstract class SoundInstance : IDisposable
    {
        protected AudioDevice Device { get; }
        internal IntPtr Handle { get; }
        public Sound Parent { get; }
        protected FAudio.F3DAUDIO_DSP_SETTINGS dspSettings;

        protected bool is3D;
        private bool IsDisposed;

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

                if (is3D) { return; }

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
                float doppler;
                if (!is3D || Device.DopplerScale == 0f)
                {
                    doppler = 1f;
                }
                else
                {
                    doppler = dspSettings.DopplerFactor * Device.DopplerScale;
                }

                _pitch = value;
                FAudio.FAudioSourceVoice_SetFrequencyRatio(
                    Handle,
                    (float) Math.Pow(2.0, _pitch) * doppler,
                    0
                );
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

                FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters();
                p.Type = FAudio.FAudioFilterType.FAudioLowPassFilter;
                p.Frequency = _lowPassFilter;
                p.OneOverQ = 1f;
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

                FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters();
                p.Type = FAudio.FAudioFilterType.FAudioHighPassFilter;
                p.Frequency = _highPassFilter;
                p.OneOverQ = 1f;
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

                FAudio.FAudioFilterParameters p = new FAudio.FAudioFilterParameters();
                p.Type = FAudio.FAudioFilterType.FAudioBandPassFilter;
                p.Frequency = _bandPassFilter;
                p.OneOverQ = 1f;
                FAudio.FAudioVoice_SetFilterParameters(
                    Handle,
                    ref p,
                    0
                );
            }
        }

        public SoundInstance(
            AudioDevice device,
            Sound parent,
            bool is3D
        ) {
            Device = device;
            Parent = parent;

            FAudio.FAudioWaveFormatEx format = Parent.Format;

            FAudio.FAudio_CreateSourceVoice(
                Device.Handle,
                out var handle,
                ref format,
                FAudio.FAUDIO_VOICE_USEFILTER,
                FAudio.FAUDIO_DEFAULT_FREQ_RATIO,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero
            );

            if (handle == IntPtr.Zero)
            {
                Logger.LogError("SoundInstance failed to initialize!");
                return;
            }

            Handle = handle;
            this.is3D = is3D;
            InitDSPSettings(Parent.Format.nChannels);
        }

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
					outputMatrix[1] = (_pan < 0.0f) ? (1.0f  + _pan) : 1.0f;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                FAudio.FAudioVoice_DestroyVoice(Handle);
                Marshal.FreeHGlobal(dspSettings.pMatrixCoefficients);
                IsDisposed = true;
            }
        }

        ~SoundInstance()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
