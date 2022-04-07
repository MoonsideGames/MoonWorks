using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// For streaming long playback.
	/// Can be extended to support custom decoders.
	/// </summary>
	public abstract class StreamingSound : SoundInstance
	{
		private readonly List<IntPtr> queuedBuffers = new List<IntPtr>();
		private readonly List<uint> queuedSizes = new List<uint>();
		private const int MINIMUM_BUFFER_CHECK = 3;

		public int PendingBufferCount => queuedBuffers.Count;

		public StreamingSound(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond
		) : base(device, formatTag, bitsPerSample, blockAlign, channels, samplesPerSecond) { }

		public override void Play(bool loop = false)
		{
			if (State == SoundState.Playing)
			{
				return;
			}

			Loop = loop;
			State = SoundState.Playing;

			Update();
			FAudio.FAudioSourceVoice_Start(Handle, 0, 0);
		}

		public override void Pause()
		{
			if (State == SoundState.Playing)
			{
				FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
				State = SoundState.Paused;
			}
		}

		public override void Stop(bool immediate = true)
		{
			if (immediate)
			{
				FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
				FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);
				ClearBuffers();
			}

			State = SoundState.Stopped;
		}

		internal void Update()
		{
			if (State != SoundState.Playing)
			{
				return;
			}

			FAudio.FAudioSourceVoice_GetState(
				Handle,
				out var state,
				FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
			);

			while (PendingBufferCount > state.BuffersQueued)
				lock (queuedBuffers)
				{
					Marshal.FreeHGlobal(queuedBuffers[0]);
					queuedBuffers.RemoveAt(0);
				}

			QueueBuffers();
		}

		protected void QueueBuffers()
		{
			for (
				int i = MINIMUM_BUFFER_CHECK - PendingBufferCount;
				i > 0;
				i -= 1
			)
			{
				AddBuffer();
			}
		}

		protected void ClearBuffers()
		{
			lock (queuedBuffers)
			{
				foreach (IntPtr buf in queuedBuffers)
				{
					Marshal.FreeHGlobal(buf);
				}
				queuedBuffers.Clear();
				queuedSizes.Clear();
			}
		}

		protected void AddBuffer()
		{
			AddBuffer(
				out var buffer,
				out var bufferOffset,
				out var bufferLength,
				out var reachedEnd
			);

			var lengthInBytes = bufferLength * sizeof(float);

			IntPtr next = Marshal.AllocHGlobal((int) lengthInBytes);
			Marshal.Copy(buffer, (int) bufferOffset, next, (int) bufferLength);

			lock (queuedBuffers)
			{
				queuedBuffers.Add(next);
				if (State != SoundState.Stopped)
				{
					FAudio.FAudioBuffer buf = new FAudio.FAudioBuffer
					{
						AudioBytes = lengthInBytes,
						pAudioData = next,
						PlayLength = (
							lengthInBytes /
							Format.nChannels /
							(uint) (Format.wBitsPerSample / 8)
						)
					};

					FAudio.FAudioSourceVoice_SubmitSourceBuffer(
						Handle,
						ref buf,
						IntPtr.Zero
					);
				}
				else
				{
					queuedSizes.Add(lengthInBytes);
				}
			}

			/* We have reached the end of the file, what do we do? */
			if (reachedEnd)
			{
				if (Loop)
				{
					SeekStart();
				}
				else
				{
					Stop(false);
				}
			}
		}

		protected abstract void AddBuffer(
			out float[] buffer,
			out uint bufferOffset, /* in floats */
			out uint bufferLength, /* in floats */
			out bool reachedEnd
		);

		protected abstract void SeekStart();

		protected override void Destroy()
		{
			Stop(true);
		}
	}
}
