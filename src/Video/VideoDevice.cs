using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MoonWorks.Graphics;

namespace MoonWorks.Video;

// Responsible for performing video operations on a single background thread.
public class VideoDevice : IDisposable
{
	private HashSet<VideoAV1> ActiveVideos = [];

	private ConcurrentQueue<VideoAV1> VideosToActivate = [];
	private ConcurrentQueue<VideoAV1> VideosToDeactivate = [];

	GraphicsDevice GraphicsDevice;
	TransferBuffer TransferBuffer;
	Dictionary<(uint, uint), Texture> RenderTextureCache = [];
	Dictionary<(uint, uint), Texture> YTextureCache = [];
	Dictionary<(uint, uint), Texture> UTextureCache = [];
	Dictionary<(uint, uint), Texture> VTextureCache = [];

	Thread Thread;
	private AutoResetEvent WakeSignal;
	private TimeSpan UpdateInterval;
	private Stopwatch ThreadTimer = new Stopwatch();

	private bool Running = false;
	public bool IsDisposed { get; private set; }

	public VideoDevice(GraphicsDevice graphicsDevice)
	{
		GraphicsDevice = graphicsDevice;

		var step = 200;
		UpdateInterval = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / step);
		WakeSignal = new AutoResetEvent(true);

		Thread = new Thread(ThreadMain);
		Thread.Start();
	}

	private void ThreadMain()
	{
		Running = true;

		while (Running)
		{
			ThreadTimer.Restart();

			while (VideosToDeactivate.TryDequeue(out var video))
			{
				Dav1dfile.Bindings.df_reset(video.Handle);
				ActiveVideos.Remove(video);
			}

			while (VideosToActivate.TryDequeue(out var video))
			{
				ActiveVideos.Add(video);
				video.BufferFrameSync();

				video.Loaded = true;
				video.LoadWaitHandle.Set();
			}

			foreach (var video in ActiveVideos)
			{
				ProcessVideo(video);
			}

			ThreadTimer.Stop();

			if (ThreadTimer.Elapsed < UpdateInterval)
			{
				WakeSignal.WaitOne(UpdateInterval - ThreadTimer.Elapsed);
			}
		}
	}

	private void ProcessVideo(VideoAV1 video)
	{
		var neededFrames = VideoAV1.BUFFERED_FRAME_COUNT - video.BufferedFrameCount();
		for (var i = 0; i < neededFrames; i += 1)
		{
			video.BufferFrameSync();

			if (video.Ended && video.Loop)
			{
				video.ResetSync();
			}
		}
	}

	internal void RegisterVideo(VideoAV1 video)
	{
		VideosToActivate.Enqueue(video);
	}

	internal void UnregisterVideo(VideoAV1 video)
	{
		VideosToDeactivate.Enqueue(video);
	}

	// This is NOT thread-safe!
	// In general use cases only one video is rendered at a time.
	// This function happens on the VideoDevice so we can share graphics resources.
	internal unsafe Texture RenderFrame(YUVFramebuffer framebuffer)
	{
		if (!RenderTextureCache.TryGetValue((framebuffer.YWidth, framebuffer.YHeight), out var renderTexture))
		{
			renderTexture = Texture.Create2D(
				GraphicsDevice,
				"Video Render Texture",
				framebuffer.YWidth,
				framebuffer.YHeight,
				TextureFormat.R8G8B8A8Unorm,
				TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
			);

			RenderTextureCache.Add((framebuffer.YWidth, framebuffer.YHeight), renderTexture);
		}

		if (!YTextureCache.TryGetValue((framebuffer.YWidth, framebuffer.YHeight), out var yTexture))
		{
			yTexture = Texture.Create2D(
				GraphicsDevice,
				"Y Sample Texture",
				framebuffer.YWidth,
				framebuffer.YHeight,
				TextureFormat.R8Unorm,
				TextureUsageFlags.Sampler
			);

			YTextureCache.Add((framebuffer.YWidth, framebuffer.YHeight), yTexture);
		}

		if (!UTextureCache.TryGetValue((framebuffer.UVWidth, framebuffer.UVHeight), out var uTexture))
		{
			uTexture = Texture.Create2D(
				GraphicsDevice,
				"U Sample Texture",
				framebuffer.UVWidth,
				framebuffer.UVHeight,
				TextureFormat.R8Unorm,
				TextureUsageFlags.Sampler
			);

			UTextureCache.Add((framebuffer.UVWidth, framebuffer.UVHeight), uTexture);
		}

		if (!VTextureCache.TryGetValue((framebuffer.UVWidth, framebuffer.UVHeight), out var vTexture))
		{
			vTexture = Texture.Create2D(
				GraphicsDevice,
				"V Sample Texture",
				framebuffer.UVWidth,
				framebuffer.UVHeight,
				TextureFormat.R8Unorm,
				TextureUsageFlags.Sampler
			);

			VTextureCache.Add((framebuffer.UVWidth, framebuffer.UVHeight), vTexture);
		}

		var ySpan = new Span<byte>(
			(void*) framebuffer.YDataBuffer,
			(int) framebuffer.YDataBufferLength);

		var uSpan = new Span<byte>(
			(void*) framebuffer.UDataBuffer,
			(int) framebuffer.UVDataBufferLength);

		var vSpan = new Span<byte>(
			(void*) framebuffer.VDataBuffer,
			(int) framebuffer.UVDataBufferLength);

		if (TransferBuffer == null || TransferBuffer.Size < ySpan.Length + uSpan.Length + vSpan.Length)
		{
			TransferBuffer?.Dispose();
			TransferBuffer = TransferBuffer.Create<byte>(
				GraphicsDevice,
				"Video Transfer Buffer",
				TransferBufferUsage.Upload,
				(uint) (ySpan.Length + uSpan.Length + vSpan.Length)
			);
		}

		var transferYSpan = TransferBuffer.Map<byte>(true);
		var transferUSpan = transferYSpan[(int) framebuffer.YDataBufferLength..];
		var transferVSpan = transferYSpan[(int) (framebuffer.YDataBufferLength + framebuffer.UVDataBufferLength)..];
		ySpan.CopyTo(transferYSpan);
		uSpan.CopyTo(transferUSpan);
		vSpan.CopyTo(transferVSpan);
		TransferBuffer.Unmap();

		var uOffset = (uint) ySpan.Length;
		var vOffset = (uint) (ySpan.Length + uSpan.Length);

		var commandBuffer = GraphicsDevice.AcquireCommandBuffer();

		var copyPass = commandBuffer.BeginCopyPass();

		copyPass.UploadToTexture(
			new TextureTransferInfo
			{
				TransferBuffer = TransferBuffer.Handle,
				Offset = 0,
				PixelsPerRow = framebuffer.YStride,
				RowsPerLayer = yTexture.Height
			},
			new TextureRegion
			{
				Texture = yTexture.Handle,
				W = yTexture.Width,
				H = yTexture.Height,
				D = 1
			},
			false
		);

		copyPass.UploadToTexture(
			new TextureTransferInfo
			{
				TransferBuffer = TransferBuffer.Handle,
				Offset = uOffset,
				PixelsPerRow = framebuffer.UVStride,
				RowsPerLayer = uTexture.Height
			},
			new TextureRegion
			{
				Texture = uTexture.Handle,
				W = uTexture.Width,
				H = uTexture.Height,
				D = 1
			},
			false
		);

		copyPass.UploadToTexture(
			new TextureTransferInfo
			{
				TransferBuffer = TransferBuffer.Handle,
				Offset = vOffset,
				PixelsPerRow = framebuffer.UVStride,
				RowsPerLayer = vTexture.Height
			},
			new TextureRegion
			{
				Texture = vTexture.Handle,
				W = vTexture.Width,
				H = vTexture.Height,
				D = 1
			},
			false
		);

		commandBuffer.EndCopyPass(copyPass);

		var renderPass = commandBuffer.BeginRenderPass(
			new ColorTargetInfo
			{
				Texture = renderTexture.Handle,
				LoadOp = LoadOp.Clear,
				ClearColor = Color.Black,
				StoreOp = StoreOp.Store,
				Cycle = false
			}
		);

		renderPass.BindGraphicsPipeline(GraphicsDevice.VideoPipeline);
		renderPass.BindFragmentSamplers(
			new TextureSamplerBinding(yTexture, GraphicsDevice.LinearSampler),
			new TextureSamplerBinding(uTexture, GraphicsDevice.LinearSampler),
			new TextureSamplerBinding(vTexture, GraphicsDevice.LinearSampler)
		);
		renderPass.DrawPrimitives(3, 1, 0, 0);

		commandBuffer.EndRenderPass(renderPass);
		GraphicsDevice.Submit(commandBuffer);

		return renderTexture;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			Running = false;

			if (disposing)
			{
				Thread.Join();

				foreach (var (_, texture) in RenderTextureCache)
				{
					texture.Dispose();
				}
				RenderTextureCache.Clear();

				foreach (var (_, texture) in YTextureCache)
				{
					texture.Dispose();
				}
				YTextureCache.Clear();

				foreach (var (_, texture) in UTextureCache)
				{
					texture.Dispose();
				}
				UTextureCache.Clear();

				foreach (var (_, texture) in VTextureCache)
				{
					texture.Dispose();
				}
				VTextureCache.Clear();
			}
		}
	}

	~VideoDevice()
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
