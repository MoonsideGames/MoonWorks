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
		public SubmixVoice(
			AudioDevice device,
			uint sourceChannelCount,
			uint sampleRate,
			uint processingStage
		) : base(device, sourceChannelCount, device.DeviceDetails.OutputFormat.Format.nChannels)
		{
			FAudio.FAudio_CreateSubmixVoice(
				device.Handle,
				out handle,
				sourceChannelCount,
				sampleRate,
				FAudio.FAUDIO_VOICE_USEFILTER,
				processingStage,
				IntPtr.Zero, // default sends to mastering voice
				IntPtr.Zero
			);
		}
	}
}
