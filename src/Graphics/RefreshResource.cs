using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MoonWorks.Graphics;

public abstract class RefreshResource : GraphicsResource
{
	public IntPtr Handle { get => handle; internal set => handle = value; }
	private IntPtr handle;

	protected abstract Action<IntPtr, IntPtr> QueueDestroyFunction { get; }

	protected RefreshResource(GraphicsDevice device) : base(device)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			// Atomically call destroy function in case this is called from the finalizer thread
			var toDispose = Interlocked.Exchange(ref handle, IntPtr.Zero);
			if (toDispose != IntPtr.Zero)
			{
				QueueDestroyFunction(Device.Handle, toDispose);
			}
		}
		base.Dispose(disposing);
	}
}
