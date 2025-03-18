using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MoonWorks.Graphics;

namespace MoonWorks.Video
{
	/// <summary>
	/// A structure for continuous decoding of AV1 videos and rendering them into a texture.
	/// </summary>
	public unsafe class VideoPlayer : GraphicsResource
	{
		public Texture RenderTexture => renderTexture;
		private Texture renderTexture = null;

		public VideoState State { get; private set; } = VideoState.Stopped;
		public float PlaybackSpeed { get; set; } = 1;
		public bool Loaded => Video != null;
		public bool Ended => Stream.Ended;

		private VideoAV1 Video = null;
		private VideoAV1BufferStream Stream { get; }

		private TimeSpan timeAccumulator;
		private TimeSpan framerateTimestep;

		private Stopwatch timer;
		private long previousTicks = 0;

		private Task LoadTask = Task.CompletedTask;

		public VideoPlayer(GraphicsDevice device) : base(device)
		{
			Name = "VideoPlayer";
			Stream = new VideoAV1BufferStream(device);
		}

		/// <summary>
		/// Prepares a VideoAV1 for decoding and rendering.
		/// </summary>
		/// <param name="video"></param>
		public void Load(VideoAV1 video, bool loop)
		{
			if (Video == video)
			{
				return;
			}

			Unload();

			Video = video;

			framerateTimestep = TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / video.FramesPerSecond));
			timeAccumulator = TimeSpan.Zero;

			LoadTask = Stream.Load(Video.Storage, Video.Path, loop);
		}

		/// <summary>
		/// Starts playing back and decoding the loaded video.
		/// </summary>
		public void Play()
		{
			if (Video == null) { return; }

			if (State == VideoState.Playing)
			{
				return;
			}

			timeAccumulator = framerateTimestep; // play first frame always!
			State = VideoState.Playing;
		}

		/// <summary>
		/// Pauses playback and decoding of the currently playing video.
		/// </summary>
		public void Pause()
		{
			if (Video == null) { return; }

			if (State != VideoState.Playing)
			{
				return;
			}

			State = VideoState.Paused;
		}

		/// <summary>
		/// Stops and resets decoding of the currently playing video.
		/// </summary>
		public void Stop()
		{
			if (Video == null) { return; }

			if (State == VideoState.Stopped)
			{
				return;
			}

			timeAccumulator = TimeSpan.Zero;

			State = VideoState.Stopped;
		}

		/// <summary>
		/// Unloads the currently playing video.
		/// </summary>
		public void Unload()
		{
			if (Video == null)
			{
				return;
			}

			timeAccumulator = TimeSpan.Zero;

			State = VideoState.Stopped;

			Stream.Unload();

			Video = null;
		}

		/// <summary>
		/// Renders the video data into RenderTexture.
		/// </summary>
		public void Render(TimeSpan delta)
		{
			if (Video == null || State == VideoState.Stopped)
			{
				return;
			}

			// Wait for loading to actually be done
			LoadTask.Wait();

			if (State == VideoState.Playing)
			{
				timeAccumulator += delta * PlaybackSpeed;
			}

			while (timeAccumulator >= framerateTimestep)
			{
				if (Stream.TryGetBufferedFrame(out var newTexture))
				{
					if (renderTexture != null)
					{
						Stream.ReleaseFrame(renderTexture);
					}

					renderTexture = newTexture;
				}

				timeAccumulator -= framerateTimestep;
			}

			if (Stream.Ended && Stream.Loop)
			{
				Stream.Reset();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Unload();
					Stream.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
