using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MoonWorks.Storage;

/// <summary>
/// UserStorage operations have to run asynchronously.
/// The storage CommandBuffer enqueues storage operations.
/// When you call UserStorage.Submit, the operations will be executed on a thread.
/// Each operation gives you back a ResultToken that you can check to obtain the status of the operation and the resulting data.
/// When you are done with a ResultToken, you must call UserStorage.ReleaseToken or the token will cause GC pressure.
/// </summary>
public class CommandBuffer
{
	UserStorage UserStorage;
	internal List<Command> Commands = [];
	internal bool Cancel; // janky bool to avoid having to do CancelToken exception crap from the thread

	internal CommandBuffer(UserStorage userStorage)
	{
		UserStorage = userStorage;
	}

	/// <summary>
	/// Gets the amount of space remaining in storage in bytes.
	/// </summary>
	public ResultToken GetSpaceRemaining()
	{
		var resultToken = UserStorage.ResultTokenPool.Obtain();

		Commands.Add(new Command
		{
			Type = CommandType.GetSpaceRemaining,
			ResultToken = resultToken,
			GetSpaceRemainingCommand = new GetSpaceRemainingCommand()
		});

		return resultToken;
	}

	/// <summary>
	/// Gets the size of a file in storage in bytes.
	/// </summary>
	public unsafe ResultToken GetFileSize(string path)
	{
		var resultToken = UserStorage.ResultTokenPool.Obtain();
		var pathBuffer = InteropUtilities.EncodeToUTF8Buffer(path, out var length);

		Commands.Add(new Command
		{
			Type = CommandType.GetFileSize,
			ResultToken = resultToken,
			GetFileSizeCommand = new GetFileSizeCommand
			{
				Path = (nint) pathBuffer,
				PathLength = length
			}
		});

		return resultToken;
	}

	/// <summary>
	/// Reads the entire contents of a file in storage into a buffer.
	/// The buffer must stay alive until this operation is complete.
	/// </summary>
	public unsafe ResultToken ReadFile(string path)
	{
		var resultToken = UserStorage.ResultTokenPool.Obtain();
		var pathBuffer = InteropUtilities.EncodeToUTF8Buffer(path, out var length);

		Commands.Add(new Command
		{
			Type = CommandType.ReadFile,
			ResultToken = resultToken,
			ReadFileCommand = new ReadFileCommand
			{
				Path = (nint) pathBuffer,
				PathLength = length
			}
		});

		return resultToken;
	}

	/// <summary>
	/// Writes the contents of a buffer into a file in storage.
	/// The buffer must remain alive until the operation is complete.
	/// </summary>
	public unsafe ResultToken WriteFile(string path, IntPtr buffer, ulong size)
	{
		var resultToken = UserStorage.ResultTokenPool.Obtain();
		var pathBuffer = InteropUtilities.EncodeToUTF8Buffer(path, out var length);

		Commands.Add(new Command
		{
			Type = CommandType.WriteFile,
			ResultToken = resultToken,
			WriteFileCommand = new WriteFileCommand
			{
				Path = (nint) pathBuffer,
				PathLength = length,
				Buffer = buffer,
				Size = size
			}
		});

		return resultToken;
	}
}

enum CommandType : uint
{
	GetSpaceRemaining,
	GetFileSize,
	ReadFile,
	WriteFile,
}

struct GetSpaceRemainingCommand
{
	// empty
}

struct GetFileSizeCommand
{
	public IntPtr Path; // marshalled!
	public int PathLength;
}

struct ReadFileCommand
{
	public IntPtr Path; // marshalled!
	public int PathLength;
}

struct WriteFileCommand
{
	public IntPtr Path; // marshalled!
	public int PathLength;
	public IntPtr Buffer;
	public ulong Size;
}

[StructLayout(LayoutKind.Explicit, Size = 96)]
struct Command
{
	[FieldOffset(0)]
	public CommandType Type;

	[FieldOffset(16)]
	public ResultToken ResultToken;

	[FieldOffset(64)]
	public GetSpaceRemainingCommand GetSpaceRemainingCommand;

	[FieldOffset(64)]
	public GetFileSizeCommand GetFileSizeCommand;

	[FieldOffset(64)]
	public ReadFileCommand ReadFileCommand;

	[FieldOffset(64)]
	public WriteFileCommand WriteFileCommand;
}

internal class CommandBufferPool
{
	private UserStorage UserStorage;
	private ConcurrentQueue<CommandBuffer> CommandBuffers = new ConcurrentQueue<CommandBuffer>();

	public CommandBufferPool(UserStorage userStorage)
	{
		UserStorage = userStorage;
	}

	public CommandBuffer Obtain()
	{
		if (CommandBuffers.TryDequeue(out var commandBuffer))
		{
			return commandBuffer;
		}
		else
		{
			return new CommandBuffer(UserStorage);
		}
	}

	public void Return(CommandBuffer commandBuffer)
	{
		CommandBuffers.Enqueue(commandBuffer);
	}
}
