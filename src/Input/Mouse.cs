using System.Collections.Generic;
using SDL2;

namespace MoonWorks.Input
{
	public class Mouse
	{
		public MouseButton LeftButton { get; }
		public MouseButton MiddleButton { get; }
		public MouseButton RightButton { get; }

		public int X { get; private set; }
		public int Y { get; private set; }
		public int DeltaX { get; private set; }
		public int DeltaY { get; private set; }

		// note that this is a delta value
		public int Wheel { get; private set; }
		internal int WheelRaw;
		private int previousWheelRaw = 0;

		public bool AnyPressed { get; private set; }
		public MouseButton AnyPressedButton { get; private set; }

		public uint ButtonMask { get; private set; }

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

		private readonly Dictionary<MouseButtonCode, MouseButton> CodeToButton;

		private IEnumerable<MouseButton> Buttons
		{
			get
			{
				yield return LeftButton;
				yield return MiddleButton;
				yield return RightButton;
			}
		}

		public Mouse()
		{
			LeftButton = new MouseButton(this, MouseButtonCode.Left, SDL.SDL_BUTTON_LMASK);
			MiddleButton = new MouseButton(this, MouseButtonCode.Middle, SDL.SDL_BUTTON_MMASK);
			RightButton = new MouseButton(this, MouseButtonCode.Right, SDL.SDL_BUTTON_RMASK);

			CodeToButton = new Dictionary<MouseButtonCode, MouseButton>
			{
				{ MouseButtonCode.Left, LeftButton },
				{ MouseButtonCode.Right, RightButton },
				{ MouseButtonCode.Middle, MiddleButton }
			};
		}

		internal void Update()
		{
			AnyPressed = false;

			ButtonMask = SDL.SDL_GetMouseState(out var x, out var y);
			var _ = SDL.SDL_GetRelativeMouseState(out var deltaX, out var deltaY);

			X = x;
			Y = y;
			DeltaX = deltaX;
			DeltaY = deltaY;

			Wheel = WheelRaw - previousWheelRaw;
			previousWheelRaw = WheelRaw;

			LeftButton.Update();
			MiddleButton.Update();
			RightButton.Update();

			foreach (var button in Buttons)
			{
				button.Update();

				if (button.IsPressed)
				{
					AnyPressed = true;
					AnyPressedButton = button;
				}
			}
		}

		public ButtonState ButtonState(MouseButtonCode buttonCode)
		{
			return CodeToButton[buttonCode].State;
		}
	}
}
