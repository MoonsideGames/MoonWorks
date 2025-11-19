using System;
using System.Collections.Generic;
using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// The keyboard input device abstraction.
	/// </summary>
	public class Keyboard
	{
		/// <summary>
		/// True if any button on the keyboard is active. Useful for input remapping.
		/// </summary>
		public bool AnyPressed { get; private set; }

		/// <summary>
		/// Contains a reference to an arbitrary KeyboardButton that was pressed this frame. Useful for input remapping.
		/// </summary>
		public KeyboardButton AnyPressedButton { get; private set; }

		private KeyboardButton[] Keys { get; }
		private List<SDL.SDL_KeyboardEvent>[] ButtonEvents;

		private static readonly char[] TextInputCharacters =
		[
			(char) 2,	// Home
			(char) 3,	// End
			(char) 8,	// Backspace
			(char) 9,	// Tab
			(char) 13,	// Enter
			(char) 127,	// Delete
			(char) 22	// Ctrl+V (Paste)
		];

		private static readonly Dictionary<ScanCode, int> TextInputBindings = new()
		{
			{ ScanCode.Home,         0 },
			{ ScanCode.End,          1 },
			{ ScanCode.Backspace,    2 },
			{ ScanCode.Tab,          3 },
			{ ScanCode.Return,       4 },
			{ ScanCode.Delete,       5 }
			// Ctrl+V is special!
		};

		internal Keyboard()
		{
			SDL.SDL_GetKeyboardState(out var numKeys);

			var scanCodes = Enum.GetValues<ScanCode>();
			Keys = new KeyboardButton[numKeys];
			ButtonEvents = new List<SDL.SDL_KeyboardEvent>[numKeys];

			foreach (var scancode in scanCodes)
			{
				var button = new KeyboardButton(this, scancode);
				Keys[(int) scancode] = button;
			}

			for (var i = 0; i < numKeys; i += 1)
            {
                ButtonEvents[i] = [];
            }
		}

		internal void AddButtonEvent(SDL.SDL_KeyboardEvent evt)
        {
			ButtonEvents[(int) evt.scancode].Add(evt);
        }

		private static bool ButtonWasPressed(ulong frameTimestamp, List<SDL.SDL_KeyboardEvent> events)
        {
			foreach (var buttonEvent in events)
			{
				if (buttonEvent.down && Inputs.TimestampDifference(frameTimestamp, buttonEvent.timestamp) < Inputs.ButtonDiscardThreshold)
				{
					return true;
				}
			}
			return false;
        }

		internal void Update(ulong timestamp)
		{
			AnyPressed = false;

			foreach (var button in Keys)
            {
				if (button == null) { continue; }

				var events = ButtonEvents[(int) button.ScanCode];

				bool isDown = button.Down;
				bool wasPressed = isDown;

				if (events.Count > 0)
                {
                    wasPressed = ButtonWasPressed(timestamp, events);
					isDown = events[^1].down;
					events.Clear();
                }

				button.Update(wasPressed, isDown);

				if (TextInputBindings.TryGetValue(button.ScanCode, out var textIndex))
				{
					Inputs.OnTextInput(TextInputCharacters[(textIndex)]);
				}
				else if (IsDown(ScanCode.LeftControl) && button.ScanCode == ScanCode.V)
				{
					Inputs.OnTextInput(TextInputCharacters[6]);
				}

				if (button.IsPressed)
				{
					AnyPressed = true;
					AnyPressedButton = button;
				}

				events.Clear();
            }
		}

		/// <summary>
		/// Translates a ScanCode from a KeyCode.
		/// </summary>
		public static ScanCode FromKeyCode(KeyCode keycode) =>
			(ScanCode) SDL.SDL_GetScancodeFromKey((uint) keycode, IntPtr.Zero);

		/// <summary>
		/// Translates a KeyCode from a ScanCode.
		/// </summary>
		public static KeyCode FromScanCode(ScanCode scancode) =>
			(KeyCode) SDL.SDL_GetKeyFromScancode((SDL.SDL_Scancode) scancode, SDL.SDL_Keymod.SDL_KMOD_NONE, false);

		/// <summary>
		/// True if the button was pressed this frame.
		/// </summary>
		public bool IsPressed(ScanCode scancode) => Keys[(int) scancode].IsPressed;

		/// <summary>
		/// True if the button was pressed this frame.
		/// </summary>
		public bool IsPressed(KeyCode keycode) => IsPressed(FromKeyCode(keycode));

		/// <summary>
		/// True if the button was pressed this frame and the previous frame.
		/// </summary>
		public bool IsHeld(ScanCode scancode) => Keys[(int) scancode].IsHeld;

		/// <summary>
		/// True if the button was pressed this frame and the previous frame.
		/// </summary>
		public bool IsHeld(KeyCode keycode) => IsHeld(FromKeyCode(keycode));

		/// <summary>
		/// True if the button was either pressed or continued to be held this frame.
		/// </summary>
		public bool IsDown(ScanCode scancode) => Keys[(int) scancode].IsDown;

		/// <summary>
		/// True if the button was either pressed or continued to be held this frame.
		/// </summary>
		public bool IsDown(KeyCode keycode) => IsDown(FromKeyCode(keycode));

		/// <summary>
		/// True if the button was let go this frame.
		/// </summary>
		public bool IsReleased(ScanCode scancode) => Keys[(int) scancode].IsReleased;

		/// <summary>
		/// True if the button was let go this frame.
		/// </summary>
		public bool IsReleased(KeyCode keycode) => IsReleased(FromKeyCode(keycode));

		/// <summary>
		/// True if the button was not pressed this frame or the previous frame.
		/// </summary>
		public bool IsIdle(ScanCode scancode) => Keys[(int) scancode].IsIdle;

		/// <summary>
		/// True if the button was not pressed this frame or the previous frame.
		/// </summary>
		public bool IsIdle(KeyCode keycode)  => IsIdle(FromKeyCode(keycode));

		/// <summary>
		/// True if the button was either idle or released this frame.
		/// </summary>
		public bool IsUp(ScanCode scancode) => Keys[(int) scancode].IsUp;

		/// <summary>
		/// True if the button was either idle or released this frame.
		/// </summary>
		public bool IsUp(KeyCode keycode) => IsUp(FromKeyCode(keycode));

		/// <summary>
		/// Gets a reference to a keyboard button object using a ScanCode.
		/// </summary>
		public KeyboardButton Button(ScanCode scancode)
		{
			return Keys[(int) scancode];
		}

		/// <summary>
		/// Gets a reference to a keyboard button object using a KeyCode.
		/// </summary>
		public KeyboardButton Button(KeyCode keycode) => Button(FromKeyCode(keycode));

		/// <summary>
		/// Gets the state of a keyboard button from a scancode.
		/// </summary>
		public ButtonState ButtonState(ScanCode scancode)
		{
			return Keys[(int) scancode].State;
		}

		/// <summary>
		/// Gets the state of a keyboard button from a keycode.
		/// </summary>
		public ButtonState ButtonState(KeyCode keycode) =>  ButtonState(FromKeyCode(keycode));
	}
}
