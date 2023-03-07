using System;
using MoonWorks.Audio;

namespace MoonWorks.Video
{
	public unsafe class StreamingSoundTheora : StreamingSound
	{
		private IntPtr VideoHandle;
		protected override int BUFFER_SIZE => 8192;
		// Theorafile is not thread safe, so let's update on the main thread.
		public override bool AutoUpdate => false;

		internal StreamingSoundTheora(
			AudioDevice device,
			IntPtr videoHandle,
			int channels,
			uint sampleRate
		) : base(
			device,
			3, /* float type */
			32, /* size of float */
			(ushort) (4 * channels),
			(ushort) channels,
			sampleRate
		) {
			VideoHandle = videoHandle;
		}

		protected override unsafe void FillBuffer(
			void* buffer,
			int bufferLengthInBytes,
			out int filledLengthInBytes,
			out bool reachedEnd
		) {
			var lengthInFloats = bufferLengthInBytes / sizeof(float);

			// FIXME: this gets gnarly with theorafile being not thread safe
			// is there some way we could just manually update in VideoPlayer
			// instead of going through AudioDevice?
			lock (Device.StateLock)
			{
				int samples = Theorafile.tf_readaudio(
					VideoHandle,
					(IntPtr) buffer,
					lengthInFloats
				);

				filledLengthInBytes = samples * sizeof(float);
				reachedEnd = Theorafile.tf_eos(VideoHandle) == 1;
			}
		}

		protected override void OnReachedEnd() { }
	}
}
