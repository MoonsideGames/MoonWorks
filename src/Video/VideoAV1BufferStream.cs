using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MoonWorks.Graphics;
using MoonWorks.Storage;

namespace MoonWorks.Video;

internal class VideoAV1BufferStream : GraphicsResource
{
	public IntPtr Handle => handle;
	IntPtr handle;

	private IntPtr ByteBuffer;

	public bool Loaded => handle != IntPtr.Zero;
	public bool Ended => Handle == IntPtr.Zero || Dav1dfile.df_eos(Handle) == 1;
	public bool Loop;

	const int BUFFERED_FRAME_COUNT = 5;
	private List<Texture> AvailableTextures = [];
	private ConcurrentQueue<Texture> BufferedTextures = [];

	private TransferBuffer TransferBuffer;
	private Texture yTexture = null;
	private Texture uTexture = null;
	private Texture vTexture = null;

	private BlockingCollection<Action> Actions = [];
	private bool Running = false;
	CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
	Thread Thread;

	public VideoAV1BufferStream(GraphicsDevice device) : base(device)
	{
		handle = IntPtr.Zero;
		Name = "VideoAV1Stream";

		Thread = new Thread(ThreadMain);
		Thread.Start();
	}

	private void ThreadMain()
	{
		Running = true;

		while (Running)
		{
			try
			{
				// Block until we can take an action, then run it
				var action = Actions.Take(CancellationTokenSource.Token);
				action.Invoke();
			}
			catch (OperationCanceledException)
			{
				// Fired on thread shutdown
				Logger.LogInfo("Cancelling AV1 thread!");
				Running = false;
			}
		}
	}

	public Task Load(TitleStorage storage, string filename, bool loop)
	{
		return Task.Run(() => LoadHelper(storage, filename, loop));
	}

	public void Reset()
	{
		Actions.Add(ResetHelper);
	}

	public void Unload()
	{
		Actions.Add(UnloadHelper);
	}

	/// <summary>
	/// Obtains a buffered frame. Returns false if no frames are available.
	/// If successful, this method also enqueues a new buffered frame.
	/// You must call ReleaseFrame eventually or the texture will leak.
	/// </summary>
	/// <returns>True if frame is available, otherwise false.</returns>
	public bool TryGetBufferedFrame(out Texture texture)
	{
		texture = null;
		Actions.Add(BufferFrame);

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
	public void ReleaseFrame(Texture texture)
	{
		if (texture == null) { return; }

		lock (AvailableTextures)
		{
			AvailableTextures.Add(texture);
		}
	}

	private unsafe void LoadHelper(TitleStorage storage, string filename, bool loop)
	{
		if (!storage.GetFileSize(filename, out var size))
		{
			return;
		}

		ByteBuffer = (nint) NativeMemory.Alloc((nuint) size);
		var span = new Span<byte>((void*) ByteBuffer, (int) size);
		if (!storage.ReadFile(filename, span))
		{
			return;
		}

		if (Dav1dfile.df_open_from_memory(ByteBuffer, (uint) size, out handle) == 0)
		{
			Logger.LogError("Failed to load video file: " + filename);
			throw new Exception("Failed to load video file!");
		}

		Dav1dfile.df_videoinfo(handle, out var width, out var height, out var pixelLayout);

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
			return;
		}

		for (var i = 0; i < BUFFERED_FRAME_COUNT; i += 1)
		{
			if (AvailableTextures.Count > i)
			{
				if (AvailableTextures[i].Width == width && AvailableTextures[i].Height == height)
				{
					continue;
				}
				else
				{
					AvailableTextures[i] = Texture.Create2D(
						Device,
						(uint) width,
						(uint) height,
						TextureFormat.R8G8B8A8Unorm,
						TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
					);
				}
			}
			else
			{
				AvailableTextures.Add(Texture.Create2D(
					Device,
					(uint) width,
					(uint) height,
					TextureFormat.R8G8B8A8Unorm,
					TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler
				));
			}
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

		// pre-buffer frames
		for (var i = 0; i < BUFFERED_FRAME_COUNT; i += 1)
		{
			Actions.Add(BufferFrame);
		}

		Loop = loop;
	}

	private void ResetHelper()
	{
		if (Loaded)
		{
			Dav1dfile.df_reset(handle);
			Actions.Add(BufferFrame);
		}
	}

	private unsafe void UnloadHelper()
	{
		if (Loaded)
		{
			Dav1dfile.df_close(handle);
			handle = IntPtr.Zero;
			NativeMemory.Free((void*) ByteBuffer);
			ByteBuffer = IntPtr.Zero;

			while (BufferedTextures.Count > 0)
			{
				if (BufferedTextures.TryDequeue(out var texture))
				{
					AvailableTextures.Add(texture);
				}
			}
		}
	}

	private unsafe void BufferFrame()
	{
		if (AvailableTextures.Count == 0) { return; }

		var result = Dav1dfile.df_readvideo(
			handle,
			1,
			out var yDataHandle,
			out var uDataHandle,
			out var vDataHandle,
			out var yDataLength,
			out var uvDataLength,
			out var yStride,
			out var uvStride
		);

		if (result == 0)
		{
			return;
		}

		var renderTexture = AcquireTexture();

		var ySpan = new Span<byte>((void*) yDataHandle, (int) yDataLength);
		var uSpan = new Span<byte>((void*) uDataHandle, (int) uvDataLength);
		var vSpan = new Span<byte>((void*) vDataHandle, (int) uvDataLength);

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
		var transferUSpan = transferYSpan[(int) yDataLength..];
		var transferVSpan = transferYSpan[(int) (yDataLength + uvDataLength)..];
		ySpan.CopyTo(transferYSpan);
		uSpan.CopyTo(transferUSpan);
		vSpan.CopyTo(transferVSpan);
		TransferBuffer.Unmap();

		var uOffset = (uint) ySpan.Length;
		var vOffset = (uint) (ySpan.Length + vSpan.Length);

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
			new TextureRegion
			{
				Texture = yTexture.Handle,
				W = yTexture.Width,
				H = yTexture.Height,
				D = 1
			},
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
			new TextureRegion
			{
				Texture = uTexture.Handle,
				W = uTexture.Width,
				H = uTexture.Height,
				D = 1
			},
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
			new TextureRegion
			{
				Texture = vTexture.Handle,
				W = vTexture.Width,
				H = vTexture.Height,
				D = 1
			},
			true
		);

		commandBuffer.EndCopyPass(copyPass);

		var renderPass = commandBuffer.BeginRenderPass(
			new ColorTargetInfo
			{
				Texture = renderTexture.Handle,
				LoadOp = LoadOp.Clear,
				ClearColor = Color.Black,
				StoreOp = StoreOp.Store,
				Cycle = true
			}
		);

		renderPass.BindGraphicsPipeline(Device.VideoPipeline);
		renderPass.BindFragmentSamplers(
			new TextureSamplerBinding(yTexture, Device.LinearSampler),
			new TextureSamplerBinding(uTexture, Device.LinearSampler),
			new TextureSamplerBinding(vTexture, Device.LinearSampler)
		);
		renderPass.DrawPrimitives(3, 1, 0, 0);

		commandBuffer.EndRenderPass(renderPass);
		Device.Submit(commandBuffer);

		BufferedTextures.Enqueue(renderTexture);
	}

	private Texture AcquireTexture()
	{
		lock (AvailableTextures)
		{
			var result = AvailableTextures[AvailableTextures.Count - 1];
			AvailableTextures.RemoveAt(AvailableTextures.Count - 1);
			return result;
		}
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

	protected override unsafe void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			Unload();
			Running = false;

			if (disposing)
			{
				CancellationTokenSource.Cancel();
				Thread.Join();

				if (Loaded)
				{
					Dav1dfile.df_close(handle);
					handle = IntPtr.Zero;
					NativeMemory.Free((void*) ByteBuffer);
					ByteBuffer = IntPtr.Zero;

					yTexture?.Dispose();
					uTexture?.Dispose();
					vTexture?.Dispose();

					while (BufferedTextures.TryDequeue(out var texture))
					{
						AvailableTextures.Add(texture);
					}

					for (var i = 0; i < AvailableTextures.Count; i += 1)
					{
						AvailableTextures[i]?.Dispose();
					}
				}
			}
		}

		base.Dispose(disposing);
	}
}
