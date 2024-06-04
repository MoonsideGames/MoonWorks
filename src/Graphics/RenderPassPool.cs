using System.Collections.Concurrent;

namespace MoonWorks.Graphics
{
	internal class RenderPassPool
	{
		private ConcurrentQueue<RenderPass> RenderPasses = new ConcurrentQueue<RenderPass>();

		public RenderPass Obtain()
		{
			if (RenderPasses.TryDequeue(out var renderPass))
			{
				return renderPass;
			}
			else
			{
				return new RenderPass();
			}
		}

		public void Return(RenderPass renderPass)
		{
			RenderPasses.Enqueue(renderPass);
		}
	}
}
