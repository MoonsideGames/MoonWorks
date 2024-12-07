using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MoonWorks.Graphics;

namespace MoonWorks.AsyncIO;

internal class TextureLoader
{
	GraphicsDevice GraphicsDevice { get; }

	readonly ConcurrentDictionary<int, Texture> Textures = [];
	readonly ConcurrentStack<int> AvailableIndices = [];
	int nextIndex = 0;

	public bool Idle => Textures.IsEmpty;

	public TextureLoader(GraphicsDevice graphicsDevice)
	{
		GraphicsDevice = graphicsDevice;
	}

	public unsafe bool EnqueueLoad(Queue loadQueue, string file, Texture texture)
	{
		if (AvailableIndices.TryPop(out var textureIndex))
		{
			Textures[textureIndex] = texture;
		}
		else
		{
			nextIndex = Interlocked.Increment(ref nextIndex);
			Textures.TryAdd(nextIndex, texture);
			textureIndex = nextIndex;
		}

		var loadData = new LoadData(LoadType.CompressedImage, textureIndex, 0); // extra ID unused
		var ptr = NativeMemory.Alloc((nuint)Marshal.SizeOf<LoadData>());
		NativeMemory.Copy(&loadData, ptr, (nuint)Marshal.SizeOf<LoadData>());
		return loadQueue.LoadFileAsync(file, (nint)ptr);
	}

	public void PerformLoadCallback(int textureIndex, ReadOnlySpan<byte> span)
	{
		if (!Textures.TryRemove(textureIndex, out var texture))
		{
			throw new InvalidOperationException("oh no!");
		}

		LoadCompressedImage(texture, span);
		AvailableIndices.Push(textureIndex);
	}

	private void LoadCompressedImage(Texture texture, ReadOnlySpan<byte> data)
	{
		var resourceUploader = new ResourceUploader(GraphicsDevice);
		resourceUploader.SetTextureDataFromCompressed(new TextureRegion(texture), data);
		resourceUploader.Upload();
		resourceUploader.Dispose();
	}
}
