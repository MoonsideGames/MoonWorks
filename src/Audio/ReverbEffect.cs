using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// A type of submix that applies a reverb effect.
	/// Use this with SetOutputVoice and the Reverb property on the source/submix voice.
	/// </summary>
	public unsafe class ReverbEffect : SubmixVoice
	{
		// Defaults based on FAUDIOFX_I3DL2_PRESET_GENERIC
		public static FAudio.FAudioFXReverbParameters DefaultParams = new FAudio.FAudioFXReverbParameters
		{
			WetDryMix = 100.0f,
			ReflectionsDelay = 7,
			ReverbDelay = 11,
			RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY,
			PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
			PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION,
			PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
			PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX,
			EarlyDiffusion = 15,
			LateDiffusion = 15,
			LowEQGain = 8,
			LowEQCutoff = 4,
			HighEQGain = 8,
			HighEQCutoff = 6,
			RoomFilterFreq = 5000f,
			RoomFilterMain = -10f,
			RoomFilterHF = -1f,
			ReflectionsGain = -26.0200005f,
			ReverbGain = 10.0f,
			DecayTime = 1.49000001f,
			Density = 100.0f,
			RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE
		};

		public FAudio.FAudioFXReverbParameters Params { get; private set; }

		// Reverb is composed of a dry signal and a wet signal - so this object will just manage both
		// the main voice handle will be the dry voice
		internal Voice WetVoice;

		// Can fail, so check just in case
		internal bool WetVoiceInitialized { get; private set; }

		private void Init()
		{
			WetVoice = new SubmixVoice(Device, OutputVoice, SourceChannelCount, SampleRate, ProcessingStage);

			/* Init reverb */
			IntPtr reverb;
			FAudio.FAudioCreateReverb(out reverb, 0);

			var chain = new FAudio.FAudioEffectChain();
			var descriptor = new FAudio.FAudioEffectDescriptor
			{
				InitialState = 1,
				OutputChannels = 1,
				pEffect = reverb
			};

			chain.EffectCount = 1;
			chain.pEffectDescriptors = (nint) (&descriptor);

			var result = FAudio.FAudioVoice_SetEffectChain(
				WetVoice.Handle,
				ref chain
			);

			if (result != 0)
			{
				Logger.LogWarn("Failed to set reverb effect chain!");
			}

			if (SetParams(DefaultParams))
			{
				WetVoiceInitialized = true;
			}

			FAudio.FAPOBase_Release(reverb);
		}

		public ReverbEffect(AudioDevice audioDevice, uint processingStage) : base(audioDevice, 1, audioDevice.DeviceDetails.OutputFormat.Format.nSamplesPerSec, processingStage)
		{
			Init();
		}

		public ReverbEffect(AudioDevice audioDevice, SubmixVoice outputVoice, uint processingStage) : base(audioDevice, outputVoice, 1, audioDevice.DeviceDetails.OutputFormat.Format.nSamplesPerSec, processingStage)
		{
			Init();
		}

		public ReverbEffect(AudioDevice audioDevice, SubmixVoice outputVoice, uint sampleRate, uint processingStage) : base(audioDevice, outputVoice, 1, sampleRate, processingStage)
		{
			Init();
		}

		public override void SetOutputVoice(SubmixVoice send)
		{
			base.SetOutputVoice(send);

			if (WetVoiceInitialized)
			{
				WetVoice.SetOutputVoice(send);
			}
		}

		public bool SetParams(in FAudio.FAudioFXReverbParameters reverbParams)
		{
			Params = reverbParams;

			fixed (FAudio.FAudioFXReverbParameters* reverbParamsPtr = &reverbParams)
			{
				var result = FAudio.FAudioVoice_SetEffectParameters(
					WetVoice.Handle,
					0,
					(nint) reverbParamsPtr,
					(uint) Marshal.SizeOf<FAudio.FAudioFXReverbParameters>(),
					0
				);

				if (result != 0)
				{
					Logger.LogWarn("Failed to set reverb effect parameters!");
				}

				return result == 0;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			WetVoice.Dispose();
		}
	}
}
