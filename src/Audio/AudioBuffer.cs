using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Audio
{
	/// <summary>
	/// Contains raw audio data in a specified Format. <br/>
	/// Submit this to a SourceVoice to play audio.
	/// </summary>
	public class AudioBuffer : AudioResource
	{
		IntPtr BufferDataPtr;
		public uint BufferDataLength { get; private set;}
		private bool OwnsBufferData;

		public Format Format { get; set; }

		/// <summary>
		/// Create a new empty AudioBuffer of a specified format.
		/// </summary>
		public static AudioBuffer Create(AudioDevice device, Format format)
		{
			return new AudioBuffer(device, format);
		}

		/// <summary>
		/// Create a new empty AudioBuffer with a format to be specified later.
		/// </summary>
		public static AudioBuffer Create(AudioDevice device)
		{
			return new AudioBuffer(device);
		}

		private AudioBuffer(AudioDevice device, Format format) : base(device)
		{
			Format = format;
		}

		private AudioBuffer(AudioDevice device) : base(device) { }

		/// <summary>
		/// Copies data from a ReadOnlySpan into the AudioBuffer.
		/// The AudioBuffer is then considered to own the data.
		/// </summary>
		/// <param name="offset">The offset of the AudioBuffer to copy data into.</param>
		public unsafe void SetData(ReadOnlySpan<byte> span, uint offset = 0)
		{
			OwnsBufferData = true;

			if (BufferDataLength < offset + span.Length)
			{
				BufferDataPtr = (nint) NativeMemory.Realloc((void*) BufferDataPtr, (nuint) (offset + span.Length));
				BufferDataLength = (uint) span.Length;
			}

			fixed (void* ptr = span)
			{
				NativeMemory.Copy(ptr, (void*)(BufferDataPtr + offset), (nuint)span.Length);
			}
		}

		/// <summary>
		/// Assigns a pointer to the AudioBuffer.
		/// If the AudioBuffer already owned data, it is freed.
		/// </summary>
		public unsafe void SetDataPointer(IntPtr bufferDataPtr, uint bufferDataLength, bool owns)
		{
			if (OwnsBufferData)
			{
				FreeBufferData();
			}

			OwnsBufferData = owns;
			BufferDataPtr = bufferDataPtr;
			BufferDataLength = bufferDataLength;
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
			var audioBuffer = Create(Device, Format);
			audioBuffer.SetDataPointer(BufferDataPtr + offset, length, false);
			return audioBuffer;
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

		private unsafe void FreeBufferData()
		{
			NativeMemory.Free((void*) BufferDataPtr);
		}

		protected override unsafe void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (OwnsBufferData)
				{
					FreeBufferData();
				}
			}
			base.Dispose(disposing);
		}
	}
}
