using System;
using System.Collections.Concurrent;

namespace MoonWorks.Storage;

/// <summary>
/// The result of a user storage operation.
/// </summary>
public enum Result
{
	/// <summary>
	/// The operation is not yet complete.
	/// </summary>
	Pending,

	/// <summary>
	/// The operation completed and was successful.
	/// </summary>
	Success,

	/// <summary>
	/// The operation failed.
	/// </summary>
	Failure
}

/// <summary>
/// Contains data about an asynchronous user storage operation.
/// </summary>
public class ResultToken
{
	public Result Result;

	/// <summary>
	/// The buffer result of a ReadFile operation.
	/// </summary>
	public IntPtr Buffer; // only used by ReadFile

	/// <summary>
	/// The size result of a GetSpaceRemaining or GetFileSize operation.
	/// </summary>
	public ulong Size;
}

internal class ResultTokenPool
{
	private ConcurrentQueue<ResultToken> Tokens = new ConcurrentQueue<ResultToken>();

	public ResultToken Obtain()
	{
		if (Tokens.TryDequeue(out var token))
		{
			return token;
		}
		else
		{
			return new ResultToken();
		}
	}

	public unsafe void Return(ResultToken token)
	{
		token.Result = Result.Pending;
		token.Size = 0;
		token.Buffer = IntPtr.Zero;
		Tokens.Enqueue(token);
	}
}
