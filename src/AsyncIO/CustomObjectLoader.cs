using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MoonWorks.AsyncIO;

internal class CustomObjectLoader
{
	readonly List<OnFileLoad> Callbacks = [];
	readonly Stack<int> AvailableIndices = [];

	public bool Idle => AvailableIndices.Count == Callbacks.Count;

	public unsafe bool EnqueueLoad(Queue loadQueue, string file, OnFileLoad callback, int objectID)
	{
		if (AvailableIndices.TryPop(out var callbackIndex))
		{
			Callbacks[callbackIndex] = callback;
		}
		else
		{
			callbackIndex = Callbacks.Count;
			Callbacks.Add(callback);
		}

		var loadData = new LoadData(LoadType.Custom, objectID, callbackIndex);
		var ptr = NativeMemory.Alloc((nuint)Marshal.SizeOf<LoadData>());
		NativeMemory.Copy(&loadData, ptr, (nuint)Marshal.SizeOf<LoadData>());
		return loadQueue.LoadFileAsync(file, (nint)ptr);
	}

	public void PerformLoadCallback(int callbackID, int objectID, ReadOnlySpan<byte> span)
	{
		var callback = Callbacks[callbackID];
		callback(objectID, span);
		AvailableIndices.Push(callbackID);
		Callbacks[callbackID] = null;
	}
}
