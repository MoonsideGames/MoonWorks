using System;
using System.IO;
using System.Runtime.InteropServices;

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

		private static ParseResult Parse(ReadOnlySpan<byte> span)
		{
			var stream = new ByteSpanStream(span);

			// RIFF Signature
			if (stream.Read<int>() != MAGIC_RIFF)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			stream.Read<uint>(); // Riff chunk size

			if (stream.Read<int>() != MAGIC_WAVE)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			// WAVE Header
			if (stream.Read<int>() != MAGIC_FMT)
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			int format_chunk_size = stream.Read<int>();

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

			// data Signature
			while (stream.Remaining > 4 && stream.Peek<int>() != MAGIC_DATA)
			{
				stream.Advance(1);
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
		public unsafe static AudioBuffer CreateBuffer(AudioDevice device, string filePath)
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var memory = NativeMemory.Alloc((nuint) stream.Length);
			var span = new Span<byte>(memory, (int) stream.Length);
			stream.ReadExactly(span);

			var result = Parse(span);

			var audioBuffer = AudioBuffer.Create(device, result.Format);
			audioBuffer.SetData(result.Data);

			NativeMemory.Free(memory);
			return audioBuffer;
		}
	}
}
