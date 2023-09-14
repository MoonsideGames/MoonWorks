namespace MoonWorks.Input
{
	/// <summary>
	/// A dummy button that can never be pressed. Used for the dummy gamepad.
	/// </summary>
	public class EmptyButton : VirtualButton
	{
		internal override bool CheckPressed()
		{
			return false;
		}
	}
}
