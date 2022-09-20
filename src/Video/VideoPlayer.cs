using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MoonWorks.Audio;
using MoonWorks.Graphics;

namespace MoonWorks.Video
{
	public unsafe class VideoPlayer : IDisposable
	{
		public Texture RenderTexture { get; private set; } = null;
		public VideoState State { get; private set; } = VideoState.Stopped;
		public bool Loop { get; set; }
		public float Volume {
			get => volume;
			set
			{
				volume = value;
				if (audioStream != null)
				{
					audioStream.Volume = value;
				}
			}
		}
		public float PlaybackSpeed { get; set; } = 1;

		private Video Video = null;

		private GraphicsDevice GraphicsDevice;
		private Texture yTexture = null;
		private Texture uTexture = null;
		private Texture vTexture = null;
		private Sampler LinearSampler;

		private void* yuvData = null;
		private int yuvDataLength = 0;

		private int currentFrame;

		private AudioDevice AudioDevice;
		private StreamingSoundTheora audioStream = null;
		private float volume = 1.0f;

		private Stopwatch timer;
		private double lastTimestamp;
		private double timeElapsed;

		private bool disposed;

		public VideoPlayer(GraphicsDevice graphicsDevice, AudioDevice audioDevice)
		{
			GraphicsDevice = graphicsDevice;
			AudioDevice = audioDevice;
			LinearSampler = new Sampler(graphicsDevice, SamplerCreateInfo.LinearClamp);

			timer = new Stopwatch();
		}

		public void Load(Video video)
		{
			if (Video != video)
			{
				Stop();

				if (RenderTexture == null)
				{
					RenderTexture = CreateRenderTexture(GraphicsDevice, video.Width, video.Height);
				}

				if (yTexture == null)
				{
					yTexture = CreateSubTexture(GraphicsDevice, video.Width, video.Height);
				}

				if (uTexture == null)
				{
					uTexture = CreateSubTexture(GraphicsDevice, video.UVWidth, video.UVHeight);
				}

				if (vTexture == null)
				{
					vTexture = CreateSubTexture(GraphicsDevice, video.UVWidth, video.UVHeight);
				}

				if (video.Width != RenderTexture.Width || video.Height != RenderTexture.Height)
				{
					RenderTexture.Dispose();
					RenderTexture = CreateRenderTexture(GraphicsDevice, video.Width, video.Height);
				}

				if (video.Width != yTexture.Width || video.Height != yTexture.Height)
				{
					yTexture.Dispose();
					yTexture = CreateSubTexture(GraphicsDevice, video.Width, video.Height);
				}

				if (video.UVWidth != uTexture.Width || video.UVHeight != uTexture.Height)
				{
					uTexture.Dispose();
					uTexture = CreateSubTexture(GraphicsDevice, video.UVWidth, video.UVHeight);
				}

				if (video.UVWidth != vTexture.Width || video.UVHeight != vTexture.Height)
				{
					vTexture.Dispose();
					vTexture = CreateSubTexture(GraphicsDevice, video.UVWidth, video.UVHeight);
				}

				var newDataLength = (
					(video.Width * video.Height) +
					(video.UVWidth * video.UVHeight * 2)
				);

				if (newDataLength != yuvDataLength)
				{
					yuvData = NativeMemory.Realloc(yuvData, (nuint) newDataLength);
					yuvDataLength = newDataLength;
				}

				Video = video;

				InitializeTheoraStream();
			}
		}

		public void Play()
		{
			if (State == VideoState.Playing)
			{
				return;
			}

			timer.Start();

			if (audioStream != null)
			{
				audioStream.Play();
			}

			State = VideoState.Playing;
		}

		public void Pause()
		{
			if (State != VideoState.Playing)
			{
				return;
			}

			timer.Stop();

			if (audioStream != null)
			{
				audioStream.Pause();
			}

			State = VideoState.Paused;
		}

		public void Stop()
		{
			if (State == VideoState.Stopped)
			{
				return;
			}

			timer.Stop();
			timer.Reset();

			Theorafile.tf_reset(Video.Handle);
			lastTimestamp = 0;
			timeElapsed = 0;

			if (audioStream != null)
			{
				audioStream.StopImmediate();
				audioStream.Dispose();
				audioStream = null;
			}

			State = VideoState.Stopped;
		}

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
				if (Theorafile.tf_readvideo(
					Video.Handle,
					(IntPtr) yuvData,
					thisFrame - currentFrame
				) == 1 || currentFrame == -1) {
					UpdateRenderTexture();
				}

				currentFrame = thisFrame;
			}

			bool ended = Theorafile.tf_eos(Video.Handle) == 1;
			if (ended)
			{
				timer.Stop();
				timer.Reset();

				if (audioStream != null)
				{
					audioStream.Stop();
					audioStream.Dispose();
					audioStream = null;
				}

				Theorafile.tf_reset(Video.Handle);

				if (Loop)
				{
					// Start over!
					InitializeTheoraStream();

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
			var commandBuffer = GraphicsDevice.AcquireCommandBuffer();

			commandBuffer.SetTextureDataYUV(
				yTexture,
				uTexture,
				vTexture,
				(IntPtr) yuvData,
				(uint) yuvDataLength
			);

			commandBuffer.BeginRenderPass(
				new ColorAttachmentInfo(RenderTexture, Color.Black)
			);

			commandBuffer.BindGraphicsPipeline(GraphicsDevice.VideoPipeline);
			commandBuffer.BindFragmentSamplers(
				new TextureSamplerBinding(yTexture, LinearSampler),
				new TextureSamplerBinding(uTexture, LinearSampler),
				new TextureSamplerBinding(vTexture, LinearSampler)
			);

			commandBuffer.DrawPrimitives(0, 1, 0, 0);

			commandBuffer.EndRenderPass();

			GraphicsDevice.Submit(commandBuffer);
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

		private void InitializeTheoraStream()
		{
			// Grab the first video frame ASAP.
			while (Theorafile.tf_readvideo(Video.Handle, (IntPtr) yuvData, 1) == 0);

			// Grab the first bit of audio. We're trying to start the decoding ASAP.
			if (AudioDevice != null && Theorafile.tf_hasaudio(Video.Handle) == 1)
			{
				int channels, sampleRate;
				Theorafile.tf_audioinfo(Video.Handle, out channels, out sampleRate);
				audioStream = new StreamingSoundTheora(AudioDevice, Video.Handle, channels, (uint) sampleRate);
			}

			currentFrame = -1;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
					RenderTexture.Dispose();
					yTexture.Dispose();
					uTexture.Dispose();
					vTexture.Dispose();
				}

				// free unmanaged resources (unmanaged objects) and override finalizer
				NativeMemory.Free(yuvData);

				disposed = true;
			}
		}

		~VideoPlayer()
		{
		    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		    Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
