using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a mouse button.
	/// </summary>
	public class MouseButton : VirtualButton
	{
		Mouse Parent;
		public MouseButtonCode Code { get; }
		public int Index { get; } // the SDL mouse button index
		internal bool Down { get; private set; } // Tracks the most recent button event

		internal MouseButton(Mouse parent, MouseButtonCode code, int index)
		{
			Parent = parent;
			Code = code;
			Index = index;
		}

		internal void Update(bool wasPressed, bool isDown)
        {
            UpdateState(wasPressed);
			Down = isDown;
        }
	}
}
