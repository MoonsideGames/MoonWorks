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
					gamepads[i] = new Gamepad(SDL.SDL_GameControllerOpen(i), i);
				}
				else
				{
					gamepads[i] = new Gamepad(IntPtr.Zero, -1);
				}
			}
		}

		// Assumes that SDL_PumpEvents has been called!
		internal void Update()
		{
			Mouse.Wheel = 0;
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

		public ButtonState ButtonState(ButtonIdentifier identifier)
		{
			if (identifier.DeviceKind == DeviceKind.Gamepad)
			{
				var gamepad = GetGamepad(identifier.Index);
				return gamepad.ButtonState((ButtonCode) identifier.Code);
			}
			else if (identifier.DeviceKind == DeviceKind.Keyboard)
			{
				return Keyboard.ButtonState((KeyCode) identifier.Code);
			}
			else if (identifier.DeviceKind == DeviceKind.Mouse)
			{
				return Mouse.ButtonState((MouseButtonCode) identifier.Code);
			}
			else
			{
				throw new System.ArgumentException("Invalid button identifier!");
			}
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
