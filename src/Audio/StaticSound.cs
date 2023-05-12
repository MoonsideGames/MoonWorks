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

		private Stack<StaticSoundInstance> AvailableInstances = new Stack<StaticSoundInstance>();
		private HashSet<StaticSoundInstance> UsedInstances = new HashSet<StaticSoundInstance>();

		private bool OwnsBuffer;

		public static unsafe StaticSound LoadOgg(AudioDevice device, string filePath)
		{
			var filePointer = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

			if (error != 0)
			{
				throw new AudioLoadException("Error loading file!");
			}
			var info = FAudio.stb_vorbis_get_info(filePointer);
			var lengthInFloats =
				FAudio.stb_vorbis_stream_length_in_samples(filePointer) * info.channels;
			var lengthInBytes = lengthInFloats * Marshal.SizeOf<float>();
			var buffer = NativeMemory.Alloc((nuint) lengthInBytes);

			FAudio.stb_vorbis_get_samples_float_interleaved(
				filePointer,
				info.channels,
				(nint) buffer,
				(int) lengthInFloats
			);

			FAudio.stb_vorbis_close(filePointer);

			return new StaticSound(
				device,
				3,
				32,
				(ushort) (4 * info.channels),
				(ushort) info.channels,
				info.sample_rate,
				(nint) buffer,
				(uint) lengthInBytes,
				true);
		}

		// mostly borrowed from https://github.com/FNA-XNA/FNA/blob/b71b4a35ae59970ff0070dea6f8620856d8d4fec/src/Audio/SoundEffect.cs#L385
		public static unsafe StaticSound LoadWav(AudioDevice device, string filePath)
		{
			// WaveFormatEx data
			ushort wFormatTag;
			ushort nChannels;
			uint nSamplesPerSec;
			uint nAvgBytesPerSec;
			ushort nBlockAlign;
			ushort wBitsPerSample;
			int samplerLoopStart = 0;
			int samplerLoopEnd = 0;

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

			var sound = new StaticSound(
				device,
				wFormatTag,
				wBitsPerSample,
				nBlockAlign,
				nChannels,
				nSamplesPerSec,
				(nint) waveDataBuffer,
				(uint) waveDataLength,
				true
			);

			return sound;
		}

		public static unsafe StaticSound FromQOA(AudioDevice device, string path)
		{
			var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			var fileDataPtr = NativeMemory.Alloc((nuint) fileStream.Length);
			var fileDataSpan = new Span<byte>(fileDataPtr, (int) fileStream.Length);
			fileStream.ReadExactly(fileDataSpan);
			fileStream.Close();

			var qoaHandle = FAudio.qoa_open_from_memory((char*) fileDataPtr, (uint) fileDataSpan.Length, 0);
			if (qoaHandle == 0)
			{
				NativeMemory.Free(fileDataPtr);
				Logger.LogError("Error opening QOA file!");
				throw new AudioLoadException("Error opening QOA file!");
			}

			FAudio.qoa_attributes(qoaHandle, out var channels, out var samplerate, out var samples_per_channel_per_frame, out var total_samples_per_channel);

			var bufferLengthInBytes = total_samples_per_channel * channels * sizeof(short);
			var buffer = NativeMemory.Alloc(bufferLengthInBytes);
			FAudio.qoa_decode_entire(qoaHandle, (short*) buffer);

			FAudio.qoa_close(qoaHandle);
			NativeMemory.Free(fileDataPtr);

			return new StaticSound(
				device,
				1,
				16,
				(ushort) (channels * 2),
				(ushort) channels,
				samplerate,
				(nint) buffer,
				bufferLengthInBytes,
				true
			);
		}

		public StaticSound(
			AudioDevice device,
			ushort formatTag,
			ushort bitsPerSample,
			ushort blockAlign,
			ushort channels,
			uint samplesPerSecond,
			IntPtr bufferPtr,
			uint bufferLengthInBytes,
			bool ownsBuffer) : base(device)
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

			OwnsBuffer = ownsBuffer;
		}

		/// <summary>
		/// Gets a sound instance from the pool.
		/// NOTE: If AutoFree is false, you will have to call StaticSoundInstance.Free() yourself or leak the instance!
		/// </summary>
		public StaticSoundInstance GetInstance(bool autoFree = true)
		{
			if (AvailableInstances.Count == 0)
			{
				AvailableInstances.Push(new StaticSoundInstance(Device, this, autoFree));
			}

			var instance = AvailableInstances.Pop();
			UsedInstances.Add(instance);
			return instance;
		}

		internal void FreeInstance(StaticSoundInstance instance)
		{
			instance.Reset();
			UsedInstances.Remove(instance);
			AvailableInstances.Push(instance);
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
