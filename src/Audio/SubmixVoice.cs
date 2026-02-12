using System;

namespace MoonWorks.Audio
{
	/// <summary>
	/// SourceVoices can send audio to a SubmixVoice for convenient effects processing.
	/// Submixes process in order of processingStage, from lowest to highest.
	/// Therefore submixes early in a chain should have a low processingStage, and later in the chain they should have a higher one.
	/// </summary>
	public class SubmixVoice : Voice
	{
		public unsafe SubmixVoice(
			AudioDevice device,
			uint sourceChannelCount,
			uint sampleRate,
			uint processingStage
		) : base(device, sourceChannelCount, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			FAudio.FAudioSendDescriptor* sendDesc = stackalloc FAudio.FAudioSendDescriptor[1];
			sendDesc[0].Flags = 0;
			sendDesc[0].pOutputVoice = device.MasteringVoice.Handle;

			var sends = new FAudio.FAudioVoiceSends
			{
				SendCount = 1,
				pSends = (nint) sendDesc
			};

			FAudio.FAudio_CreateSubmixVoice(
				device.Handle,
				out handle,
				sourceChannelCount,
				sampleRate,
				FAudio.FAUDIO_VOICE_USEFILTER,
				processingStage,
				(nint) (&sends),
				IntPtr.Zero
			);
		}

		public unsafe SubmixVoice(
			AudioDevice device,
			SubmixVoice outputVoice,
			uint sourceChannelCount,
			uint sampleRate,
			uint processingStage
		) : base(device, sourceChannelCount, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			FAudio.FAudioSendDescriptor* sendDesc = stackalloc FAudio.FAudioSendDescriptor[1];
			sendDesc[0].Flags = 0;
			sendDesc[0].pOutputVoice = outputVoice.Handle;

			var sends = new FAudio.FAudioVoiceSends
			{
				SendCount = 1,
				pSends = (nint) sendDesc
			};

			FAudio.FAudio_CreateSubmixVoice(
				device.Handle,
				out handle,
				sourceChannelCount,
				sampleRate,
				FAudio.FAUDIO_VOICE_USEFILTER,
				processingStage,
				(nint) (&sends),
				IntPtr.Zero
			);

			OutputVoice = outputVoice;
		}

		public unsafe SubmixVoice(
			AudioDevice device,
			SubmixVoice outputVoice,
			ReverbEffect reverbEffect,
			uint sourceChannelCount,
			uint sampleRate,
			uint processingStage
		) : base(device, sourceChannelCount, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			FAudio.FAudioSendDescriptor* sendDesc = stackalloc FAudio.FAudioSendDescriptor[2];
			sendDesc[0].Flags = 0;
			sendDesc[0].pOutputVoice = outputVoice.Handle;
			sendDesc[1].Flags = 0;
			sendDesc[1].pOutputVoice = reverbEffect.handle;

			var sends = new FAudio.FAudioVoiceSends
			{
				SendCount = 2,
				pSends = (nint) sendDesc
			};

			FAudio.FAudio_CreateSubmixVoice(
				device.Handle,
				out handle,
				sourceChannelCount,
				sampleRate,
				FAudio.FAUDIO_VOICE_USEFILTER,
				processingStage,
				(nint) (&sends),
				IntPtr.Zero
			);

			OutputVoice = outputVoice;
			ReverbEffect = reverbEffect;
		}

		private SubmixVoice(
			AudioDevice device
		) : base(device, device.DeviceDetails.OutputFormat.Format.nChannels, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			FAudio.FAudio_CreateSubmixVoice(
				device.Handle,
				out handle,
				device.DeviceDetails.OutputFormat.Format.nChannels,
				device.DeviceDetails.OutputFormat.Format.nSamplesPerSec,
				FAudio.FAUDIO_VOICE_USEFILTER,
				uint.MaxValue,
				IntPtr.Zero, // default sends to mastering voice
				IntPtr.Zero
			);

			OutputVoice = null;
		}

		internal static SubmixVoice CreateFauxMasteringVoice(AudioDevice device)
		{
			return new SubmixVoice(device);
		}
	}
}
