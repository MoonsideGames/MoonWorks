using System;
using System.IO;
using MoonWorks.Graphics;

namespace MoonWorks.Video
{
	/// <summary>
	/// This class takes in a filename for AV1 data in .obu (open bitstream unit) format
	/// </summary>
	public unsafe class VideoAV1 : GraphicsResource
	{
		public string Filename { get; }

		// "double buffering" so we can loop without a stutter
		internal VideoAV1Stream StreamA { get; }
		internal VideoAV1Stream StreamB { get; }

		public int Width => width;
		public int Height => height;
		public double FramesPerSecond { get; set; }
		public Dav1dfile.PixelLayout PixelLayout => pixelLayout;
		public int UVWidth { get; }
		public int UVHeight { get; }

		private int width;
		private int height;
		private Dav1dfile.PixelLayout pixelLayout;

		/// <summary>
		/// Opens an AV1 file so it can be loaded by VideoPlayer. You must also provide a playback framerate.
		/// </summary>
		public VideoAV1(GraphicsDevice device, string filename, double framesPerSecond) : base(device)
		{
			if (!File.Exists(filename))
			{
				throw new ArgumentException("Video file not found!");
			}

			if (Dav1dfile.df_fopen(filename, out var handle) == 0)
			{
				throw new Exception("Failed to open video file!");
			}

			Dav1dfile.df_videoinfo(handle, out width, out height, out pixelLayout);
			Dav1dfile.df_close(handle);

			if (pixelLayout == Dav1dfile.PixelLayout.I420)
			{
				UVWidth = Width / 2;
				UVHeight = Height / 2;
			}
			else if (pixelLayout == Dav1dfile.PixelLayout.I422)
			{
				UVWidth = Width / 2;
				UVHeight = Height;
			}
			else if (pixelLayout == Dav1dfile.PixelLayout.I444)
			{
				UVWidth = width;
				UVHeight = height;
			}
			else
			{
				throw new NotSupportedException("Unrecognized YUV format!");
			}

			FramesPerSecond = framesPerSecond;

			Filename = filename;

			StreamA = new VideoAV1Stream(device, this);
			StreamB = new VideoAV1Stream(device, this);
		}

		// NOTE: if you call this while a VideoPlayer is playing the stream, your program will explode
		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					StreamA.Dispose();
					StreamB.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
