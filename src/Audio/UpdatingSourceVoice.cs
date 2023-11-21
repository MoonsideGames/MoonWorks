namespace MoonWorks.Audio
{
	public abstract class UpdatingSourceVoice : SourceVoice
	{
		protected UpdatingSourceVoice(AudioDevice device, Format format) : base(device, format)
		{
		}

		public abstract void Update();
	}
}
