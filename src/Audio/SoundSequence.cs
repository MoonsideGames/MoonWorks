namespace MoonWorks.Audio
{
	/// <summary>
	/// Plays back a series of AudioBuffers in sequence. Set the OnSoundNeeded callback to add AudioBuffers dynamically.
	/// </summary>
	public class SoundSequence : SourceVoice, IPoolable<SoundSequence>
	{
		public int NeedSoundThreshold = 0;
		public delegate void OnSoundNeededFunc();
		public OnSoundNeededFunc OnSoundNeeded;

		public SoundSequence(AudioDevice device, Format format) : base(device, format)
		{

		}

		public SoundSequence(AudioDevice device, AudioBuffer templateSound) : base(device, templateSound.Format)
		{

		}

		public static SoundSequence Create(AudioDevice device, Format format)
		{
			return new SoundSequence(device, format);
		}

		public override void Update()
		{
			lock (StateLock)
			{
				if (State != SoundState.Playing) { return; }

				if (NeedSoundThreshold > 0)
				{
					var buffersNeeded = NeedSoundThreshold - (int) BuffersQueued;

					for (int i = 0; i < buffersNeeded; i += 1)
					{
						if (OnSoundNeeded != null)
						{
							OnSoundNeeded();
						}
					}
				}
			}
		}

		public void EnqueueSound(AudioBuffer buffer)
		{
#if DEBUG
			if (!(buffer.Format == Format))
			{
				Logger.LogWarn("Sound sequence audio format mismatch!");
				return;
			}
#endif

			lock (StateLock)
			{
				Submit(buffer.ToFAudioBuffer());
			}
		}
	}
}
