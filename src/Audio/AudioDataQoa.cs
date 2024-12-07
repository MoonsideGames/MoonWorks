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
		private IntPtr QoaHandle = IntPtr.Zero;
		private IntPtr FileDataPtr = IntPtr.Zero;

		private string FilePath;

		private const uint QOA_MAGIC = 0x716f6166; /* 'qoaf' */

		public override bool Loaded => QoaHandle != IntPtr.Zero;

		private uint decodeBufferSize;
		public override uint DecodeBufferSize => decodeBufferSize;

		public AudioDataQoa(AudioDevice device, string filePath) : base(device)
		{
			FilePath = filePath;

			using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
			using var reader = new BinaryReader(stream);

			UInt64 fileHeader = ReverseEndianness(reader.ReadUInt64());
			if ((fileHeader >> 32) != QOA_MAGIC)
			{
				throw new InvalidOperationException("Specified file is not a QOA file.");
			}

			uint totalSamplesPerChannel = (uint) (fileHeader & (0xFFFFFFFF));
			if (totalSamplesPerChannel == 0)
			{
				throw new InvalidOperationException("Specified file is not a valid QOA file.");
			}

			UInt64 frameHeader = ReverseEndianness(reader.ReadUInt64());
			uint channels = (uint) ((frameHeader >> 56) & 0x0000FF);
			uint samplerate = (uint) ((frameHeader >> 32) & 0xFFFFFF);
			uint samplesPerChannelPerFrame = (uint) ((frameHeader >> 16) & 0x00FFFF);

			Format = new Format
			{
				Tag = FormatTag.PCM,
				BitsPerSample = 16,
				Channels = (ushort) channels,
				SampleRate = samplerate
			};

			decodeBufferSize = channels * samplesPerChannelPerFrame * sizeof(short);
		}

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
		public override unsafe void Load()
		{
			if (!Loaded)
			{
				var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
				FileDataPtr = (nint) NativeMemory.Alloc((nuint) fileStream.Length);
				var fileDataSpan = new Span<byte>((void*) FileDataPtr, (int) fileStream.Length);
				fileStream.ReadExactly(fileDataSpan);
				fileStream.Close();

				QoaHandle = FAudio.qoa_open_from_memory((char*) FileDataPtr, (uint) fileDataSpan.Length, 0);
				if (QoaHandle == IntPtr.Zero)
				{
					NativeMemory.Free((void*) FileDataPtr);
					Logger.LogError("Error opening QOA file!");
					throw new InvalidOperationException("Error opening QOA file!");
				}
			}
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.qoa_seek_frame(QoaHandle, (int) sampleFrame);
		}

		/// <summary>
		/// Unloads the qoa data, freeing resources.
		/// </summary>
		public override unsafe void Unload()
		{
			if (Loaded)
			{
				FAudio.qoa_close(QoaHandle);
				NativeMemory.Free((void*) FileDataPtr);

				QoaHandle = IntPtr.Zero;
				FileDataPtr = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Loads the entire qoa file into an AudioBuffer. Useful for static audio.
		/// </summary>
		public unsafe static AudioBuffer CreateBuffer(AudioDevice device, string filePath)
		{
			using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var fileDataPtr = NativeMemory.Alloc((nuint) fileStream.Length);
			var fileDataSpan = new Span<byte>(fileDataPtr, (int) fileStream.Length);
			fileStream.ReadExactly(fileDataSpan);
			fileStream.Close();

			var qoaHandle = FAudio.qoa_open_from_memory((char*) fileDataPtr, (uint) fileDataSpan.Length, 0);
			if (qoaHandle == 0)
			{
				NativeMemory.Free(fileDataPtr);
				Logger.LogError("Error opening QOA file!");
				throw new InvalidOperationException("Error opening QOA file!");
			}

			FAudio.qoa_attributes(qoaHandle, out var channels, out var samplerate, out var samples_per_channel_per_frame, out var total_samples_per_channel);

			var bufferLengthInBytes = total_samples_per_channel * channels * sizeof(short);
			var buffer = NativeMemory.Alloc(bufferLengthInBytes);
			FAudio.qoa_decode_entire(qoaHandle, (short*) buffer);

			FAudio.qoa_close(qoaHandle);
			NativeMemory.Free(fileDataPtr);

			var format = new Format
			{
				Tag = FormatTag.PCM,
				BitsPerSample = 16,
				Channels = (ushort) channels,
				SampleRate = samplerate
			};

			var audioBuffer = AudioBuffer.Create(device, format);
			audioBuffer.SetDataPointer((nint) buffer, bufferLengthInBytes, true);
			return audioBuffer;
		}

		private static unsafe UInt64 ReverseEndianness(UInt64 value)
		{
			byte* bytes = (byte*) &value;

			return
				((UInt64)(bytes[0]) << 56) | ((UInt64)(bytes[1]) << 48) |
				((UInt64)(bytes[2]) << 40) | ((UInt64)(bytes[3]) << 32) |
				((UInt64)(bytes[4]) << 24) | ((UInt64)(bytes[5]) << 16) |
				((UInt64)(bytes[6]) <<  8) | ((UInt64)(bytes[7]) <<  0);
		}
	}
}
