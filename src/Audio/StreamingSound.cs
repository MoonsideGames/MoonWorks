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
		// Are we actively consuming buffers?
		protected bool ConsumingBuffers = false;

		private const int BUFFER_COUNT = 3;
		private nuint BufferSize;
		private readonly IntPtr[] buffers;
		private int nextBufferIndex = 0;
		private uint queuedBufferCount = 0;

		private readonly object StateLock = new object();

		public bool AutoUpdate { get; }

		public abstract bool Loaded { get; }

		public unsafe StreamingSound(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond,
			uint bufferSize,
			bool autoUpdate // should the AudioDevice thread automatically update this sound?
		) : base(device, formatTag, bitsPerSample, blockAlign, channels, samplesPerSecond)
		{
			BufferSize = bufferSize;

			buffers = new IntPtr[BUFFER_COUNT];
			for (int i = 0; i < BUFFER_COUNT; i += 1)
			{
				buffers[i] = (IntPtr) NativeMemory.Alloc(bufferSize);
			}

			AutoUpdate = autoUpdate;
		}

		public override void Play()
		{
			PlayUsingOperationSet(0);
		}

		public override void QueueSyncPlay()
		{
			PlayUsingOperationSet(1);
		}

		private void PlayUsingOperationSet(uint operationSet)
		{
			lock (StateLock)
			{
				if (!Loaded)
				{
					Logger.LogError("Cannot play StreamingSound before calling Load!");
					return;
				}

				if (State == SoundState.Playing)
				{
					return;
				}

				State = SoundState.Playing;

				ConsumingBuffers = true;
				if (AutoUpdate)
				{
					Device.AddAutoUpdateStreamingSoundInstance(this);
				}

				QueueBuffers();
				FAudio.FAudioSourceVoice_Start(Voice, 0, operationSet);
			}
		}

		public override void Pause()
		{
			lock (StateLock)
			{
				if (State == SoundState.Playing)
				{
					ConsumingBuffers = false;
					FAudio.FAudioSourceVoice_Stop(Voice, 0, 0);
					State = SoundState.Paused;
				}
			}
		}

		public override void Stop()
		{
			lock (StateLock)
			{
				ConsumingBuffers = false;
				State = SoundState.Stopped;
			}
		}

		public override void StopImmediate()
		{
			lock (StateLock)
			{
				ConsumingBuffers = false;
				FAudio.FAudioSourceVoice_Stop(Voice, 0, 0);
				FAudio.FAudioSourceVoice_FlushSourceBuffers(Voice);
				ClearBuffers();

				State = SoundState.Stopped;
			}
		}

		internal unsafe void Update()
		{
			lock (StateLock)
			{
				if (!IsDisposed)
				{
					if (State != SoundState.Playing)
					{
						return;
					}

					QueueBuffers();
				}
			}
		}

		protected void QueueBuffers()
		{
			FAudio.FAudioSourceVoice_GetState(
				Voice,
				out var state,
				FAudio.FAUDIO_VOICE_NOSAMPLESPLAYED
			);

			queuedBufferCount = state.BuffersQueued;

			if (ConsumingBuffers)
			{
				for (int i = 0; i < BUFFER_COUNT - queuedBufferCount; i += 1)
				{
					AddBuffer();
				}
			}
			else if (queuedBufferCount == 0)
			{
				Stop();
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
				(int) BufferSize,
				out int filledLengthInBytes,
				out bool reachedEnd
			);

			if (filledLengthInBytes > 0)
			{
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
					Voice,
					ref buf,
					IntPtr.Zero
				);

				queuedBufferCount += 1;
			}

			if (reachedEnd)
			{
				/* We have reached the end of the data, what do we do? */
				ConsumingBuffers = false;
				OnReachedEnd();
			}
		}

		public abstract void Load();
		public abstract void Unload();

		protected unsafe abstract void FillBuffer(
			void* buffer,
			int bufferLengthInBytes, /* in bytes */
			out int filledLengthInBytes, /* in bytes */
			out bool reachedEnd
		);

		protected abstract void OnReachedEnd();

		protected unsafe override void Destroy()
		{
			lock (StateLock)
			{
				if (!IsDisposed)
				{
					StopImmediate();
					Unload();

					for (int i = 0; i < BUFFER_COUNT; i += 1)
					{
						NativeMemory.Free((void*) buffers[i]);
					}
				}
			}
		}
	}
}
