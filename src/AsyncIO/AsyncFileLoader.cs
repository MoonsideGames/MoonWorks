using System;
using System.Collections.Generic;
using System.Threading;
using MoonWorks.Audio;
using MoonWorks.Graphics;

namespace MoonWorks.AsyncIO;

/// <summary>
/// A convenience structure for asynchronously loading data from files.
/// When Submit is called, file loads are performed asynchronously and 
/// contextual actions are executed on a thread for every file load that is completed.
/// </summary>
public class AsyncFileLoader : IDisposable
{
	enum LoadType
	{
		CompressedImage,
		AudioWav,
		AudioOggStatic,
		AudioOggStreaming,
		AudioQoaStatic,
		AudioQoaStreaming,
		Custom
	}

	readonly record struct LoadData(
		string File,
		LoadType LoadType,
		object Object,
		OnFileLoad Callback // only used with LoadType.Custom
	);

    Queue LoadQueue = Queue.Create();

	GraphicsDevice GraphicsDevice;
	ResourceUploader ResourceUploader;

	List<LoadData> PendingLoads = [];
	int LoadsCompleted = 0;

	public AsyncFileLoaderStatus Status { get; private set; }

	Thread Thread;
    private bool IsDisposed;

    public AsyncFileLoader(GraphicsDevice graphicsDevice)
    {
		GraphicsDevice = graphicsDevice;
		ResourceUploader = new ResourceUploader(GraphicsDevice);
		Thread = new Thread(ThreadMain);
    }

	/// <summary>
	/// Asynchronously load an arbitrary object from a file using a custom callback.
	/// </summary>
	public void EnqueueCustomObjectLoad(string file, OnFileLoad callback, object callbackObject)
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}
		PendingLoads.Add(new LoadData(file, LoadType.Custom, callbackObject, callback));
	}

	/// <summary>
	/// Asynchronously load a texture from a compressed image file.
	/// </summary>
	public void EnqueueCompressedImageLoad(string file, Texture texture)
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}
		PendingLoads.Add(new LoadData(file, LoadType.CompressedImage, texture, null));
	}

	/// <summary>
	/// Asynchronously load audio from a WAV file.
	/// </summary>
	public void EnqueueWavLoad(string file, AudioBuffer buffer)
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}
		PendingLoads.Add(new LoadData(file, LoadType.AudioWav, buffer, null));
	}

	/// <summary>
	/// Asynchronously load and decode an OGG file into an audio buffer.
	/// </summary>
	public void EnqueueOggStaticLoad(string file, AudioBuffer audioBuffer)
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}
		PendingLoads.Add(new LoadData(file, LoadType.AudioOggStatic, audioBuffer, null));
	}

	/// <summary>
	/// Asynchronously load an OGG file into memory.
	/// </summary>
	public void EnqueueOggStreamingLoad(string file, AudioDataOgg audioDataOgg)
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}
		PendingLoads.Add(new LoadData(file, LoadType.AudioOggStreaming, audioDataOgg, null));
	}

	/// <summary>
	/// Asynchronously load and decode a QOA file into an audio buffer.
	/// </summary>
	public void EnqueueQoaStaticLoad(string file, AudioBuffer audioBuffer)
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}
		PendingLoads.Add(new LoadData(file, LoadType.AudioQoaStatic, audioBuffer, null));
	}

	/// <summary>
	/// Asynchronously load a QOA file into memory.
	/// </summary>
	public void EnqueueQoaStreamingLoad(string file, AudioDataQoa audioDataQoa)
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}
		PendingLoads.Add(new LoadData(file, LoadType.AudioQoaStreaming, audioDataQoa, null));
	}

	/// <summary>
	/// Submit async file loads and execute load callbacks on a thread until all are complete.
	/// </summary>
	public void Submit()
	{
		if (Status == AsyncFileLoaderStatus.Running)
		{
			Logger.LogError("AsyncFileLoader already running!");
			return;
		}

		for (var i = 0; i < PendingLoads.Count; i += 1)
		{
			if (!LoadQueue.LoadFileAsync(PendingLoads[i].File, i))
			{
				Status = AsyncFileLoaderStatus.Failed;
				Logger.LogError("Async file open failed!");
				RecreateQueue();
				return;
			}
		}

		Status = AsyncFileLoaderStatus.Running;
        Thread.Start();
	}

	// Execute load callbacks until all are complete.
    private unsafe void ThreadMain()
    {
        while (PendingLoads.Count != LoadsCompleted)
        {
            if (LoadQueue.WaitResult(out var outcome, -1))
			{
				if (outcome.Result == Result.Complete)
				{
					var loadData = PendingLoads[(int)outcome.UserData];
					var span = new ReadOnlySpan<byte>((void*)outcome.Buffer, (int)outcome.BytesTransferred);

					switch (loadData.LoadType)
					{
						case LoadType.CompressedImage:
						{
							LoadCompressedImage((Texture) loadData.Object, span);
							break;
						}

						case LoadType.AudioWav:
						{
							LoadWavData((AudioBuffer) loadData.Object, span);
							break;
						}

						case LoadType.AudioOggStatic:
						{
							LoadOggStaticData((AudioBuffer) loadData.Object, span);
							break;
						}

						case LoadType.AudioOggStreaming:
						{
							LoadOggStreamingData((AudioDataOgg) loadData.Object, span);
							break;
						}

						case LoadType.AudioQoaStatic:
						{
							LoadQoaStaticData((AudioBuffer) loadData.Object, span);
							break;
						}

						case LoadType.AudioQoaStreaming:
						{
							LoadQoaStreamingData((AudioDataQoa) loadData.Object, span);
							break;
						}

						case LoadType.Custom:
						{
							PerformLoadCallback(loadData.Callback, loadData.Object, span);
							break;
						}
					}

					SDL3.SDL.SDL_free(outcome.Buffer);
					LoadsCompleted += 1;
				}
				else if (outcome.Result == Result.Failure)
				{
					Status = AsyncFileLoaderStatus.Failed;
					Logger.LogError("Async file load failed: " + SDL3.SDL.SDL_GetError());
					RecreateQueue();
					return;
				}
			}
        }

		Status = AsyncFileLoaderStatus.Complete;
		Reset();
    }

	private void Reset()
	{
		PendingLoads.Clear();
		LoadsCompleted = 0;
	}

	// on failure, we recreate queue to finish waiting on already-submitted operations
	private void RecreateQueue()
	{
		Reset();
		LoadQueue.Destroy();
		LoadQueue = Queue.Create();
	}

	private void LoadCompressedImage(Texture texture, ReadOnlySpan<byte> data)
	{
		ResourceUploader.SetTextureDataFromCompressed(new TextureRegion(texture), data);
		ResourceUploader.Upload();
	}

	private void LoadWavData(AudioBuffer audioBuffer, ReadOnlySpan<byte> data)
	{
		AudioDataWav.SetData(audioBuffer, data);
	}

	private void LoadOggStaticData(AudioBuffer audioBuffer, ReadOnlySpan<byte> data)
	{
		AudioDataOgg.SetData(audioBuffer, data);
	}

	private void LoadOggStreamingData(AudioDataOgg audioDataOgg, ReadOnlySpan<byte> data)
	{
		audioDataOgg.Open(data);
	}

	private void LoadQoaStaticData(AudioBuffer audioBuffer, ReadOnlySpan<byte> data)
	{
		AudioDataQoa.SetData(audioBuffer, data);
	}

	private void LoadQoaStreamingData(AudioDataQoa audioDataQoa, ReadOnlySpan<byte> data)
	{
		audioDataQoa.Open(data);
	}

	private void PerformLoadCallback(OnFileLoad callback, object callbackObject, ReadOnlySpan<byte> data)
	{
		callback(callbackObject, data);
	}

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                LoadQueue.Destroy();
				ResourceUploader.Dispose();
            }

            IsDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A custom callback that can be used by AsyncFileLoader.
/// </summary>
public delegate void OnFileLoad(object Object, ReadOnlySpan<byte> buffer);

/// <summary>
/// The status of an AsyncFileLoader.
/// </summary>
public enum AsyncFileLoaderStatus
{
	/// <summary>
	/// The AsyncFileLoader is idle.
	/// Operations may be enqueued and submitted.
	/// </summary>
	Idle,
	/// <summary>
	/// The AsyncFileLoader is running submissions. 
	/// No operations can be enqueued and Submit cannot be called.
	/// </summary>
	Running,
	/// <summary>
	/// The AsyncFileLoader has completed its submissions succesfully.
	/// Operations may be enqueued and submitted.
	/// </summary>
	Complete,
	/// <summary>
	/// A submission has failed. 
	/// Operations may be enqueued and submitted.
	/// </summary>
	Failed
}