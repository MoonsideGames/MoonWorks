using System;
using System.Collections.Generic;
using MoonWorks.Math;
using SDL2;

namespace MoonWorks.Input
{
	public class Gamepad
	{
		internal IntPtr Handle;
		internal int Index;

		public GamepadButton A { get; }
		public GamepadButton B { get; }
		public GamepadButton X { get; }
		public GamepadButton Y { get; }
		public GamepadButton Back { get; }
		public GamepadButton Guide { get; }
		public GamepadButton Start { get; }
		public GamepadButton LeftStick { get; }
		public GamepadButton RightStick { get; }
		public GamepadButton LeftShoulder { get; }
		public GamepadButton RightShoulder { get; }
		public GamepadButton DpadUp { get; }
		public GamepadButton DpadDown { get; }
		public GamepadButton DpadLeft { get; }
		public GamepadButton DpadRight { get; }

		public Axis LeftX { get; }
		public Axis LeftY { get; }
		public Axis RightX { get; }
		public Axis RightY { get; }

		public AxisButton LeftXLeft { get; }
		public AxisButton LeftXRight { get; }
		public AxisButton LeftYUp { get; }
		public AxisButton LeftYDown { get; }

		public AxisButton RightXLeft { get; }
		public AxisButton RightXRight { get; }
		public AxisButton RightYUp { get; }
		public AxisButton RightYDown { get; }

		public Trigger TriggerLeft { get; }
		public Trigger TriggerRight { get; }

		public TriggerButton TriggerLeftButton { get; }
		public TriggerButton TriggerRightButton { get; }

		public bool IsDummy => Handle == IntPtr.Zero;

		public bool AnyPressed { get; private set; }
		public VirtualButton AnyPressedButton { get; private set; }

		private Dictionary<SDL.SDL_GameControllerButton, GamepadButton> EnumToButton;
		private Dictionary<SDL.SDL_GameControllerAxis, Axis> EnumToAxis;
		private Dictionary<SDL.SDL_GameControllerAxis, Trigger> EnumToTrigger;

		private Dictionary<AxisButtonCode, AxisButton> AxisButtonCodeToAxisButton;
		private Dictionary<TriggerCode, TriggerButton> TriggerCodeToTriggerButton;

		private VirtualButton[] VirtualButtons;

		internal Gamepad(IntPtr handle, int index)
		{
			Handle = handle;
			Index = index;

			AnyPressed = false;

			A = new GamepadButton(handle, GamepadButtonCode.A, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A);
			B = new GamepadButton(handle, GamepadButtonCode.B, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B);
			X = new GamepadButton(handle, GamepadButtonCode.X, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X);
			Y = new GamepadButton(handle, GamepadButtonCode.Y, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y);

			Back = new GamepadButton(handle, GamepadButtonCode.Back, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK);
			Guide = new GamepadButton(handle, GamepadButtonCode.Guide, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE);
			Start = new GamepadButton(handle, GamepadButtonCode.Start, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START);

			LeftStick = new GamepadButton(handle, GamepadButtonCode.LeftStick, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK);
			RightStick = new GamepadButton(handle, GamepadButtonCode.RightStick, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK);

			LeftShoulder = new GamepadButton(handle, GamepadButtonCode.LeftShoulder, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
			RightShoulder = new GamepadButton(handle, GamepadButtonCode.RightShoulder, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);

			DpadUp = new GamepadButton(handle, GamepadButtonCode.DpadUp, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP);
			DpadDown = new GamepadButton(handle, GamepadButtonCode.DpadDown, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN);
			DpadLeft = new GamepadButton(handle, GamepadButtonCode.DpadLeft, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT);
			DpadRight = new GamepadButton(handle, GamepadButtonCode.DpadRight, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT);

			LeftX = new Axis(handle, AxisCode.LeftX, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
			LeftY = new Axis(handle, AxisCode.LeftY, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY);
			RightX = new Axis(handle, AxisCode.RightX, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX);
			RightY = new Axis(handle, AxisCode.RightY, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY);

			LeftXLeft = new AxisButton(LeftX, false);
			LeftXRight = new AxisButton(LeftX, true);
			LeftYUp = new AxisButton(LeftY, false);
			LeftYDown = new AxisButton(LeftY, true);

			RightXLeft = new AxisButton(RightX, false);
			RightXRight = new AxisButton(RightX, true);
			RightYUp = new AxisButton(RightY, false);
			RightYDown = new AxisButton(RightY, true);

			TriggerLeft = new Trigger(handle, TriggerCode.Left, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT);
			TriggerRight = new Trigger(handle, TriggerCode.Right, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT);

			TriggerLeftButton = new TriggerButton(TriggerLeft);
			TriggerRightButton = new TriggerButton(TriggerRight);

			EnumToButton = new Dictionary<SDL.SDL_GameControllerButton, GamepadButton>
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

			AxisButtonCodeToAxisButton = new Dictionary<AxisButtonCode, AxisButton>
			{
				{ AxisButtonCode.LeftX_Left, LeftXLeft },
				{ AxisButtonCode.LeftX_Right, LeftXRight },
				{ AxisButtonCode.LeftY_Down, LeftYDown },
				{ AxisButtonCode.LeftY_Up, LeftYUp },
				{ AxisButtonCode.RightX_Left, RightXLeft },
				{ AxisButtonCode.RightX_Right, RightXRight },
				{ AxisButtonCode.RightY_Up, RightYUp },
				{ AxisButtonCode.RightY_Down, RightYDown }
			};

			VirtualButtons = new VirtualButton[]
			{
				A,
				B,
				X,
				Y,
				Back,
				Guide,
				Start,
				LeftStick,
				RightStick,
				LeftShoulder,
				RightShoulder,
				DpadUp,
				DpadDown,
				DpadLeft,
				DpadRight,
				LeftXLeft,
				LeftXRight,
				LeftYUp,
				LeftYDown,
				RightXLeft,
				RightXRight,
				RightYUp,
				RightYDown,
				TriggerLeftButton,
				TriggerRightButton
			};
		}

		internal void Update()
		{
			AnyPressed = false;

			if (!IsDummy)
			{
				foreach (var button in EnumToButton.Values)
				{
					button.Update();
				}

				foreach (var axis in EnumToAxis.Values)
				{
					axis.Update();
				}

				foreach (var trigger in EnumToTrigger.Values)
				{
					trigger.Update();
				}

				LeftXLeft.Update();
				LeftXRight.Update();
				LeftYUp.Update();
				LeftYDown.Update();
				RightXLeft.Update();
				RightXRight.Update();
				RightYUp.Update();
				RightYDown.Update();

				TriggerLeftButton.Update();
				TriggerRightButton.Update();

				foreach (var button in VirtualButtons)
				{
					if (button.IsPressed)
					{
						AnyPressed = true;
						AnyPressedButton = button;
						break;
					}
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

		public GamepadButton Button(GamepadButtonCode buttonCode)
		{
			return EnumToButton[(SDL.SDL_GameControllerButton) buttonCode];
		}

		public AxisButton Button(AxisButtonCode axisButtonCode)
		{
			return AxisButtonCodeToAxisButton[axisButtonCode];
		}

		public TriggerButton Button(TriggerCode triggerCode)
		{
			return TriggerCodeToTriggerButton[triggerCode];
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
	}
}
