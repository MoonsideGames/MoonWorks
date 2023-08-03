using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Use this in conjunction with SourceVoice.SetReverbEffectChain to add reverb to a voice.
	/// </summary>
	public unsafe class ReverbEffect : SubmixVoice
	{
		public ReverbEffect(AudioDevice audioDevice, uint processingStage) : base(audioDevice, 1, audioDevice.DeviceDetails.OutputFormat.Format.nSamplesPerSec, processingStage)
		{
			/* Init reverb */
			IntPtr reverb;
			FAudio.FAudioCreateReverb(out reverb, 0);

			var chain = new FAudio.FAudioEffectChain();
			var descriptor = new FAudio.FAudioEffectDescriptor();

			descriptor.InitialState = 1;
			descriptor.OutputChannels = 1;
			descriptor.pEffect = reverb;

			chain.EffectCount = 1;
			chain.pEffectDescriptors = (nint) (&descriptor);

			FAudio.FAudioVoice_SetEffectChain(
				Handle,
				ref chain
			);

			FAudio.FAPOBase_Release(reverb);

			/* Init reverb params */
			// Defaults based on FAUDIOFX_I3DL2_PRESET_GENERIC

			FAudio.FAudioFXReverbParameters reverbParams;
			reverbParams.WetDryMix = 100.0f;
			reverbParams.ReflectionsDelay = 7;
			reverbParams.ReverbDelay = 11;
			reverbParams.RearDelay = FAudio.FAUDIOFX_REVERB_DEFAULT_REAR_DELAY;
			reverbParams.PositionLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
			reverbParams.PositionRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION;
			reverbParams.PositionMatrixLeft = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
			reverbParams.PositionMatrixRight = FAudio.FAUDIOFX_REVERB_DEFAULT_POSITION_MATRIX;
			reverbParams.EarlyDiffusion = 15;
			reverbParams.LateDiffusion = 15;
			reverbParams.LowEQGain = 8;
			reverbParams.LowEQCutoff = 4;
			reverbParams.HighEQGain = 8;
			reverbParams.HighEQCutoff = 6;
			reverbParams.RoomFilterFreq = 5000f;
			reverbParams.RoomFilterMain = -10f;
			reverbParams.RoomFilterHF = -1f;
			reverbParams.ReflectionsGain = -26.0200005f;
			reverbParams.ReverbGain = 10.0f;
			reverbParams.DecayTime = 1.49000001f;
			reverbParams.Density = 100.0f;
			reverbParams.RoomSize = FAudio.FAUDIOFX_REVERB_DEFAULT_ROOM_SIZE;

			SetParams(reverbParams);
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
