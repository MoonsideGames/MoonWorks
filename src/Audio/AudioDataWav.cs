using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public static class AudioDataWav
	{
		/// <summary>
		/// Create an AudioBuffer containing all the WAV audio data in a file.
		/// </summary>
		/// <returns></returns>
		public unsafe static AudioBuffer CreateBuffer(AudioDevice device, string filePath)
		{
			// mostly borrowed from https://github.com/FNA-XNA/FNA/blob/b71b4a35ae59970ff0070dea6f8620856d8d4fec/src/Audio/SoundEffect.cs#L385

			// WaveFormatEx data
			ushort wFormatTag;
			ushort nChannels;
			uint nSamplesPerSec;
			uint nAvgBytesPerSec;
			ushort nBlockAlign;
			ushort wBitsPerSample;

			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			using var reader = new BinaryReader(stream);

			// RIFF Signature
			string signature = new string(reader.ReadChars(4));
			if (signature != "RIFF")
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			reader.ReadUInt32(); // Riff Chunk Size

			string wformat = new string(reader.ReadChars(4));
			if (wformat != "WAVE")
			{
				throw new NotSupportedException("Specified stream is not a wave file.");
			}

			// WAVE Header
			string format_signature = new string(reader.ReadChars(4));
			while (format_signature != "fmt ")
			{
				reader.ReadBytes(reader.ReadInt32());
				format_signature = new string(reader.ReadChars(4));
			}

			int format_chunk_size = reader.ReadInt32();

			wFormatTag = reader.ReadUInt16();
			nChannels = reader.ReadUInt16();
			nSamplesPerSec = reader.ReadUInt32();
			nAvgBytesPerSec = reader.ReadUInt32();
			nBlockAlign = reader.ReadUInt16();
			wBitsPerSample = reader.ReadUInt16();

			// Reads residual bytes
			if (format_chunk_size > 16)
			{
				reader.ReadBytes(format_chunk_size - 16);
			}

			// data Signature
			string data_signature = new string(reader.ReadChars(4));
			while (data_signature.ToLowerInvariant() != "data")
			{
				reader.ReadBytes(reader.ReadInt32());
				data_signature = new string(reader.ReadChars(4));
			}
			if (data_signature != "data")
			{
				throw new NotSupportedException("Specified wave file is not supported.");
			}

			int waveDataLength = reader.ReadInt32();
			var waveDataBuffer = NativeMemory.Alloc((nuint) waveDataLength);
			var waveDataSpan = new Span<byte>(waveDataBuffer, waveDataLength);
			stream.ReadExactly(waveDataSpan);

			var format = new Format
			{
				Tag = (FormatTag) wFormatTag,
				BitsPerSample = wBitsPerSample,
				Channels = nChannels,
				SampleRate = nSamplesPerSec
			};

			return new AudioBuffer(
				device,
				format,
				(nint) waveDataBuffer,
				(uint) waveDataLength,
				true
			);
		}
	}
}
