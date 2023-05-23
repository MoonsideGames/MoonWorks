using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public class StreamingSoundQoa : StreamingSoundSeekable
	{
		private IntPtr QoaHandle = IntPtr.Zero;
		private IntPtr FileDataPtr = IntPtr.Zero;

		uint Channels;
		uint SamplesPerChannelPerFrame;
		uint TotalSamplesPerChannel;

		public override bool Loaded => QoaHandle != IntPtr.Zero;
		private string FilePath;

		private const uint QOA_MAGIC = 0x716f6166; /* 'qoaf' */

		private static unsafe UInt64 ReverseEndianness(UInt64 value)
		{
			byte* bytes = (byte*) &value;

			return
				((UInt64)(bytes[0]) << 56) | ((UInt64)(bytes[1]) << 48) |
				((UInt64)(bytes[2]) << 40) | ((UInt64)(bytes[3]) << 32) |
				((UInt64)(bytes[4]) << 24) | ((UInt64)(bytes[5]) << 16) |
				((UInt64)(bytes[6]) <<  8) | ((UInt64)(bytes[7]) <<  0);
		}

		public unsafe static StreamingSoundQoa Create(AudioDevice device, string filePath)
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			using var reader = new BinaryReader(stream);

			UInt64 fileHeader = ReverseEndianness(reader.ReadUInt64());
			if ((fileHeader >> 32) != QOA_MAGIC)
			{
				throw new AudioLoadException("Specified file is not a QOA file.");
			}

			uint totalSamplesPerChannel = (uint) (fileHeader & (0xFFFFFFFF));
			if (totalSamplesPerChannel == 0)
			{
				throw new AudioLoadException("Specified file is not a valid QOA file.");
			}

			UInt64 frameHeader = ReverseEndianness(reader.ReadUInt64());
			uint channels = (uint) ((frameHeader >> 56) & 0x0000FF);
			uint samplerate = (uint) ((frameHeader >> 32) & 0xFFFFFF);
			uint samplesPerChannelPerFrame = (uint) ((frameHeader >> 16) & 0x00FFFF);

			return new StreamingSoundQoa(
				device,
				filePath,
				channels,
				samplerate,
				samplesPerChannelPerFrame,
				totalSamplesPerChannel
			);
		}

		internal unsafe StreamingSoundQoa(
			AudioDevice device,
			string filePath,
			uint channels,
			uint samplesPerSecond,
			uint samplesPerChannelPerFrame,
			uint totalSamplesPerChannel
		) : base(
			device,
			1,
			16,
			(ushort) (2 * channels),
			(ushort) channels,
			samplesPerSecond,
			samplesPerChannelPerFrame * channels * sizeof(short),
			true
		) {
			Channels = channels;
			SamplesPerChannelPerFrame = samplesPerChannelPerFrame;
			TotalSamplesPerChannel = totalSamplesPerChannel;
			FilePath = filePath;
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.qoa_seek_frame(QoaHandle, (int) sampleFrame);
		}

		public override unsafe void Load()
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
				throw new AudioLoadException("Error opening QOA file!");
			}
		}

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

		protected override unsafe void FillBuffer(
			void* buffer,
			int bufferLengthInBytes,
			out int filledLengthInBytes,
			out bool reachedEnd
		) {
			var lengthInShorts = bufferLengthInBytes / sizeof(short);

			// NOTE: this function returns samples per channel!
			var samples = FAudio.qoa_decode_next_frame(QoaHandle, (short*) buffer);

			var sampleCount = samples * Channels;
			reachedEnd = sampleCount < lengthInShorts;
			filledLengthInBytes = (int) (sampleCount * sizeof(short));
		}
	}
}
