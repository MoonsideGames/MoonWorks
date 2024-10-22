using System;

namespace MoonWorks.Graphics;

/// <summary>
/// Fences allow you to track the status of a submitted command buffer. <br/>
/// You should only acquire a Fence if you will need to track the command buffer. <br/>
/// You should make sure to call GraphicsDevice.ReleaseFence when done with a Fence to avoid memory growth. <br/>
/// The Fence object itself is just a wrapper for an SDL_GPUFence handle. <br/>
/// The internal handle is replaced so that we can pool Fence objects to manage garbage.
/// </summary>
public class Fence
{
	public IntPtr Handle { get; internal set; }

	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}
}
