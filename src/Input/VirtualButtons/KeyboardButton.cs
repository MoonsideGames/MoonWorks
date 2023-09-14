using System.Runtime.InteropServices;

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

		internal override bool CheckPressed()
		{
			return Conversions.ByteToBool(Marshal.ReadByte(Parent.State, (int) KeyCode));
		}
	}
}
