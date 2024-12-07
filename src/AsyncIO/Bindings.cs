using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SDLBool = SDL3.SDL.SDLBool;

namespace MoonWorks.AsyncIO;

public enum TaskType
{
	Read,
	Write,
	Close
}

public enum Result
{
	Complete,
	Failure,
	Cancelled
}

public struct Outcome
{
	public IntPtr AsyncIO;
	public TaskType Type;
	public Result Result;
	public IntPtr Buffer;
	public ulong Offset;
	public ulong BytesRequested;
	public ulong BytesTransferred;
	public IntPtr UserData;
}

internal static partial class SDL_AsyncIO
{
	const string nativeLibName = "SDL3";

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_AsyncIOFromFile(string file, string mode);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial long SDL_GetAsyncIOSize(IntPtr asyncio);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_ReadAsyncIO(IntPtr asyncio, IntPtr ptr, ulong offset, ulong size, IntPtr queue, IntPtr userdata);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_WriteAsyncIO(IntPtr asyncio, IntPtr ptr, ulong offset, ulong size, IntPtr queue, IntPtr userdata);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_CloseAsyncIO(IntPtr asyncio, SDLBool flush, IntPtr queue, IntPtr userdata);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial IntPtr SDL_CreateAsyncIOQueue();

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_DestroyAsyncIOQueue(IntPtr queue);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_GetAsyncIOResult(IntPtr queue, out Outcome outcome);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_WaitAsyncIOResult(IntPtr queue, out Outcome outcome, int timeoutMS);

	[LibraryImport(nativeLibName)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial void SDL_SignalAsyncIOQueue(IntPtr queue);

	[LibraryImport(nativeLibName, StringMarshalling = StringMarshalling.Utf8)]
	[UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
	public static partial SDLBool SDL_LoadFileAsync(string file, IntPtr queue, IntPtr userdata);
}
