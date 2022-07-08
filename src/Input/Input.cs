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
		public ButtonIdentifier AnyPressedButton { get; private set; }

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
				AnyPressedButton = new ButtonIdentifier(Keyboard.AnyPressedKeyCode);
			}

			Mouse.Update();

			if (Mouse.AnyPressed)
			{
				AnyPressed = true;
				AnyPressedButton = new ButtonIdentifier(Mouse.AnyPressedButtonCode);
			}

			foreach (var gamepad in gamepads)
			{
				gamepad.Update();

				if (gamepad.AnyPressed)
				{
					AnyPressed = true;
					AnyPressedButton = new ButtonIdentifier(gamepad, gamepad.AnyPressedButtonCode);
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
			else if (identifier.DeviceKind == DeviceKind.None)
			{
				return new ButtonState(ButtonStatus.Released);
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
