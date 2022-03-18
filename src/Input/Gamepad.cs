using System;
using System.Collections.Generic;
using MoonWorks.Math;
using SDL2;

namespace MoonWorks.Input
{
	public class Gamepad
	{
		internal IntPtr Handle;

		public Button A { get; private set; } = new Button();
		public Button B { get; private set; } = new Button();
		public Button X { get; private set; } = new Button();
		public Button Y { get; private set; } = new Button();
		public Button Back { get; private set; } = new Button();
		public Button Guide { get; private set; } = new Button();
		public Button Start { get; private set; } = new Button();
		public Button LeftStick { get; private set; } = new Button();
		public Button RightStick { get; private set; } = new Button();
		public Button LeftShoulder { get; private set; } = new Button();
		public Button RightShoulder { get; private set; } = new Button();
		public Button DpadUp { get; private set; } = new Button();
		public Button DpadDown { get; private set; } = new Button();
		public Button DpadLeft { get; private set; } = new Button();
		public Button DpadRight { get; private set; } = new Button();

		public Axis LeftX { get; private set; }
		public Axis LeftY { get; private set; }
		public Axis RightX { get; private set; }
		public Axis RightY { get; private set; }

		public Trigger TriggerLeft { get; private set; }
		public Trigger TriggerRight { get; private set; }

		private Dictionary<SDL.SDL_GameControllerButton, Button> EnumToButton;
		private Dictionary<SDL.SDL_GameControllerAxis, Axis> EnumToAxis;
		private Dictionary<SDL.SDL_GameControllerAxis, Trigger> EnumToTrigger;

		internal Gamepad(IntPtr handle)
		{
			Handle = handle;

			EnumToButton = new Dictionary<SDL.SDL_GameControllerButton, Button>
			{
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A, A },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B, B },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X, X },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y, Y },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK, Back },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE, Guide },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START, Start },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK, LeftStick },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK, RightStick },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER, LeftShoulder },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER, RightShoulder },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP, DpadUp },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN, DpadDown },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT, DpadLeft },
				{ SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT, DpadRight }
			};

			EnumToAxis = new Dictionary<SDL.SDL_GameControllerAxis, Axis>
			{
				{ SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX, LeftX },
				{ SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY, LeftY },
				{ SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX, RightX },
				{ SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY, RightY }
			};

			EnumToTrigger = new Dictionary<SDL.SDL_GameControllerAxis, Trigger>
			{
				{ SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT, TriggerLeft },
				{ SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT, TriggerRight }
			};
		}

		internal void Update()
		{
			foreach (var (sdlEnum, button) in EnumToButton)
			{
				button.Update(CheckPressed(sdlEnum));
			}

			foreach (var (sdlEnum, axis) in EnumToAxis)
			{
				var sdlAxisValue = SDL.SDL_GameControllerGetAxis(Handle, sdlEnum);
				var axisValue = Normalize(sdlAxisValue, short.MinValue, short.MaxValue, -1, 1);
				axis.Update(axisValue);
			}

			foreach (var (sdlEnum, trigger) in EnumToTrigger)
			{
				var sdlAxisValue = SDL.SDL_GameControllerGetAxis(Handle, sdlEnum);
				var axisValue = Normalize(sdlAxisValue, 0, short.MaxValue, 0, 1);
				trigger.Update(axisValue);
			}
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

		public bool IsDown(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsDown;
		}

		public bool IsPressed(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsPressed;
		}

		public bool IsHeld(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsHeld;
		}

		public bool IsReleased(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsReleased;
		}

		public ButtonState ButtonState(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].State;
		}

		public float AxisValue(AxisCode axisCode)
		{
			return EnumToAxis[(SDL.SDL_GameControllerAxis) axisCode].Value;
		}

		public float TriggerValue(TriggerCode triggerCode)
		{
			return EnumToTrigger[(SDL.SDL_GameControllerAxis) triggerCode].Value;
		}

		private bool CheckPressed(SDL.SDL_GameControllerButton button)
		{
			return MoonWorks.Conversions.ByteToBool(SDL.SDL_GameControllerGetButton(Handle, button));
		}

		private float Normalize(float value, short min, short max, short newMin, short newMax)
		{
			return ((value - min) * (newMax - newMin)) / (max - min) + newMin;
		}
	}
}
