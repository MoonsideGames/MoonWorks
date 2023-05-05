using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public class StreamingSoundQoa : StreamingSoundSeekable
	{
		private IntPtr QoaHandle;
		private IntPtr FileDataPtr;

		public override bool AutoUpdate => true;

		uint Channels;
		uint SamplesPerChannelPerFrame;
		uint TotalSamplesPerChannel;

		public unsafe static StreamingSoundQoa Load(AudioDevice device, string filePath)
		{
			var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
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

			FAudio.qoa_attributes(qoaHandle, out var channels, out var sampleRate, out var samplesPerChannelPerFrame, out var totalSamplesPerChannel);

			return new StreamingSoundQoa(
				device,
				(IntPtr) fileDataPtr,
				qoaHandle,
				channels,
				sampleRate,
				samplesPerChannelPerFrame,
				totalSamplesPerChannel
			);
		}

		internal unsafe StreamingSoundQoa(
			AudioDevice device,
			IntPtr fileDataPtr, // MUST BE A NATIVE MEMORY HANDLE!!
			IntPtr qoaHandle,
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
			samplesPerChannelPerFrame * channels * sizeof(short)
		) {
			FileDataPtr = fileDataPtr;
			QoaHandle = qoaHandle;
			Channels = channels;
			SamplesPerChannelPerFrame = samplesPerChannelPerFrame;
			TotalSamplesPerChannel = totalSamplesPerChannel;
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.qoa_seek_frame(QoaHandle, (int) sampleFrame);
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

		protected override unsafe void Destroy()
		{
			base.Destroy();

			if (!IsDisposed)
			{
				FAudio.qoa_close(QoaHandle);
				NativeMemory.Free((void*) FileDataPtr);
			}
		}
	}
}
