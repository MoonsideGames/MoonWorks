using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// For streaming long playback.
	/// Must be extended with a decoder routine called by FillBuffer.
	/// See StreamingSoundOgg for an example.
	/// </summary>
	public abstract class StreamingSound : SoundInstance
	{
		private const int BUFFER_COUNT = 3;
		private readonly IntPtr[] buffers;
		private int nextBufferIndex = 0;
		private uint queuedBufferCount = 0;
		protected abstract int BUFFER_SIZE { get; }

		public unsafe StreamingSound(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond
		) : base(device, formatTag, bitsPerSample, blockAlign, channels, samplesPerSecond)
		{
			device.AddDynamicSoundInstance(this);

			buffers = new IntPtr[BUFFER_COUNT];
			for (int i = 0; i < BUFFER_COUNT; i += 1)
			{
				buffers[i] = (IntPtr) NativeMemory.Alloc((nuint) BUFFER_SIZE);
			}
		}

		public override void Play()
		{
			if (State == SoundState.Playing)
			{
				return;
			}

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

		public override void Stop()
		{
			State = SoundState.Stopped;
		}

		public override void StopImmediate()
		{
			FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
			FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);
			ClearBuffers();

			State = SoundState.Stopped;
		}

		internal unsafe void Update()
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

			queuedBufferCount = state.BuffersQueued;

			QueueBuffers();
		}

		protected void QueueBuffers()
		{
			for (int i = 0; i < BUFFER_COUNT - queuedBufferCount; i += 1)
			{
				AddBuffer();
			}
		}

		protected unsafe void ClearBuffers()
		{
			nextBufferIndex = 0;
			queuedBufferCount = 0;
		}

		protected unsafe void AddBuffer()
		{
			var buffer = buffers[nextBufferIndex];
			nextBufferIndex = (nextBufferIndex + 1) % BUFFER_COUNT;

			FillBuffer(
				(void*) buffer,
				BUFFER_SIZE,
				out int filledLengthInBytes,
				out bool reachedEnd
			);

			FAudio.FAudioBuffer buf = new FAudio.FAudioBuffer
			{
				AudioBytes = (uint) filledLengthInBytes,
				pAudioData = (IntPtr) buffer,
				PlayLength = (
					(uint) (filledLengthInBytes /
					Format.nChannels /
					(uint) (Format.wBitsPerSample / 8))
				)
			};

			FAudio.FAudioSourceVoice_SubmitSourceBuffer(
				Handle,
				ref buf,
				IntPtr.Zero
			);

			queuedBufferCount += 1;

			/* We have reached the end of the file, what do we do? */
			if (reachedEnd)
			{
				OnReachedEnd();
			}
		}

		protected virtual void OnReachedEnd()
		{
			Stop();
		}

		protected unsafe abstract void FillBuffer(
			void* buffer,
			int bufferLengthInBytes, /* in bytes */
			out int filledLengthInBytes, /* in bytes */
			out bool reachedEnd
		);

		protected unsafe override void Destroy()
		{
			StopImmediate();

			for (int i = 0; i < BUFFER_COUNT; i += 1)
			{
				NativeMemory.Free((void*) buffers[i]);
			}
		}
	}
}
