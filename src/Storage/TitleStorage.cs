using System;
using SDL3;

namespace MoonWorks;

/// <summary>
/// Read-only abstraction over platform file storage. Use this instead of System.IO for maximum portability.
/// </summary>
public class TitleStorage
{
    public IntPtr Handle { get; private set; }

    /// <summary>
    /// Opens up a read-only container for the application's filesystem.
    /// It's OK to leave this open long term but make sure to close it on shutdown.
    /// </summary>
    /// <param name="overrideRoot">A path to override the default root. Null will use the default root.</param>
    /// <param name="propertiesID">An optional property list that may contain backend-specific information.</param>
    /// <returns></returns>
    public bool Open(string overrideRoot, uint propertiesID = 0)
    {
        if (Handle != IntPtr.Zero)
        {
            Logger.LogError("Storage already open! Close it first!");
            return false;
        }

        var handle = SDL.SDL_OpenTitleStorage(overrideRoot, propertiesID);
        if (handle == IntPtr.Zero)
        {
            Logger.LogError(SDL.SDL_GetError());
            return false;
        }

        Handle = handle;
        return true;
    }

    /// <summary>
    /// Opens up a read-only container for the application's filesystem.
    /// It's OK to leave this open long term but make sure to close it on shutdown.
    /// </summary>
    /// <param name="propertiesID">An optional property list that may contain backend-specific information.</param>
    /// <returns></returns>
    public bool Open(uint propertiesID = 0) => Open(null, propertiesID);

    /// <summary>
    /// Closes the storage container.
    /// </summary>
    public void Close()
    {
        if (!SDL.SDL_CloseStorage(Handle))
        {
            Logger.LogError(SDL.SDL_GetError());
        }

        Handle = IntPtr.Zero;
    }

    /// <summary>
    /// Check if the file exists or not.
    /// </summary>
    /// <param name="path">A path relative to the title root.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    public bool Exists(string path)
    {
        // FIXME: is it possible to pass null to the out var here?
        return SDL.SDL_GetStoragePathInfo(Handle, path, out var _);
    }

    /// <summary>
    /// Query the size of a file within a storage container.
    /// </summary>
    /// <param name="path">A path relative to the title root.</param>
    /// <param name="size">Filled in with the size of the file.</param>
    /// <returns>True if the query succeeded, false otherwise.</returns>
    public bool GetFileSize(string path, out ulong size)
    {
        if (!SDL.SDL_GetStorageFileSize(Handle, path, out size))
        {
            Logger.LogError(SDL.SDL_GetError());
            return false;
        }

        return true;
    }

    /// <summary>
    /// Synchronously read a file into a client-provided buffer.
    /// The provided span must match the file length exactly. Call GetFileSize to get this information.
    /// </summary>
    /// <param name="path">The relative path from the title root.</param>
    /// <param name="destination">The span to read the data into.</param>
    /// <returns></returns>
    public unsafe bool ReadFile(string path, ReadOnlySpan<byte> destination)
    {
        fixed (byte* ptr = destination)
        {
            if (!SDL.SDL_ReadStorageFile(Handle, path, (nint) ptr, (ulong) destination.Length))
            {
                Logger.LogError(SDL.SDL_GetError());
                return false;
            }

            return true;
        }
    }

    public TitleStorage() { Handle = IntPtr.Zero; }
}
