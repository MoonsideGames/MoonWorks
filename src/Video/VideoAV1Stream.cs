using System;
using MoonWorks.Graphics;

namespace MoonWorks.Video
{
	internal class VideoAV1Stream : GraphicsResource
	{
		public IntPtr Handle => handle;
		IntPtr handle;

		public bool Loaded => handle != IntPtr.Zero;
		public bool Ended => Dav1dfile.df_eos(Handle) == 1;

		public IntPtr yDataHandle;
		public IntPtr uDataHandle;
		public IntPtr vDataHandle;
		public uint yDataLength;
		public uint uvDataLength;
		public uint yStride;
		public uint uvStride;

		public bool FrameDataUpdated { get; set; }

		private VideoAV1 Parent;

		public VideoAV1Stream(GraphicsDevice device, VideoAV1 video) : base(device)
		{
			handle = IntPtr.Zero;
			Parent = video;
		}

		public void Load()
		{
			if (!Loaded)
			{
				if (Dav1dfile.df_fopen(Parent.Filename, out handle) == 0)
				{
					throw new Exception("Failed to load video file!");
				}

				Reset();
			}
		}

		public void Unload()
		{
			if (Loaded)
			{
				Dav1dfile.df_close(handle);
				handle = IntPtr.Zero;
			}
		}

		public void Reset()
		{
			lock (this)
			{
				Dav1dfile.df_reset(handle);
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
						handle,
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
				Unload();
			}
			base.Dispose(disposing);
		}
	}
}
