using System;

namespace MoonWorks.Storage;

public interface IStorage
{
	/// <summary>
    /// Check if the file exists or not.
    /// </summary>
    /// <param name="path">A path relative to the title root.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
	public bool Exists(string path);

	/// <summary>
    /// Query the size of a file within a storage container.
    /// </summary>
    /// <param name="path">A path relative to the title root.</param>
    /// <param name="size">Filled in with the size of the file.</param>
    /// <returns>True if the query succeeded, false otherwise.</returns>
	public bool GetFileSize(string path, out ulong size);

	/// <summary>
	/// Synchronously read a file and return the contents in a ReadOnlySpan.
	/// TitleStorage will allocate the file memory for you.
	/// You MUST call NativeMemory.Free when you are done with the memory.
	/// </summary>
    /// <param name="path">The relative path from the title root.</param>
	/// <param name="size">The size of the file in bytes.</param>
	/// <returns>A buffer of the file size on success, null on failure.</returns>
	public unsafe byte* ReadFile(string path, out ulong size);

	/// <summary>
	/// Synchronously read a file into a client-provided Span.
	/// The span must be the same length as the file size.
	/// </summary>
    /// <param name="path">The relative path from the title root.</param>
	/// <returns>True on success, false on failure.</returns>
	public bool ReadFile(string path, ReadOnlySpan<byte> span);

	/// <summary>
	/// Synchronously read a file and return the contents in a managed array.
	/// Calling this will cause GC pressure, use one of the ReadFile methods to avoid that.
	/// </summary>
    /// <param name="path">The relative path from the title root.</param>
	/// <returns>An array of the file data on success, a zero-length array on failure.</returns>
	public byte[] ReadFileManaged(string path);
}
