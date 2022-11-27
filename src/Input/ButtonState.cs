namespace MoonWorks.Input
{
	public struct ButtonState
	{
		public ButtonStatus ButtonStatus { get; }

		public bool IsPressed => ButtonStatus == ButtonStatus.Pressed;
		public bool IsHeld => ButtonStatus == ButtonStatus.Held;
		public bool IsDown => ButtonStatus == ButtonStatus.Pressed || ButtonStatus == ButtonStatus.Held;
		public bool IsReleased => ButtonStatus == ButtonStatus.Released;
		public bool IsIdle => ButtonStatus == ButtonStatus.Idle;
		public bool IsUp => ButtonStatus == ButtonStatus.Idle || ButtonStatus == ButtonStatus.Released;

		public ButtonState(ButtonStatus buttonStatus)
		{
			ButtonStatus = buttonStatus;
		}

		internal ButtonState Update(bool isPressed)
		{
			if (isPressed)
			{
				if (IsUp)
				{
					return new ButtonState(ButtonStatus.Pressed);
				}
				else
				{
					return new ButtonState(ButtonStatus.Held);
				}
			}
			else
			{
				if (IsDown)
				{
					return new ButtonState(ButtonStatus.Released);
				}
				else
				{
					return new ButtonState(ButtonStatus.Idle);
				}
			}
		}

		/// <summary>
		/// Combines two button states. Useful for alt controls or input buffering.
		/// </summary>
		public static ButtonState operator |(ButtonState a, ButtonState b)
		{
			if (a.ButtonStatus == ButtonStatus.Idle || a.ButtonStatus == ButtonStatus.Released)
			{
				return b;
			}
			else if (a.ButtonStatus == ButtonStatus.Pressed)
			{
				if (b.ButtonStatus == ButtonStatus.Held)
				{
					return new ButtonState(ButtonStatus.Held);
				}
				else
				{
					return a;
				}
			}
			else // held
			{
				return a;
			}
		}
	}
}
