﻿using System;
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

			Keys = new ButtonState[numKeys];
			foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
			{
				Keys[(int) keycode] = new ButtonState();
			}
		}

		internal void Update()
		{
			IntPtr keyboardState = SDL.SDL_GetKeyboardState(out _);

			foreach (int keycode in Enum.GetValues(typeof(KeyCode)))
			{
				var keyDown = Marshal.ReadByte(keyboardState, keycode);
				Keys[keycode] = Keys[keycode].Update(Conversions.ByteToBool(keyDown));

				if (Conversions.ByteToBool(keyDown))
				{
					if (TextInputBindings.TryGetValue((KeyCode) keycode, out var textIndex))
					{
						Inputs.OnTextInput(TextInputCharacters[(textIndex)]);
					}
					else if (IsDown(KeyCode.LeftControl) && (KeyCode) keycode == KeyCode.V)
					{
						Inputs.OnTextInput(TextInputCharacters[6]);
					}
				}
			}
		}

		public bool IsDown(KeyCode keycode)
		{
			return Keys[(int) keycode].IsDown;
		}

		public bool IsPressed(KeyCode keycode)
		{
			return Keys[(int) keycode].IsPressed;
		}

		public bool IsHeld(KeyCode keycode)
		{
			return Keys[(int) keycode].IsHeld;
		}

		public bool IsReleased(KeyCode keycode)
		{
			return Keys[(int) keycode].IsReleased;
		}

		public ButtonState ButtonState(KeyCode keycode)
		{
			return Keys[(int) keycode];
		}
	}
}
