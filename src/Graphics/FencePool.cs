using System.Collections.Generic;

namespace MoonWorks.Graphics
{
	internal class FencePool
	{
		private GraphicsDevice GraphicsDevice;
		private Queue<Fence> Fences = new Queue<Fence>();

		public FencePool(GraphicsDevice graphicsDevice)
		{
			GraphicsDevice = graphicsDevice;
		}

		public Fence Obtain()
		{
			if (Fences.Count == 0)
			{
				return new Fence(GraphicsDevice);
			}

			return Fences.Dequeue();
		}

		public void Return(Fence fence)
		{
			Fences.Enqueue(fence);
		}
	}
}
