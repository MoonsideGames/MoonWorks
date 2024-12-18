using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Use this in conjunction with a StreamingVoice to play back streaming audio data.
	/// </summary>
	public abstract class AudioDataStreamable : AudioResource
	{
		public Format Format { get; protected set; }
		public abstract bool Loaded { get; }
		public abstract uint DecodeBufferSize { get; }

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
		public unsafe void Open(string filePath)
		{
			using var filestream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var bytes = NativeMemory.Alloc((nuint) filestream.Length);
			var span = new Span<byte>(bytes, (int) filestream.Length);
			filestream.ReadExactly(span);

			Open(span);

			NativeMemory.Free(bytes);
		}

		/// <summary>
		/// Unloads the raw audio data from memory.
		/// </summary>
		public abstract void Close();

		/// <summary>
		/// Seeks to the given sample frame.
		/// </summary>
		public abstract void Seek(uint sampleFrame);

		/// <summary>
		/// Attempts to decodes data of length bufferLengthInBytes into the provided buffer.
		/// </summary>
		/// <param name="buffer">The buffer that decoded bytes will be placed into.</param>
		/// <param name="bufferLengthInBytes">Requested length of decoded audio data.</param>
		/// <param name="filledLengthInBytes">How much data was actually filled in by the decode.</param>
		/// <param name="reachedEnd">Whether the end of the data was reached on this decode.</param>
		public abstract unsafe void Decode(void* buffer, int bufferLengthInBytes, out int filledLengthInBytes, out bool reachedEnd);

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
