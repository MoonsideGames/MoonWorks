using System;
using System.Threading;

namespace MoonWorks.Graphics;

public abstract class SDL_GpuResource : GraphicsResource
{
	public IntPtr Handle { get => handle; internal set => handle = value; }
	private IntPtr handle;

	protected abstract Action<IntPtr, IntPtr> ReleaseFunction { get; }

	protected SDL_GpuResource(GraphicsDevice device) : base(device)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			// Atomically call release function in case this is called from the finalizer thread
			var toDispose = Interlocked.Exchange(ref handle, IntPtr.Zero);
			if (toDispose != IntPtr.Zero)
			{
				ReleaseFunction(Device.Handle, toDispose);
			}
		}
		base.Dispose(disposing);
	}
}
