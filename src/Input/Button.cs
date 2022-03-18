namespace MoonWorks.Input
{
	public class Button
	{
		public ButtonState State { get; private set; }

		/// <summary>
		/// True if the button is pressed or held.
		/// </summary>
		public bool IsDown => State.IsDown;

		/// <summary>
		/// True if the button has been continuously held for more than one frame.
		/// </summary>
		public bool IsHeld => State.IsHeld;

		/// <summary>
		/// True if the button was pressed this exact frame.
		/// </summary>
		public bool IsPressed => State.IsPressed;

		/// <summary>
		/// True if the button is not pressed.
		/// </summary>
		public bool IsReleased => State.IsReleased;

		internal void Update(bool isPressed)
		{
			State = State.Update(isPressed);
		}
	}
}
