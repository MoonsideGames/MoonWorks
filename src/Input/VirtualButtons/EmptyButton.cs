namespace MoonWorks.Input
{
	public class EmptyButton : VirtualButton
	{
		internal override bool CheckPressed()
		{
			return false;
		}
	}
}
