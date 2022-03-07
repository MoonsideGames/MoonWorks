using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SDL2;

namespace MoonWorks.Input
{
	public class Keyboard
	{
		private ButtonState[] Keys { get; }
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

		private static readonly Dictionary<Keycode, int> TextInputBindings = new Dictionary<Keycode, int>()
		{
			{ Keycode.Home,         0 },
			{ Keycode.End,          1 },
			{ Keycode.Backspace,    2 },
			{ Keycode.Tab,          3 },
			{ Keycode.Return,       4 },
			{ Keycode.Delete,       5 }
			// Ctrl+V is special!
		};

		internal Keyboard()
		{
			SDL.SDL_GetKeyboardState(out numKeys);

			Keys = new ButtonState[numKeys];
			foreach (Keycode keycode in Enum.GetValues(typeof(Keycode)))
			{
				Keys[(int) keycode] = new ButtonState();
			}
		}

		internal void Update()
		{
			IntPtr keyboardState = SDL.SDL_GetKeyboardState(out _);

			foreach (int keycode in Enum.GetValues(typeof(Keycode)))
			{
				var keyDown = Marshal.ReadByte(keyboardState, keycode);
				Keys[keycode] = Keys[keycode].Update(Conversions.ByteToBool(keyDown));

				if (Conversions.ByteToBool(keyDown))
				{
					if (TextInputBindings.TryGetValue((Keycode) keycode, out var textIndex))
					{
						Inputs.OnTextInput(TextInputCharacters[(textIndex)]);
					}
					else if (IsDown(Keycode.LeftControl) && (Keycode) keycode == Keycode.V)
					{
						Inputs.OnTextInput(TextInputCharacters[6]);
					}
				}
			}
		}

		public bool IsDown(Keycode keycode)
		{
			return Keys[(int) keycode].IsDown;
		}

		public bool IsPressed(Keycode keycode)
		{
			return Keys[(int) keycode].IsPressed;
		}

		public bool IsHeld(Keycode keycode)
		{
			return Keys[(int) keycode].IsHeld;
		}

		public bool IsReleased(Keycode keycode)
		{
			return Keys[(int) keycode].IsReleased;
		}

		public ButtonState ButtonState(Keycode keycode)
		{
			return Keys[(int) keycode];
		}
	}
}
