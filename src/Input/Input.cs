using SDL2;
using System;

namespace MoonWorks.Input
{
	/// <summary>
	/// The main container class for all input tracking.
	/// Your Game class will automatically have a reference to this class.
	/// </summary>
	public class Inputs
	{
		public const int MAX_GAMEPADS = 4;

		/// <summary>
		/// The reference to the Keyboard input abstraction.
		/// </summary>
		public Keyboard Keyboard { get; }

		/// <summary>
		/// The reference to the Mouse input abstraction.
		/// </summary>
		public Mouse Mouse { get; }

		Gamepad[] Gamepads;

		public static event Action<char> TextInput;

		/// <summary>
		/// True if any input on any input device is active. Useful for input remapping.
		/// </summary>
		public bool AnyPressed { get; private set; }

		/// <summary>
		/// Contains a reference to an arbitrary VirtualButton that was pressed this frame. Useful for input remapping.
		/// </summary>
		public VirtualButton AnyPressedButton { get; private set; }

		public delegate void OnGamepadConnectedFunc(int slot);

		/// <summary>
		/// Called when a gamepad has been connected.
		/// </summary>
		/// <param name="slot">The slot where the connection occurred.</param>
		public OnGamepadConnectedFunc OnGamepadConnected;

		public delegate void OnGamepadDisconnectedFunc(int slot);

		/// <summary>
		/// Called when a gamepad has been disconnected.
		/// </summary>
		/// <param name="slot">The slot where the disconnection occurred.</param>
		public OnGamepadDisconnectedFunc OnGamepadDisconnected;

		internal Inputs()
		{
			Keyboard = new Keyboard();
			Mouse = new Mouse();

			Gamepads = new Gamepad[MAX_GAMEPADS];

			// initialize dummy controllers
			for (var slot = 0; slot < MAX_GAMEPADS; slot += 1)
			{
				Gamepads[slot] = new Gamepad(IntPtr.Zero, slot);
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

			foreach (var gamepad in Gamepads)
			{
				gamepad.Update();

				if (gamepad.AnyPressed)
				{
					AnyPressed = true;
					AnyPressedButton = gamepad.AnyPressedButton;
				}
			}
		}

		/// <summary>
		/// Returns true if a gamepad is currently connected in the given slot.
		/// </summary>
		/// <param name="slot">Range: 0-3</param>
		/// <returns></returns>
		public bool GamepadExists(int slot)
		{
			if (slot < 0 || slot >= MAX_GAMEPADS)
			{
				return false;
			}

			return !Gamepads[slot].IsDummy;
		}

		/// <summary>
		/// Gets a gamepad associated with the given slot.
		/// The first n slots are guaranteed to occupied with gamepads if they are connected.
		/// If a gamepad does not exist for the given slot, a dummy object with all inputs in default state will be returned.
		/// You can check if a gamepad is connected in a slot with the GamepadExists function.
		/// </summary>
		/// <param name="slot">Range: 0-3</param>
		public Gamepad GetGamepad(int slot)
		{
			return Gamepads[slot];
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
						Logger.LogError("Error opening gamepad!");
						Logger.LogError(SDL.SDL_GetError());
					}
					else
					{
						Gamepads[slot].Register(openResult);
						Logger.LogInfo($"Gamepad added to slot {slot}!");
						OnGamepadConnected(slot);
					}

					return;
				}
			}

			Logger.LogInfo("Too many gamepads already!");
		}

		internal void RemoveGamepad(int joystickInstanceID)
		{
			for (int slot = 0; slot < MAX_GAMEPADS; slot += 1)
			{
				if (joystickInstanceID == Gamepads[slot].JoystickInstanceID)
				{
					SDL.SDL_GameControllerClose(Gamepads[slot].Handle);
					Gamepads[slot].Unregister();
					Logger.LogInfo($"Removing gamepad from slot {slot}!");
					OnGamepadDisconnected(slot);
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
