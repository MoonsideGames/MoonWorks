using System;
using MoonWorks.Graphics;

namespace MoonWorks.Video
{
	internal class VideoAV1Stream : GraphicsResource
	{
		public IntPtr Handle => handle;
		IntPtr handle;

		public bool Ended => Dav1dfile.df_eos(Handle) == 1;

		public IntPtr yDataHandle;
		public IntPtr uDataHandle;
		public IntPtr vDataHandle;
		public uint yDataLength;
		public uint uvDataLength;
		public uint yStride;
		public uint uvStride;

		public bool FrameDataUpdated { get; set; }

		public VideoAV1Stream(GraphicsDevice device, VideoAV1 video) : base(device)
		{
			if (Dav1dfile.df_fopen(video.Filename, out handle) == 0)
			{
				throw new Exception("Failed to open video file!");
			}

			Reset();
		}

		public void Reset()
		{
			lock (this)
			{
				Dav1dfile.df_reset(Handle);
				ReadNextFrame();
			}
		}

		public void ReadNextFrame()
		{
			lock (this)
			{
				if (!Ended)
				{
					if (Dav1dfile.df_readvideo(
						Handle,
						1,
						out var yDataHandle,
						out var uDataHandle,
						out var vDataHandle,
						out var yDataLength,
						out var uvDataLength,
						out var yStride,
						out var uvStride) == 1
					) {
						this.yDataHandle = yDataHandle;
						this.uDataHandle = uDataHandle;
						this.vDataHandle = vDataHandle;
						this.yDataLength = yDataLength;
						this.uvDataLength = uvDataLength;
						this.yStride = yStride;
						this.uvStride = uvStride;

						FrameDataUpdated = true;
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Dav1dfile.df_close(Handle);
			}
			base.Dispose(disposing);
		}
	}
}
