﻿using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
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
		public bool Ended => Handle == IntPtr.Zero || (QueuedBuffers.IsEmpty && Dav1dfile.df_eos(Handle) == 1);
		public double FramesPerSecond { get; set; }

		public bool Loop { get; private set; }

		// This texture is owned by the VideoDevice.
		public Texture RenderTexture { get; internal set; }

		private YUVFramebuffer CurrentFrameBuffer;

		public VideoState State { get; private set; } = VideoState.Stopped;
		public float PlaybackSpeed { get; set; } = 1;

		private TimeSpan timeAccumulator;
		private TimeSpan framerateTimestep;

		public const int BUFFERED_FRAME_COUNT = 5;
		private ConcurrentQueue<YUVFramebuffer> AvailableBuffers = [];
		private ConcurrentQueue<YUVFramebuffer> QueuedBuffers = [];

		public uint Width { get; private set; }
		public uint Height { get; private set; }
		public uint UVWidth { get; private set; }
		public uint UVHeight { get; private set; }

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

			Width = width;
			Height = height;
			UVWidth = uvWidth;
			UVHeight = uvHeight;

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

			for (var i = 0; i < BUFFERED_FRAME_COUNT - AvailableBuffers.Count; i += 1)
			{
				AvailableBuffers.Enqueue(new YUVFramebuffer());
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
		public void Unload()
		{
			timeAccumulator = TimeSpan.Zero;
			State = VideoState.Stopped;

			VideoDevice.UnregisterVideo(this);
			Loaded = false;

			if (Loaded)
			{
				while (QueuedBuffers.TryDequeue(out var texture))
				{
					AvailableBuffers.Enqueue(texture);
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
				if (TryGetFramebuffer(out var newFramebuffer))
				{
					if (CurrentFrameBuffer != null)
					{
						ReleaseFramebuffer(CurrentFrameBuffer);
					}

					CurrentFrameBuffer = newFramebuffer;

					// now that we have a new framebuffer, render it
					RenderTexture = VideoDevice.RenderFrame(CurrentFrameBuffer);
				}

				timeAccumulator -= framerateTimestep;
			}
		}

		internal int BufferedFrameCount()
		{
			return QueuedBuffers.Count;
		}

		internal YUVFramebuffer AcquireFramebuffer()
		{
			if (!AvailableBuffers.TryDequeue(out var framebuffer))
			{
				// FIXME: this should never happen?
				return new YUVFramebuffer();
			}

			return framebuffer;
		}

		/// <summary>
		/// Obtains a frame buffer container. Returns false if none are available.
		/// You must call ReleaseFrame eventually or the texture will leak.
		/// </summary>
		/// <returns>True if frame is available, otherwise false.</returns>
		private bool TryGetFramebuffer(out YUVFramebuffer buffer)
		{
			if (QueuedBuffers.Count > 0)
			{
				bool success = QueuedBuffers.TryDequeue(out buffer);
				return success;
			}

			buffer = default;
			return false;
		}

		/// <summary>
		/// Releases a buffered frame that was previously obtained by TryGetBufferedFrame.
		/// </summary>
		private void ReleaseFramebuffer(YUVFramebuffer framebuffer)
		{
			AvailableBuffers.Enqueue(framebuffer);
		}

		internal void EnqueueFramebuffer(YUVFramebuffer framebuffer)
		{
			QueuedBuffers.Enqueue(framebuffer);
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				Dav1dfile.df_close(Handle);
				handle = IntPtr.Zero;
				NativeMemory.Free((void*) ByteBuffer);
				ByteBuffer = IntPtr.Zero;

				while (QueuedBuffers.TryDequeue(out var queuedBuffer))
				{
					queuedBuffer.Dispose();
				}

				while (AvailableBuffers.TryDequeue(out var availableBuffer))
				{
					availableBuffer.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
