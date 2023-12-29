namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a keyboard button.
	/// </summary>
	public class KeyboardButton : VirtualButton
	{
		Keyboard Parent;
		public KeyCode KeyCode { get; }

		internal KeyboardButton(Keyboard parent, KeyCode keyCode)
		{
			Parent = parent;
			KeyCode = keyCode;
		}

		internal unsafe override bool CheckPressed()
		{
			return Conversions.ByteToBool(((byte*) Parent.State)[(int) KeyCode]);
		}
	}
}
