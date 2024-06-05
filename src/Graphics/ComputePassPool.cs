using System.Collections.Concurrent;

namespace MoonWorks.Graphics;

internal class ComputePassPool
{
	private ConcurrentQueue<ComputePass> ComputePasses = new ConcurrentQueue<ComputePass>();

	public ComputePass Obtain()
	{
		if (ComputePasses.TryDequeue(out var computePass))
		{
			return computePass;
		}
		else
		{
			return new ComputePass();
		}
	}

	public void Return(ComputePass computePass)
	{
		ComputePasses.Enqueue(computePass);
	}
}
