using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public class StreamingSoundOgg : StreamingSoundSeekable
	{
		private IntPtr FileDataPtr = IntPtr.Zero;
		private IntPtr VorbisHandle = IntPtr.Zero;
		private FAudio.stb_vorbis_info Info;

		public override bool Loaded => VorbisHandle != IntPtr.Zero;
		private string FilePath;

		public unsafe static StreamingSoundOgg Create(AudioDevice device, string filePath)
		{
			var handle = FAudio.stb_vorbis_open_filename(filePath, out int error, IntPtr.Zero);
			if (error != 0)
			{
				Logger.LogError("Error: " + error);
				throw new AudioLoadException("Error opening ogg file!");
			}

			var info = FAudio.stb_vorbis_get_info(handle);

			var streamingSound = new StreamingSoundOgg(
				device,
				filePath,
				info
			);

			FAudio.stb_vorbis_close(handle);

			return streamingSound;
		}

		internal unsafe StreamingSoundOgg(
			AudioDevice device,
			string filePath,
			FAudio.stb_vorbis_info info,
			uint bufferSize = 32768
		) : base(
			device,
			3, /* float type */
			32, /* size of float */
			(ushort) (4 * info.channels),
			(ushort) info.channels,
			info.sample_rate,
			bufferSize,
			true
		) {
			Info = info;
			FilePath = filePath;
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.stb_vorbis_seek(VorbisHandle, sampleFrame);
		}

		public override unsafe void Load()
		{
			var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
			FileDataPtr = (nint) NativeMemory.Alloc((nuint) fileStream.Length);
			var fileDataSpan = new Span<byte>((void*) FileDataPtr, (int) fileStream.Length);
			fileStream.ReadExactly(fileDataSpan);
			fileStream.Close();

			VorbisHandle = FAudio.stb_vorbis_open_memory(FileDataPtr, fileDataSpan.Length, out int error, IntPtr.Zero);
			if (error != 0)
			{
				NativeMemory.Free((void*) FileDataPtr);
				Logger.LogError("Error opening OGG file!");
				Logger.LogError("Error: " + error);
				throw new AudioLoadException("Error opening OGG file!");
			}
		}

		public override unsafe void Unload()
		{
			if (Loaded)
			{
				FAudio.stb_vorbis_close(VorbisHandle);
				NativeMemory.Free((void*) FileDataPtr);

				VorbisHandle = IntPtr.Zero;
				FileDataPtr = IntPtr.Zero;
			}
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
	}
}
