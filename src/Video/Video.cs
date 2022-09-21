/* Heavily based on https://github.com/FNA-XNA/FNA/blob/master/src/Media/Xiph/VideoPlayer.cs */
using System;
using System.Runtime.InteropServices;

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

		public double FramesPerSecond => fps;
		public int Width => yWidth;
		public int Height => yHeight;
		public int UVWidth { get; }
		public int UVHeight { get; }

		private double fps;
		private int yWidth;
		private int yHeight;

		private bool disposed;

		public Video(string filename)
		{
			if (!System.IO.File.Exists(filename))
			{
				throw new ArgumentException("Video file not found!");
			}

			var bytes = System.IO.File.ReadAllBytes(filename);
			videoData = NativeMemory.Alloc((nuint) bytes.Length);
			Marshal.Copy(bytes, 0, (IntPtr) videoData, bytes.Length);
			rwData = SDL2.SDL.SDL_RWFromMem((IntPtr) videoData, bytes.Length);

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
			if (!disposed)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
				}

				// free unmanaged resources (unmanaged objects)
				Theorafile.tf_close(ref Handle);
				NativeMemory.Free(videoData);

				disposed = true;
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
