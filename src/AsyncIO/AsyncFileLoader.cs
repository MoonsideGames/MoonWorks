using System;
using System.Collections.Generic;
using System.Threading;
using MoonWorks.Audio;
using MoonWorks.Graphics;

namespace MoonWorks.AsyncIO;

public delegate void OnFileLoad(object Object, ReadOnlySpan<byte> buffer);

/// <summary>
/// A convenience structure for asynchronously loading data from a file.
/// When the data becomes available, it executes a contextual action on a thread.
/// Note that this object does not clean itself up as it executes.
/// A good pattern is to issue load requests, wait for Idle to be true,
/// and then call Dispose.
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

    public bool Idle => LoadDatas.Count == LoadsCompleted;

	int LoadsCompleted = 0;

    bool Running = false;
	readonly Thread Thread;
    private bool IsDisposed;

    public AsyncFileLoader(GraphicsDevice graphicsDevice)
    {
		GraphicsDevice = graphicsDevice;
		ResourceUploader = new ResourceUploader(GraphicsDevice);

        Running = true;
        Thread = new Thread(ThreadMain);
        Thread.Start();
    }

	/// <summary>
	/// Asynchronously load an arbitrary object from a file using a custom callback.
	/// The ID is provided so you can contextually identify the object in the callback.
	/// The callback will be called on a non-main thread.
	/// </summary>
	public bool EnqueueCustomObjectLoad(string file, OnFileLoad callback, object callbackObject)
	{
		LoadDatas.Add(new LoadData(LoadType.Custom, callbackObject, callback));
		return LoadQueue.LoadFileAsync(file, LoadDatas.Count - 1);
	}


	/// <summary>
	/// Asynchronously load a texture from a compressed image file.
	/// Once the data is available, it will be uploaded to the GPU on a non-main thread.
	/// </summary>
	public bool EnqueueCompressedImageLoad(string file, Texture texture)
	{
		LoadDatas.Add(new LoadData(LoadType.CompressedImage, texture, null));
		return LoadQueue.LoadFileAsync(file, LoadDatas.Count - 1);
	}

	/// <summary>
	/// Asynchronously load an audio from a WAV file.
	/// Once the data is available, it will be parsed and copied to the buffer on a non-main thread.
	/// </summary>
	public bool EnqueueWavLoad(string file, AudioBuffer buffer)
	{
		LoadDatas.Add(new LoadData(LoadType.AudioWav, buffer, null));
		return LoadQueue.LoadFileAsync(file, LoadDatas.Count - 1);
	}

    private unsafe void ThreadMain()
    {
        while (Running)
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
					Logger.LogError(SDL3.SDL.SDL_GetError());
				}
			}
        }
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
				Running = false;
                LoadQueue.Signal();
                Thread.Join();
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
