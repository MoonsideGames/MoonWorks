using System;
using SDL2;

namespace MoonWorks.Input
{
    public class Gamepad
    {
        internal IntPtr Handle;

        public ButtonState A { get; } = new ButtonState();
        public ButtonState B { get; } = new ButtonState();
        public ButtonState X { get; } = new ButtonState();
        public ButtonState Y { get; } = new ButtonState();
        public ButtonState Back { get; } = new ButtonState();
        public ButtonState Guide { get; } = new ButtonState();
        public ButtonState Start { get; } = new ButtonState();
        public ButtonState LeftStick { get; } = new ButtonState();
        public ButtonState RightStick { get; } = new ButtonState();
        public ButtonState LeftShoulder { get; } = new ButtonState();
        public ButtonState RightShoulder { get; } = new ButtonState();
        public ButtonState DpadUp { get; } = new ButtonState();
        public ButtonState DpadDown { get; } = new ButtonState();
        public ButtonState DpadLeft { get; } = new ButtonState();
        public ButtonState DpadRight { get; } = new ButtonState();

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
            A.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A));
            B.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B));
            X.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X));
            Y.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y));
            Back.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK));
            Guide.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE));
            Start.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START));
            LeftStick.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK));
            RightStick.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK));
            LeftShoulder.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER));
            RightShoulder.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER));
            DpadUp.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP));
            DpadDown.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN));
            DpadLeft.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT));
            DpadRight.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT));

            LeftX = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
            LeftY = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY);
            RightX = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX);
            RightY = UpdateAxis(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY);
            TriggerLeft = UpdateTrigger(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT);
            TriggerRight = UpdateTrigger(SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT);
        }

        private bool IsPressed(SDL.SDL_GameControllerButton button)
        {
            return MoonWorks.Conversions.ByteToBool(SDL.SDL_GameControllerGetButton(Handle, button));
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
