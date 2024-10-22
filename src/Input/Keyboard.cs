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

		internal IntPtr State { get; private set; }

		private KeyCode[] KeyCodes;
		private KeyboardButton[] Keys { get; }
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

		private static readonly Dictionary<KeyCode, int> TextInputBindings = new Dictionary<KeyCode, int>()
		{
			{ KeyCode.Home,         0 },
			{ KeyCode.End,          1 },
			{ KeyCode.Backspace,    2 },
			{ KeyCode.Tab,          3 },
			{ KeyCode.Return,       4 },
			{ KeyCode.Delete,       5 }
			// Ctrl+V is special!
		};

		internal Keyboard()
		{
			SDL.SDL_GetKeyboardState(out numKeys);

			KeyCodes = Enum.GetValues<KeyCode>();
			Keys = new KeyboardButton[numKeys];

			foreach (KeyCode keycode in KeyCodes)
			{
				Keys[(int) keycode] = new KeyboardButton(this, keycode);
			}
		}

		internal void Update()
		{
			AnyPressed = false;

			State = SDL.SDL_GetKeyboardState(out _);

			foreach (KeyCode keycode in KeyCodes)
			{
				var button = Keys[(int) keycode];
				button.Update();

				if (button.IsPressed)
				{
					if (TextInputBindings.TryGetValue(keycode, out var textIndex))
					{
						Inputs.OnTextInput(TextInputCharacters[(textIndex)]);
					}
					else if (IsDown(KeyCode.LeftControl) && keycode == KeyCode.V)
					{
						Inputs.OnTextInput(TextInputCharacters[6]);
					}

					AnyPressed = true;
					AnyPressedButton = button;
				}
			}
		}

		/// <summary>
		/// True if the button was pressed this frame.
		/// </summary>
		public bool IsPressed(KeyCode keycode)
		{
			return Keys[(int) keycode].IsPressed;
		}

		/// <summary>
		/// True if the button was pressed this frame and the previous frame.
		/// </summary>
		public bool IsHeld(KeyCode keycode)
		{
			return Keys[(int) keycode].IsHeld;
		}

		/// <summary>
		/// True if the button was either pressed or continued to be held this frame.
		/// </summary>
		public bool IsDown(KeyCode keycode)
		{
			return Keys[(int) keycode].IsDown;
		}

		/// <summary>
		/// True if the button was let go this frame.
		/// </summary>
		public bool IsReleased(KeyCode keycode)
		{
			return Keys[(int) keycode].IsReleased;
		}

		/// <summary>
		/// True if the button was not pressed this frame or the previous frame.
		/// </summary>
		public bool IsIdle(KeyCode keycode)
		{
			return Keys[(int) keycode].IsIdle;
		}

		/// <summary>
		/// True if the button was either idle or released this frame.
		/// </summary>
		public bool IsUp(KeyCode keycode)
		{
			return Keys[(int) keycode].IsUp;
		}

		/// <summary>
		/// Gets a reference to a keyboard button object using a key code.
		/// </summary>
		public KeyboardButton Button(KeyCode keycode)
		{
			return Keys[(int) keycode];
		}

		/// <summary>
		/// Gets the state of a keyboard button from a key code.
		/// </summary>
		public ButtonState ButtonState(KeyCode keycode)
		{
			return Keys[(int) keycode].State;
		}
	}
}
