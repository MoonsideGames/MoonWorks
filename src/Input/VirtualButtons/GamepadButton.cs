using System;
using SDL2;

namespace MoonWorks.Input
{
	public class GamepadButton : VirtualButton
	{
		IntPtr GamepadHandle;
		SDL.SDL_GameControllerButton SDL_Button;

		public GamepadButtonCode Code { get; private set; }

		internal GamepadButton(IntPtr gamepadHandle, GamepadButtonCode code, SDL.SDL_GameControllerButton sdlButton)
		{
			GamepadHandle = gamepadHandle;
			Code = code;
			SDL_Button = sdlButton;
		}

		internal override bool CheckPressed()
		{
			return MoonWorks.Conversions.ByteToBool(SDL.SDL_GameControllerGetButton(GamepadHandle, SDL_Button));
		}
	}
}
