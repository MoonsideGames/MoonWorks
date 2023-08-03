namespace MoonWorks.Audio
{
	/// <summary>
	/// PersistentVoice should be used when you need to maintain a long-term reference to a source voice.
	/// </summary>
	public class PersistentVoice : SourceVoice, IPoolable<PersistentVoice>
	{
		public PersistentVoice(AudioDevice device, Format format) : base(device, format)
		{
		}

		public static PersistentVoice Create(AudioDevice device, Format format)
		{
			return new PersistentVoice(device, format);
		}

		/// <summary>
		/// Adds an AudioBuffer to the voice queue.
		/// The voice processes and plays back the buffers in its queue in the order that they were submitted.
		/// </summary>
		/// <param name="buffer">The buffer to submit to the voice.</param>
		/// <param name="loop">Whether the voice should loop this buffer.</param>
		public void Submit(AudioBuffer buffer, bool loop = false)
		{
			Submit(buffer.ToFAudioBuffer(loop));
		}
	}
}
