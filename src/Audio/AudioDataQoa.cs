using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Streamable audio in QOA format.
	/// </summary>
	public class AudioDataQoa : AudioDataStreamable
	{
		private const uint QOA_MAGIC = 0x716f6166; /* 'qoaf' */

		private IntPtr QoaHandle = IntPtr.Zero;
		private IntPtr BufferDataPtr = IntPtr.Zero;
		private uint BufferDataLength = 0;

		public override bool Loaded => QoaHandle != IntPtr.Zero;

		private uint decodeBufferSize;
		public override uint DecodeBufferSize => decodeBufferSize;

		public static AudioDataQoa Create(AudioDevice device)
		{
			return new AudioDataQoa(device);
		}

		private AudioDataQoa(AudioDevice device) : base(device) { }

		public override unsafe void Decode(void* buffer, int bufferLengthInBytes, out int filledLengthInBytes, out bool reachedEnd)
		{
			var lengthInShorts = bufferLengthInBytes / sizeof(short);

			// NOTE: this function returns samples per channel!
			var samples = FAudio.qoa_decode_next_frame(QoaHandle, (short*) buffer);

			var sampleCount = samples * Format.Channels;
			reachedEnd = sampleCount < lengthInShorts;
			filledLengthInBytes = (int) (sampleCount * sizeof(short));
		}

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

			QoaHandle = FAudio.qoa_open_from_memory((char*) BufferDataPtr, BufferDataLength, 0);
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

			decodeBufferSize = channels * samplesPerChannelPerFrame * sizeof(short);
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.qoa_seek_frame(QoaHandle, (int) sampleFrame);
		}

		/// <summary>
		/// Unloads the qoa data, freeing resources.
		/// </summary>
		public override unsafe void Close()
		{
			if (Loaded)
			{
				FAudio.qoa_close(QoaHandle);
				NativeMemory.Free((void*) BufferDataPtr);

				QoaHandle = IntPtr.Zero;
				BufferDataPtr = IntPtr.Zero;
				BufferDataLength = 0;
			}
		}

		/// <summary>
		/// Get audio format data without decoding the entire file.
		/// </summary>
		public static Format GetFormat(string filePath)
		{
			var handle = FAudio.qoa_open_from_filename(filePath);
			if (handle == IntPtr.Zero)
			{
				throw new InvalidOperationException("Error loading QOA file!");
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
				var qoaHandle = FAudio.qoa_open_from_memory((char*) ptr, (uint) data.Length, 0);
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
		public unsafe static AudioBuffer CreateBuffer(AudioDevice device, string filePath)
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
