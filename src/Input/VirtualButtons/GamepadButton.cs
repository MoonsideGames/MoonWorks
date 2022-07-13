using SDL2;

namespace MoonWorks.Input
{
	public class GamepadButton : VirtualButton
	{
		public Gamepad Parent { get; }
		SDL.SDL_GameControllerButton SDL_Button;
		public GamepadButtonCode Code { get; }

		internal GamepadButton(Gamepad parent, GamepadButtonCode code, SDL.SDL_GameControllerButton sdlButton)
		{
			Parent = parent;
			Code = code;
			SDL_Button = sdlButton;
		}

		internal override bool CheckPressed()
		{
			return MoonWorks.Conversions.ByteToBool(SDL.SDL_GameControllerGetButton(Parent.Handle, SDL_Button));
		}
	}
}
