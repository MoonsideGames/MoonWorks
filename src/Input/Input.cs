using SDL2;
using System.Collections.Generic;

namespace MoonWorks
{
    public class Input
    {
        public Keyboard Keyboard { get; }

        List<Gamepad> gamepads = new List<Gamepad>();

        internal Input()
        {
            Keyboard = new Keyboard();

            for (int i = 0; i < SDL.SDL_NumJoysticks(); i++)
            {
                if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
                {
                    gamepads.Add(new Gamepad(SDL.SDL_GameControllerOpen(i)));
                }
            }
        }

        // Assumes that SDL_PumpEvents has been called!
        internal void Update()
        {
            Keyboard.Update();

            foreach (var gamepad in gamepads)
            {
                gamepad.Update();
            }
        }

        public Gamepad GetGamepad(int slot)
        {
            return gamepads[slot];
        }
    }
}
