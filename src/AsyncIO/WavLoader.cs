using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MoonWorks.Audio;

namespace MoonWorks.AsyncIO;

internal class WavLoader
{
	readonly List<AudioBuffer> WavBuffers = [];
	readonly Stack<int> AvailableIndices = [];

	public bool Idle => WavBuffers.Count == AvailableIndices.Count;

	public unsafe bool EnqueueLoad(Queue loadQueue, string file, AudioBuffer buffer)
	{
		if (AvailableIndices.TryPop(out var bufferIndex))
		{
			WavBuffers[bufferIndex] = buffer;
		}
		else
		{
			bufferIndex = WavBuffers.Count;
			WavBuffers.Add(buffer);
		}

		var loadData = new LoadData(LoadType.AudioWav, bufferIndex, 0); // extra ID unused
		var ptr = NativeMemory.Alloc((nuint)Marshal.SizeOf<LoadData>());
		NativeMemory.Copy(&loadData, ptr, (nuint)Marshal.SizeOf<LoadData>());
		return loadQueue.LoadFileAsync(file, (nint)ptr);
	}

	public void PerformLoadCallback(int bufferIndex, ReadOnlySpan<byte> span)
	{
		LoadAudioData(WavBuffers[bufferIndex], span);
		AvailableIndices.Push(bufferIndex);
		WavBuffers[bufferIndex] = null;
	}

	private void LoadAudioData(AudioBuffer audioBuffer, ReadOnlySpan<byte> span)
	{
		AudioDataWav.SetDataFromWAV(audioBuffer, span);
	}
}
