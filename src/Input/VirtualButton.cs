namespace MoonWorks.Input
{
	/// <summary>
	/// VirtualButtons map inputs to binary inputs, like a trigger threshold or joystick axis threshold.
	/// </summary>
	public abstract class VirtualButton
	{
		public ButtonState State { get; protected set; }

		/// <summary>
		/// True if the button was pressed this exact frame.
		/// </summary>
		public bool IsPressed => State.IsPressed;

		/// <summary>
		/// True if the button has been continuously held for more than one frame.
		/// </summary>
		public bool IsHeld => State.IsHeld;

		/// <summary>
		/// True if the button is pressed or held.
		/// </summary>
		public bool IsDown => State.IsDown;

		/// <summary>
		/// True if the button was released this frame.
		/// </summary>
		public bool IsReleased => State.IsReleased;

		/// <summary>
		/// True if the button was not pressed the previous or current frame.
		/// </summary>
		public bool IsIdle => State.IsIdle;

		/// <summary>
		/// True if the button is idle or released.
		/// </summary>
		public bool IsUp => State.IsUp;

		protected void UpdateState(bool isPressed)
		{
			State = State.Update(isPressed);
		}
	}
}
