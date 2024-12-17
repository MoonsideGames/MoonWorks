using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Streamable audio in Ogg format.
	/// </summary>
	public class AudioDataOgg : AudioDataStreamable
	{
		private IntPtr VorbisHandle = IntPtr.Zero;
		private IntPtr BufferDataPtr = IntPtr.Zero;
		private uint BufferDataLength = 0;

		public override bool Loaded => VorbisHandle != IntPtr.Zero;
		public override uint DecodeBufferSize => 32768;

		public static AudioDataOgg Create(AudioDevice device)
		{
			return new AudioDataOgg(device);
		}

		private AudioDataOgg(AudioDevice device) : base(device) { }

		public override unsafe void Decode(void* buffer, int bufferLengthInBytes, out int filledLengthInBytes, out bool reachedEnd)
		{
			var lengthInFloats = bufferLengthInBytes / sizeof(float);

			/* NOTE: this function returns samples per channel, not total samples */
			var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
				VorbisHandle,
				Format.Channels,
				(IntPtr) buffer,
				lengthInFloats
			);

			var sampleCount = samples * Format.Channels;
			reachedEnd = sampleCount < lengthInFloats;
			filledLengthInBytes = sampleCount * sizeof(float);
		}

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
			BufferDataLength = (uint) data.Length;

			fixed (void *ptr = data)
			{
				NativeMemory.Copy(ptr, (void*) BufferDataPtr, BufferDataLength);
			}

			VorbisHandle = FAudio.stb_vorbis_open_memory(BufferDataPtr, (int) BufferDataLength, out int error, IntPtr.Zero);
			if (error != 0)
			{
				NativeMemory.Free((void*) BufferDataPtr);
				BufferDataPtr = IntPtr.Zero;
				BufferDataLength = 0;
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
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.stb_vorbis_seek(VorbisHandle, sampleFrame);
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
			}
		}

		/// <summary>
		/// Get audio format data without decoding the entire file.
		/// </summary>
		public static Format GetFormat(string filePath)
		{
			var handle = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);
			if (error != 0)
			{
				throw new InvalidOperationException("Error loading file: " + error);
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
		public static unsafe AudioBuffer CreateBuffer(AudioDevice device, string filePath)
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var fileMemory = NativeMemory.Alloc((nuint) stream.Length);
			var fileSpan = new Span<byte>(fileMemory, (int) stream.Length);
			stream.ReadExactly(fileSpan);

			var result = Load(fileSpan);

			var audioBuffer = AudioBuffer.Create(device, result.Format);
			audioBuffer.SetDataPointer(result.DataPtr, result.Length, true);

			NativeMemory.Free(fileMemory);
			return audioBuffer;
		}
	}
}
