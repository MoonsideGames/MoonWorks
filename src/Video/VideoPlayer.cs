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
		public Texture RenderTexture { get; private set; } = null;
		public VideoState State { get; private set; } = VideoState.Stopped;
		public bool Loop { get; set; }
		public float PlaybackSpeed { get; set; } = 1;

		private VideoAV1 Video = null;
		private VideoAV1Stream CurrentStream = null;

		private Task ReadNextFrameTask;
		private Task ResetStreamATask;
		private Task ResetStreamBTask;

		private Texture yTexture = null;
		private Texture uTexture = null;
		private Texture vTexture = null;
		private Sampler LinearSampler;

		private TransferBuffer TransferBuffer;

		private int currentFrame;

		private Stopwatch timer;
		private double lastTimestamp;
		private double timeElapsed;

		public VideoPlayer(GraphicsDevice device) : base(device)
		{
			LinearSampler = new Sampler(device, SamplerCreateInfo.LinearClamp);

			timer = new Stopwatch();
		}

		/// <summary>
		/// Prepares a VideoAV1 for decoding and rendering.
		/// </summary>
		/// <param name="video"></param>
		public void Load(VideoAV1 video)
		{
			if (Video != video)
			{
				Stop();

				if (RenderTexture == null)
				{
					RenderTexture = CreateRenderTexture(Device, video.Width, video.Height);
				}

				if (yTexture == null)
				{
					yTexture = CreateSubTexture(Device, video.Width, video.Height);
				}

				if (uTexture == null)
				{
					uTexture = CreateSubTexture(Device, video.UVWidth, video.UVHeight);
				}

				if (vTexture == null)
				{
					vTexture = CreateSubTexture(Device, video.UVWidth, video.UVHeight);
				}

				if (video.Width != RenderTexture.Width || video.Height != RenderTexture.Height)
				{
					RenderTexture.Dispose();
					RenderTexture = CreateRenderTexture(Device, video.Width, video.Height);
				}

				if (video.Width != yTexture.Width || video.Height != yTexture.Height)
				{
					yTexture.Dispose();
					yTexture = CreateSubTexture(Device, video.Width, video.Height);
				}

				if (video.UVWidth != uTexture.Width || video.UVHeight != uTexture.Height)
				{
					uTexture.Dispose();
					uTexture = CreateSubTexture(Device, video.UVWidth, video.UVHeight);
				}

				if (video.UVWidth != vTexture.Width || video.UVHeight != vTexture.Height)
				{
					vTexture.Dispose();
					vTexture = CreateSubTexture(Device, video.UVWidth, video.UVHeight);
				}

				Video = video;

				InitializeDav1dStream();
			}
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

			timer.Start();

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

			timer.Stop();

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

			timer.Stop();
			timer.Reset();

			lastTimestamp = 0;
			timeElapsed = 0;

			InitializeDav1dStream();

			State = VideoState.Stopped;
		}

		/// <summary>
		/// Unloads the currently playing video.
		/// </summary>
		public void Unload()
		{
			Stop();
			ResetStreamATask?.Wait();
			ResetStreamBTask?.Wait();
			Video = null;
		}

		/// <summary>
		/// Renders the video data into RenderTexture.
		/// </summary>
		public void Render()
		{
			if (Video == null || State == VideoState.Stopped)
			{
				return;
			}

			timeElapsed += (timer.Elapsed.TotalMilliseconds - lastTimestamp) * PlaybackSpeed;
			lastTimestamp = timer.Elapsed.TotalMilliseconds;

			int thisFrame = ((int) (timeElapsed / (1000.0 / Video.FramesPerSecond)));
			if (thisFrame > currentFrame)
			{
				if (CurrentStream.FrameDataUpdated)
				{
					UpdateRenderTexture();
					CurrentStream.FrameDataUpdated = false;
				}

				currentFrame = thisFrame;
				ReadNextFrameTask = Task.Run(CurrentStream.ReadNextFrame);
				ReadNextFrameTask.ContinueWith(HandleTaskException, TaskContinuationOptions.OnlyOnFaulted);
			}

			if (CurrentStream.Ended)
			{
				timer.Stop();
				timer.Reset();

				var task = Task.Run(CurrentStream.Reset);
				task.ContinueWith(HandleTaskException, TaskContinuationOptions.OnlyOnFaulted);

				if (CurrentStream == Video.StreamA)
				{
					ResetStreamATask = task;
				}
				else
				{
					ResetStreamBTask = task;
				}

				if (Loop)
				{
					// Start over on the next stream!
					CurrentStream = (CurrentStream == Video.StreamA) ? Video.StreamB : Video.StreamA;
					currentFrame = -1;
					timer.Start();
				}
				else
				{
					State = VideoState.Stopped;
				}
			}
		}

		private void UpdateRenderTexture()
		{
			lock (CurrentStream)
			{
				var commandBuffer = Device.AcquireCommandBuffer();

				var ySpan = new Span<byte>((void*) CurrentStream.yDataHandle, (int) CurrentStream.yDataLength);
				var uSpan = new Span<byte>((void*) CurrentStream.uDataHandle, (int) CurrentStream.uvDataLength);
				var vSpan = new Span<byte>((void*) CurrentStream.vDataHandle, (int) CurrentStream.uvDataLength);

				if (TransferBuffer == null || TransferBuffer.Size < ySpan.Length + uSpan.Length + vSpan.Length)
				{
					TransferBuffer?.Dispose();
					TransferBuffer = new TransferBuffer(Device, (uint) (ySpan.Length + uSpan.Length + vSpan.Length));
				}
				TransferBuffer.SetData(ySpan, 0, TransferOptions.Cycle);
				TransferBuffer.SetData(uSpan, (uint) ySpan.Length, TransferOptions.Overwrite);
				TransferBuffer.SetData(vSpan, (uint) (ySpan.Length + uSpan.Length), TransferOptions.Overwrite);

				commandBuffer.BeginCopyPass();

				commandBuffer.UploadToTexture(
					TransferBuffer,
					yTexture,
					new BufferImageCopy
					{
						BufferOffset = 0,
						BufferStride = CurrentStream.yStride,
						BufferImageHeight = yTexture.Height
					},
					WriteOptions.Cycle
				);

				commandBuffer.UploadToTexture(
					TransferBuffer,
					uTexture,
					new BufferImageCopy{
						BufferOffset = (uint) ySpan.Length,
						BufferStride = CurrentStream.uvStride,
						BufferImageHeight = uTexture.Height
					},
					WriteOptions.Cycle
				);

				commandBuffer.UploadToTexture(
					TransferBuffer,
					vTexture,
					new BufferImageCopy
					{
						BufferOffset = (uint) (ySpan.Length + uSpan.Length),
						BufferStride = CurrentStream.uvStride,
						BufferImageHeight = vTexture.Height
					},
					WriteOptions.Cycle
				);

				commandBuffer.EndCopyPass();

				commandBuffer.BeginRenderPass(
					new ColorAttachmentInfo(RenderTexture, WriteOptions.Cycle, Color.Black)
				);

				commandBuffer.BindGraphicsPipeline(Device.VideoPipeline);
				commandBuffer.BindFragmentSamplers(
					new TextureSamplerBinding(yTexture, LinearSampler),
					new TextureSamplerBinding(uTexture, LinearSampler),
					new TextureSamplerBinding(vTexture, LinearSampler)
				);

				commandBuffer.DrawPrimitives(0, 1);

				commandBuffer.EndRenderPass();

				Device.Submit(commandBuffer);
			}
		}

		private static Texture CreateRenderTexture(GraphicsDevice graphicsDevice, int width, int height)
		{
			return Texture.CreateTexture2D(
				graphicsDevice,
				(uint) width,
				(uint) height,
				TextureFormat.R8G8B8A8,
				TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
			);
		}

		private static Texture CreateSubTexture(GraphicsDevice graphicsDevice, int width, int height)
		{
			return Texture.CreateTexture2D(
				graphicsDevice,
				(uint) width,
				(uint) height,
				TextureFormat.R8,
				TextureUsageFlags.Sampler
			);
		}

		private void InitializeDav1dStream()
		{
			ReadNextFrameTask?.Wait();

			ResetStreamATask = Task.Run(Video.StreamA.Reset);
			ResetStreamATask.ContinueWith(HandleTaskException, TaskContinuationOptions.OnlyOnFaulted);
			ResetStreamBTask = Task.Run(Video.StreamB.Reset);
			ResetStreamBTask.ContinueWith(HandleTaskException, TaskContinuationOptions.OnlyOnFaulted);

			CurrentStream = Video.StreamA;
			currentFrame = -1;
		}

		private static void HandleTaskException(Task task)
		{
			if (task.Exception.InnerException is not TaskCanceledException)
			{
				throw task.Exception;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					Unload();

					RenderTexture?.Dispose();
					yTexture?.Dispose();
					uTexture?.Dispose();
					vTexture?.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
