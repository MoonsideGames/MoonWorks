using System;
using MoonWorks.Math;
using SDL2;

namespace MoonWorks.Input
{
	public class Gamepad
	{
		internal IntPtr Handle;

		public ButtonState A { get; private set; } = new ButtonState();
		public ButtonState B { get; private set; } = new ButtonState();
		public ButtonState X { get; private set; } = new ButtonState();
		public ButtonState Y { get; private set; } = new ButtonState();
		public ButtonState Back { get; private set; } = new ButtonState();
		public ButtonState Guide { get; private set; } = new ButtonState();
		public ButtonState Start { get; private set; } = new ButtonState();
		public ButtonState LeftStick { get; private set; } = new ButtonState();
		public ButtonState RightStick { get; private set; } = new ButtonState();
		public ButtonState LeftShoulder { get; private set; } = new ButtonState();
		public ButtonState RightShoulder { get; private set; } = new ButtonState();
		public ButtonState DpadUp { get; private set; } = new ButtonState();
		public ButtonState DpadDown { get; private set; } = new ButtonState();
		public ButtonState DpadLeft { get; private set; } = new ButtonState();
		public ButtonState DpadRight { get; private set; } = new ButtonState();

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

		public bool SetVibration(float leftMotor, float rightMotor, uint durationInMilliseconds)
		{
			return SDL.SDL_GameControllerRumble(
				Handle,
				(ushort) (MathHelper.Clamp(leftMotor, 0f, 1f) * 0xFFFF),
				(ushort) (MathHelper.Clamp(rightMotor, 0f, 1f) * 0xFFFF),
				durationInMilliseconds
			) == 0;
		}

		internal void Update()
		{
			A = A.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A));
			B = B.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B));
			X = X.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X));
			Y = Y.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y));
			Back = Back.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK));
			Guide = Guide.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE));
			Start = Start.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START));
			LeftStick = LeftStick.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK));
			RightStick = RightStick.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK));
			LeftShoulder = LeftShoulder.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER));
			RightShoulder = RightShoulder.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER));
			DpadUp = DpadUp.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP));
			DpadDown = DpadDown.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN));
			DpadLeft = DpadLeft.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT));
			DpadRight = DpadRight.Update(IsPressed(SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT));

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
			return Normalize(axisValue, short.MinValue, short.MaxValue, -1, 1);
		}

		// Triggers only go from 0 to short.MaxValue
		private float UpdateTrigger(SDL.SDL_GameControllerAxis trigger)
		{
			var triggerValue = SDL.SDL_GameControllerGetAxis(Handle, trigger);
			return Normalize(triggerValue, 0, short.MaxValue, 0, 1);
		}

		private float Normalize(float value, short min, short max, short newMin, short newMax)
		{
			return ((value - min) * (newMax - newMin)) / (max - min) + newMin;
		}
	}
}
