using System;
using System.Collections.Generic;
using System.Threading;
using MoonWorks.Audio;
using MoonWorks.Graphics;

namespace MoonWorks.AsyncIO;

public delegate void OnFileLoad(object Object, ReadOnlySpan<byte> buffer);

public enum AsyncFileLoaderStatus
{
	Idle,
	Running,
	Complete,
	Failed
}

/// <summary>
/// A convenience structure for asynchronously loading data from a file.
/// When Submit is called, contextual actions are executed on a thread for every file load that is completed.
/// </summary>
public class AsyncFileLoader : IDisposable
{
	enum LoadType
	{
		CompressedImage,
		AudioWav,
		Custom
	}

	readonly record struct LoadData(
		LoadType LoadType,
		object Object,
		OnFileLoad Callback // only used with LoadType.Custom
	);

    readonly Queue LoadQueue = Queue.Create();

	GraphicsDevice GraphicsDevice;
	ResourceUploader ResourceUploader;

	List<LoadData> LoadDatas = [];
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
	/// On Submit, the callback will be called on a non-main thread.
	/// </summary>
	public void EnqueueCustomObjectLoad(string file, OnFileLoad callback, object callbackObject)
	{
		LoadDatas.Add(new LoadData(LoadType.Custom, callbackObject, callback));
		var result = LoadQueue.LoadFileAsync(file, LoadDatas.Count - 1);
		if (!result)
		{
			Status = AsyncFileLoaderStatus.Failed;
		}
	}

	/// <summary>
	/// Asynchronously load a texture from a compressed image file.
	/// On Submit, the data will be uploaded to the GPU on a non-main thread.
	/// </summary>
	public void EnqueueCompressedImageLoad(string file, Texture texture)
	{
		LoadDatas.Add(new LoadData(LoadType.CompressedImage, texture, null));
		var result = LoadQueue.LoadFileAsync(file, LoadDatas.Count - 1);
		if (!result)
		{
			Status = AsyncFileLoaderStatus.Failed;
		}
	}

	/// <summary>
	/// Asynchronously load an audio from a WAV file.
	/// On Submit, the data will be parsed and copied to the buffer on a non-main thread.
	/// </summary>
	public void EnqueueWavLoad(string file, AudioBuffer buffer)
	{
		LoadDatas.Add(new LoadData(LoadType.AudioWav, buffer, null));
		var result = LoadQueue.LoadFileAsync(file, LoadDatas.Count - 1);
		if (!result)
		{
			Status = AsyncFileLoaderStatus.Failed;
		}
	}

	/// <summary>
	/// Execute load callbacks on a thread until all are complete.
	/// </summary>
	public void Submit()
	{
		if (Status == AsyncFileLoaderStatus.Failed)
		{
			Logger.LogError("Async operation already failed, call Reset and re-submit to continue");
			return;
		}

		Status = AsyncFileLoaderStatus.Running;
        Thread.Start();
	}

	public void Reset()
	{
		LoadDatas.Clear();
		LoadsCompleted = 0;
	}

	// Execute load callbacks until all are complete.
    private unsafe void ThreadMain()
    {
        while (LoadDatas.Count != LoadsCompleted)
        {
            if (LoadQueue.WaitResult(out var outcome, -1))
			{
				if (outcome.Result == Result.Complete)
				{
					var loadData = LoadDatas[(int)outcome.UserData];
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
					Logger.LogError(SDL3.SDL.SDL_GetError());
					return;
				}
			}
        }

		Status = AsyncFileLoaderStatus.Complete;
		Reset();
    }

	private void LoadCompressedImage(Texture texture, ReadOnlySpan<byte> data)
	{
		ResourceUploader.SetTextureDataFromCompressed(new TextureRegion(texture), data);
		ResourceUploader.Upload();
	}

	private void LoadWavData(AudioBuffer audioBuffer, ReadOnlySpan<byte> span)
	{
		AudioDataWav.SetDataFromWAV(audioBuffer, span);
	}

	private void PerformLoadCallback(OnFileLoad callback, object callbackObject, ReadOnlySpan<byte> span)
	{
		callback(callbackObject, span);
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
