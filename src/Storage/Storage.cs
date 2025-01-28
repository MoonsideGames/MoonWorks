using System;

namespace MoonWorks.Storage;

public interface IStorage
{
	public bool Exists(string path);
	public bool GetFileSize(string path, out ulong size);
	public unsafe byte* ReadFile(string path, out ulong size);
	public bool ReadFile(string path, ReadOnlySpan<byte> span);
	public byte[] ReadFileManaged(string path);
}
