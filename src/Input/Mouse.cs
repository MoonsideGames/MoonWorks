using System.Collections.Generic;
using SDL2;

namespace MoonWorks.Input
{
	public class Mouse
	{
		public Button LeftButton { get; } = new Button();
		public Button MiddleButton { get; } = new Button();
		public Button RightButton { get; } = new Button();

		public int X { get; private set; }
		public int Y { get; private set; }
		public int DeltaX { get; private set; }
		public int DeltaY { get; private set; }

		public int Wheel { get; internal set; }

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

		private readonly Dictionary<MouseButtonCode, Button> CodeToButton;

		public Mouse()
		{
			CodeToButton = new Dictionary<MouseButtonCode, Button>
			{
				{ MouseButtonCode.Left, LeftButton },
				{ MouseButtonCode.Right, RightButton },
				{ MouseButtonCode.Middle, MiddleButton }
			};
		}

		internal void Update()
		{
			var buttonMask = SDL.SDL_GetMouseState(out var x, out var y);
			var _ = SDL.SDL_GetRelativeMouseState(out var deltaX, out var deltaY);

			X = x;
			Y = y;
			DeltaX = deltaX;
			DeltaY = deltaY;

			LeftButton.Update(IsPressed(buttonMask, SDL.SDL_BUTTON_LMASK));
			MiddleButton.Update(IsPressed(buttonMask, SDL.SDL_BUTTON_MMASK));
			RightButton.Update(IsPressed(buttonMask, SDL.SDL_BUTTON_RMASK));
		}

		public ButtonState ButtonState(MouseButtonCode buttonCode)
		{
			return CodeToButton[buttonCode].State;
		}

		private bool IsPressed(uint buttonMask, uint buttonFlag)
		{
			return (buttonMask & buttonFlag) != 0;
		}
	}
}
