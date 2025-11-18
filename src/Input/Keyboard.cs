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

		private ScanCode[] KeyCodes;
		private KeyboardButton[] Keys { get; }
		private List<SDL.SDL_KeyboardEvent>[] ButtonEvents = [];
		private int numKeys;

		private static readonly char[] TextInputCharacters = new char[]
		{
			(char) 2,	// Home
			(char) 3,	// End
			(char) 8,	// Backspace
			(char) 9,	// Tab
			(char) 13,	// Enter
			(char) 127,	// Delete
			(char) 22	// Ctrl+V (Paste)
		};

		private static readonly Dictionary<ScanCode, int> TextInputBindings = new Dictionary<ScanCode, int>()
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
			SDL.SDL_GetKeyboardState(out numKeys);

			KeyCodes = Enum.GetValues<ScanCode>();
			Keys = new KeyboardButton[numKeys];
			ButtonEvents = new List<SDL.SDL_KeyboardEvent>[numKeys];

			foreach (ScanCode keycode in KeyCodes)
			{
				var button = new KeyboardButton(this, keycode);
				Keys[(int) keycode] = button;
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

		private static bool ButtonDown(List<SDL.SDL_KeyboardEvent> events)
        {
			foreach (var buttonEvent in events)
			{
				if (buttonEvent.down)
				{
					return true;
				}
			}
			return false;
        }

		internal void Update()
		{
			AnyPressed = false;

			foreach (var button in Keys)
            {
				if (button == null) { continue; }

				var events = ButtonEvents[(int) button.ScanCode];

				button.Update(events.Count > 0 ? ButtonDown(events) : button.IsDown);

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
		/// True if the button was pressed this frame.
		/// </summary>
		public bool IsPressed(ScanCode keycode)
		{
			return Keys[(int) keycode].IsPressed;
		}

		/// <summary>
		/// True if the button was pressed this frame and the previous frame.
		/// </summary>
		public bool IsHeld(ScanCode keycode)
		{
			return Keys[(int) keycode].IsHeld;
		}

		/// <summary>
		/// True if the button was either pressed or continued to be held this frame.
		/// </summary>
		public bool IsDown(ScanCode keycode)
		{
			return Keys[(int) keycode].IsDown;
		}

		/// <summary>
		/// True if the button was let go this frame.
		/// </summary>
		public bool IsReleased(ScanCode keycode)
		{
			return Keys[(int) keycode].IsReleased;
		}

		/// <summary>
		/// True if the button was not pressed this frame or the previous frame.
		/// </summary>
		public bool IsIdle(ScanCode keycode)
		{
			return Keys[(int) keycode].IsIdle;
		}

		/// <summary>
		/// True if the button was either idle or released this frame.
		/// </summary>
		public bool IsUp(ScanCode keycode)
		{
			return Keys[(int) keycode].IsUp;
		}

		/// <summary>
		/// Gets a reference to a keyboard button object using a key code.
		/// </summary>
		public KeyboardButton Button(ScanCode keycode)
		{
			return Keys[(int) keycode];
		}

		/// <summary>
		/// Gets the state of a keyboard button from a key code.
		/// </summary>
		public ButtonState ButtonState(ScanCode keycode)
		{
			return Keys[(int) keycode].State;
		}
	}
}
