using SDL2;

namespace MoonWorks.Input
{
    public class Mouse
    {
        public ButtonState LeftButton { get; } = new ButtonState();
        public ButtonState MiddleButton { get; } = new ButtonState();
        public ButtonState RightButton { get; } = new ButtonState();

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
            var buttonMask = SDL.SDL_GetMouseState(out var x, out var y);
            var _ = SDL.SDL_GetRelativeMouseState(out var deltaX, out var deltaY);

            X = x;
            Y = y;
            DeltaX = deltaX;
            DeltaY = deltaY;

            LeftButton.Update(IsPressed(buttonMask, SDL.SDL_BUTTON_LMASK));
            MiddleButton.Update(IsPressed(buttonMask, SDL.SDL_BUTTON_MMASK));
            RightButton.Update(IsPressed(buttonMask, SDL.SDL_BUTTON_RMASK));
        }

        private bool IsPressed(uint buttonMask, uint buttonFlag)
        {
            return (buttonMask & buttonFlag) != 0;
        }
    }
}
