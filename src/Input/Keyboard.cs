using System;
using System.Collections.Generic;
using SDL2;

namespace MoonWorks.Input
{
	public class Keyboard
	{
		public bool AnyPressed { get; private set; }
		public KeyboardButton AnyPressedButton { get; private set; }

		public IntPtr State { get; private set; }

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

			Keys = new KeyboardButton[numKeys];
			foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
			{
				Keys[(int) keycode] = new KeyboardButton(this, keycode);
			}
		}

		internal void Update()
		{
			AnyPressed = false;

			State = SDL.SDL_GetKeyboardState(out _);

			foreach (int keycode in Enum.GetValues(typeof(KeyCode)))
			{
				var button = Keys[keycode];
				button.Update();

				if (button.IsPressed)
				{
					if (TextInputBindings.TryGetValue((KeyCode) keycode, out var textIndex))
					{
						Inputs.OnTextInput(TextInputCharacters[(textIndex)]);
					}
					else if (IsDown(KeyCode.LeftControl) && (KeyCode) keycode == KeyCode.V)
					{
						Inputs.OnTextInput(TextInputCharacters[6]);
					}

					AnyPressed = true;
					AnyPressedButton = button;
				}
			}
		}

		public bool IsPressed(KeyCode keycode)
		{
			return Keys[(int) keycode].IsPressed;
		}

		public bool IsHeld(KeyCode keycode)
		{
			return Keys[(int) keycode].IsHeld;
		}

		public bool IsDown(KeyCode keycode)
		{
			return Keys[(int) keycode].IsDown;
		}

		public bool IsReleased(KeyCode keycode)
		{
			return Keys[(int) keycode].IsReleased;
		}

		public bool IsIdle(KeyCode keycode)
		{
			return Keys[(int) keycode].IsIdle;
		}

		public bool IsUp(KeyCode keycode)
		{
			return Keys[(int) keycode].IsUp;
		}

		public KeyboardButton Button(KeyCode keycode)
		{
			return Keys[(int) keycode];
		}

		public ButtonState ButtonState(KeyCode keycode)
		{
			return Keys[(int) keycode].State;
		}
	}
}
