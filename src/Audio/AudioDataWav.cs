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

		private static Format ParseFormat(ref ByteSpanReader reader)
		{
			// RIFF Signature
			if (reader.Read<int>() != MAGIC_RIFF)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			reader.Read<uint>(); // Riff chunk size

			// WAVE Header
			if (reader.Read<int>() != MAGIC_WAVE)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			// Skip over non-format chunks
			while (reader.Remaining >= 4 && reader.Read<int>() != MAGIC_FMT)
			{
				var chunkSize = reader.Read<uint>();
				reader.Advance(chunkSize);
			}

			if (reader.Remaining < 4)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			uint format_chunk_size = reader.Read<uint>();

			// WaveFormatEx data
			ushort wFormatTag = reader.Read<ushort>();
			ushort nChannels = reader.Read<ushort>();
			uint nSamplesPerSec = reader.Read<uint>();
			uint nAvgBytesPerSec = reader.Read<uint>();
			ushort nBlockAlign = reader.Read<ushort>();
			ushort wBitsPerSample = reader.Read<ushort>();

			// Reads residual bytes
			if (format_chunk_size > 16)
			{
				reader.Advance(format_chunk_size - 16);
			}

			// Skip over non-data chunks
			while (reader.Remaining > 4 && reader.Read<int>() != MAGIC_DATA)
			{
				var chunkSize = reader.Read<uint>();
				reader.Advance(chunkSize);
			}

			if (reader.Remaining < 4)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			int waveDataLength = reader.Read<int>();
			var dataSpan = reader.SliceRemainder();

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
			var stream = new ByteSpanReader(span);

			var format = ParseFormat(ref stream);

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

			var result = Parse(span);

			var audioBuffer = AudioBuffer.Create(device, result.Format);
			audioBuffer.SetData(result.Data);
			NativeMemory.Free(buffer);

			return audioBuffer;
		}

		/// <summary>
		/// Get audio format data without reading the entire file.
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

			var reader = new ByteSpanReader(span);
			var format = ParseFormat(ref reader);
			NativeMemory.Free(buffer);

			return format;
		}
	}
}
