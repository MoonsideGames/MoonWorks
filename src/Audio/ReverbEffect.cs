using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Use this in conjunction with SourceVoice.SetReverbEffectChain to add reverb to a voice.
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

		public ReverbEffect(AudioDevice audioDevice, uint processingStage) : base(audioDevice, 1, audioDevice.DeviceDetails.OutputFormat.Format.nSamplesPerSec, processingStage)
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

			FAudio.FAudioVoice_SetEffectChain(
				Handle,
				ref chain
			);

			FAudio.FAPOBase_Release(reverb);

			SetParams(DefaultParams);
		}

		public void SetParams(in FAudio.FAudioFXReverbParameters reverbParams)
		{
			fixed (FAudio.FAudioFXReverbParameters* reverbParamsPtr = &reverbParams)
			{
				FAudio.FAudioVoice_SetEffectParameters(
					Handle,
					0,
					(nint) reverbParamsPtr,
					(uint) Marshal.SizeOf<FAudio.FAudioFXReverbParameters>(),
					0
				);
			}
		}
	}
}
