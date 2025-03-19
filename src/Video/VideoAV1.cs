using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MoonWorks.Graphics;
using MoonWorks.Storage;

namespace MoonWorks.Video
{
	/// <summary>
	/// A structure for continuous decoding of AV1 videos and rendering them into a texture.
	/// </summary>
	public unsafe class VideoAV1 : GraphicsResource
	{
		internal TitleStorage Storage { get; private init; }
		internal string Path { get; private init; }

		// One of these per Game class
		private VideoAV1BufferStream Stream { get; }

		public IntPtr Handle => handle;
		internal IntPtr handle;

		internal IntPtr ByteBuffer;

		public bool Loaded => handle != IntPtr.Zero;
		public bool Ended => Handle == IntPtr.Zero || Dav1dfile.df_eos(Handle) == 1;
		public double FramesPerSecond { get; set; }

		public bool Loop { get; private set; }

		public Texture RenderTexture => renderTexture;
		private Texture renderTexture = null;

		public VideoState State { get; private set; } = VideoState.Stopped;
		public float PlaybackSpeed { get; set; } = 1;

		private TimeSpan timeAccumulator;
		private TimeSpan framerateTimestep;

		private Task LoadTask = Task.CompletedTask;

		public const int BUFFERED_FRAME_COUNT = 5;
		private ConcurrentQueue<Texture> AvailableTextures = [];
		private ConcurrentQueue<Texture> BufferedTextures = [];

		internal Texture yTexture = null;
		internal Texture uTexture = null;
		internal Texture vTexture = null;

		uint Width;
		uint Height;

		public VideoAV1(GraphicsDevice device, VideoAV1BufferStream stream, TitleStorage storage, string filepath, double framesPerSecond) : base(device)
		{
			Name = "VideoPlayer";
			Storage = storage;
			Path = filepath;
			Stream = stream;
			FramesPerSecond = framesPerSecond;
		}

		/// <summary>
		/// Prepares a VideoAV1 for decoding and rendering.
		/// </summary>
		/// <param name="video"></param>
		public void Load(bool loop)
		{
			LoadTask.Wait(); // if we're still loading, wait first

			Unload();

			framerateTimestep = TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / FramesPerSecond));
			timeAccumulator = TimeSpan.Zero;

			Loop = loop;

			LoadTask = Task.Run(LoadHelper);
		}

		private void LoadHelper()
		{
			// FIXME: should the storage be on the video object?
			if (!Storage.GetFileSize(Path, out var size))
			{
				Logger.LogError("Failed to open video file: " + Path);
				return;
			}

			ByteBuffer = (nint) NativeMemory.Alloc((nuint) size);
			var span = new Span<byte>((void*) ByteBuffer, (int) size);
			if (!Storage.ReadFile(Path, span))
			{
				Logger.LogError("Failed to open video file: " + Path);
				NativeMemory.Free((void*) ByteBuffer);
				return;
			}

			if (Dav1dfile.df_open_from_memory(ByteBuffer, (uint) size, out handle) == 0)
			{
				Logger.LogError("Failed to load video file: " + Path);
				NativeMemory.Free((void*) ByteBuffer);
				return;
			}

			Dav1dfile.df_videoinfo(Handle, out var width, out var height, out var pixelLayout);

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
				Unload();
				return;
			}

			for (var i = 0; i < BUFFERED_FRAME_COUNT; i += 1)
			{
				AvailableTextures.Enqueue(Texture.Create2D(
					Device,
					(uint) width,
					(uint) height,
					TextureFormat.R8G8B8A8Unorm,
					TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
				));
			}

			if (yTexture == null || yTexture.Width != width || yTexture.Height != height)
			{
				yTexture?.Dispose();
				yTexture = CreateSubTexture(Device, width, height);
			}

			if (uTexture == null || uTexture.Width != uvWidth || uTexture.Height != uvHeight)
			{
				uTexture?.Dispose();
				uTexture = CreateSubTexture(Device, uvWidth, uvHeight);
			}

			if (vTexture == null || vTexture.Width != uvWidth || vTexture.Height != uvHeight)
			{
				vTexture?.Dispose();
				vTexture = CreateSubTexture(Device, uvWidth, uvHeight);
			}

			Width = (uint) width;
			Height = (uint) height;

			Stream.RegisterVideo(this);

			// FIXME: jaaaaaank
			// wait until 3 frames are buffered by the video thread
			while (BufferedFrameCount() < 3)
			{
				Thread.Sleep(1);
			}
		}

		/// <summary>
		/// Starts playing back and decoding the loaded video.
		/// </summary>
		public void Play()
		{
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
			timeAccumulator = TimeSpan.Zero;

			State = VideoState.Stopped;

			Stream.UnregisterVideo(this);

			if (Loaded)
			{
				Dav1dfile.df_close(Handle);
				handle = IntPtr.Zero;
				NativeMemory.Free((void*) ByteBuffer);
				ByteBuffer = IntPtr.Zero;

				while (BufferedTextures.TryDequeue(out var texture))
				{
					AvailableTextures.Enqueue(texture);
				}
			}
		}

		/// <summary>
		/// Renders the video data into RenderTexture.
		/// </summary>
		public void Update(TimeSpan delta)
		{
			// Wait for loading to actually be done
			LoadTask.Wait();

			if (!Loaded || State == VideoState.Stopped)
			{
				return;
			}

			if (State == VideoState.Playing)
			{
				timeAccumulator += delta * PlaybackSpeed;
			}

			while (timeAccumulator >= framerateTimestep)
			{
				if (TryGetBufferedFrame(out var newTexture))
				{
					if (renderTexture != null)
					{
						ReleaseFrame(renderTexture);
					}

					renderTexture = newTexture;
				}

				timeAccumulator -= framerateTimestep;
			}
		}

		internal int BufferedFrameCount()
		{
			return BufferedTextures.Count;
		}

		internal Texture AcquireTexture()
		{
			if (!AvailableTextures.TryDequeue(out var texture))
			{
				return Texture.Create2D(
					Device,
					Width,
					Height,
					TextureFormat.R8G8B8A8Unorm,
					TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
				);
			}
			return texture;
		}

		/// <summary>
		/// Obtains a buffered frame. Returns false if no frames are available.
		/// If successful, this method also enqueues a new buffered frame.
		/// You must call ReleaseFrame eventually or the texture will leak.
		/// </summary>
		/// <returns>True if frame is available, otherwise false.</returns>
		private bool TryGetBufferedFrame(out Texture texture)
		{
			texture = null;

			if (BufferedTextures.Count > 0)
			{
				bool success = BufferedTextures.TryDequeue(out texture);
				return success;
			}

			return false;
		}

		/// <summary>
		/// Releases a buffered frame that was previously obtained by TryGetBufferedFrame.
		/// </summary>
		private void ReleaseFrame(Texture texture)
		{
			if (texture == null) { return; }
			AvailableTextures.Enqueue(texture);
		}

		internal void BufferFrame(Texture texture)
		{
			BufferedTextures.Enqueue(texture);
		}

		private static Texture CreateSubTexture(GraphicsDevice graphicsDevice, int width, int height)
		{
			return Texture.Create2D(
				graphicsDevice,
				(uint) width,
				(uint) height,
				TextureFormat.R8Unorm,
				TextureUsageFlags.Sampler
			);
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
