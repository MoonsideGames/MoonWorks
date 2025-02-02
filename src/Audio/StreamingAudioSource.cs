using System;

namespace MoonWorks.Audio;

/// <summary>
/// Use this in conjunction with a SourceVoice to play streaming audio data.
/// </summary>
public abstract class StreamingAudioSource : AudioResource
{
	protected const int BUFFER_COUNT = 3;
	protected SourceVoice SendVoice;

	/// <summary>
	/// Indicates that all available audio data from the source has been consumed.
	/// </summary>
	public bool OutOfData { get; protected set; }

	protected unsafe StreamingAudioSource(AudioDevice device) : base(device)
	{
	}

	/// <summary>
	/// Sets the source voice to stream the audio through.
	/// </summary>
	public void SendTo(SourceVoice sourceVoice)
	{
		SendVoice = sourceVoice;
		QueueBuffers();
	}

	/// <summary>
	/// Disconnect from the source voice.
	/// </summary>
	public void Disconnect()
	{
		SendVoice.Stop();
		SendVoice = null;
	}

	/// <summary>
	/// Called on the audio thread when the voice needs another buffer.
	/// The subclass must then provide an audio buffer.
	/// This method should set OutOfData when there is no data left.
	/// If there is no data to submit, this should return a zeroed struct.
	/// </summary>
	protected abstract FAudio.FAudioBuffer OnBufferNeeded();

	internal void Update()
	{
		if (
			SendVoice != null &&
			!OutOfData &&
			SendVoice.State == SoundState.Playing &&
			SendVoice.BuffersQueued < BUFFER_COUNT
		) {
			QueueBuffers();
		}
	}

	protected void QueueBuffers()
	{
		if (SendVoice != null)
		{
			int buffersNeeded = BUFFER_COUNT - (int) SendVoice.BuffersQueued; // don't get got by uint underflow!
			for (int i = 0; i < buffersNeeded; i += 1)
			{
				AddBuffer();
			}
		}
	}

	private void AddBuffer()
	{
		var buffer = OnBufferNeeded();
		if (buffer.pAudioData != IntPtr.Zero)
		{
			SendVoice.Submit(buffer);
		}
	}

	protected override unsafe void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			if (disposing)
			{
				SendVoice?.Stop();
			}
		}

		base.Dispose(disposing);
	}
}
