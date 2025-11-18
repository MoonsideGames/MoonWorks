using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a mouse button.
	/// </summary>
	public class MouseButton : VirtualButton
	{
		Mouse Parent;
		SDL.SDL_MouseButtonFlags ButtonMask;
		public MouseButtonCode Code { get; private set; }

		internal MouseButton(Mouse parent, MouseButtonCode code, SDL.SDL_MouseButtonFlags buttonMask)
		{
			Parent = parent;
			Code = code;
			ButtonMask = buttonMask;
		}

		internal void Update(bool isPressed)
        {
            UpdateState(isPressed);
        }
	}
}
