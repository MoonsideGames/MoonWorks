using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public class StaticSound : AudioResource
	{
		internal FAudio.FAudioBuffer Handle;
		public ushort FormatTag { get; }
		public ushort BitsPerSample { get; }
		public ushort Channels { get; }
		public uint SamplesPerSecond { get; }
		public ushort BlockAlign { get; }

		public uint LoopStart { get; set; } = 0;
		public uint LoopLength { get; set; } = 0;

		private Stack<StaticSoundInstance> Instances = new Stack<StaticSoundInstance>();

		private bool OwnsBuffer;

		public static StaticSound LoadOgg(AudioDevice device, string filePath)
		{
			var filePointer = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

			if (error != 0)
			{
				throw new AudioLoadException("Error loading file!");
			}
			var info = FAudio.stb_vorbis_get_info(filePointer);
			var bufferSize = FAudio.stb_vorbis_stream_length_in_samples(filePointer) * info.channels;
			var buffer = new float[bufferSize];

			FAudio.stb_vorbis_get_samples_float_interleaved(
				filePointer,
				info.channels,
				buffer,
				(int) bufferSize
			);

			FAudio.stb_vorbis_close(filePointer);

			return new StaticSound(
				device,
				(ushort) info.channels,
				info.sample_rate,
				buffer,
				0,
				(uint) buffer.Length
			);
		}

		// mostly borrowed from https://github.com/FNA-XNA/FNA/blob/b71b4a35ae59970ff0070dea6f8620856d8d4fec/src/Audio/SoundEffect.cs#L385
		public static StaticSound LoadWav(AudioDevice device, string filePath)
		{
			// Sample data
			byte[] data;

			// WaveFormatEx data
			ushort wFormatTag;
			ushort nChannels;
			uint nSamplesPerSec;
			uint nAvgBytesPerSec;
			ushort nBlockAlign;
			ushort wBitsPerSample;
			int samplerLoopStart = 0;
			int samplerLoopEnd = 0;

			using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
			{
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
				data = reader.ReadBytes(waveDataLength);

				// Scan for other chunks
				while (reader.PeekChar() != -1)
				{
					char[] chunkIDChars = reader.ReadChars(4);
					if (chunkIDChars.Length < 4)
					{
						break; // EOL!
					}
					byte[] chunkSizeBytes = reader.ReadBytes(4);
					if (chunkSizeBytes.Length < 4)
					{
						break; // EOL!
					}
					string chunk_signature = new string(chunkIDChars);
					int chunkDataSize = BitConverter.ToInt32(chunkSizeBytes, 0);
					if (chunk_signature == "smpl") // "smpl", Sampler Chunk Found
					{
						reader.ReadUInt32(); // Manufacturer
						reader.ReadUInt32(); // Product
						reader.ReadUInt32(); // Sample Period
						reader.ReadUInt32(); // MIDI Unity Note
						reader.ReadUInt32(); // MIDI Pitch Fraction
						reader.ReadUInt32(); // SMPTE Format
						reader.ReadUInt32(); // SMPTE Offset
						uint numSampleLoops = reader.ReadUInt32();
						int samplerData = reader.ReadInt32();

						for (int i = 0; i < numSampleLoops; i += 1)
						{
							reader.ReadUInt32(); // Cue Point ID
							reader.ReadUInt32(); // Type
							int start = reader.ReadInt32();
							int end = reader.ReadInt32();
							reader.ReadUInt32(); // Fraction
							reader.ReadUInt32(); // Play Count

							if (i == 0) // Grab loopStart and loopEnd from first sample loop
							{
								samplerLoopStart = start;
								samplerLoopEnd = end;
							}
						}

						if (samplerData != 0) // Read Sampler Data if it exists
						{
							reader.ReadBytes(samplerData);
						}
					}
					else // Read unwanted chunk data and try again
					{
						reader.ReadBytes(chunkDataSize);
					}
				}
				// End scan
			}

			return new StaticSound(
				device,
				wFormatTag,
				wBitsPerSample,
				nBlockAlign,
				nChannels,
				nSamplesPerSec,
				data,
				0,
				(uint) data.Length
			);
		}

		public unsafe StaticSound(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond,
			byte[] buffer,
			uint bufferOffset, /* number of bytes */
			uint bufferLength /* number of bytes */
		) : base(device)
		{
			FormatTag = formatTag;
			BitsPerSample = bitsPerSample;
			BlockAlign = blockAlign;
			Channels = channels;
			SamplesPerSecond = samplesPerSecond;

			Handle = new FAudio.FAudioBuffer();
			Handle.Flags = FAudio.FAUDIO_END_OF_STREAM;
			Handle.pContext = IntPtr.Zero;
			Handle.AudioBytes = bufferLength;
			Handle.pAudioData = (nint) NativeMemory.Alloc(bufferLength);
			Marshal.Copy(buffer, (int) bufferOffset, Handle.pAudioData, (int) bufferLength);
			Handle.PlayBegin = 0;
			Handle.PlayLength = 0;

			LoopStart = 0;
			LoopLength = 0;

			OwnsBuffer = true;
		}

		public unsafe StaticSound(
			AudioDevice device,
			ushort channels,
			uint samplesPerSecond,
			float[] buffer,
			uint bufferOffset, /* in floats */
			uint bufferLength  /* in floats */
		) : base(device)
		{
			FormatTag = 3;
			BitsPerSample = 32;
			BlockAlign = (ushort) (4 * channels);
			Channels = channels;
			SamplesPerSecond = samplesPerSecond;

			var bufferLengthInBytes = (int) (bufferLength * sizeof(float));
			Handle = new FAudio.FAudioBuffer();
			Handle.Flags = FAudio.FAUDIO_END_OF_STREAM;
			Handle.pContext = IntPtr.Zero;
			Handle.AudioBytes = (uint) bufferLengthInBytes;
			Handle.pAudioData = (nint) NativeMemory.Alloc((nuint) bufferLengthInBytes);
			Marshal.Copy(buffer, (int) bufferOffset, Handle.pAudioData, (int) bufferLength);
			Handle.PlayBegin = 0;
			Handle.PlayLength = 0;

			LoopStart = 0;
			LoopLength = 0;

			OwnsBuffer = true;
		}

		public StaticSound(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond,
			IntPtr bufferPtr,
			uint bufferLengthInBytes) : base(device)
		{
			FormatTag = formatTag;
			BitsPerSample = bitsPerSample;
			BlockAlign = blockAlign;
			Channels = channels;
			SamplesPerSecond = samplesPerSecond;

			Handle = new FAudio.FAudioBuffer
			{
				Flags = FAudio.FAUDIO_END_OF_STREAM,
				pContext = IntPtr.Zero,
				pAudioData = bufferPtr,
				AudioBytes = bufferLengthInBytes,
				PlayBegin = 0,
				PlayLength = 0
			};

			OwnsBuffer = false;
		}

		/// <summary>
		/// Gets a sound instance from the pool.
		/// NOTE: If you lose track of instances, you will create garbage collection pressure!
		/// </summary>
		public StaticSoundInstance GetInstance()
		{
			if (Instances.Count == 0)
			{
				Instances.Push(new StaticSoundInstance(Device, this));
			}

			return Instances.Pop();
		}

		internal void FreeInstance(StaticSoundInstance instance)
		{
			instance.Reset();
			Instances.Push(instance);
		}

		protected override unsafe void Destroy()
		{
			if (OwnsBuffer)
			{
				NativeMemory.Free((void*) Handle.pAudioData);
			}
		}
	}
}
