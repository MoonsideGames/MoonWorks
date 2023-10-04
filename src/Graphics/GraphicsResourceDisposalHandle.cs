using System;

namespace MoonWorks.Graphics
{
    // This allows us to defer native disposal calls from the finalizer thread.
    internal struct GraphicsResourceDisposalHandle
    {
	    internal Action<IntPtr, IntPtr> QueueDestroyAction;
		internal IntPtr ResourceHandle;

		public void Dispose(GraphicsDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException(nameof(device));
			}

			if (QueueDestroyAction == null)
			{
				return;
			}

			QueueDestroyAction(device.Handle, ResourceHandle);
		}
    }
}
