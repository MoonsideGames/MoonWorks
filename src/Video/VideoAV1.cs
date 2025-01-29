using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Storage;

namespace MoonWorks.Video
{
	/// <summary>
	/// This class takes in a filename for AV1 data in .obu (open bitstream unit) format
	/// </summary>
	public unsafe class VideoAV1 : GraphicsResource
	{
		internal TitleStorage Storage { get; private init; }
		internal string Path { get; private init; }

		public int Width { get; private init; }
		public int Height { get; private init; }
		public Dav1dfile.PixelLayout PixelLayout { get; private init; }
		public int UVWidth { get; private init; }
		public int UVHeight { get; private init; }
		public double FramesPerSecond { get; set; }

		/// <summary>
		/// Opens an AV1 file so it can be loaded by VideoPlayer. You must also provide a playback framerate.
		/// </summary>
		public static VideoAV1 Create(GraphicsDevice device, TitleStorage storage, string path, double framesPerSecond)
		{
			var videoBytes = storage.ReadFile(path, out var size);
			if (videoBytes == null)
			{
				Logger.LogError("Video file not found!");
				return null;
			}

			if (Dav1dfile.df_open_from_memory((nint) videoBytes, (uint) size, out var handle) == 0)
			{
				Logger.LogError("Failed to open video file!");
				return null;
			}

			Dav1dfile.df_videoinfo(handle, out var width, out var height, out var pixelLayout);
			Dav1dfile.df_close(handle);

			NativeMemory.Free(videoBytes);

			int uvWidth;
			int uvHeight;

			if (pixelLayout == Dav1dfile.PixelLayout.I420)
			{
				uvWidth = width / 2;
				uvHeight = height / 2;
			}
			else if (pixelLayout == Dav1dfile.PixelLayout.I422)
			{
				uvWidth = width / 2;
				uvHeight = height;
			}
			else if (pixelLayout == Dav1dfile.PixelLayout.I444)
			{
				uvWidth = width;
				uvHeight = height;
			}
			else
			{
				Logger.LogError("Failed to load video: unrecognized YUV format!");
				return null;
			}

			return new VideoAV1(device)
			{
				Storage = storage,
				Path = path,
				Name = System.IO.Path.GetFileNameWithoutExtension(path),
				Width = width,
				Height = height,
				PixelLayout = pixelLayout,
				UVWidth = uvWidth,
				UVHeight = uvHeight,
				FramesPerSecond = framesPerSecond
			};
		}

		private VideoAV1(GraphicsDevice device) : base(device) { }
	}
}
