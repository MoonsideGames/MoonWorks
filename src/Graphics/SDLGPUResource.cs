using System;
using System.Threading;

namespace MoonWorks.Graphics;

/// <summary>
/// Extends <see cref="GraphicsResource"/>, describing the blueprints to
/// a visual representation and/or GPU-related resource that is managed by
/// the SDL_GPU API within SDL3.
/// </summary>
public abstract class SDLGPUResource : GraphicsResource
{
	private IntPtr handle;

	/// <summary>
	/// The method to invoke when disposing of the associated unmanaged resource(s).
	/// </summary>
	protected abstract Action<IntPtr, IntPtr> ReleaseFunction { get; }

	/// <summary>
	/// Gets the native address of the SDL GPU Resource.
	/// </summary>
	public IntPtr Handle { get => handle; internal set => handle = value; }

	/// <summary>
	/// Create a new instance of the <see cref="SDLGPUResource"/> class.
	/// </summary>
	/// <param name="device">The graphical device to allocate on.</param>
	protected SDLGPUResource(GraphicsDevice device) : base(device)
	{
	}

	/// <summary>
	/// Implicitly converts a <see cref="SDLGPUResource"/> to a <see cref="IntPtr"/>
	/// by its <see cref="SDLGPUResource.Handle"/>.
	/// </summary>
	/// <param name="resource">The instance to implicitly convert.</param>
	public static implicit operator IntPtr(SDLGPUResource resource)
	{
		return resource.Handle;
	}

	/// <inheritdoc/>
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
