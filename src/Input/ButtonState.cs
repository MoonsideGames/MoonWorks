namespace MoonWorks.Input
{
	/// <summary>
	/// Container for the current state of a binary input.
	/// </summary>
	public struct ButtonState
	{
		public ButtonStatus ButtonStatus { get; }

		/// <summary>
		/// True if the button was pressed this frame.
		/// </summary>
		public bool IsPressed => ButtonStatus == ButtonStatus.Pressed;

		/// <summary>
		/// True if the button was pressed this frame and the previous frame.
		/// </summary>
		public bool IsHeld => ButtonStatus == ButtonStatus.Held;

		/// <summary>
		/// True if the button was either pressed or continued to be held this frame.
		/// </summary>
		public bool IsDown => ButtonStatus == ButtonStatus.Pressed || ButtonStatus == ButtonStatus.Held;

		/// <summary>
		/// True if the button was let go this frame.
		/// </summary>
		public bool IsReleased => ButtonStatus == ButtonStatus.Released;

		/// <summary>
		/// True if the button was not pressed this frame or the previous frame.
		/// </summary>
		public bool IsIdle => ButtonStatus == ButtonStatus.Idle;

		/// <summary>
		/// True if the button was either idle or released this frame.
		/// </summary>
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
		/// Combines two button states. Useful for alt control sets or input buffering.
		/// </summary>
		public static ButtonState operator |(ButtonState a, ButtonState b)
		{
			if (a.ButtonStatus == ButtonStatus.Idle)
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
			else if (a.ButtonStatus == ButtonStatus.Released)
			{
				if (b.ButtonStatus == ButtonStatus.Pressed || b.ButtonStatus == ButtonStatus.Held)
				{
					return b;
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
