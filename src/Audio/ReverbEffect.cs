using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	// sound instances can send their audio to this voice to add reverb
	public unsafe class ReverbEffect : AudioResource
	{
		private IntPtr voice;
		public IntPtr Voice => voice;

		public ReverbEffect(AudioDevice audioDevice) : base(audioDevice)
		{
			/* Init reverb */

			IntPtr reverb;
			FAudio.FAudioCreateReverb(out reverb, 0);

			IntPtr chainPtr;
			chainPtr = (nint) NativeMemory.Alloc(
				(nuint) Marshal.SizeOf<FAudio.FAudioEffectChain>()
			);

			FAudio.FAudioEffectChain* reverbChain = (FAudio.FAudioEffectChain*) chainPtr;
			reverbChain->EffectCount = 1;
			reverbChain->pEffectDescriptors = (nint) NativeMemory.Alloc(
				(nuint) Marshal.SizeOf<FAudio.FAudioEffectDescriptor>()
			);

			FAudio.FAudioEffectDescriptor* reverbDescriptor =
				(FAudio.FAudioEffectDescriptor*) reverbChain->pEffectDescriptors;

			reverbDescriptor->InitialState = 1;
			reverbDescriptor->OutputChannels = (uint) (
				(audioDevice.DeviceDetails.OutputFormat.Format.nChannels == 6) ? 6 : 1
			);
			reverbDescriptor->pEffect = reverb;

			FAudio.FAudio_CreateSubmixVoice(
				audioDevice.Handle,
				out voice,
				1, /* omnidirectional reverb */
				audioDevice.DeviceDetails.OutputFormat.Format.nSamplesPerSec,
				0,
				0,
				IntPtr.Zero,
				chainPtr
			);
			FAudio.FAPOBase_Release(reverb);

			NativeMemory.Free((void*) reverbChain->pEffectDescriptors);
			NativeMemory.Free((void*) chainPtr);

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
					voice,
					0,
					(nint) reverbParamsPtr,
					(uint) Marshal.SizeOf<FAudio.FAudioFXReverbParameters>(),
					0
				);
			}
		}

		protected override void Destroy()
		{
			FAudio.FAudioVoice_DestroyVoice(Voice);
		}
	}
}
