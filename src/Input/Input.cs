using SDL2;
using System;

namespace MoonWorks.Input
{
	public class Inputs
	{
		public const int MAX_GAMEPADS = 4;

		public Keyboard Keyboard { get; }
		public Mouse Mouse { get; }

		Gamepad[] gamepads;

		public static event Action<char> TextInput;

		internal Inputs()
		{
			Keyboard = new Keyboard();
			Mouse = new Mouse();

			gamepads = new Gamepad[MAX_GAMEPADS];

			for (var i = 0; i < 4; i += 1)
			{
				if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
				{
					gamepads[i] = new Gamepad(SDL.SDL_GameControllerOpen(i));
				}
				else
				{
					gamepads[i] = new Gamepad(IntPtr.Zero);
				}
			}
		}

		// Assumes that SDL_PumpEvents has been called!
		internal void Update()
		{
			Keyboard.Update();
			Mouse.Update();

			foreach (var gamepad in gamepads)
			{
				gamepad.Update();
			}
		}

		public bool GamepadExists(int slot)
		{
			return !gamepads[slot].IsDummy;
		}

		public Gamepad GetGamepad(int slot)
		{
			return gamepads[slot];
		}

		internal static void OnTextInput(char c)
		{
			if (TextInput != null)
			{
				TextInput(c);
			}
		}
	}
}
