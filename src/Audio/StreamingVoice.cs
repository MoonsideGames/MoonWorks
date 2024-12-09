using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Use in conjunction with an AudioDataStreamable object to play back streaming audio data.
	/// </summary>
	public class StreamingVoice : UpdatingSourceVoice, IPoolable<StreamingVoice>
	{
		private const int BUFFER_COUNT = 3;
		private readonly IntPtr[] buffers;
		private int nextBufferIndex = 0;
		private uint BufferSize;

		public bool Loop { get; set; }

		public AudioDataStreamable AudioData { get; protected set; }

		public unsafe StreamingVoice(AudioDevice device, Format format) : base(device, format)
		{
			buffers = new IntPtr[BUFFER_COUNT];
		}

		public static StreamingVoice Create(AudioDevice device, Format format)
		{
			return new StreamingVoice(device, format);
		}

		/// <summary>
		/// Loads and prepares an AudioDataStreamable for streaming playback.
		/// The streamable data must already be loaded.
		/// </summary>
		public void Load(AudioDataStreamable data)
		{
			lock (StateLock)
			{
				if (AudioData != null)
				{
					Unload();
				}

				if (!data.Loaded)
				{
					Logger.LogError("Streamable data not loaded!");
					return;
				}

				AudioData = data;
				InitializeBuffers();
				QueueBuffers();
			}
		}

		/// <summary>
		/// Unloads AudioDataStreamable from this voice.
		/// </summary>
		public void Unload()
		{
			lock (StateLock)
			{
				if (AudioData != null)
				{
					Stop();
					AudioData = null;
				}
			}
		}

		public override void Reset()
		{
			Unload();
			base.Reset();
		}

		public override void Update()
		{
			lock (StateLock)
			{
				if (AudioData == null || State != SoundState.Playing)
				{
					return;
				}

				QueueBuffers();
			}
		}

		private void QueueBuffers()
		{
			int buffersNeeded = BUFFER_COUNT - (int) BuffersQueued; // don't get got by uint underflow!
			for (int i = 0; i < buffersNeeded; i += 1)
			{
				AddBuffer();
			}
		}

		private unsafe void AddBuffer()
		{
			var buffer = buffers[nextBufferIndex];
			nextBufferIndex = (nextBufferIndex + 1) % BUFFER_COUNT;

			AudioData.Decode(
				(void*) buffer,
				(int) BufferSize,
				out int filledLengthInBytes,
				out bool reachedEnd
			);

			if (filledLengthInBytes > 0)
			{
				var buf = new FAudio.FAudioBuffer
				{
					AudioBytes = (uint) filledLengthInBytes,
					pAudioData = buffer,
					PlayLength = (
						(uint) (filledLengthInBytes /
						Format.Channels /
						(uint) (Format.BitsPerSample / 8))
					)
				};

				Submit(buf);
			}

			if (reachedEnd)
			{
				/* We have reached the end of the data, what do we do? */
				if (Loop)
				{
					AudioData.Seek(0);
				}
			}
		}

		private unsafe void InitializeBuffers()
		{
			BufferSize = AudioData.DecodeBufferSize;

			for (int i = 0; i < BUFFER_COUNT; i += 1)
			{
				if (buffers[i] != IntPtr.Zero)
				{
					NativeMemory.Free((void*) buffers[i]);
				}

				buffers[i] = (IntPtr) NativeMemory.AllocZeroed(BufferSize);
			}
		}

		protected override unsafe void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				lock (StateLock)
				{
					Stop();

					for (int i = 0; i < BUFFER_COUNT; i += 1)
					{
						if (buffers[i] != IntPtr.Zero)
						{
							NativeMemory.Free((void*) buffers[i]);
						}
					}
				}
			}
			base.Dispose(disposing);
		}
	}
}
