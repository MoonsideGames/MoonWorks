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
		private VideoAV1Stream Stream { get; }

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
			Stream = new VideoAV1Stream(device);

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
				Unload();

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

			ResetDav1dStreams();

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

			timer.Stop();
			timer.Reset();

			lastTimestamp = 0;
			timeElapsed = 0;

			State = VideoState.Stopped;

			Stream.Unload();

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
				if (Stream.FrameDataUpdated)
				{
					UpdateRenderTexture();
					Stream.FrameDataUpdated = false;
				}

				currentFrame = thisFrame;
				Stream.ReadNextFrame();
			}

			if (Stream.Ended)
			{
				timer.Stop();
				timer.Reset();

				Stream.Reset();

				if (Loop)
				{
					// Start over!
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
			uint uOffset;
			uint vOffset;
			uint yStride;
			uint uvStride;

			lock (Stream)
			{
				var ySpan = new Span<byte>((void*) Stream.yDataHandle, (int) Stream.yDataLength);
				var uSpan = new Span<byte>((void*) Stream.uDataHandle, (int) Stream.uvDataLength);
				var vSpan = new Span<byte>((void*) Stream.vDataHandle, (int) Stream.uvDataLength);

				if (TransferBuffer == null || TransferBuffer.Size < ySpan.Length + uSpan.Length + vSpan.Length)
				{
					TransferBuffer?.Dispose();
					TransferBuffer = TransferBuffer.Create(Device, new TransferBufferCreateInfo
					{
						Usage = TransferBufferUsage.Upload,
						Size = (uint) (ySpan.Length + uSpan.Length + vSpan.Length)
					});
				}
				var transferYSpan = TransferBuffer.Map<byte>(true);
				var transferUSpan = transferYSpan[(int) Stream.yDataLength..];
				var transferVSpan = transferYSpan[(int) (Stream.yDataLength + Stream.uvDataLength)..];
				ySpan.CopyTo(transferYSpan);
				uSpan.CopyTo(transferUSpan);
				vSpan.CopyTo(transferVSpan);
				TransferBuffer.Unmap();

				uOffset = (uint) ySpan.Length;
				vOffset = (uint) (ySpan.Length + vSpan.Length);

				yStride = Stream.yStride;
				uvStride = Stream.uvStride;
			}

			var commandBuffer = Device.AcquireCommandBuffer();

			var copyPass = commandBuffer.BeginCopyPass();

			copyPass.UploadToTexture(
				new TextureTransferInfo
				{
					TransferBuffer = TransferBuffer.Handle,
					Offset = 0,
					PixelsPerRow = yStride,
					RowsPerLayer = yTexture.Height
				},
				yTexture,
				true
			);

			copyPass.UploadToTexture(
				new TextureTransferInfo
				{
					TransferBuffer = TransferBuffer.Handle,
					Offset = uOffset,
					PixelsPerRow = uvStride,
					RowsPerLayer = uTexture.Height
				},
				uTexture,
				true
			);

			copyPass.UploadToTexture(
				new TextureTransferInfo
				{
					TransferBuffer = TransferBuffer.Handle,
					Offset = vOffset,
					PixelsPerRow = uvStride,
					RowsPerLayer = vTexture.Height
				},
				vTexture,
				true
			);

			commandBuffer.EndCopyPass(copyPass);

			var renderPass = commandBuffer.BeginRenderPass(
				new ColorTargetInfo
				{
					Texture = RenderTexture.Handle,
					LoadOp = LoadOp.Clear,
					ClearColor = Color.Black,
					StoreOp = StoreOp.Store,
					Cycle = true
				}
			);

			renderPass.BindGraphicsPipeline(Device.VideoPipeline);
			renderPass.BindFragmentSampler(new TextureSamplerBinding(yTexture, LinearSampler), 0);
			renderPass.BindFragmentSampler(new TextureSamplerBinding(uTexture, LinearSampler), 1);
			renderPass.BindFragmentSampler(new TextureSamplerBinding(vTexture, LinearSampler), 2);
			renderPass.DrawPrimitives(3, 1, 0, 0);

			commandBuffer.EndRenderPass(renderPass);

			Device.Submit(commandBuffer);
		}

		private static Texture CreateRenderTexture(GraphicsDevice graphicsDevice, int width, int height)
		{
			return Texture.CreateTexture2D(
				graphicsDevice,
				(uint) width,
				(uint) height,
				TextureFormat.R8G8B8A8_UNORM,
				TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
			);
		}

		private static Texture CreateSubTexture(GraphicsDevice graphicsDevice, int width, int height)
		{
			return Texture.CreateTexture2D(
				graphicsDevice,
				(uint) width,
				(uint) height,
				TextureFormat.R8_UNORM,
				TextureUsageFlags.Sampler
			);
		}

		private void InitializeDav1dStream()
		{
			Stream.Load(Video.Filename);
			currentFrame = -1;
		}

		private void ResetDav1dStreams()
		{
			Stream.Reset();
			currentFrame = -1;
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
