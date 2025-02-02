using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using SDL3;

namespace MoonWorks.Storage;

/// <summary>
/// A read-write container in userspace.
/// These containers have to be mounted, which takes time.
/// For this reason, the operations are buffered.
/// </summary>
public class UserStorage : IDisposable
{
	private AppInfo AppInfo;

	internal ResultTokenPool ResultTokenPool = new();
	private CommandBufferPool CommandBufferPool;
	private BlockingCollection<CommandBuffer> PendingCommandBuffers = [];

	private bool Running = false;
	private Thread Thread;

	private bool IsDisposed;

	internal UserStorage(AppInfo appInfo)
	{
		AppInfo = appInfo;
		CommandBufferPool = new CommandBufferPool(this);

		Running = true;
		Thread = new Thread(ThreadMain);
		Thread.Start();
	}

	/// <summary>
	/// Acquires a command buffer.
	/// All user storage operations must be requested via command buffer.
	/// All operations are deferred until the command buffer is submitted.
	/// </summary>
	public CommandBuffer AcquireCommandBuffer()
	{
		return CommandBufferPool.Obtain();
	}

	/// <summary>
	/// Submits a storage command buffer for processing.
	/// </summary>
	/// <param name="commandBuffer"></param>
	public void Submit(CommandBuffer commandBuffer)
	{
		PendingCommandBuffers.Add(commandBuffer);
	}

	/// <summary>
	/// Releases a ResultToken so it can be reused later.
	/// You must call this or you will cause GC pressure.
	/// </summary>
	public void ReleaseToken(ResultToken token)
	{
		ResultTokenPool.Return(token);
	}

	private void ThreadMain()
	{
		while (Running)
		{
			var commandBuffer = PendingCommandBuffers.Take();

			if (commandBuffer.Cancel)
			{
				break;
			}

			if (Open(out var handle))
			{
				// Wait until storage is ready
				if (!SDL.SDL_StorageReady(handle))
				{
					SDL.SDL_Delay(1);
				}

				foreach (var command in commandBuffer.Commands)
				{
					ProcessCommand(handle, command);
				}

				Close(handle);
			}
			else
			{
				Logger.LogError("Failed to open user storage!");

				foreach (var command in commandBuffer.Commands)
				{
					command.ResultToken.Result = Result.Failure;

					if (command.Type == CommandType.WriteFile)
					{
						command.ResultToken.Buffer = command.WriteFileCommand.Buffer;
					}
				}
			}

			commandBuffer.Commands.Clear();
			CommandBufferPool.Return(commandBuffer);
		}
	}

	private unsafe void ProcessCommand(IntPtr handle, Command command)
	{
		switch (command.Type)
		{
			case CommandType.GetSpaceRemaining:
			{
				command.ResultToken.Size = SDL.SDL_GetStorageSpaceRemaining(handle);
				command.ResultToken.Result = Result.Success;
				return;
			}

			case CommandType.GetFileSize:
			{
				// FIXME: Can SDL3-CS just take a byte* overload for strings to avoid this silly round trip?
				var str = InteropUtilities.DecodeFromUTF8Buffer((byte*) command.WriteFileCommand.Path, command.WriteFileCommand.PathLength);
				NativeMemory.Free((void*) command.GetFileSizeCommand.Path);

				var success = SDL.SDL_GetStorageFileSize(handle, str, out command.ResultToken.Size);
				command.ResultToken.Result = success ? Result.Success : Result.Failure;
				return;
			}

			case CommandType.ReadFile:
			{
				// FIXME: Can SDL3-CS just take a byte* overload for strings to avoid this silly round trip?
				var str = InteropUtilities.DecodeFromUTF8Buffer((byte*) command.WriteFileCommand.Path, command.WriteFileCommand.PathLength);
				NativeMemory.Free((void*) command.ReadFileCommand.Path);

				command.ResultToken.Buffer = ReadFile(handle, str, out command.ResultToken.Size);
				command.ResultToken.Result = command.ResultToken.Buffer == IntPtr.Zero ? Result.Failure : Result.Success;
				return;
			}

			case CommandType.WriteFile:
			{
				// FIXME: Can SDL3-CS just take a byte* overload for strings to avoid this silly round trip?
				var str = InteropUtilities.DecodeFromUTF8Buffer((byte*) command.WriteFileCommand.Path, command.WriteFileCommand.PathLength);
				NativeMemory.Free((void*) command.WriteFileCommand.Path);

				var success = WriteFile(handle, str, command.WriteFileCommand.Buffer, command.WriteFileCommand.Size);
				command.ResultToken.Result = success ? Result.Success : Result.Failure;
				return;
			}

			default:
				Logger.LogError("Unrecognized UserStorage command! This shouldn't happen!");
				return;
		}
	}

	/// <summary>
	/// Opens the storage container so files can be read and written.
	/// </summary>
	/// <param name="propertiesID">An optional property list that may contain backend-specific information.</param>
	/// <returns>True on success, false on failure.</returns>
	private bool Open(out IntPtr handle)
	{
		handle = SDL.SDL_OpenUserStorage(AppInfo.OrganizationName, AppInfo.ApplicationName, 0);
		if (handle == IntPtr.Zero)
		{
			Logger.LogError(SDL.SDL_GetError());
			return false;
		}

		return true;
	}

    /// <summary>
    /// Closes the storage container.
    /// </summary>
    private void Close(IntPtr handle)
    {
        if (!SDL.SDL_CloseStorage(handle))
        {
            Logger.LogError(SDL.SDL_GetError());
        }
    }

    /// <summary>
    /// Query the size of a file within a storage container.
    /// </summary>
    /// <param name="path">A path relative to the title root.</param>
    /// <param name="size">Filled in with the size of the file.</param>
    /// <returns>True if the query succeeded, false otherwise.</returns>
    private static bool GetFileSize(IntPtr handle, string path, out ulong size)
    {
        if (!SDL.SDL_GetStorageFileSize(handle, path, out size))
        {
            Logger.LogError(SDL.SDL_GetError());
            return false;
        }

        return true;
    }

	/// <summary>
	/// Synchronously read a file and return the contents in a ReadOnlySpan.
	/// UserStorage will allocate the file memory for you.
	/// You MUST call NativeMemory.Free when you are done with the memory.
	/// </summary>
    /// <param name="path">The relative path from the title root.</param>
	/// <param name="size">The size of the file in bytes.</param>
	/// <returns>A buffer of the file size on success, null on failure.</returns>
	private static unsafe IntPtr ReadFile(IntPtr handle, string path, out ulong size)
	{
		if (!GetFileSize(handle, path, out size))
		{
			return IntPtr.Zero;
		}

		var buffer = (nint) NativeMemory.Alloc((nuint) size);
		var span = new ReadOnlySpan<byte>((void*) buffer, (int) size);

		fixed (byte* spanPtr = span)
		{
			if (!SDL.SDL_ReadStorageFile(handle, path, (nint) spanPtr, size))
			{
				Logger.LogError(SDL.SDL_GetError());
				return IntPtr.Zero;
			}
		}

		return buffer;
	}

	/// <summary>
	/// Synchronously write a file from client memory into the storage container.
	/// This function might be rate-limited on certain platforms - you can fail certification for writing too often.
	/// Be smart and minimize the frequency at which you call this function.
	/// </summary>
	/// <param name="path">A path.</param>
	/// <param name="span">The data to write into the file.</param>
	/// <returns>True on success, false on failure.</returns>
	private bool WriteFile(IntPtr handle, string path, IntPtr buffer, ulong size)
	{
		if (!SDL.SDL_WriteStorageFile(handle, path, buffer, size))
		{
			Logger.LogError(SDL.SDL_GetError());
			return false;
		}

		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			Running = false;

			// Submit a "cancel" command buffer to shut down the thread loop
			var commandBuffer = AcquireCommandBuffer();
			commandBuffer.Cancel = true;
			Submit(commandBuffer);

			if (disposing)
			{
				// dispose managed state
				Thread.Join();
			}

			IsDisposed = true;
		}
	}

	~UserStorage()
	{
	    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		#if DEBUG
		Logger.LogWarn($"UserStorage was not Disposed!");
		#endif

	    Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
