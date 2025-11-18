using System;
using System.Collections.Generic;
using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// A Gamepad input abstraction that represents input coming from a console controller or other such devices.
	/// The button names map to a standard Xbox 360 controller.
	/// For different controllers the relative position of the face buttons will determine the button mapping.
	/// For example on a DualShock controller the Cross button will map to the A button.
	/// </summary>
	public class Gamepad
	{
		internal IntPtr Handle;
		internal uint JoystickInstanceID;

		public int Slot { get; internal set; }

		public GamepadType GamepadType { get; internal set; }
		public GamepadFamily GamepadFamily { get; internal set; }

		public bool HasLED { get; internal set; }

		/// <summary>
		/// Bottom face button (e.g. Xbox A button)
		/// </summary>
		public GamepadButton South { get; }

		/// <summary>
		/// Right face button (e.g. Xbox B button)
		/// </summary>
		public GamepadButton East { get; }

		/// <summary>
		/// Left face button (e.g. Xbox X button)
		/// </summary>
		public GamepadButton West { get; }

		/// <summary>
		/// Top face button (e.g. Xbox Y button)
		/// </summary>
		public GamepadButton North { get; }

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

		/// <summary>
		/// Additional button (e.g. Xbox Series X share button, Nintendo Switch Pro capture button)
		/// </summary>
		public GamepadButton Misc1 { get; }

		/// <summary>
		/// Upper or primary paddle, under your right hand (e.g. Xbox Elite paddle P1)
		/// </summary>
		public GamepadButton RightPaddle1 { get; }

		/// <summary>
		/// Upper or primary paddle, under your left hand (e.g. Xbox Elite paddle P3)
		/// </summary>
		public GamepadButton LeftPaddle1 { get; }

		/// <summary>
		/// Lower or secondary paddle, under your right hand (e.g. Xbox Elite paddle P2)
		/// </summary>
		public GamepadButton RightPaddle2 { get; }

		/// <summary>
		/// Lower or secondary paddle, under your left hand (e.g. Xbox Elite paddle P4)
		/// </summary>
		public GamepadButton LeftPaddle2 { get; }

		/// <summary>
		/// PS4/PS5 touchpad button
		/// </summary>
		public GamepadButton TouchPad { get; }

		/// <summary>
		/// Additional button
		/// </summary>
		public GamepadButton Misc2 { get; }

		/// <summary>
		/// Additional button
		/// </summary>
		public GamepadButton Misc3 { get; }

		/// <summary>
		/// Additional button
		/// </summary>
		public GamepadButton Misc4 { get; }

		/// <summary>
		/// Additional button
		/// </summary>
		public GamepadButton Misc5 { get; }

		/// <summary>
		/// Additional button
		/// </summary>
		public GamepadButton Misc6 { get; }

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

		/// <summary>
		/// True if any input on the gamepad is active. Useful for input remapping.
		/// </summary>
		public bool AnyPressed { get; private set; }

		/// <summary>
		/// Contains a reference to an arbitrary VirtualButton that was pressed on the gamepad this frame. Useful for input remapping.
		/// </summary>
		public VirtualButton AnyPressedButton { get; private set; }

		/// <summary>
		/// The implementation-dependent name of the gamepad.
		/// </summary>
		public string Name { get; private set; }

		private List<SDL.SDL_GamepadButtonEvent>[] ButtonEvents = [];
		private List<SDL.SDL_GamepadAxisEvent>[] AxisEvents = [];
		private List<SDL.SDL_GamepadAxisEvent>[] TriggerEvents = [];

		private Dictionary<SDL.SDL_GamepadButton, GamepadButton> EnumToButton;
		private Dictionary<SDL.SDL_GamepadAxis, Axis> EnumToAxis;
		private Dictionary<SDL.SDL_GamepadAxis, Trigger> EnumToTrigger;

		private Dictionary<AxisButtonCode, AxisButton> AxisButtonCodeToAxisButton;
		private Dictionary<TriggerCode, TriggerButton> TriggerCodeToTriggerButton;

		private VirtualButton[] VirtualButtons;

		internal Gamepad(IntPtr handle, int slot)
		{
			Handle = handle;
			Slot = slot;
			JoystickInstanceID = 0;

			Name = "Not Connected";

			AnyPressed = false;

			South = new GamepadButton(this, GamepadButtonCode.South, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_SOUTH);
			East = new GamepadButton(this, GamepadButtonCode.East, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_EAST);
			West = new GamepadButton(this, GamepadButtonCode.West, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_WEST);
			North = new GamepadButton(this, GamepadButtonCode.North, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_NORTH);

			Back = new GamepadButton(this, GamepadButtonCode.Back, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_BACK);
			Guide = new GamepadButton(this, GamepadButtonCode.Guide, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_GUIDE);
			Start = new GamepadButton(this, GamepadButtonCode.Start, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_START);

			LeftStick = new GamepadButton(this, GamepadButtonCode.LeftStick, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_STICK);
			RightStick = new GamepadButton(this, GamepadButtonCode.RightStick, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_STICK);

			LeftShoulder = new GamepadButton(this, GamepadButtonCode.LeftShoulder, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_SHOULDER);
			RightShoulder = new GamepadButton(this, GamepadButtonCode.RightShoulder, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER);

			DpadUp = new GamepadButton(this, GamepadButtonCode.DpadUp, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_UP);
			DpadDown = new GamepadButton(this, GamepadButtonCode.DpadDown, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_DOWN);
			DpadLeft = new GamepadButton(this, GamepadButtonCode.DpadLeft, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_LEFT);
			DpadRight = new GamepadButton(this, GamepadButtonCode.DpadRight, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_RIGHT);

			Misc1 = new GamepadButton(this, GamepadButtonCode.Misc1, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC1);

			RightPaddle1 = new GamepadButton(this, GamepadButtonCode.RightPaddle1, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_PADDLE1);
			LeftPaddle1 = new GamepadButton(this, GamepadButtonCode.LeftPaddle1, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_PADDLE1);
			RightPaddle2 = new GamepadButton(this, GamepadButtonCode.RightPaddle2, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_PADDLE2);
			LeftPaddle2 = new GamepadButton(this, GamepadButtonCode.LeftPaddle2, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_PADDLE2);

			TouchPad = new GamepadButton(this, GamepadButtonCode.TouchPad, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_TOUCHPAD);

			Misc2 = new GamepadButton(this, GamepadButtonCode.Misc2, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC2);
			Misc3 = new GamepadButton(this, GamepadButtonCode.Misc3, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC3);
			Misc4 = new GamepadButton(this, GamepadButtonCode.Misc4, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC4);
			Misc5 = new GamepadButton(this, GamepadButtonCode.Misc5, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC5);
			Misc6 = new GamepadButton(this, GamepadButtonCode.Misc6, SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC6);

			LeftX = new Axis(this, AxisCode.LeftX, SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTX);
			LeftY = new Axis(this, AxisCode.LeftY, SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTY);
			RightX = new Axis(this, AxisCode.RightX, SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTX);
			RightY = new Axis(this, AxisCode.RightY, SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTY);

			LeftXLeft = new AxisButton(LeftX, false);
			LeftXRight = new AxisButton(LeftX, true);
			LeftYUp = new AxisButton(LeftY, false);
			LeftYDown = new AxisButton(LeftY, true);

			RightXLeft = new AxisButton(RightX, false);
			RightXRight = new AxisButton(RightX, true);
			RightYUp = new AxisButton(RightY, false);
			RightYDown = new AxisButton(RightY, true);

			TriggerLeft = new Trigger(this, TriggerCode.Left, SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFT_TRIGGER);
			TriggerRight = new Trigger(this, TriggerCode.Right, SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHT_TRIGGER);

			TriggerLeftButton = new TriggerButton(TriggerLeft);
			TriggerRightButton = new TriggerButton(TriggerRight);

			EnumToButton = new Dictionary<SDL.SDL_GamepadButton, GamepadButton>
			{
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_SOUTH, South },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_EAST, East },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_WEST, West },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_NORTH, North },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_BACK, Back },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_GUIDE, Guide },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_START, Start },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_STICK, LeftStick },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_STICK, RightStick },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_SHOULDER, LeftShoulder },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER, RightShoulder },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_UP, DpadUp },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_DOWN, DpadDown },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_LEFT, DpadLeft },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_RIGHT, DpadRight },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC1, Misc1 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_PADDLE1, RightPaddle1 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_PADDLE1, LeftPaddle1 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_PADDLE2, RightPaddle2 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_PADDLE2, LeftPaddle2 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_TOUCHPAD, TouchPad },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC2, Misc2 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC3, Misc3 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC4, Misc4 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC5, Misc5 },
				{ SDL.SDL_GamepadButton.SDL_GAMEPAD_BUTTON_MISC6, Misc6 }
			};

			EnumToAxis = new Dictionary<SDL.SDL_GamepadAxis, Axis>
			{
				{ SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTX, LeftX },
				{ SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFTY, LeftY },
				{ SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTX, RightX },
				{ SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHTY, RightY }
			};

			EnumToTrigger = new Dictionary<SDL.SDL_GamepadAxis, Trigger>
			{
				{ SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_LEFT_TRIGGER, TriggerLeft },
				{ SDL.SDL_GamepadAxis.SDL_GAMEPAD_AXIS_RIGHT_TRIGGER, TriggerRight }
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

			TriggerCodeToTriggerButton = new Dictionary<TriggerCode, TriggerButton>
			{
				{ TriggerCode.Left, TriggerLeftButton },
				{ TriggerCode.Right, TriggerRightButton }
			};

			VirtualButtons = new VirtualButton[]
			{
				South,
				East,
				West,
				North,
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
				Misc1,
				RightPaddle1,
				LeftPaddle1,
				RightPaddle2,
				LeftPaddle2,
				TouchPad,
				Misc2,
				Misc3,
				Misc4,
				Misc5,
				Misc6,
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

			ButtonEvents = new List<SDL.SDL_GamepadButtonEvent>[EnumToButton.Count];
			for (var i = 0; i < EnumToButton.Count; i += 1)
            {
                ButtonEvents[i] = [];
            }

			AxisEvents = new List<SDL.SDL_GamepadAxisEvent>[EnumToAxis.Count];
			for (var i = 0; i < EnumToAxis.Count; i += 1)
            {
                AxisEvents[i] = [];
            }

			TriggerEvents = new List<SDL.SDL_GamepadAxisEvent>[EnumToTrigger.Count];
			for (var i = 0; i < EnumToTrigger.Count; i += 1)
            {
                TriggerEvents[i] = [];
            }
		}

		internal void Register(IntPtr handle)
		{
			Handle = handle;

			IntPtr joystickHandle = SDL.SDL_GetGamepadJoystick(Handle);
			JoystickInstanceID = SDL.SDL_GetJoystickID(joystickHandle);

			GamepadType = (GamepadType) SDL.SDL_GetGamepadType(Handle);
			GamepadFamily = GamepadType switch
			{
				GamepadType.Xbox360 => GamepadFamily.Xbox,
				GamepadType.XboxOne => GamepadFamily.Xbox,
				GamepadType.PS3 => GamepadFamily.PlayStation,
				GamepadType.PS4 => GamepadFamily.PlayStation,
				GamepadType.PS5 => GamepadFamily.PlayStation,
				GamepadType.SwitchPro => GamepadFamily.Nintendo,
				GamepadType.SwitchJoyConLeft => GamepadFamily.Nintendo,
				GamepadType.SwitchJoyConRight => GamepadFamily.Nintendo,
				GamepadType.SwitchJoyConPair => GamepadFamily.Nintendo,
				_ => GamepadFamily.Generic
			};

			Name = SDL.SDL_GetGamepadName(Handle);

			var props = SDL.SDL_GetGamepadProperties(Handle);
			HasLED = SDL.SDL_GetBooleanProperty(props, "SDL.joystick.cap.rgb_led", false);
			SDL.SDL_DestroyProperties(props);
		}

		internal void Unregister()
		{
			Handle = IntPtr.Zero;
			JoystickInstanceID = 0;
			GamepadType = GamepadType.Unknown;
			GamepadFamily = GamepadFamily.Generic;
			Name = "Not Connected";
		}

		internal void AddButtonEvent(SDL.SDL_GamepadButtonEvent evt)
        {
			ButtonEvents[evt.button].Add(evt);
        }

		internal void AddAxisEvent(SDL.SDL_GamepadAxisEvent evt)
        {
			if (EnumToAxis.TryGetValue((SDL.SDL_GamepadAxis) evt.axis, out var axis))
            {
                AxisEvents[(int) axis.Code].Add(evt);
            }
			else if (EnumToTrigger.TryGetValue((SDL.SDL_GamepadAxis) evt.axis, out var trigger))
            {
                TriggerEvents[(int) trigger.Code - 4].Add(evt);
            }
        }

		private static bool ButtonDown(List<SDL.SDL_GamepadButtonEvent> events)
        {
			foreach (var buttonEvent in events)
			{
				if (buttonEvent.down)
				{
					return true;
				}
			}
			return false;
        }

		internal void Update()
		{
			AnyPressed = false;

			if (!IsDummy)
			{
				// Update input state from events

				foreach (var button in EnumToButton.Values)
                {
                    var events = ButtonEvents[(int) button.Code];
					button.Update(events.Count > 0 ? ButtonDown(events) : button.IsDown);
					events.Clear();
                }

				foreach (var axis in EnumToAxis.Values)
				{
					var events = AxisEvents[(int) axis.Code];
					if (events.Count > 0)
                    {
						var latest = events[^1];
                        axis.SetValue(latest.value);
                    }

					switch (axis.Code)
					{
						case AxisCode.LeftX:
							LeftXLeft.Update();
							LeftXRight.Update();
							break;

						case AxisCode.LeftY:
							LeftYUp.Update();
							LeftYDown.Update();
							break;

						case AxisCode.RightX:
							RightXLeft.Update();
							RightXRight.Update();
							break;

						case AxisCode.RightY:
							RightYUp.Update();
							RightYDown.Update();
							break;
					}

					events.Clear();
				}

				foreach (var trigger in EnumToTrigger.Values)
				{
					var events = TriggerEvents[(int) trigger.Code - 4];
					if (events.Count > 0)
                    {
						var latest = events[^1];
						trigger.SetValue(latest.value);
                    }

					switch (trigger.Code)
					{
						case TriggerCode.Left:
							TriggerLeftButton.Update();
							break;

						case TriggerCode.Right:
							TriggerRightButton.Update();
							break;
					}

					events.Clear();
				}

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
			return SDL.SDL_RumbleGamepad(
				Handle,
				(ushort) (float.Clamp(leftMotor, 0f, 1f) * 0xFFFF),
				(ushort) (float.Clamp(rightMotor, 0f, 1f) * 0xFFFF),
				durationInMilliseconds
			);
		}

		public bool SetLED(MoonWorks.Graphics.Color color)
		{
			if (!HasLED)
			{
				return false;
			}

			if (!SDL.SDL_SetGamepadLED(Handle, color.R, color.G, color.B))
			{
				Logger.LogError(SDL.SDL_GetError());
				return false;
			}

			return true;
		}

		/// <summary>
		/// Obtains a gamepad button object given a button code.
		/// </summary>
		public VirtualButton Button(GamepadButtonCode buttonCode)
		{
			// The invalid button code exists, so wrap this for safety
			if (EnumToButton.TryGetValue((SDL.SDL_GamepadButton) buttonCode, out var virtualButton))
			{
				return virtualButton;
			}

			return EmptyButton.Empty;
		}

		/// <summary>
		/// Obtains an axis button object given a button code.
		/// </summary>
		public VirtualButton Button(AxisButtonCode axisButtonCode)
		{
			return AxisButtonCodeToAxisButton[axisButtonCode];
		}

		/// <summary>
		/// Obtains a trigger button object given a button code.
		/// </summary>
		public VirtualButton Button(TriggerCode triggerCode)
		{
			return TriggerCodeToTriggerButton[triggerCode];
		}

		/// <summary>
		/// Obtains the axis value given an AxisCode.
		/// </summary>
		/// <returns>A value between -1 and 1.</returns>
		public float AxisValue(AxisCode axisCode)
		{
			return EnumToAxis[(SDL.SDL_GamepadAxis) axisCode].Value;
		}

		/// <summary>
		/// Obtains the trigger value given an TriggerCode.
		/// </summary>
		/// <returns>A value between 0 and 1.</returns>
		public float TriggerValue(TriggerCode triggerCode)
		{
			return EnumToTrigger[(SDL.SDL_GamepadAxis) triggerCode].Value;
		}
	}
}
