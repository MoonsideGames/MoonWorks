using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics;

/// <summary>
/// Describes the blueprints of a visual representation(s) and/or GPU-related resource(s).
/// </summary>
public abstract class GraphicsResource : IDisposable
{
	private GCHandle SelfReference;

	/// <summary>
	/// Gets the device that the resource is allocated on.
	/// </summary>
	public GraphicsDevice Device { get; }

	/// <summary>
	/// Gets a value indicating if the resource is disposed.
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <summary>
	/// Gets the name of the resource.
	/// </summary>
	public string Name { get; protected set; }

	/// <summary>
	/// Create a new instance of the <see cref="GraphicsResource"/> class.
	/// </summary>
	/// <param name="device">The graphical device to allocate the resource on.</param>
	protected GraphicsResource(GraphicsDevice device)
	{
		Device = device;

		SelfReference = GCHandle.Alloc(this, GCHandleType.Weak);
		Device.AddResourceReference(SelfReference);
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			if (disposing)
			{
				Device.RemoveResourceReference(SelfReference);
				SelfReference.Free();
			}

			IsDisposed = true;
		}
	}

	/// <inheritdoc/>
	~GraphicsResource()
	{
		#if DEBUG
		// If you see this log message, you leaked a graphics resource without disposing it!
		// We'll try to clean it up for you but you really should fix this.
		Logger.LogWarn($"A resource named {Name} of type {GetType().Name} was not Disposed.");
		#endif

		Dispose(false);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
