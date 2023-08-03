using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Contains raw audio data in the format specified by Format.
	/// Submit this to a SourceVoice to play audio.
	/// </summary>
	public class AudioBuffer : AudioResource
	{
		IntPtr BufferDataPtr;
		uint BufferDataLength;
		private bool OwnsBufferData;

		public Format Format { get; }

		public AudioBuffer(
			AudioDevice device,
			Format format,
			IntPtr bufferPtr,
			uint bufferLengthInBytes,
			bool ownsBufferData) : base(device)
		{
			Format = format;
			BufferDataPtr = bufferPtr;
			BufferDataLength = bufferLengthInBytes;
			OwnsBufferData = ownsBufferData;
		}

		/// <summary>
		/// Create another AudioBuffer from this audio buffer.
		/// It will not own the buffer data.
		/// </summary>
		/// <param name="offset">Offset in bytes from the top of the original buffer.</param>
		/// <param name="length">Length in bytes of the new buffer.</param>
		/// <returns></returns>
		public AudioBuffer Slice(int offset, uint length)
		{
			return new AudioBuffer(Device, Format, BufferDataPtr + offset, length, false);
		}

		/// <summary>
		/// Create an FAudioBuffer struct from this AudioBuffer.
		/// </summary>
		/// <param name="loop">Whether we should set the FAudioBuffer to loop.</param>
		public FAudio.FAudioBuffer ToFAudioBuffer(bool loop = false)
		{
			return new FAudio.FAudioBuffer
			{
				Flags = FAudio.FAUDIO_END_OF_STREAM,
				pContext = IntPtr.Zero,
				pAudioData = BufferDataPtr,
				AudioBytes = BufferDataLength,
				PlayBegin = 0,
				PlayLength = 0,
				LoopBegin = 0,
				LoopLength = 0,
				LoopCount = loop ? FAudio.FAUDIO_LOOP_INFINITE : 0
			};
		}

		protected override unsafe void Destroy()
		{
			if (OwnsBufferData)
			{
				NativeMemory.Free((void*) BufferDataPtr);
			}
		}
	}
}
