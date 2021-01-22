using SDL2;

namespace MoonWorks.Input
{
    public class Mouse
    {
        public ButtonState LeftButton { get; private set; }
        public ButtonState MiddleButton { get; private set; }
        public ButtonState RightButton { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int DeltaX { get; private set; }
        public int DeltaY { get; private set; }

        private bool relativeMode;
        public bool RelativeMode
        {
            get => relativeMode;
            set
            {
                relativeMode = value;
                SDL.SDL_SetRelativeMouseMode(
                    relativeMode ? 
                    SDL.SDL_bool.SDL_TRUE : 
                    SDL.SDL_bool.SDL_FALSE
                );
            }
        }

        internal void Update()
        {
            var buttons = SDL.SDL_GetMouseState(out var x, out var y);
            var _ = SDL.SDL_GetRelativeMouseState(out var deltaX, out var deltaY);

            X = x;
            Y = y;
            DeltaX = deltaX;
            DeltaY = deltaY;

            LeftButton = UpdateState(LeftButton, buttons, SDL.SDL_BUTTON_LEFT);
            MiddleButton = UpdateState(MiddleButton, buttons, SDL.SDL_BUTTON_MIDDLE);
            RightButton = UpdateState(RightButton, buttons, SDL.SDL_BUTTON_RIGHT);
        }

        private ButtonState UpdateState(ButtonState state, uint buttonMask, uint buttonFlag)
        {
            var isPressed = buttonMask & buttonFlag;

            if (isPressed != 0)
            {
                if (state == ButtonState.Pressed)
                {
                    return ButtonState.Held;
                }
                else if (state == ButtonState.Released)
                {
                    return ButtonState.Pressed;
                }
            }

            return ButtonState.Released;
        }
    }
}
