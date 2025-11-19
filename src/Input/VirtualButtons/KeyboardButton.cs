namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a keyboard button.
	/// </summary>
	public class KeyboardButton : VirtualButton
	{
		Keyboard Parent;
		public ScanCode ScanCode { get; }
		internal bool Down { get; private set; } // Tracks the most recent button event

		internal KeyboardButton(Keyboard parent, ScanCode scanCode)
		{
			Parent = parent;
			ScanCode = scanCode;
		}

		internal void Update(bool wasPressed, bool isDown)
        {
            UpdateState(wasPressed);
			Down = isDown;
        }
	}
}
