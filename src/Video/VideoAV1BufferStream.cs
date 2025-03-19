using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using MoonWorks.Graphics;

namespace MoonWorks.Video;

// Responsible for performing video operations on a single background thread.
public class VideoAV1BufferStream : IDisposable
{
	private GraphicsDevice GraphicsDevice;
	private TransferBuffer TransferBuffer;

	private HashSet<VideoAV1> ActiveVideos = [];

	Thread Thread;
	private AutoResetEvent WakeSignal;
	private TimeSpan UpdateInterval;

	private bool Running = false;
	public bool IsDisposed { get; private set; }

	public VideoAV1BufferStream(GraphicsDevice device)
	{
		GraphicsDevice = device;

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
			// this lock might be really bad?
			lock (ActiveVideos)
			{
				foreach (var video in ActiveVideos)
				{
					ProcessVideo(video);
				}
			}

			WakeSignal.WaitOne(UpdateInterval);
		}
	}

	private void ProcessVideo(VideoAV1 video)
	{
		var neededFrames = VideoAV1.BUFFERED_FRAME_COUNT - video.BufferedFrameCount();
		for (var i = 0; i < neededFrames; i += 1)
		{
			BufferFrameSync(video);

			if (video.Ended && video.Loop)
			{
				ResetSync(video);
			}
		}
	}

	internal void RegisterVideo(VideoAV1 video)
	{
		lock (ActiveVideos)
		{
			ActiveVideos.Add(video);
		}
	}

	internal void UnregisterVideo(VideoAV1 video)
	{
		lock (ActiveVideos)
		{
			ActiveVideos.Remove(video);
		}
	}

	private void ResetSync(VideoAV1 videoPlayer)
	{
		Dav1dfile.df_reset(videoPlayer.Handle);
		BufferFrameSync(videoPlayer);
	}

	public unsafe void BufferFrameSync(VideoAV1 videoPlayer)
	{
		nint yDataHandle;
		nint uDataHandle;
		nint vDataHandle;
		uint yDataLength;
		uint uvDataLength;
		uint yStride;
		uint uvStride;

		var result = Dav1dfile.df_readvideo(
			videoPlayer.handle,
			1,
			out yDataHandle,
			out uDataHandle,
			out vDataHandle,
			out yDataLength,
			out uvDataLength,
			out yStride,
			out uvStride
		);

		if (result == 0)
		{
			return;
		}

		var renderTexture = videoPlayer.AcquireTexture();

		var ySpan = new Span<byte>((void*) yDataHandle, (int) yDataLength);
		var uSpan = new Span<byte>((void*) uDataHandle, (int) uvDataLength);
		var vSpan = new Span<byte>((void*) vDataHandle, (int) uvDataLength);

		if (TransferBuffer == null || TransferBuffer.Size < ySpan.Length + uSpan.Length + vSpan.Length)
		{
			TransferBuffer?.Dispose();
			TransferBuffer = TransferBuffer.Create(GraphicsDevice, new TransferBufferCreateInfo
			{
				Usage = TransferBufferUsage.Upload,
				Size = (uint) (ySpan.Length + uSpan.Length + vSpan.Length)
			});
		}

		var transferYSpan = TransferBuffer.Map<byte>(true);
		var transferUSpan = transferYSpan[(int) yDataLength..];
		var transferVSpan = transferYSpan[(int) (yDataLength + uvDataLength)..];
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
				PixelsPerRow = yStride,
				RowsPerLayer = videoPlayer.yTexture.Height
			},
			new TextureRegion
			{
				Texture = videoPlayer.yTexture.Handle,
				W = videoPlayer.yTexture.Width,
				H = videoPlayer.yTexture.Height,
				D = 1
			},
			false
		);

		copyPass.UploadToTexture(
			new TextureTransferInfo
			{
				TransferBuffer = TransferBuffer.Handle,
				Offset = uOffset,
				PixelsPerRow = uvStride,
				RowsPerLayer = videoPlayer.uTexture.Height
			},
			new TextureRegion
			{
				Texture = videoPlayer.uTexture.Handle,
				W = videoPlayer.uTexture.Width,
				H = videoPlayer.uTexture.Height,
				D = 1
			},
			false
		);

		copyPass.UploadToTexture(
			new TextureTransferInfo
			{
				TransferBuffer = TransferBuffer.Handle,
				Offset = vOffset,
				PixelsPerRow = uvStride,
				RowsPerLayer = videoPlayer.vTexture.Height
			},
			new TextureRegion
			{
				Texture = videoPlayer.vTexture.Handle,
				W = videoPlayer.vTexture.Width,
				H = videoPlayer.vTexture.Height,
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
			new TextureSamplerBinding(videoPlayer.yTexture, GraphicsDevice.LinearSampler),
			new TextureSamplerBinding(videoPlayer.uTexture, GraphicsDevice.LinearSampler),
			new TextureSamplerBinding(videoPlayer.vTexture, GraphicsDevice.LinearSampler)
		);
		renderPass.DrawPrimitives(3, 1, 0, 0);

		commandBuffer.EndRenderPass(renderPass);
		GraphicsDevice.Submit(commandBuffer);

		videoPlayer.BufferFrame(renderTexture);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			Running = false;

			if (disposing)
			{
				Thread.Join();

				TransferBuffer?.Dispose();
			}
		}
	}

	~VideoAV1BufferStream()
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
