namespace MoonWorks.Input
{
	public enum ButtonStatus
	{
		/// <summary>
		/// Indicates that the button was not pressed last frame and is still not pressed.
		/// </summary>
		Idle,
		/// <summary>
		/// Indicates that the button was released this frame.
		/// </summary>
		Released,
		/// <summary>
		/// Indicates that the button was pressed this frame.
		/// </summary>
		Pressed,
		/// <summary>
		/// Indicates that the button has been held for multiple frames.
		/// </summary>
		Held
	}
}
