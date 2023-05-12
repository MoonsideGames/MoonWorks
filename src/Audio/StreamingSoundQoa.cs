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

		public unsafe static StreamingSoundQoa Create(AudioDevice device, string filePath)
		{
			var handle = FAudio.qoa_open_from_filename(filePath);
			if (handle == IntPtr.Zero)
			{
				throw new AudioLoadException("Error opening QOA file!");
			}

			FAudio.qoa_attributes(handle, out var channels, out var samplerate, out var samplesPerChannelPerFrame, out var totalSamplesPerChannel);

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
