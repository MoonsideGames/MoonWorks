using System.Collections.Concurrent;

namespace MoonWorks.Graphics;

internal class CopyPassPool
{
	private ConcurrentQueue<CopyPass> CopyPasses = new ConcurrentQueue<CopyPass>();

	public CopyPass Obtain()
	{
		if (CopyPasses.TryDequeue(out var copyPass))
		{
			return copyPass;
		}
		else
		{
			return new CopyPass();
		}
	}

	public void Return(CopyPass copyPass)
	{
		CopyPasses.Enqueue(copyPass);
	}
}
