using System;
using SDL2_gpuCS;

namespace MoonWorks.Graphics;

/// <summary>
/// Fences allow you to track the status of a submitted command buffer. <br/>
/// You should only acquire a Fence if you will need to track the command buffer. <br/>
/// You should make sure to call GraphicsDevice.ReleaseFence when done with a Fence to avoid memory growth. <br/>
/// The Fence object itself is basically just a wrapper for the Refresh_Fence. <br/>
/// The internal handle is replaced so that we can pool Fence objects to manage garbage.
/// </summary>
public class Fence : SDL_GpuResource
{
	protected override Action<nint, nint> ReleaseFunction => SDL_Gpu.SDL_GpuReleaseFence;

	internal Fence(GraphicsDevice device) : base(device)
	{
	}

	internal void SetHandle(nint handle)
	{
		Handle = handle;
	}
}
