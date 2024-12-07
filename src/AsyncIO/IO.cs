using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MoonWorks.AsyncIO;

/// <summary>
/// The asynchronous IO operation structure.
/// One can request read or write operations on it.
/// </summary>
public class IO
{
	// A pool of IO objects to avoid garbage collection.
	private static class IOPool
	{
		private static readonly ConcurrentQueue<IO> Queue = new();

		public static IO Obtain()
		{
			if (Queue.TryDequeue(out var io))
			{
				return io;
			}
			else
			{
				return new IO();
			}
		}

		public static void Return(IO io)
		{
			io.Handle = IntPtr.Zero;
			Queue.Enqueue(io);
		}
	}

	public enum FileMode
	{
		/// <summary>
		/// Open file for reading only. The file must exist.
		/// </summary>
		ReadOnly,
		/// <summary>
		/// Open file for writing only. This will create missing files or truncate existing ones.
		/// </summary>
		WriteOnly,
		/// <summary>
		/// Open a file for update both reading and writing. The file must exist.
		/// </summary>
		ReadWriteExisting,
		/// <summary>
		/// Create an empty file for both reading and writing. If a file with the same name already exists its content is eraseed.
		/// </summary>
		ReadWriteNew
	}

	public nint Handle { get; internal set; }

	/// <summary>
	/// Use this function to create a new SDL_AsyncIO object for reading from
	/// and/or writing to a named file.<br/><br/>
	///
	/// This function supports Unicode filenames, but they must be encoded in UTF-8
	/// format, regardless of the underlying operating system.<br/><br/>
	///
	/// This call is NOT asynchronous; it will open the file before returning,
	/// under the assumption that doing so is generally a fast operation. Future
	/// reads and writes to the opened file will be async, however.
	/// </summary>
	/// <param name="file">A string representing the filename to open.</param>
	/// <param name="mode">The mode to be used for opening the file.</param>
	/// <returns>A valid IO object or null on failure.</returns>
	public static IO FromFile(string file, FileMode mode)
	{
		string modeString;
		switch (mode)
		{
			case FileMode.ReadOnly:
				modeString = "r";
				break;

			case FileMode.WriteOnly:
				modeString = "w";
				break;

			case FileMode.ReadWriteExisting:
				modeString = "r+";
				break;

			case FileMode.ReadWriteNew:
				modeString = "w+";
				break;

			default:
				Logger.LogError("Unrecognized async IO file mode!");
				return null;
		}

		var handle = SDL_AsyncIO.SDL_AsyncIOFromFile(file, modeString);

		if (handle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var io = IOPool.Obtain();
		io.Handle = handle;
		return io;
	}

	/// <summary>
	/// Use this function to get the size of the data stream in an SDL_AsyncIO.<br/><br/>
	///
	/// This call is NOT asynchronous; it assumes that obtaining this info is a
	/// non-blocking operation in most reasonable cases.
	/// </summary>
	/// <returns>The size of the data stream or a negative error code on failure.</returns>
	public long GetSize()
	{
		var result = SDL_AsyncIO.SDL_GetAsyncIOSize(Handle);

		if (result < 0)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
		}

		return result;
	}

	/// <summary>
	/// Start an async read.<br/><br/>
	///
	/// This function reads up to `size` bytes from `offset` position in the data
	/// source to the area pointed at by `ptr`. This function may read less bytes
	/// than requested.<br/><br/>
	///
	/// This function returns as quickly as possible; it does not wait for the read
	/// to complete. On a successful return, this work will continue in the
	/// background. If the work begins, even failure is asynchronous: a failing
	/// return value from this function only means the work couldn't start at all.<br/><br/>
	///
	/// `ptr` must remain available until the work is done, and may be accessed by
	/// the system at any time until then. Do not allocate it on the stack, as this
	/// might take longer than the life of the calling function to complete!<br/><br/>
	///
	/// An SDL_AsyncIOQueue must be specified. The newly-created task will be added
	/// to it when it completes its work.<br/><br/>
	/// </summary>
	/// <param name="buffer">A pointer to a buffer to read data into.</param>
	/// <param name="offset">The position to start reading in the data source.</param>
	/// <param name="size">The number of bytes to read from the data source.</param>
	/// <param name="queue">A queue to add the new read operation to.</param>
	/// <param name="userdata">An app-defined pointer that will be provided with the task results.</param>
	/// <returns>True on success or false on failure.</returns>
	public bool Read(IntPtr buffer, ulong offset, ulong size, Queue queue, IntPtr userdata)
	{
		var result = SDL_AsyncIO.SDL_ReadAsyncIO(Handle, buffer, offset, size, queue.Handle, userdata);

		if (!result)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
		}

		return result;
	}

	/// <summary>
	/// Start an async write.
	///
	/// This function writes `size` bytes from `offset` position in the data source
	/// to the area pointed at by `ptr`.
	///
	/// This function returns as quickly as possible; it does not wait for the
	/// write to complete. On a successful return, this work will continue in the
	/// background. If the work begins, even failure is asynchronous: a failing
	/// return value from this function only means the work couldn't start at all.
	///
	/// `ptr` must remain available until the work is done, and may be accessed by
	/// the system at any time until then. Do not allocate it on the stack, as this
	/// might take longer than the life of the calling function to complete!
	///
	/// An SDL_AsyncIOQueue must be specified. The newly-created task will be added
	/// to it when it completes its work.
	/// </summary>
	/// <param name="buffer">A pointer to a buffer to write data from.</param>
	/// <param name="offset">The position to start writing to the data source.</param>
	/// <param name="size">The number of bytes to write to the data source.</param>
	/// <param name="queue">A queue to add the new write operation to.</param>
	/// <param name="userdata">An app-defined pointer that will be provided with the task results.</param>
	/// <returns>True on success or false on failure.</returns>
	public bool Write(IntPtr buffer, ulong offset, ulong size, Queue queue, IntPtr userdata)
	{
		var result = SDL_AsyncIO.SDL_WriteAsyncIO(Handle, buffer, offset, size, queue.Handle, userdata);

		if (!result)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
		}

		return result;
	}

	/// <summary>
	/// Close and free any allocated resources for an async I/O object.<br/>
	///
	/// Closing a file is also an asynchronous task! If a write failure were to happen during the closing process, for example, the task results will report it as usual.<br/>
	///
	/// Closing a file that has been written to does not guarantee the data has made it to physical media; it may remain in the operating system's file cache, for later writing to disk.
	/// This means that a successfully-closed file can be lost if the system crashes or loses power in this small window.
	/// To prevent this, call this function with the `flush` parameter set to true.
	/// This will make the operation take longer, and perhaps increase system load
	/// in general, but a successful result guarantees that the data has made it to physical storage. Don't use this for temporary files, caches, and unimportant data, and definitely use it for crucial irreplaceable files, like game saves.<br/>
	///
	/// If this function returns false, the close wasn't started at all, and it's safe to attempt to close again later.<br/>
	/// </summary>
	/// <param name="flush">True if data should sync to disk before task completes.</param>
	/// <param name="queue">A Queue to add the close operation to.</param>
	/// <param name="userdata">An app-defined pointer that will be provided with the task results.</param>
	/// <returns></returns>
	public bool Close(bool flush, Queue queue, IntPtr userdata)
	{
		var result = SDL_AsyncIO.SDL_CloseAsyncIO(Handle, flush, queue.Handle, userdata);

		if (!result)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return false;
		}

		IOPool.Return(this);
		return result;
	}

	/// <summary>
	/// Start an async read.<br/><br/>
	///
	/// This function reads up to `size` bytes from `offset` position in the data
	/// source to the area pointed at by `ptr`. This function may read less bytes
	/// than requested.<br/><br/>
	///
	/// This function returns as quickly as possible; it does not wait for the read
	/// to complete. On a successful return, this work will continue in the
	/// background. If the work begins, even failure is asynchronous: a failing
	/// return value from this function only means the work couldn't start at all.<br/><br/>
	///
	/// `ptr` must remain available until the work is done, and may be accessed by
	/// the system at any time until then. Do not allocate it on the stack, as this
	/// might take longer than the life of the calling function to complete!<br/><br/>
	///
	/// An SDL_AsyncIOQueue must be specified. The newly-created task will be added
	/// to it when it completes its work.<br/><br/>
	/// </summary>
	/// <param name="buffer">A pointer to a buffer to read data into.</param>
	/// <param name="offset">The position to start reading in the data source.</param>
	/// <param name="size">The number of bytes to read from the data source.</param>
	/// <param name="queue">A queue to add the new read operation to.</param>
	/// <returns>True on success or false on failure.</returns>
	public bool Read(IntPtr buffer, ulong offset, ulong size, Queue queue) => Read(buffer, offset, size, queue, IntPtr.Zero);

	/// <summary>
	/// Start an async write.
	///
	/// This function writes `size` bytes from `offset` position in the data source
	/// to the area pointed at by `ptr`.
	///
	/// This function returns as quickly as possible; it does not wait for the
	/// write to complete. On a successful return, this work will continue in the
	/// background. If the work begins, even failure is asynchronous: a failing
	/// return value from this function only means the work couldn't start at all.
	///
	/// `ptr` must remain available until the work is done, and may be accessed by
	/// the system at any time until then. Do not allocate it on the stack, as this
	/// might take longer than the life of the calling function to complete!
	///
	/// An SDL_AsyncIOQueue must be specified. The newly-created task will be added
	/// to it when it completes its work.
	/// </summary>
	/// <param name="buffer">A pointer to a buffer to write data from.</param>
	/// <param name="offset">The position to start writing to the data source.</param>
	/// <param name="size">The number of bytes to write to the data source.</param>
	/// <param name="queue">A queue to add the new write operation to.</param>
	/// <returns>True on success or false on failure.</returns>
	public bool Write(IntPtr buffer, ulong offset, ulong size, Queue queue) => Write(buffer, offset, size, queue, IntPtr.Zero);

	/// <summary>
	/// Close and free any allocated resources for an async I/O object.<br/>
	///
	/// Closing a file is also an asynchronous task! If a write failure were to happen during the closing process, for example, the task results will report it as usual.<br/>
	///
	/// Closing a file that has been written to does not guarantee the data has made it to physical media; it may remain in the operating system's file cache, for later writing to disk.
	/// This means that a successfully-closed file can be lost if the system crashes or loses power in this small window.
	/// To prevent this, call this function with the `flush` parameter set to true.
	/// This will make the operation take longer, and perhaps increase system load
	/// in general, but a successful result guarantees that the data has made it to physical storage. Don't use this for temporary files, caches, and unimportant data, and definitely use it for crucial irreplaceable files, like game saves.<br/>
	///
	/// If this function returns false, the close wasn't started at all, and it's safe to attempt to close again later.<br/>
	/// </summary>
	/// <param name="flush">True if data should sync to disk before task completes.</param>
	/// <param name="queue">A Queue to add the close operation to.</param>
	/// <returns></returns>
	public bool Close(bool flush, Queue queue) => Close(flush, queue, IntPtr.Zero);

	private IO() { }
}
