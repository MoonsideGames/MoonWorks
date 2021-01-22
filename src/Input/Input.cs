using SDL2;
using System.Collections.Generic;

namespace MoonWorks.Input
{
    public class Inputs
    {
        public Keyboard Keyboard { get; }
        public Mouse Mouse { get; }

        List<Gamepad> gamepads = new List<Gamepad>();

        internal Inputs()
        {
            Keyboard = new Keyboard();
            Mouse = new Mouse();

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
            Mouse.Update();

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
