using System;
using System.Runtime.InteropServices;
using MoonWorks.Storage;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Streamable audio in Ogg format.
	/// </summary>
	public class AudioDataOgg : AudioDataStreamable
	{
		private IntPtr VorbisHandle = IntPtr.Zero;
		private IntPtr BufferDataPtr = IntPtr.Zero;

		private const uint AUDIO_BUFFER_SIZE = 32768;
		private IntPtr[] AudioBuffers = new IntPtr[BUFFER_COUNT];
		private int NextBufferIndex = 0;

		public Format Format { get; private set; }

		public bool Loaded => VorbisHandle != IntPtr.Zero;

		public bool Loop { get; set; }

		public static AudioDataOgg Create(AudioDevice device)
		{
			return new AudioDataOgg(device);
		}

		private AudioDataOgg(AudioDevice device) : base(device) { }

		/// <summary>
		/// Prepares the Ogg data for streaming.
		/// </summary>
		public override unsafe void Open(ReadOnlySpan<byte> data)
		{
			if (Loaded)
			{
				Close();
			}

			BufferDataPtr = (nint) NativeMemory.Alloc((nuint) data.Length);

			fixed (void *ptr = data)
			{
				NativeMemory.Copy(ptr, (void*) BufferDataPtr, (nuint) data.Length);
			}

			VorbisHandle = FAudio.stb_vorbis_open_memory(BufferDataPtr, data.Length, out int error, IntPtr.Zero);
			if (error != 0)
			{
				NativeMemory.Free((void*) BufferDataPtr);
				BufferDataPtr = IntPtr.Zero;
				throw new InvalidOperationException("Error opening OGG file!");
			}

			var format = new Format
			{
				Tag = FormatTag.IEEE_FLOAT,
				BitsPerSample = 32
			};

			var info = FAudio.stb_vorbis_get_info(VorbisHandle);
			format.Channels = (ushort) info.channels;
			format.SampleRate = info.sample_rate;

			Format = format;

			for (int i = 0; i < BUFFER_COUNT; i += 1)
			{
				AudioBuffers[i] = (nint) NativeMemory.Alloc(32768);
			}

			OutOfData = false;
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.stb_vorbis_seek(VorbisHandle, sampleFrame);
			OutOfData = false;
		}

		protected override FAudio.FAudioBuffer OnBufferNeeded()
		{
			if (!Loaded)
			{
				OutOfData = true;
				return new FAudio.FAudioBuffer();
			}

			var buffer = AudioBuffers[NextBufferIndex];
			NextBufferIndex = (NextBufferIndex + 1) % BUFFER_COUNT;

			var requestedLengthInFloats = AUDIO_BUFFER_SIZE / sizeof(float);

			/* NOTE: this function returns samples per channel, not total samples */
			var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
				VorbisHandle,
				Format.Channels,
				buffer,
				(int) requestedLengthInFloats
			);

			var sampleCount = samples * Format.Channels;
			var filledLengthInBytes = (uint) (sampleCount * sizeof(float));

			if (sampleCount < requestedLengthInFloats)
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
		/// Unloads the Ogg data, freeing resources.
		/// </summary>
		public override unsafe void Close()
		{
			if (Loaded)
			{
				FAudio.stb_vorbis_close(VorbisHandle);
				NativeMemory.Free((void*) BufferDataPtr);

				VorbisHandle = IntPtr.Zero;
				BufferDataPtr = IntPtr.Zero;

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
		public static unsafe Format GetFormat(TitleStorage storage, string path)
		{
			if (!storage.GetFileSize(path, out var size))
			{
				return new Format();
			}

			var buffer = NativeMemory.Alloc((nuint) size);
			var span = new Span<byte>(buffer, (int) size);
			if (!storage.ReadFile(path, span))
			{
				return new Format();
			}

			var handle = FAudio.stb_vorbis_open_memory((nint) buffer, (int) size, out var error, IntPtr.Zero);
			if (error != 0)
			{
				Logger.LogError("Error loading file: " + error);
				NativeMemory.Free(buffer);
				return new Format();
			}

			var info = FAudio.stb_vorbis_get_info(handle);
			var format = new Format
			{
				Tag = FormatTag.IEEE_FLOAT,
				BitsPerSample = 32,
				Channels = (ushort) info.channels,
				SampleRate = info.sample_rate
			};

			FAudio.stb_vorbis_close(handle);
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
				Tag = FormatTag.IEEE_FLOAT,
				BitsPerSample = 32
			};

			fixed (void* ptr = data)
			{
				nint filePointer = FAudio.stb_vorbis_open_memory((nint) ptr, data.Length, out var error, IntPtr.Zero);
				if (error != 0)
				{
					throw new InvalidOperationException("Error loading file!");
				}
				var info = FAudio.stb_vorbis_get_info(filePointer);
				var lengthInFloats =
					FAudio.stb_vorbis_stream_length_in_samples(filePointer) * info.channels;
				lengthInBytes = (uint) (lengthInFloats * Marshal.SizeOf<float>());
				buffer = (nint) NativeMemory.Alloc((nuint) lengthInBytes);

				FAudio.stb_vorbis_get_samples_float_interleaved(
					filePointer,
					info.channels,
					buffer,
					(int) lengthInFloats
				);

				FAudio.stb_vorbis_close(filePointer);

				format.Channels = (ushort) info.channels;
				format.SampleRate = info.sample_rate;
			}

			return new LoadResult(format, buffer, lengthInBytes);
		}

		/// <summary>
		/// Decodes an entire OGG data buffer into an AudioBuffer.
		/// </summary>
		public static void SetData(AudioBuffer audioBuffer, ReadOnlySpan<byte> data)
		{
			var result = Load(data);
			audioBuffer.SetDataPointer(result.DataPtr, result.Length, true);
		}

		/// <summary>
		/// Decodes an entire OGG file into an AudioBuffer.
		/// </summary>
		public static unsafe AudioBuffer CreateBuffer(AudioDevice device, TitleStorage storage, string path)
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
