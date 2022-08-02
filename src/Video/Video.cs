/* Heavily based on https://github.com/FNA-XNA/FNA/blob/master/src/Media/Xiph/VideoPlayer.cs */
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MoonWorks.Audio;
using MoonWorks.Graphics;

namespace MoonWorks.Video
{
	public enum VideoState
	{
		Playing,
		Paused,
		Stopped
	}

	public unsafe class Video : IDisposable
	{
		internal IntPtr Handle;

		public bool Loop { get; private set; }
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
		public float PlaybackSpeed { get; set; }
		public double FramesPerSecond => fps;
		private VideoState State = VideoState.Stopped;

		private double fps;
		private int yWidth;
		private int yHeight;
		private int uvWidth;
		private int uvHeight;

		private void* yuvData = null;
		private int yuvDataLength;
		private int currentFrame;

		private GraphicsDevice GraphicsDevice;
		private Texture RenderTexture = null;
		private Texture yTexture = null;
		private Texture uTexture = null;
		private Texture vTexture = null;
		private Sampler LinearSampler;

		private AudioDevice AudioDevice = null;
		private StreamingSoundTheora audioStream = null;
		private float volume = 1.0f;

		private Stopwatch timer;
		private double lastTimestamp;
		private double timeElapsed;

		private bool disposed;

		/* TODO: is there some way for us to load the data into memory? */
		public Video(GraphicsDevice graphicsDevice, AudioDevice audioDevice, string filename)
		{
			GraphicsDevice = graphicsDevice;
			AudioDevice = audioDevice;

			if (!System.IO.File.Exists(filename))
			{
				throw new ArgumentException("Video file not found!");
			}

			if (Theorafile.tf_fopen(filename, out Handle) < 0)
			{
				throw new ArgumentException("Invalid video file!");
			}

			Theorafile.th_pixel_fmt format;
			Theorafile.tf_videoinfo(
				Handle,
				out yWidth,
				out yHeight,
				out fps,
				out format
			);

			if (format == Theorafile.th_pixel_fmt.TH_PF_420)
			{
				uvWidth = yWidth / 2;
				uvHeight = yHeight / 2;
			}
			else if (format == Theorafile.th_pixel_fmt.TH_PF_422)
			{
				uvWidth = yWidth / 2;
				uvHeight = yHeight;
			}
			else if (format == Theorafile.th_pixel_fmt.TH_PF_444)
			{
				uvWidth = yWidth;
				uvHeight = yHeight;
			}
			else
			{
				throw new NotSupportedException("Unrecognized YUV format!");
			}

			yuvDataLength = (
				(yWidth * yHeight) +
				(uvWidth * uvHeight * 2)
			);

			yuvData = NativeMemory.Alloc((nuint) yuvDataLength);

			InitializeTheoraStream();

			if (Theorafile.tf_hasvideo(Handle) == 1)
			{
				RenderTexture = Texture.CreateTexture2D(
					GraphicsDevice,
					(uint) yWidth,
					(uint) yHeight,
					TextureFormat.R8G8B8A8,
					TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
				);

				yTexture = Texture.CreateTexture2D(
					GraphicsDevice,
					(uint) yWidth,
					(uint) yHeight,
					TextureFormat.R8,
					TextureUsageFlags.Sampler
				);

				uTexture = Texture.CreateTexture2D(
					GraphicsDevice,
					(uint) uvWidth,
					(uint) uvHeight,
					TextureFormat.R8,
					TextureUsageFlags.Sampler
				);

				vTexture = Texture.CreateTexture2D(
					GraphicsDevice,
					(uint) uvWidth,
					(uint) uvHeight,
					TextureFormat.R8,
					TextureUsageFlags.Sampler
				);

				LinearSampler = new Sampler(GraphicsDevice, SamplerCreateInfo.LinearClamp);
			}

			timer = new Stopwatch();
		}

		public void Play(bool loop = false)
		{
			if (State == VideoState.Playing)
			{
				return;
			}

			Loop = loop;
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

			Theorafile.tf_reset(Handle);
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

		public Texture GetTexture()
		{
			if (RenderTexture == null)
			{
				throw new InvalidOperationException();
			}

			if (State == VideoState.Stopped)
			{
				return RenderTexture;
			}

			timeElapsed += (timer.Elapsed.TotalMilliseconds - lastTimestamp) * PlaybackSpeed;
			lastTimestamp = timer.Elapsed.TotalMilliseconds;

			int thisFrame = ((int) (timeElapsed / (1000.0 / FramesPerSecond)));
			if (thisFrame > currentFrame)
			{
				if (Theorafile.tf_readvideo(
					Handle,
					(IntPtr) yuvData,
					thisFrame - currentFrame
				) == 1 || currentFrame == -1) {
					UpdateTexture();
				}

				currentFrame = thisFrame;
			}

			bool ended = Theorafile.tf_eos(Handle) == 1;
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

				Theorafile.tf_reset(Handle);

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

			return RenderTexture;
		}

		private void UpdateTexture()
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

		private void InitializeTheoraStream()
		{
			// Grab the first video frame ASAP.
			while (Theorafile.tf_readvideo(Handle, (IntPtr) yuvData, 1) == 0);

			// Grab the first bit of audio. We're trying to start the decoding ASAP.
			if (AudioDevice != null && Theorafile.tf_hasaudio(Handle) == 1)
			{
				int channels, sampleRate;
				Theorafile.tf_audioinfo(Handle, out channels, out sampleRate);
				audioStream = new StreamingSoundTheora(AudioDevice, Handle, channels, (uint) sampleRate);
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

				// free unmanaged resources (unmanaged objects)
				Theorafile.tf_close(ref Handle);
				NativeMemory.Free(yuvData);

				disposed = true;
			}
		}

		~Video()
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
