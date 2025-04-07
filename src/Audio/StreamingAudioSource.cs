using System;

namespace MoonWorks.Audio;

/// <summary>
/// Use this in conjunction with a SourceVoice to play streaming audio data.
/// </summary>
public abstract class StreamingAudioSource : AudioResource
{
	protected const int BUFFER_COUNT = 3;
	public SourceVoice SendVoice { get; protected set;}

	/// <summary>
	/// Indicates that all available audio data from the source has been consumed.
	/// </summary>
	public bool OutOfData { get; protected set; }

	protected unsafe StreamingAudioSource(AudioDevice device) : base(device)
	{
	}

	/// <summary>
	/// Sets the source voice to stream the audio through.
	/// This will also enqueue some buffers to avoid stuttering when Play is called.
	/// Seek before calling this if you are not starting from the beginnign of the audio source!
	/// </summary>
	public void SendTo(SourceVoice sourceVoice)
	{
		if (SendVoice != null)
		{
			Disconnect();
		}

		QueueBuffers();
		Device.RegisterStreamingAudioSource(this);
		SendVoice = sourceVoice;
	}

	/// <summary>
	/// Disconnect from the source voice.
	/// This will stop voice playback.
	/// </summary>
	public void Disconnect()
	{
		Device.UnregisterStreamingAudioSource(this);
		SendVoice?.Stop();
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
			SendVoice.BuffersQueued < BUFFER_COUNT
		) {
			QueueBuffers();
		}
	}

	// This is called by the AudioDevice thread after the source is registered.
	// This is NOT thread-safe!
	// If you call this while the thread is running bad things will happen!
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
				Disconnect();
			}
		}

		base.Dispose(disposing);
	}
}
