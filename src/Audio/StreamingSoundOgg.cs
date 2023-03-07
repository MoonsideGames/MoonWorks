using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public class StreamingSoundOgg : StreamingSoundSeekable
	{
		private IntPtr VorbisHandle;
		private IntPtr FileDataPtr;
		private FAudio.stb_vorbis_info Info;

		protected override int BUFFER_SIZE => 32768;
		public override bool AutoUpdate => true;

		public unsafe static StreamingSoundOgg Load(AudioDevice device, string filePath)
		{
			var fileData = File.ReadAllBytes(filePath);
			var fileDataPtr = NativeMemory.Alloc((nuint) fileData.Length);
			Marshal.Copy(fileData, 0, (IntPtr) fileDataPtr, fileData.Length);
			var vorbisHandle = FAudio.stb_vorbis_open_memory((IntPtr) fileDataPtr, fileData.Length, out int error, IntPtr.Zero);
			if (error != 0)
			{
				NativeMemory.Free(fileDataPtr);
				Logger.LogError("Error opening OGG file!");
				Logger.LogError("Error: " + error);
				throw new AudioLoadException("Error opening OGG file!");
			}
			var info = FAudio.stb_vorbis_get_info(vorbisHandle);

			return new StreamingSoundOgg(
				device,
				(IntPtr) fileDataPtr,
				vorbisHandle,
				info
			);
		}

		internal unsafe StreamingSoundOgg(
			AudioDevice device,
			IntPtr fileDataPtr, // MUST BE A NATIVE MEMORY HANDLE!!
			IntPtr vorbisHandle,
			FAudio.stb_vorbis_info info
		) : base(
			device,
			3, /* float type */
			32, /* size of float */
			(ushort) (4 * info.channels),
			(ushort) info.channels,
			info.sample_rate
		) {
			FileDataPtr = fileDataPtr;
			VorbisHandle = vorbisHandle;
			Info = info;
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.stb_vorbis_seek(VorbisHandle, sampleFrame);
		}

		protected unsafe override void FillBuffer(
			void* buffer,
			int bufferLengthInBytes,
			out int filledLengthInBytes,
			out bool reachedEnd
		) {
			var lengthInFloats = bufferLengthInBytes / sizeof(float);

			/* NOTE: this function returns samples per channel, not total samples */
			var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
				VorbisHandle,
				Info.channels,
				(IntPtr) buffer,
				lengthInFloats
			);

			var sampleCount = samples * Info.channels;
			reachedEnd = sampleCount < lengthInFloats;
			filledLengthInBytes = sampleCount * sizeof(float);
		}

		protected unsafe override void Destroy()
		{
			base.Destroy();

			if (!IsDisposed)
			{
				FAudio.stb_vorbis_close(VorbisHandle);
				NativeMemory.Free((void*) FileDataPtr);
			}
		}
	}
}
