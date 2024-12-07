using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MoonWorks.Audio;
using MoonWorks.Graphics;

namespace MoonWorks.AsyncIO;

internal enum LoadType
{
	CompressedImage,
	AudioWav,
	Custom
}

public delegate void OnFileLoad(int objectID, ReadOnlySpan<byte> buffer);
internal readonly record struct LoadData(LoadType LoadType, int ObjectID, int AdditionalID);

public class AsyncIOLoader : IDisposable
{
    readonly Queue LoadQueue = Queue.Create();

	readonly CustomObjectLoader CustomObjectLoader;
	readonly TextureLoader TextureLoader;
	readonly WavLoader WavLoader;

    public bool Idle =>
		CustomObjectLoader.Idle &&
		TextureLoader.Idle &&
		WavLoader.Idle;

    bool Running = false;
	readonly Thread Thread;
    private bool IsDisposed;

    internal AsyncIOLoader(GraphicsDevice graphicsDevice)
    {
		CustomObjectLoader = new CustomObjectLoader();
		TextureLoader = new TextureLoader(graphicsDevice);
		WavLoader = new WavLoader();

        Running = true;
        Thread = new Thread(ThreadMain);
        Thread.Start();
    }

	/// <summary>
	/// Asynchronously load an arbitrary object from a file using a custom callback.
	/// The ID is provided so you can contextually identify the object in the callback.
	/// The callback will be called on a non-main thread.
	/// </summary>
	public bool EnqueueCustomObjectLoad(string file, OnFileLoad callback, int objectID)
	{
		return CustomObjectLoader.EnqueueLoad(LoadQueue, file, callback, objectID);
	}

	/// <summary>
	/// Asynchronously load a texture from a compressed image file.
	/// Once the data is available, it will be uploaded to the GPU on a non-main thread.
	/// </summary>
	public bool EnqueueCompressedImageLoad(string file, Texture texture)
	{
		return TextureLoader.EnqueueLoad(LoadQueue, file, texture);
	}

	/// <summary>
	/// Asynchronously load an audio from a WAV file.
	/// Once the data is available, it will be parsed and copied to the buffer on a non-main thread.
	/// </summary>
	public bool EnqueueWavLoad(string file, AudioBuffer buffer)
	{
		return WavLoader.EnqueueLoad(LoadQueue, file, buffer);
	}

    private unsafe void ThreadMain()
    {
        while (Running)
        {
            if (LoadQueue.WaitResult(out var outcome, -1))
			{
				if (outcome.Result == Result.Complete)
				{
					var loadData = (LoadData*)outcome.UserData;
					var span = new ReadOnlySpan<byte>((void*)outcome.Buffer, (int)outcome.BytesTransferred);

					switch (loadData->LoadType)
					{
						case LoadType.CompressedImage:
						{
							TextureLoader.PerformLoadCallback(loadData->ObjectID, span);
							break;
						}

						case LoadType.AudioWav:
						{
							WavLoader.PerformLoadCallback(loadData->ObjectID, span);
							break;
						}

						case LoadType.Custom:
						{
							CustomObjectLoader.PerformLoadCallback(loadData->AdditionalID, loadData->ObjectID, span);
							break;
						}
					}

					SDL3.SDL.SDL_free(outcome.Buffer);
				}
				else if (outcome.Result == Result.Failure)
				{
					Logger.LogError(SDL3.SDL.SDL_GetError());
				}

				NativeMemory.Free((void*) outcome.UserData);
			}
        }
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
