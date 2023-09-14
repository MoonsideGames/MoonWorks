namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a mouse button.
	/// </summary>
	public class MouseButton : VirtualButton
	{
		Mouse Parent;
		uint ButtonMask;

		public MouseButtonCode Code { get; private set; }

		internal MouseButton(Mouse parent, MouseButtonCode code, uint buttonMask)
		{
			Parent = parent;
			Code = code;
			ButtonMask = buttonMask;
		}

		internal override bool CheckPressed()
		{
			return (Parent.ButtonMask & ButtonMask) != 0;
		}
	}
}
