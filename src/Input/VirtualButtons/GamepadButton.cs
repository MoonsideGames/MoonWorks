using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// A virtual button corresponding to a gamepad button.
	/// </summary>
	public class GamepadButton : VirtualButton
	{
		public Gamepad Parent { get; }
		SDL.SDL_GamepadButton SDL_Button;
		public GamepadButtonCode Code { get; }
		internal bool Down { get; private set; } // Tracks the most recent button event

		internal GamepadButton(Gamepad parent, GamepadButtonCode code, SDL.SDL_GamepadButton sdlButton)
		{
			Parent = parent;
			Code = code;
			SDL_Button = sdlButton;
		}

		internal void Update(bool wasPressed, bool isDown)
        {
            UpdateState(wasPressed);
			Down = isDown;
        }
	}
}
