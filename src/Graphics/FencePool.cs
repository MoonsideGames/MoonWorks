using System.Collections.Concurrent;

namespace MoonWorks.Graphics
{
	internal class FencePool
	{
		private GraphicsDevice GraphicsDevice;
		private ConcurrentQueue<Fence> Fences = new ConcurrentQueue<Fence>();

		public FencePool(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice;
		}

		public Fence Obtain()
		{
			if (Fences.TryDequeue(out var fence))
			{
				return fence;
			}
			else
			{
				return new Fence(GraphicsDevice);
			}
		}

		public void Return(Fence fence)
		{
			Fences.Enqueue(fence);
		}
	}
}
