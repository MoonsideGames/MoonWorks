namespace MoonWorks.Input
{
	/// <summary>
	/// A dummy button that can never be pressed. Used for the dummy gamepad.
	/// </summary>
	public class EmptyButton : VirtualButton
	{
		public static readonly EmptyButton Empty = new();

		internal override bool CheckPressed()
		{
			return false;
		}
	}
}
