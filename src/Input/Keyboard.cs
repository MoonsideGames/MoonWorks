using System;
using System.Runtime.InteropServices;
using SDL2;

namespace MoonWorks.Input
{
    public class Keyboard
    {
        private ButtonState[] Keys { get; }
        private int numKeys;

        internal Keyboard()
        {
            SDL.SDL_GetKeyboardState(out numKeys);

            Keys = new ButtonState[numKeys];
            foreach (Keycode keycode in Enum.GetValues(typeof(Keycode)))
            {
                Keys[(int)keycode] = new ButtonState();
            }
        }

        internal void Update()
        {
            IntPtr keyboardState = SDL.SDL_GetKeyboardState(out _);

            foreach (int keycode in Enum.GetValues(typeof(Keycode)))
            {
                var keyDown = Marshal.ReadByte(keyboardState, keycode);
                Keys[keycode].Update(Conversions.ByteToBool(keyDown));
            }
        }

        public bool IsDown(Keycode keycode)
        {
            return Keys[(int)keycode].IsDown;
        }

        public bool IsPressed(Keycode keycode)
        {
            return Keys[(int)keycode].IsPressed;
        }

        public bool IsHeld(Keycode keycode)
        {
            return Keys[(int)keycode].IsHeld;
        }

        public bool IsReleased(Keycode keycode)
        {
            return Keys[(int)keycode].IsReleased;
        }
    }
}
