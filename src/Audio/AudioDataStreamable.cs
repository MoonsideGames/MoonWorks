using System;
using System.Runtime.InteropServices;
using MoonWorks.Storage;

namespace MoonWorks.Audio
{
	/// <summary>
	/// A streaming audio source decoded from compressed audio data.
	/// </summary>
	public abstract class AudioDataStreamable : StreamingAudioSource
	{
		protected AudioDataStreamable(AudioDevice device) : base(device)
		{
		}

		/// <summary>
		/// Loads raw audio data into memory to prepare it for stream decoding.
		/// </summary>
		public abstract void Open(ReadOnlySpan<byte> data);

		/// <summary>
		/// Loads raw audio data from a file into memory to prepare it for stream decoding.
		/// </summary>
		public unsafe void Open(TitleStorage storage, string filePath)
		{
			if (!storage.GetFileSize(filePath, out var size))
			{
				return;
			}

			var buffer = NativeMemory.Alloc((nuint) size);
			var span = new Span<byte>(buffer, (int) size);
			Open(span);
			NativeMemory.Free(buffer);
		}

		/// <summary>
		/// Unloads the raw audio data from memory.
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// Seeks to the given sample frame.
		/// </summary>
		public abstract void Seek(uint sampleFrame);

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Close();
			}
			base.Dispose(disposing);
		}
	}
}
