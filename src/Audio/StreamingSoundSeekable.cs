namespace MoonWorks.Audio
{
	public abstract class StreamingSoundSeekable : StreamingSound
	{
		public bool Loop { get; set; }

		protected StreamingSoundSeekable(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond,
			uint bufferSize
		) : base(
			device,
			formatTag,
			bitsPerSample,
			blockAlign,
			channels,
			samplesPerSecond,
			bufferSize
		) {

		}

		public abstract void Seek(uint sampleFrame);

		protected override void OnReachedEnd()
		{
			if (Loop)
			{
				ConsumingBuffers = true;
				Seek(0);
			}
		}
	}
}
