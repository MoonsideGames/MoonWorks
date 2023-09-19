namespace MoonWorks.Audio
{
	/// <summary>
	/// TransientVoice is intended for playing one-off sound effects that don't have a long term reference. <br/>
	/// It will be automatically returned to the AudioDevice SourceVoice pool once it is done playing back.
	/// </summary>
	public class TransientVoice : SourceVoice, IPoolable<TransientVoice>
	{
		static TransientVoice IPoolable<TransientVoice>.Create(AudioDevice device, Format format)
		{
			return new TransientVoice(device, format);
		}

		public TransientVoice(AudioDevice device, Format format) : base(device, format)
		{
		}

		public override void Update()
		{
			lock (StateLock)
			{
				if (PlaybackInitiated && BuffersQueued == 0)
				{
					Return();
				}
			}
		}
	}
}
