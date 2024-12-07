using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Streamable audio in Ogg format.
	/// </summary>
	public class AudioDataOgg : AudioDataStreamable
	{
		private IntPtr FileDataPtr = IntPtr.Zero;
		private IntPtr VorbisHandle = IntPtr.Zero;

		private string FilePath;

		public override bool Loaded => VorbisHandle != IntPtr.Zero;
		public override uint DecodeBufferSize => 32768;

		public AudioDataOgg(AudioDevice device, string filePath) : base(device)
		{
			FilePath = filePath;

			var handle = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

			if (error != 0)
			{
				throw new InvalidOperationException("Error loading file!");
			}

			var info = FAudio.stb_vorbis_get_info(handle);

			Format = new Format
			{
				Tag = FormatTag.IEEE_FLOAT,
				BitsPerSample = 32,
				Channels = (ushort) info.channels,
				SampleRate = info.sample_rate
			};

			FAudio.stb_vorbis_close(handle);
		}

		public override unsafe void Decode(void* buffer, int bufferLengthInBytes, out int filledLengthInBytes, out bool reachedEnd)
		{
			var lengthInFloats = bufferLengthInBytes / sizeof(float);

			/* NOTE: this function returns samples per channel, not total samples */
			var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
				VorbisHandle,
				Format.Channels,
				(IntPtr) buffer,
				lengthInFloats
			);

			var sampleCount = samples * Format.Channels;
			reachedEnd = sampleCount < lengthInFloats;
			filledLengthInBytes = sampleCount * sizeof(float);
		}

		/// <summary>
		/// Prepares the Ogg data for streaming.
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

				VorbisHandle = FAudio.stb_vorbis_open_memory(FileDataPtr, fileDataSpan.Length, out int error, IntPtr.Zero);
				if (error != 0)
				{
					NativeMemory.Free((void*) FileDataPtr);
					Logger.LogError("Error opening OGG file!");
					Logger.LogError("Error: " + error);
					throw new InvalidOperationException("Error opening OGG file!");
				}
			}
		}

		public override void Seek(uint sampleFrame)
		{
			FAudio.stb_vorbis_seek(VorbisHandle, sampleFrame);
		}

		/// <summary>
		/// Unloads the Ogg data, freeing resources.
		/// </summary>
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

		/// <summary>
		/// Loads an entire ogg file into an AudioBuffer. Useful for static audio.
		/// </summary>
		public static unsafe AudioBuffer CreateBuffer(AudioDevice device, string filePath)
		{
			var filePointer = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

			if (error != 0)
			{
				throw new InvalidOperationException("Error loading file!");
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

			var format = new Format
			{
				Tag = FormatTag.IEEE_FLOAT,
				BitsPerSample = 32,
				Channels = (ushort) info.channels,
				SampleRate = info.sample_rate
			};

			var audioBuffer = AudioBuffer.Create(device, format);
			audioBuffer.SetDataPointer((nint) buffer, (uint) lengthInBytes, true);
			return audioBuffer;
		}
	}
}
