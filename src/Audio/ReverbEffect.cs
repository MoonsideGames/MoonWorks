using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Use this in conjunction with SourceVoice.SetReverbEffectChain to add reverb to a voice.
	/// Creating the effect chain can fail on certain configurations, if this happens then
	/// setting the reverb effect chain will become a no-op.
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

		/// <summary>
        /// Creating the reverb effect can fail, so we don't want weird dry output if that happens.
        /// </summary>
		internal bool Valid { get; private set; }

		private void Init()
		{
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
				Handle,
				ref chain
			);

			FAudio.FAPOBase_Release(reverb);

			Valid = false;

			if (result == 0)
			{
				// Success!
				if (SetParams(DefaultParams))
				{
					Valid = true;
				}
			}
			else
			{
				Logger.LogWarn("Failed to set reverb effect chain!");
			}
		}

		public ReverbEffect(AudioDevice audioDevice, uint processingStage) : base(audioDevice, 1, audioDevice.DeviceDetails.OutputFormat.Format.nSamplesPerSec, processingStage)
		{
			Init();
		}

		public ReverbEffect(AudioDevice audioDevice, SubmixVoice outputVoice, uint processingStage) : base(audioDevice, outputVoice, 1, audioDevice.DeviceDetails.OutputFormat.Format.nSamplesPerSec, processingStage)
		{
			Init();
		}

		public bool SetParams(in FAudio.FAudioFXReverbParameters reverbParams)
		{
			Params = reverbParams;

			fixed (FAudio.FAudioFXReverbParameters* reverbParamsPtr = &reverbParams)
			{
				var result = FAudio.FAudioVoice_SetEffectParameters(
					Handle,
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
	}
}
