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

			// initialize dummy controllers
			for (var slot = 0; slot < MAX_GAMEPADS; slot += 1)
			{
				gamepads[slot] = new Gamepad(IntPtr.Zero, slot);
			}
		}

		// Assumes that SDL_PumpEvents has been called!
		internal void Update()
		{
			AnyPressed = false;
			AnyPressedButton = default; // DeviceKind.None

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
			if (slot < 0 || slot >= MAX_GAMEPADS)
			{
				return false;
			}

			return !gamepads[slot].IsDummy;
		}

		// From 0-4
		public Gamepad GetGamepad(int slot)
		{
			return gamepads[slot];
		}

		internal void AddGamepad(int index)
		{
			for (var slot = 0; slot < MAX_GAMEPADS; slot += 1)
			{
				if (!GamepadExists(slot))
				{
					var openResult = SDL.SDL_GameControllerOpen(index);
					if (openResult == 0)
					{
						System.Console.WriteLine($"Error opening gamepad!");
						System.Console.WriteLine(SDL.SDL_GetError());
					}
					else
					{
						gamepads[slot].Register(openResult);
						System.Console.WriteLine($"Gamepad added to slot {slot}!");
					}
					return;
				}
			}

			System.Console.WriteLine("Too many gamepads already!");
		}

		internal void RemoveGamepad(int joystickInstanceID)
		{
			for (int slot = 0; slot < MAX_GAMEPADS; slot += 1)
			{
				if (joystickInstanceID == gamepads[slot].JoystickInstanceID)
				{
					SDL.SDL_GameControllerClose(gamepads[slot].Handle);
					gamepads[slot].Unregister();
					System.Console.WriteLine($"Removing gamepad from slot {slot}!");
					return;
				}
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
