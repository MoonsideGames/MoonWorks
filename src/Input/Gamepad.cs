using System;
using System.Collections.Generic;
using MoonWorks.Math;
using SDL2;

namespace MoonWorks.Input
{
	public class Gamepad
	{
		internal IntPtr Handle;

		public Button A { get; } = new Button();
		public Button B { get; } = new Button();
		public Button X { get; } = new Button();
		public Button Y { get; } = new Button();
		public Button Back { get; } = new Button();
		public Button Guide { get; } = new Button();
		public Button Start { get; } = new Button();
		public Button LeftStick { get; } = new Button();
		public Button RightStick { get; } = new Button();
		public Button LeftShoulder { get; } = new Button();
		public Button RightShoulder { get; } = new Button();
		public Button DpadUp { get; } = new Button();
		public Button DpadDown { get; } = new Button();
		public Button DpadLeft { get; } = new Button();
		public Button DpadRight { get; } = new Button();

		public Axis LeftX { get; } = new Axis();
		public Axis LeftY { get; } = new Axis();
		public Axis RightX { get; } = new Axis();
		public Axis RightY { get; } = new Axis();

		public Trigger TriggerLeft { get; } = new Trigger();
		public Trigger TriggerRight { get; } = new Trigger();

		public bool IsDummy => Handle == IntPtr.Zero;

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
			if (!IsDummy)
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
		}

		/// <summary>
		/// Sets vibration values on the left and right motors.
		/// </summary>
		public bool SetVibration(float leftMotor, float rightMotor, uint durationInMilliseconds)
		{
			return SDL.SDL_GameControllerRumble(
				Handle,
				(ushort) (MathHelper.Clamp(leftMotor, 0f, 1f) * 0xFFFF),
				(ushort) (MathHelper.Clamp(rightMotor, 0f, 1f) * 0xFFFF),
				durationInMilliseconds
			) == 0;
		}

		/// <summary>
		/// True if the button is pressed or held.
		/// </summary>
		public bool IsDown(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsDown;
		}

		/// <summary>
		/// True if the button was pressed this exact frame.
		/// </summary>
		public bool IsPressed(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsPressed;
		}

		/// <summary>
		/// True if the button has been continuously held for more than one frame.
		/// </summary>
		public bool IsHeld(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsHeld;
		}

		/// <summary>
		/// True if the button is not pressed.
		/// </summary>
		public bool IsReleased(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].IsReleased;
		}

		/// <summary>
		/// Obtains the button state given a ButtonCode.
		/// </summary>
		public ButtonState ButtonState(ButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode].State;
		}

		/// <summary>
		/// Obtains the axis value given an AxisCode.
		/// </summary>
		/// <returns>A value between -1 and 1.</returns>
		public float AxisValue(AxisCode axisCode)
		{
			return EnumToAxis[(SDL.SDL_GameControllerAxis) axisCode].Value;
		}

		/// <summary>
		/// Obtains the trigger value given an TriggerCode.
		/// </summary>
		/// <returns>A value between 0 and 1.</returns>
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
