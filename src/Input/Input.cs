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

		public bool AnyPressed { get; private set; }
		public VirtualButton AnyPressedButton { get; private set; }

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
			AnyPressed = false;
			AnyPressedButton = default; // DeviceKind.None

			Mouse.Wheel = 0;
			Keyboard.Update();

			if (Keyboard.AnyPressed)
			{
				AnyPressed = true;
				AnyPressedButton = Keyboard.AnyPressedButton;
			}

			Mouse.Update();

			if (Mouse.AnyPressed)
			{
				AnyPressed = true;
				AnyPressedButton = Mouse.AnyPressedButton;
			}

			foreach (var gamepad in gamepads)
			{
				gamepad.Update();

				if (gamepad.AnyPressed)
				{
					AnyPressed = true;
					AnyPressedButton = gamepad.AnyPressedButton;
				}
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
