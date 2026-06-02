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
		public uint SampleRate { get; }
		public uint ProcessingStage { get; }

		public unsafe SubmixVoice(
			AudioDevice device,
			uint sourceChannelCount,
			uint sampleRate,
			uint processingStage
		) : base(device, sourceChannelCount, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			SampleRate = sampleRate;
			ProcessingStage = processingStage;

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
			SampleRate = sampleRate;
			ProcessingStage = processingStage;

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

			SetOutputVoice(outputVoice);
		}

		private SubmixVoice(
			AudioDevice device
		) : base(device, device.DeviceDetails.OutputFormat.Format.nChannels, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			SampleRate = device.DeviceDetails.OutputFormat.Format.nSamplesPerSec;
			ProcessingStage = uint.MaxValue;

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
