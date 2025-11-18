namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a keyboard button.
	/// </summary>
	public class KeyboardButton : VirtualButton
	{
		Keyboard Parent;
		public ScanCode ScanCode { get; }

		internal KeyboardButton(Keyboard parent, ScanCode scanCode)
		{
			Parent = parent;
			ScanCode = scanCode;
		}

		internal void Update(bool isPressed)
        {
            UpdateState(isPressed);
        }
	}
}
