namespace MoonWorks.Input
{
	public class ButtonState
	{
		private ButtonStatus ButtonStatus { get; set; }

		public bool IsPressed => ButtonStatus == ButtonStatus.Pressed;
		public bool IsHeld => ButtonStatus == ButtonStatus.Held;
		public bool IsDown => ButtonStatus == ButtonStatus.Pressed || ButtonStatus == ButtonStatus.Held;
		public bool IsReleased => ButtonStatus == ButtonStatus.Released;

		internal void Update(bool isPressed)
		{
			if (isPressed)
			{
				if (ButtonStatus == ButtonStatus.Pressed)
				{
					ButtonStatus = ButtonStatus.Held;
				}
				else if (ButtonStatus == ButtonStatus.Released)
				{
					ButtonStatus = ButtonStatus.Pressed;
				}
			}
			else
			{
				ButtonStatus = ButtonStatus.Released;
			}
		}
	}
}
