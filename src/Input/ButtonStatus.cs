namespace MoonWorks.Input
{
	internal enum ButtonStatus
	{
		/// <summary>
		/// Indicates that the input is not pressed.
		/// </summary>
		Released,
		/// <summary>
		/// Indicates that the input was pressed this frame.
		/// </summary>
		Pressed,
		/// <summary>
		/// Indicates that the input has been held for multiple frames.
		/// </summary>
		Held
	}
}
