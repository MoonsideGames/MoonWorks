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
		internal string Path { get; private init; }

		// One of these per Game class
		private VideoDevice VideoDevice { get; }

		public IntPtr Handle => handle;
		internal IntPtr handle;

		internal IntPtr ByteBuffer;

		public bool Loaded { get; internal set; }
		public bool Ended => Handle == IntPtr.Zero || (BufferedTextures.IsEmpty && Dav1dfile.df_eos(Handle) == 1);
		public double FramesPerSecond { get; set; }

		public bool Loop { get; private set; }

		public Texture RenderTexture => renderTexture;
		private Texture renderTexture = null;

		public VideoState State { get; private set; } = VideoState.Stopped;
		public float PlaybackSpeed { get; set; } = 1;

		private TimeSpan timeAccumulator;
		private TimeSpan framerateTimestep;

		public const int BUFFERED_FRAME_COUNT = 5;
		private ConcurrentQueue<Texture> AvailableTextures = [];
		private ConcurrentQueue<Texture> BufferedTextures = [];

		internal Texture yTexture = null;
		internal Texture uTexture = null;
		internal Texture vTexture = null;

		uint Width;
		uint Height;

		// if signaled, loading is complete.
		internal EventWaitHandle LoadWaitHandle;

		// Returns null if unsuccessful.
		public static VideoAV1 Create(GraphicsDevice graphicsDevice, VideoDevice videoDevice, TitleStorage storage, string filepath, double framesPerSecond)
		{
			if (!storage.GetFileSize(filepath, out var size))
			{
				Logger.LogError("Failed to open video file: " + filepath);
				return null;
			}

			var byteBuffer = (nint) NativeMemory.Alloc((nuint) size);
			var span = new Span<byte>((void*) byteBuffer, (int) size);
			if (!storage.ReadFile(filepath, span))
			{
				Logger.LogError("Failed to open video file: " + filepath);
				NativeMemory.Free((void*) byteBuffer);
				return null;
			}

			if (Dav1dfile.df_open_from_memory(byteBuffer, (uint) size, out var handle) == 0)
			{
				Logger.LogError("Failed to load video file: " + filepath);
				NativeMemory.Free((void*) byteBuffer);
				return null;
			}

			Dav1dfile.df_videoinfo(handle, out var videoWidth, out var videoHeight, out var pixelLayout);

			uint width = (uint) videoWidth;
			uint height = (uint) videoHeight;
			uint uvWidth;
			uint uvHeight;

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
				Dav1dfile.df_close(handle);
				NativeMemory.Free((void*) byteBuffer);
				return null;
			}

			return new VideoAV1(
				handle,
				byteBuffer,
				graphicsDevice,
				videoDevice,
				filepath,
				framesPerSecond,
				width,
				height,
				uvWidth,
				uvHeight);
		}

		private VideoAV1(
			IntPtr dav1dHandle,
			IntPtr byteBuffer,
			GraphicsDevice graphicsDevice,
			VideoDevice videoDevice,
			string filepath,
			double framesPerSecond,
			uint width,
			uint height,
			uint uvWidth,
			uint uvHeight
		) : base(graphicsDevice)
		{
			handle = dav1dHandle;
			ByteBuffer = byteBuffer;
			Name = "VideoPlayer";
			Path = filepath;
			VideoDevice = videoDevice;
			FramesPerSecond = framesPerSecond;

			yTexture = CreateSubTexture(Device, width, height);
			uTexture = CreateSubTexture(Device, uvWidth, uvHeight);
			vTexture = CreateSubTexture(Device, uvWidth, uvHeight);

			Width = width;
			Height = height;

			LoadWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
		}

		/// <summary>
		/// Prepares a VideoAV1 for decoding and rendering.
		/// </summary>
		public void Load(bool loop)
		{
			if (Loaded)
			{
				return;
			}

			for (var i = 0; i < BUFFERED_FRAME_COUNT - AvailableTextures.Count; i += 1)
			{
				AvailableTextures.Enqueue(Texture.Create2D(
					Device,
					Width,
					Height,
					TextureFormat.R8G8B8A8Unorm,
					TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
				));
			}

			framerateTimestep = TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / FramesPerSecond));
			timeAccumulator = TimeSpan.Zero;

			Loop = loop;

			LoadWaitHandle.Reset(); //de-signal
			VideoDevice.RegisterVideo(this);
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
		/// Unloads the currently playing video.
		/// </summary>
		/// <param name="freeTextures">
		/// Set this to true to free the render textures.
		/// </param>
		public void Unload(bool freeTextures)
		{
			timeAccumulator = TimeSpan.Zero;
			State = VideoState.Stopped;

			VideoDevice.UnregisterVideo(this);
			Loaded = false;

			if (Loaded)
			{
				while (BufferedTextures.TryDequeue(out var texture))
				{
					AvailableTextures.Enqueue(texture);
				}

				if (freeTextures)
				{
					while (AvailableTextures.TryDequeue(out var texture))
					{
						texture.Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Renders the video data into RenderTexture.
		/// </summary>
		public void Update(TimeSpan delta)
		{
			if (!Loaded || State == VideoState.Stopped)
			{
				return;
			}

			// Wait for loading to actually be done
			LoadWaitHandle.WaitOne();

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

		private static Texture CreateSubTexture(GraphicsDevice graphicsDevice, uint width, uint height)
		{
			return Texture.Create2D(
				graphicsDevice,
				width,
				height,
				TextureFormat.R8Unorm,
				TextureUsageFlags.Sampler
			);
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Dav1dfile.df_close(Handle);
				handle = IntPtr.Zero;
				NativeMemory.Free((void*) ByteBuffer);
				ByteBuffer = IntPtr.Zero;
			}
			base.Dispose(disposing);
		}
	}
}
