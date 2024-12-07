using System;
using System.Collections.Concurrent;

namespace MoonWorks.AsyncIO;

/// <summary>
/// A queue of completed asynchronous I/O tasks.<br/><br/>
///
/// When starting an asynchronous operation, you specify a queue for the new
/// task. A queue can be asked later if any tasks in it have completed,
/// allowing an app to manage multiple pending tasks in one place, in whatever
/// order they complete.
/// </summary>
public class Queue
{
	// A pool of Queue objects to avoid garbage collection.
	private static class QueuePool
	{
		private static readonly ConcurrentQueue<Queue> Queue = new();

		public static Queue Obtain()
		{
			if (Queue.TryDequeue(out var queue))
			{
				return queue;
			}
			else
			{
				return new Queue();
			}
		}

		public static void Return(Queue queue)
		{
			queue.Handle = IntPtr.Zero;
			Queue.Enqueue(queue);
		}
	}

	public nint Handle { get; internal set; }

	/// <summary>
	/// Create a task queue for tracking multiple I/O operations.<br/><br/>
	///
	/// Async I/O operations are assigned to a queue when started. The queue can be
	/// checked for completed tasks thereafter.
	/// </summary>
	/// <returns>A new task queue object or null if there was an error.</returns>
	public static Queue Create()
	{
		var handle = SDL_AsyncIO.SDL_CreateAsyncIOQueue();

		if (handle == IntPtr.Zero)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
			return null;
		}

		var queue = QueuePool.Obtain();
		queue.Handle = handle;
		return queue;
	}

	/// <summary>
	/// Load all the data from a file path, asynchronously.<br/><br/>
	///
	/// This function returns as quickly as possible; it does not wait for the read
	/// to complete. On a successful return, this work will continue in the
	/// background. If the work begins, even failure is asynchronous: a failing
	/// return value from this function only means the work couldn't start at all.<br/><br/>
	///
	/// The data is allocated with a zero byte at the end (null terminated) for
	/// convenience. This extra byte is not included in SDL_AsyncIOOutcome's
	/// bytes_transferred value.<br/><br/>
	///
	/// This function will allocate the buffer to contain the file. It must be
	/// deallocated by calling SDL_free() on SDL_AsyncIOOutcome's buffer field
	/// after completion.
	/// </summary>
	/// <param name="file">The path to read all available data from.</param>
	/// <param name="userdata">An app-defined pointer that will be provided with the task results.</param>
	/// <returns>True on success or false on failure.</returns>
	public bool LoadFileAsync(string file, IntPtr userdata)
	{
		var result = SDL_AsyncIO.SDL_LoadFileAsync(file, Handle, userdata);

		if (!result)
		{
			Logger.LogError(SDL3.SDL.SDL_GetError());
		}

		return result;
	}

	/// <summary>
	/// Load all the data from a file path, asynchronously.<br/><br/>
	///
	/// This function returns as quickly as possible; it does not wait for the read
	/// to complete. On a successful return, this work will continue in the
	/// background. If the work begins, even failure is asynchronous: a failing
	/// return value from this function only means the work couldn't start at all.<br/><br/>
	///
	/// The data is allocated with a zero byte at the end (null terminated) for
	/// convenience. This extra byte is not included in SDL_AsyncIOOutcome's
	/// bytes_transferred value.<br/><br/>
	///
	/// This function will allocate the buffer to contain the file. It must be
	/// deallocated by calling SDL_free() on SDL_AsyncIOOutcome's buffer field
	/// after completion.
	/// </summary>
	/// <param name="file">The path to read all available data from.</param>
	/// <returns>True on success or false on failure.</returns>
	public bool LoadFileAsync(string file) => LoadFileAsync(file, IntPtr.Zero);

	/// <summary>
	/// Query an async I/O task queue for completed tasks.<br/><br/>
	///
	/// If a task assigned to this queue has finished, this will return true and
	/// fill in `outcome` with the details of the task. If no task in the queue has
	/// finished, this function will return false. This function does not block.<br/><br/>
	///
	/// If a task has completed, this function will free its resources and the task
	/// pointer will no longer be valid. The task will be removed from the queue.<br/><br/>
	///
	/// It is safe for multiple threads to call this function on the same queue at
	/// once; a completed task will only go to one of the threads.<br/><br/>
	/// </summary>
	/// <param name="outcome">Details of a finished task will be written here.</param>
	/// <returns>True if task has completed, false otherwise.</returns>
	public bool GetResult(out Outcome outcome)
	{
		return SDL_AsyncIO.SDL_GetAsyncIOResult(Handle, out outcome);
	}

	/// <summary>
	/// Block until an async I/O task queue has a completed task.<br/><br/>
	///
	/// This function puts the calling thread to sleep until there a task assigned
	/// to the queue that has finished.<br/><br/>
	///
	/// If a task assigned to the queue has finished, this will return true and
	/// fill in `outcome` with the details of the task. If no task in the queue has
	/// finished, this function will return false.<br/><br/>
	///
	/// If a task has completed, this function will free its resources and the task
	/// pointer will no longer be valid. The task will be removed from the queue.<br/><br/>
	///
	/// It is safe for multiple threads to call this function on the same queue at
	/// once; a completed task will only go to one of the threads.<br/><br/>
	///
	/// Note that by the nature of various platforms, more than one waiting thread
	/// may wake to handle a single task, but only one will obtain it, so
	/// `timeoutMS` is a _maximum_ wait time, and this function may return false
	/// sooner.<br/><br/>
	///
	/// This function may return false if there was a system error, the OS
	/// inadvertently awoke multiple threads, or if SDL_SignalAsyncIOQueue() was
	/// called to wake up all waiting threads without a finished task.<br/><br/>
	///
	/// A timeout can be used to specify a maximum wait time, but rather than
	/// polling, it is possible to have a timeout of -1 to wait forever, and use
	/// SDL_SignalAsyncIOQueue() to wake up the waiting threads later.<br/><br/>
	/// </summary>
	/// <param name="outcome">Details of a finished task will be written here.</param>
	/// <param name="timeoutInMilliseconds">The maximum time to wait, or -1 to wait indefinitely.</param>
	///
	/// <returns>True if task has completed, false otherwise.</returns>
	public bool WaitResult(out Outcome outcome, int timeoutInMilliseconds)
	{
		return SDL_AsyncIO.SDL_WaitAsyncIOResult(Handle, out outcome, timeoutInMilliseconds);
	}

	/// <summary>
	/// Wake up any threads that are blocking in SDL_WaitAsyncIOResult().<br/><br/>
	///
	/// This will unblock any threads that are sleeping in a call to
	/// SDL_WaitAsyncIOResult for the specified queue, and cause them to return
	/// from that function.<br/><br/>
	///
	/// This can be useful when destroying a queue to make sure nothing is touching
	/// it indefinitely. In this case, once this call completes, the caller should
	/// take measures to make sure any previously-blocked threads have returned
	/// from their wait and will not touch the queue again (perhaps by setting a
	/// flag to tell the threads to terminate and then using SDL_WaitThread() to
	/// make sure they've done so).<br/><br/>
	/// </summary>
	public void Signal()
	{
		SDL_AsyncIO.SDL_SignalAsyncIOQueue(Handle);
	}

	/// <summary>
	/// Destroy a previously-created async I/O task queue.<br/><br/>
	///
	/// If there are still tasks pending for this queue, this call will block until
	/// those tasks are finished. All those tasks will be deallocated. Their
	/// results will be lost to the app.<br/><br/>
	///
	/// Any pending reads from SDL_LoadFileAsync() that are still in this queue
	/// will have their buffers deallocated by this function, to prevent a memory
	/// leak.<br/><br/>
	///
	/// Once this function is called, the queue is no longer valid and should not
	/// be used, including by other threads that might access it while destruction
	/// is blocking on pending tasks.<br/><br/>
	///
	/// Do not destroy a queue that still has threads waiting on it through
	/// SDL_WaitAsyncIOResult(). You can call SDL_SignalAsyncIOQueue() first to
	/// unblock those threads, and take measures (such as SDL_WaitThread()) to make
	/// sure they have finished their wait and won't wait on the queue again.
	/// </summary>
	public void Destroy()
	{
		SDL_AsyncIO.SDL_DestroyAsyncIOQueue(Handle);
		QueuePool.Return(this);
	}

	private Queue() { }
}
