using System.Collections.Generic;
using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// The mouse input device abstraction.
	/// </summary>
	public class Mouse
	{
		public MouseButton LeftButton { get; }
		public MouseButton MiddleButton { get; }
		public MouseButton RightButton { get; }
		public MouseButton X1Button { get; }
		public MouseButton X2Button { get; }

		public int X { get; private set; }
		public int Y { get; private set; }
		public int DeltaX { get; private set; }
		public int DeltaY { get; private set; }

		/// <summary>
		/// NOTE: this is a delta value.
		/// </summary>
		public int Wheel { get; private set; }
		internal int WheelRaw;
		private int previousWheelRaw = 0;

		/// <summary>
		/// True if any button on the keyboard is active. Useful for input remapping.
		/// </summary>
		public bool AnyPressed { get; private set; }

		/// <summary>
		/// Contains a reference to an arbitrary MouseButton that was pressed this frame. Useful for input remapping.
		/// </summary>
		public MouseButton AnyPressedButton { get; private set; }

		public bool Visible => SDL.SDL_CursorVisible();

		private readonly MouseButton[] CodeToButton;
		private readonly List<SDL.SDL_MouseButtonEvent>[] ButtonEvents = [];

		internal Mouse()
		{
			LeftButton = new MouseButton(this, MouseButtonCode.Left, 1);
			MiddleButton = new MouseButton(this, MouseButtonCode.Middle, 2);
			RightButton = new MouseButton(this, MouseButtonCode.Right, 3);
			X1Button = new MouseButton(this, MouseButtonCode.X1, 4);
			X2Button = new MouseButton(this, MouseButtonCode.X2, 5);

			CodeToButton =
			[
				LeftButton,
				RightButton,
				MiddleButton,
				X1Button,
				X2Button
			];
			ButtonEvents = new List<SDL.SDL_MouseButtonEvent>[CodeToButton.Length];

			for (var i = 0; i < CodeToButton.Length; i += 1)
            {
                ButtonEvents[i] = [];
            }
		}

		internal void AddButtonEvent(SDL.SDL_MouseButtonEvent evt)
        {
			ButtonEvents[evt.button - 1].Add(evt);
        }

		private static bool ButtonWasPressed(ulong frameTimestamp, List<SDL.SDL_MouseButtonEvent> events)
        {
			foreach (var buttonEvent in events)
			{
				if (buttonEvent.down && Inputs.TimestampDifference(frameTimestamp, buttonEvent.timestamp) < Inputs.ButtonDiscardThreshold)
				{
					return true;
				}
			}
			return false;
        }

		internal void Update(ulong timestamp)
		{
			AnyPressed = false;
			_ = SDL.SDL_GetMouseState(out var x, out var y);
			_ = SDL.SDL_GetRelativeMouseState(out var deltaX, out var deltaY);

			// TODO: should we support subpixel movement?
			X = (int) x;
			Y = (int) y;
			DeltaX = (int) deltaX;
			DeltaY = (int) deltaY;

			Wheel = WheelRaw - previousWheelRaw;
			previousWheelRaw = WheelRaw;

			foreach (var button in CodeToButton)
            {
                var events = ButtonEvents[button.Index - 1];

				bool isDown = button.Down;
				bool wasPressed = isDown;

				if (events.Count > 0)
                {
                    wasPressed = ButtonWasPressed(timestamp, events);
					isDown = events[^1].down;
					events.Clear();
                }

				button.Update(wasPressed, isDown);

				if (button.IsPressed)
				{
					AnyPressed = true;
					AnyPressedButton = button;
				}

            }
		}

		internal void ReleaseInputs()
		{
			AnyPressed = false;

			DeltaX = 0;
			DeltaY = 0;
			Wheel = 0;

			foreach (var button in CodeToButton)
			{
				if (button == null) { continue; }

				bool wasPressed = button.Down;
				bool isDown = false;

				button.Update(wasPressed, isDown);
			}
		}

		/// <summary>
		/// Gets a button from the mouse given a MouseButtonCode.
		/// </summary>
		public MouseButton Button(MouseButtonCode buttonCode)
		{
			return CodeToButton[(int) buttonCode];
		}

		/// <summary>
		/// Gets a button state from a mouse button corresponding to the given MouseButtonCode.
		/// </summary>
		public ButtonState ButtonState(MouseButtonCode buttonCode)
		{
			return CodeToButton[(int) buttonCode].State;
		}

		public void Show()
		{
			SDL.SDL_ShowCursor();
		}

		public void Hide()
		{
			SDL.SDL_HideCursor();
		}

		public void SetRelativeMode(Window window, bool enabled) => SDL.SDL_SetWindowRelativeMouseMode(window.Handle, enabled);
		public bool IsRelative(Window window) => SDL.SDL_GetWindowRelativeMouseMode(window.Handle);
	}
}
