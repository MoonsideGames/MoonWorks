using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	public class StreamingSoundOgg : StreamingSound
	{
		// FIXME: what should this value be?
		public const int BUFFER_SIZE = 1024 * 128;

		private IntPtr VorbisHandle;
		private IntPtr FileDataPtr;
		private FAudio.stb_vorbis_info Info;

		private readonly float[] buffer; // currently decoded bytes

		public override SoundState State { get; protected set; }

		public static StreamingSoundOgg Load(AudioDevice device, string filePath)
		{
			var fileData = File.ReadAllBytes(filePath);
			var fileDataPtr = Marshal.AllocHGlobal(fileData.Length);
			Marshal.Copy(fileData, 0, fileDataPtr, fileData.Length);
			var vorbisHandle = FAudio.stb_vorbis_open_memory(fileDataPtr, fileData.Length, out int error, IntPtr.Zero);
			if (error != 0)
			{
				((GCHandle) fileDataPtr).Free();
				Logger.LogError("Error opening OGG file!");
				Logger.LogError("Error: " + error);
				throw new AudioLoadException("Error opening OGG file!");
			}
			var info = FAudio.stb_vorbis_get_info(vorbisHandle);

			return new StreamingSoundOgg(
				device,
				fileDataPtr,
				vorbisHandle,
				info
			);
		}

		internal StreamingSoundOgg(
			AudioDevice device,
			IntPtr fileDataPtr, // MUST BE AN ALLOCHGLOBAL HANDLE!!
			IntPtr vorbisHandle,
			FAudio.stb_vorbis_info info
		) : base(
			device,
			3, /* float type */
			32, /* size of float */
			(ushort) (4 * info.channels),
			(ushort) info.channels,
			info.sample_rate
		)
		{
			FileDataPtr = fileDataPtr;
			VorbisHandle = vorbisHandle;
			Info = info;
			buffer = new float[BUFFER_SIZE];

			device.AddDynamicSoundInstance(this);
		}

		private void PerformSeek(uint sampleFrame)
		{
			if (State == SoundState.Playing)
			{
				FAudio.FAudioSourceVoice_Stop(Handle, 0, 0);
			}

			FAudio.FAudioSourceVoice_FlushSourceBuffers(Handle);
			ClearBuffers();
			FAudio.stb_vorbis_seek(VorbisHandle, sampleFrame);
			QueueBuffers();

			if (State == SoundState.Playing)
			{
				Play();
			}
		}

		public override void Seek(float seconds)
		{
			uint sampleFrame = (uint) (Info.sample_rate * seconds);
			PerformSeek(sampleFrame);
		}

		public override void Seek(uint sampleFrame)
		{
			PerformSeek(sampleFrame);
		}

		protected override void AddBuffer(
			out float[] buffer,
			out uint bufferOffset,
			out uint bufferLength,
			out bool reachedEnd
		)
		{
			buffer = this.buffer;

			/* NOTE: this function returns samples per channel, not total samples */
			var samples = FAudio.stb_vorbis_get_samples_float_interleaved(
				VorbisHandle,
				Info.channels,
				buffer,
				buffer.Length
			);

			var sampleCount = samples * Info.channels;
			bufferOffset = 0;
			bufferLength = (uint) sampleCount;
			reachedEnd = sampleCount < buffer.Length;
		}

		protected override void SeekStart()
		{
			FAudio.stb_vorbis_seek_start(VorbisHandle);
		}

		protected override void Destroy()
		{
			FAudio.stb_vorbis_close(VorbisHandle);
			Marshal.FreeHGlobal(FileDataPtr);
		}
	}
}
