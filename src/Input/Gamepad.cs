using System;
using SDL2;

namespace MoonWorks.Input
{
    public class Gamepad
    {
        internal IntPtr Handle;

        public ButtonState A { get; private set; }
        public ButtonState B { get; private set; }
        public ButtonState X { get; private set; }
        public ButtonState Y { get; private set; }
        public ButtonState Back { get; private set; }
        public ButtonState Guide { get; private set; }
        public ButtonState Start { get; private set; }
        public ButtonState LeftStick { get; private set; }
        public ButtonState RightStick { get; private set; }
        public ButtonState LeftShoulder { get; private set; }
        public ButtonState RightShoulder { get; private set; }
        public ButtonState DpadUp { get; private set; }
        public ButtonState DpadDown { get; private set; }
        public ButtonState DpadLeft { get; private set; }
        public ButtonState DpadRight { get; private set; }

        public float LeftX { get; private set; }
        public float LeftY { get; private set; }
        public float RightX { get; private set; }
        public float RightY { get; private set; }
        public float TriggerLeft { get; private set; }
        public float TriggerRight { get; private set; }

        internal Gamepad(IntPtr handle)
        {
            Handle = handle;
        }

        internal void Update()
        {
            A = UpdateState(A, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A);
            B = UpdateState(B, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B);
            X = UpdateState(X, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X);
            Y = UpdateState(Y, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y);
            Back = UpdateState(Back, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK);
            Guide = UpdateState(Guide, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE);
            Start = UpdateState(Start, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START);
            LeftStick = UpdateState(LeftStick, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK);
            RightStick = UpdateState(RightStick, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK);
            LeftShoulder = UpdateState(LeftShoulder, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
            RightShoulder = UpdateState(RightShoulder, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);
            DpadUp = UpdateState(DpadUp, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP);
            DpadDown = UpdateState(DpadDown, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN);
            DpadLeft = UpdateState(DpadLeft, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT);
            DpadRight = UpdateState(DpadRight, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT);

            LeftX = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
            LeftY = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY);
            RightX = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX);
            RightY = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY);
            TriggerLeft = UpdateTrigger(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT);
            TriggerRight = UpdateTrigger(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT);
        }

        private ButtonState UpdateState(ButtonState state, SDL.SDL_GameControllerButton button)
        {
            var isPressed = SDL.SDL_GameControllerGetButton(Handle, button);

            if (isPressed == 1)
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

        private float UpdateAxis(SDL.SDL_GameControllerAxis axis)
        {
            var axisValue = SDL.SDL_GameControllerGetAxis(Handle, axis);
            return Normalize(axisValue, short.MinValue, short.MaxValue);
        }

        // Triggers only go from 0 to short.MaxValue
        private float UpdateTrigger(SDL.SDL_GameControllerAxis trigger)
        {
            var triggerValue = SDL.SDL_GameControllerGetAxis(Handle, trigger);
            return Normalize(triggerValue, 0, short.MaxValue);
        }

        private float Normalize(float value, short min, short max)
        {
            return (value - min) / (max - min);
        }
    }
}
