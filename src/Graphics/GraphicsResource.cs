using System;
using System.Runtime.InteropServices;

namespace MoonWorks.Graphics
{
	public abstract class GraphicsResource : IDisposable
	{
		public GraphicsDevice Device { get; }
		public IntPtr Handle { get; internal set; }

		public bool IsDisposed { get; private set; }
		protected abstract Action<IntPtr, IntPtr> QueueDestroyFunction { get; }

		internal GCHandle selfReference;

		public GraphicsResource(GraphicsDevice device, bool trackResource = true)
		{
			Device = device;

			selfReference = GCHandle.Alloc(this, GCHandleType.Weak);
			Device.AddResourceReference(selfReference);
		}

		internal GraphicsResourceDisposalHandle CreateDisposalHandle()
			{
			return new GraphicsResourceDisposalHandle
			{
				QueueDestroyAction = QueueDestroyFunction,
				ResourceHandle = Handle
			};
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (Handle != IntPtr.Zero)
				{
					QueueDestroyFunction(Device.Handle, Handle);
					Device.RemoveResourceReference(selfReference);
					selfReference.Free();

					Handle = IntPtr.Zero;
				}

				IsDisposed = true;
			}
		}

		~GraphicsResource()
		{
			#if DEBUG
			if (!IsDisposed && Device != null && !Device.IsDisposed)
			{
				// If you see this log message, you leaked a graphics resource without disposing it!
				// This means your game may eventually run out of native memory for mysterious reasons.
				Logger.LogWarn($"A resource of type {GetType().Name} was not Disposed.");
			}
			#endif

			// While we only log in debug builds, in both debug and release builds we want to free
			// any native resources associated with this object at the earliest opportunity.
			// This will at least prevent you from running out of memory rapidly.
			Device.RegisterForEmergencyDisposal(CreateDisposalHandle());
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
