namespace MoonWorks.Input
{
	public class Button
	{
		public ButtonState State { get; private set; }

		public bool IsDown => State.IsDown;
		public bool IsHeld => State.IsHeld;
		public bool IsPressed => State.IsPressed;
		public bool IsReleased => State.IsReleased;

		internal void Update(bool isPressed)
		{
			State = State.Update(isPressed);
		}
	}
}
