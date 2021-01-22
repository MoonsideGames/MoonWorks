using System;
using System.Runtime.InteropServices;
using SDL2;

namespace MoonWorks.Input
{
    public class Keyboard
    {
        private Key[] Keys { get; }
        private int numKeys;

        internal Keyboard()
        {
            SDL.SDL_GetKeyboardState(out numKeys);

            Keys = new Key[numKeys];
            foreach (Keycode keycode in Enum.GetValues(typeof(Keycode)))
            {
                Keys[(int)keycode] = new Key(keycode);
            }
        }

        internal void Update()
        {
            IntPtr keyboardState = SDL.SDL_GetKeyboardState(out _);

            foreach (int keycode in Enum.GetValues(typeof(Keycode)))
            {
                var keyDown = Marshal.ReadByte(keyboardState, keycode);

                if (keyDown == 1)
                {
                    if (Keys[keycode].InputState == ButtonState.Released)
                    {
                        Keys[keycode].InputState = ButtonState.Pressed;
                    }
                    else if (Keys[keycode].InputState == ButtonState.Pressed)
                    {
                        Keys[keycode].InputState = ButtonState.Held;
                    }
                }
                else
                {
                    Keys[keycode].InputState = ButtonState.Released;
                }
            }
        }

        public bool IsDown(Keycode keycode)
        {
            var key = Keys[(int)keycode];
            return (key.InputState == ButtonState.Pressed) || (key.InputState == ButtonState.Held);
        }

        public bool IsPressed(Keycode keycode)
        {
            var key = Keys[(int)keycode];
            return key.InputState == ButtonState.Pressed;
        }

        public bool IsHeld(Keycode keycode)
        {
            var key = Keys[(int)keycode];
            return key.InputState == ButtonState.Held;
        }

        public bool IsUp(Keycode keycode)
        {
            var key = Keys[(int)keycode];
            return key.InputState == ButtonState.Released;
        }
    }
}
