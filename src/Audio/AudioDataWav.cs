using System;
using System.Runtime.InteropServices;
using MoonWorks.Storage;

namespace MoonWorks.Audio
{
	public static class AudioDataWav
	{
		const int MAGIC_RIFF = 0x46464952;
		const int MAGIC_WAVE = 0x45564157;
		const int MAGIC_FMT  = 0x20746d66;
		const int MAGIC_DATA = 0x61746164;

		private ref struct ParseResult
		{
			public Format Format;
			public ReadOnlySpan<byte> Data;

			public ParseResult(Format format, ReadOnlySpan<byte> data)
			{
				Format = format;
				Data = data;
			}
		}

		private static Format ParseFormat(ByteSpanStream stream)
		{
			// RIFF Signature
			if (stream.Read<int>() != MAGIC_RIFF)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			stream.Read<uint>(); // Riff chunk size

			// WAVE Header
			if (stream.Read<int>() != MAGIC_WAVE)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			// Skip over non-format chunks
			while (stream.Remaining >= 4 && stream.Read<int>() != MAGIC_FMT)
			{
				var chunkSize = stream.Read<uint>();
				stream.Advance(chunkSize);
			}

			if (stream.Remaining < 4)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			uint format_chunk_size = stream.Read<uint>();

			// WaveFormatEx data
			ushort wFormatTag = stream.Read<ushort>();
			ushort nChannels = stream.Read<ushort>();
			uint nSamplesPerSec = stream.Read<uint>();
			uint nAvgBytesPerSec = stream.Read<uint>();
			ushort nBlockAlign = stream.Read<ushort>();
			ushort wBitsPerSample = stream.Read<ushort>();

			// Reads residual bytes
			if (format_chunk_size > 16)
			{
				stream.Advance(format_chunk_size - 16);
			}

			// Skip over non-data chunks
			while (stream.Remaining > 4 && stream.Read<int>() != MAGIC_DATA)
			{
				var chunkSize = stream.Read<uint>();
				stream.Advance(chunkSize);
			}

			if (stream.Remaining < 4)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			int waveDataLength = stream.Read<int>();
			var dataSpan = stream.SliceRemainder();

			var format = new Format
			{
				Tag = (FormatTag) wFormatTag,
				BitsPerSample = wBitsPerSample,
				Channels = nChannels,
				SampleRate = nSamplesPerSec
			};

			return format;
		}

		private static ParseResult Parse(ReadOnlySpan<byte> span)
		{
			var stream = new ByteSpanStream(span);

			var format = ParseFormat(stream);

			int waveDataLength = stream.Read<int>();
			var dataSpan = stream.SliceRemainder(waveDataLength);

			return new ParseResult(format, dataSpan);
		}

		/// <summary>
		/// Sets an audio buffer from a span of raw WAV data.
		/// </summary>
		public static void SetData(AudioBuffer audioBuffer, ReadOnlySpan<byte> span)
		{
			var result = Parse(span);
			audioBuffer.Format = result.Format;
			audioBuffer.SetData(result.Data);
		}

		/// <summary>
		/// Create an AudioBuffer containing all the WAV audio data in a file.
		/// </summary>
		public unsafe static AudioBuffer CreateBuffer(AudioDevice device, IStorage storage, string path)
		{
			var fileBuffer = storage.ReadFile(path, out var size);
			if (fileBuffer == null)
			{
				return null;
			}

			var span = new Span<byte>(fileBuffer, (int) size);
			var result = Parse(span);

			var audioBuffer = AudioBuffer.Create(device, result.Format);
			audioBuffer.SetData(result.Data);

			NativeMemory.Free(fileBuffer);
			return audioBuffer;
		}

		/// <summary>
		/// Get audio format data without reading the entire file.
		/// </summary>
		public static unsafe Format GetFormat(IStorage storage, string path)
		{
			var buffer = storage.ReadFile(path, out var size);
			if (buffer == null)
			{
				return new Format();
			}
			var span = new ReadOnlySpan<byte>(buffer, (int) size);
			var reader = new ByteSpanStream(span);
			var format = ParseFormat(reader);

			NativeMemory.Free(buffer);

			return format;
		}
	}
}
