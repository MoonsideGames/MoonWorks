/* Heavily based on https://github.com/FNA-XNA/FNA/blob/master/src/Media/Xiph/VideoPlayer.cs */
using System;
using System.IO;
using System.Runtime.InteropServices;
using SDL2;

namespace MoonWorks.Video
{
	public enum VideoState
	{
		Playing,
		Paused,
		Stopped
	}

	public unsafe class Video : IDisposable
	{
		internal IntPtr Handle;
		private IntPtr rwData;
		private void* videoData;
		private int videoDataLength;

		public double FramesPerSecond => fps;
		public int Width => yWidth;
		public int Height => yHeight;
		public int UVWidth { get; }
		public int UVHeight { get; }

		private double fps;
		private int yWidth;
		private int yHeight;

		private bool IsDisposed;

		public Video(string filename)
		{
			if (!File.Exists(filename))
			{
				throw new ArgumentException("Video file not found!");
			}

			var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
			videoDataLength = (int) fileStream.Length;
			videoData = NativeMemory.Alloc((nuint) videoDataLength);
			var fileBufferSpan = new Span<byte>(videoData, videoDataLength);
			fileStream.ReadExactly(fileBufferSpan);
			fileStream.Close();

			rwData = SDL.SDL_RWFromMem((IntPtr) videoData, videoDataLength);
			if (Theorafile.tf_open_callbacks(rwData, out Handle, callbacks) < 0)
			{
				throw new ArgumentException("Invalid video file!");
			}

			Theorafile.th_pixel_fmt format;
			Theorafile.tf_videoinfo(
				Handle,
				out yWidth,
				out yHeight,
				out fps,
				out format
			);

			if (format == Theorafile.th_pixel_fmt.TH_PF_420)
			{
				UVWidth = Width / 2;
				UVHeight = Height / 2;
			}
			else if (format == Theorafile.th_pixel_fmt.TH_PF_422)
			{
				UVWidth = Width / 2;
				UVHeight = Height;
			}
			else if (format == Theorafile.th_pixel_fmt.TH_PF_444)
			{
				UVWidth = Width;
				UVHeight = Height;
			}
			else
			{
				throw new NotSupportedException("Unrecognized YUV format!");
			}
		}

		private static IntPtr Read(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr datasource) => (IntPtr) SDL2.SDL.SDL_RWread(datasource, ptr, size, nmemb);
		private static int Seek(IntPtr datasource, long offset, Theorafile.SeekWhence whence) => (int) SDL2.SDL.SDL_RWseek(datasource, offset, (int) whence);
		private static int Close(IntPtr datasource) => (int) SDL2.SDL.SDL_RWclose(datasource);

		private static Theorafile.tf_callbacks callbacks = new Theorafile.tf_callbacks
		{
			read_func = Read,
			seek_func = Seek,
			close_func = Close
		};

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
				}

				// free unmanaged resources (unmanaged objects)
				Theorafile.tf_close(ref Handle);
				SDL.SDL_RWclose(rwData);
				NativeMemory.Free(videoData);

				IsDisposed = true;
			}
		}

		~Video()
		{
		    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		    Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
