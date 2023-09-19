using System;
using RefreshCS;

namespace MoonWorks.Graphics
{
	/// <summary>
	/// Fences allow you to track the status of a submitted command buffer.
	/// You should only acquire a Fence if you will need to track the command buffer.
	/// You should make sure to call GraphicsDevice.ReleaseFence when done with a Fence to avoid memory growth.
	///
	/// The Fence object itself is basically just a wrapper for the Refresh_Fence.
	/// The internal handle is replaced so that we can pool Fence objects to manage garbage.
	/// </summary>
	public class Fence : GraphicsResource
	{
		protected override Action<nint, nint> QueueDestroyFunction => Release;

		internal Fence(GraphicsDevice device) : base(device, true)
		{
		}

		internal void SetHandle(nint handle)
		{
			Handle = handle;
		}

		private void Release(nint deviceHandle, nint fenceHandle)
		{
			if (fenceHandle != IntPtr.Zero)
			{
				// This will only be called if the client forgot to release a handle. Oh no!
				Refresh.Refresh_ReleaseFence(deviceHandle, fenceHandle);
			}
		}
	}
}
