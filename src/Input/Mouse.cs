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

		// note that this is a delta value
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

		internal SDL.SDL_MouseButtonFlags ButtonMask { get; private set; }

		public bool Visible => SDL.SDL_CursorVisible();

		private readonly Dictionary<MouseButtonCode, MouseButton> CodeToButton;

		internal Mouse()
		{
			LeftButton = new MouseButton(this, MouseButtonCode.Left, SDL.SDL_MouseButtonFlags.SDL_BUTTON_LMASK);
			MiddleButton = new MouseButton(this, MouseButtonCode.Middle, SDL.SDL_MouseButtonFlags.SDL_BUTTON_MMASK);
			RightButton = new MouseButton(this, MouseButtonCode.Right, SDL.SDL_MouseButtonFlags.SDL_BUTTON_RMASK);
			X1Button = new MouseButton(this, MouseButtonCode.X1, SDL.SDL_MouseButtonFlags.SDL_BUTTON_X1MASK);
			X2Button = new MouseButton(this, MouseButtonCode.X2, SDL.SDL_MouseButtonFlags.SDL_BUTTON_X2MASK);

			CodeToButton = new Dictionary<MouseButtonCode, MouseButton>
			{
				{ MouseButtonCode.Left, LeftButton },
				{ MouseButtonCode.Right, RightButton },
				{ MouseButtonCode.Middle, MiddleButton },
				{ MouseButtonCode.X1, X1Button },
				{ MouseButtonCode.X2, X2Button }
			};
		}

		internal void Update()
		{
			AnyPressed = false;

			ButtonMask = SDL.SDL_GetMouseState(out var x, out var y);
			var _ = SDL.SDL_GetRelativeMouseState(out var deltaX, out var deltaY);

			// TODO: should we support subpixel movement?
			X = (int) x;
			Y = (int) y;
			DeltaX = (int) deltaX;
			DeltaY = (int) deltaY;

			Wheel = WheelRaw - previousWheelRaw;
			previousWheelRaw = WheelRaw;

			foreach (var button in CodeToButton.Values)
			{
				button.Update();

				if (button.IsPressed)
				{
					AnyPressed = true;
					AnyPressedButton = button;
				}
			}
		}

		/// <summary>
		/// Gets a button from the mouse given a MouseButtonCode.
		/// </summary>
		public MouseButton Button(MouseButtonCode buttonCode)
		{
			return CodeToButton[buttonCode];
		}

		/// <summary>
		/// Gets a button state from a mouse button corresponding to the given MouseButtonCode.
		/// </summary>
		public ButtonState ButtonState(MouseButtonCode buttonCode)
		{
			return CodeToButton[buttonCode].State;
		}

		public void Show()
		{
			SDL.SDL_ShowCursor();
		}

		public void Hide()
		{
			SDL.SDL_HideCursor();
		}
	}
}
