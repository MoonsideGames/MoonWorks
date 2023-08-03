namespace MoonWorks.Audio
{
	public enum FormatTag : ushort
	{
		Unknown = 0,
		PCM = 1,
		MSADPCM = 2,
		IEEE_FLOAT = 3
	}

	public record struct Format
	{
		public FormatTag Tag;
		public ushort Channels;
		public uint SampleRate;
		public ushort BitsPerSample;

		internal FAudio.FAudioWaveFormatEx ToFAudioFormat()
		{
			var blockAlign = (ushort) ((BitsPerSample / 8) * Channels);

			return new FAudio.FAudioWaveFormatEx
			{
				wFormatTag = (ushort) Tag,
				nChannels = Channels,
				nSamplesPerSec = SampleRate,
				wBitsPerSample = BitsPerSample,
				nBlockAlign = blockAlign,
				nAvgBytesPerSec = blockAlign * SampleRate
			};
		}
	}
}
