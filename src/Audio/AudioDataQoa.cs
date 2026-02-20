using System;
using System.Runtime.InteropServices;
using MoonWorks.Storage;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Streamable audio in QOA format.
	/// </summary>
	public class AudioDataQoa : AudioDataStreamable
	{
		private IntPtr QoaHandle = IntPtr.Zero;
		private IntPtr BufferDataPtr = IntPtr.Zero;
		private uint BufferDataLength = 0;

		private const uint AUDIO_BUFFER_SIZE = 32768;
		private IntPtr[] AudioBuffers = new IntPtr[BUFFER_COUNT];
		private int NextBufferIndex = 0;
		private uint DecodeBufferSize = 0;

		public Format Format { get; private set; }

		public bool Loaded => QoaHandle != IntPtr.Zero;

		public bool Loop { get; set; }

		public static AudioDataQoa Create(AudioDevice device)
		{
			return new AudioDataQoa(device);
		}

		private AudioDataQoa(AudioDevice device) : base(device) { }

		/// <summary>
		/// Prepares qoa data for streaming.
		/// </summary>
		public override unsafe void Open(ReadOnlySpan<byte> data)
		{
			if (Loaded)
			{
				Close();
			}

			BufferDataPtr = (nint) NativeMemory.Alloc((nuint) data.Length);
			BufferDataLength = (uint) data.Length;

			fixed (void *ptr = data)
			{
				NativeMemory.Copy(ptr, (void*) BufferDataPtr, BufferDataLength);
			}

			QoaHandle = FAudio.qoa_open_from_memory(BufferDataPtr, BufferDataLength, 0);
			if (QoaHandle == IntPtr.Zero)
			{
				NativeMemory.Free((void*) BufferDataPtr);
				BufferDataPtr = IntPtr.Zero;
				BufferDataLength = 0;
				throw new InvalidOperationException("Error opening QOA file!");
			}

			var format = new Format
			{
				Tag = FormatTag.PCM,
				BitsPerSample = 16
			};
			FAudio.qoa_attributes(
				QoaHandle,
				out var channels,
				out format.SampleRate,
				out var samplesPerChannelPerFrame,
				out var total_samples_per_channel
			);
			format.Channels = (ushort) channels;
			Format = format;

			DecodeBufferSize = channels * samplesPerChannelPerFrame * sizeof(short);

			for (int i = 0; i < BUFFER_COUNT; i += 1)
			{
				AudioBuffers[i] = (nint) NativeMemory.Alloc(DecodeBufferSize);
			}

			OutOfData = false;
		}

		public override void Seek(uint sampleFrame, bool flush = false)
		{
			FAudio.qoa_seek_frame(QoaHandle, (int) sampleFrame);
			OutOfData = false;

			if (flush)
			{
				SendVoice?.Flush();
				QueueBuffers();
			}
		}

		protected override unsafe FAudio.FAudioBuffer OnBufferNeeded()
		{
			if (!Loaded)
			{
				OutOfData = true;
				return new FAudio.FAudioBuffer();
			}

			var buffer = AudioBuffers[NextBufferIndex];
			NextBufferIndex = (NextBufferIndex + 1) % BUFFER_COUNT;

			var lengthInShorts = DecodeBufferSize / sizeof(short);

			// NOTE: this function returns samples per channel!
			var samples = FAudio.qoa_decode_next_frame(QoaHandle, (short*) buffer);

			var sampleCount = samples * Format.Channels;
			var filledLengthInBytes = sampleCount * sizeof(short);

			if (sampleCount < lengthInShorts)
			{
				if (Loop)
				{
					Seek(0);
				}
				else
				{
					OutOfData = true;
				}
			}

			if (filledLengthInBytes > 0)
			{
				return new FAudio.FAudioBuffer
				{
					AudioBytes = filledLengthInBytes,
					pAudioData = buffer,
					PlayLength = (
						filledLengthInBytes /
						Format.Channels /
						(uint) (Format.BitsPerSample / 8)
					)
				};
			}

			return new FAudio.FAudioBuffer(); // no data, return zeroed struct
		}

		/// <summary>
		/// Unloads the QOA data, freeing resources.
		/// This will automatically disconnect from the source voice.
		/// </summary>
		public override unsafe void Close()
		{
			if (Loaded)
			{
				if (SendVoice != null)
				{
					Disconnect();
				}

				FAudio.qoa_close(QoaHandle);
				NativeMemory.Free((void*) BufferDataPtr);

				QoaHandle = IntPtr.Zero;
				BufferDataPtr = IntPtr.Zero;
				BufferDataLength = 0;

				for (int i = 0; i < BUFFER_COUNT; i += 1)
				{
					NativeMemory.Free((void*) AudioBuffers[i]);
					AudioBuffers[i] = IntPtr.Zero;
				}
			}
		}

		/// <summary>
		/// Get audio format data without decoding the entire file.
		/// </summary>
		public static unsafe Format GetFormat(TitleStorage storage, string filePath)
		{
			if (!storage.GetFileSize(filePath, out var size))
			{
				return new Format();
			}

			var buffer = NativeMemory.Alloc((nuint) size);
			var span = new Span<byte>(buffer, (int) size);
			if (!storage.ReadFile(filePath, span))
			{
				return new Format();
			}

			var handle = FAudio.qoa_open_from_memory((nint) buffer, (uint) size, 0);
			if (handle == IntPtr.Zero)
			{
				Logger.LogError("Error loading QOA file!");
				return new Format();
			}

			FAudio.qoa_attributes(handle, out var channels, out var samplerate, out var _, out var _);
			var format = new Format
			{
				Tag = FormatTag.PCM,
				BitsPerSample = 16,
				Channels = (ushort) channels,
				SampleRate = samplerate
			};

			FAudio.qoa_close(handle);
			NativeMemory.Free(buffer);
			return format;
		}

		private ref struct LoadResult
		{
			public Format Format;
			public IntPtr DataPtr;
			public uint Length;

			public LoadResult(Format format, IntPtr dataPtr, uint length)
			{
				Format = format;
				DataPtr = dataPtr;
				Length = length;
			}
		}

		private unsafe static LoadResult Load(ReadOnlySpan<byte> data)
		{
			IntPtr buffer;
			uint lengthInBytes;

			var format = new Format
			{
				Tag = FormatTag.PCM,
				BitsPerSample = 16
			};

			fixed (void* ptr = data)
			{
				var qoaHandle = FAudio.qoa_open_from_memory((nint) ptr, (uint) data.Length, 0);
				if (qoaHandle == IntPtr.Zero)
				{
					throw new InvalidOperationException("Error opening QOA file!");
				}

				FAudio.qoa_attributes(qoaHandle, out var channels, out var samplerate, out var samples_per_channel_per_frame, out var total_samples_per_channel);

				lengthInBytes = total_samples_per_channel * channels * sizeof(short);
				buffer = (nint) NativeMemory.Alloc(lengthInBytes);
				FAudio.qoa_decode_entire(qoaHandle, (short*) buffer);
				FAudio.qoa_close(qoaHandle);

				format.Channels = (ushort) channels;
				format.SampleRate = samplerate;
			}

			return new LoadResult(format, buffer, lengthInBytes);
		}

		/// <summary>
		/// Decodes an entire QOA data buffer into an AudioBuffer.
		/// </summary>
		public static void SetData(AudioBuffer audioBuffer, ReadOnlySpan<byte> data)
		{
			var result = Load(data);
			audioBuffer.SetDataPointer(result.DataPtr, result.Length, true);
		}

		/// <summary>
		/// Decodes an entire QOA file into an AudioBuffer.
		/// </summary>
		public unsafe static AudioBuffer CreateBuffer(AudioDevice device, TitleStorage storage, string path)
		{
			if (!storage.GetFileSize(path, out var size))
			{
				return null;
			}

			var buffer = NativeMemory.Alloc((nuint) size);
			var span = new Span<byte>(buffer, (int) size);

			if (!storage.ReadFile(path, span))
			{
				return null;
			}

			var result = Load(span);

			var audioBuffer = AudioBuffer.Create(device, result.Format);
			audioBuffer.SetDataPointer(result.DataPtr, result.Length, true);
			NativeMemory.Free(buffer);

			return audioBuffer;
		}
	}
}
