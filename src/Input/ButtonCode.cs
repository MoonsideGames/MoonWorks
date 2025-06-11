namespace MoonWorks.Input
{
	/// <summary>
	/// Can be used to access a gamepad button without a direct reference to the button object.
 	/// Enum values are equivalent to the SDL_GamepadButton value.
	/// </summary>
	public enum GamepadButtonCode
	{
		Invalid,
		South,
		East,
		West,
		North,
		Back,
		Guide,
		Start,
		LeftStick,
		RightStick,
		LeftShoulder,
		RightShoulder,
		DpadUp,
		DpadDown,
		DpadLeft,
		DpadRight,
		Misc1,
		RightPaddle1,
		LeftPaddle1,
		RightPaddle2,
		LeftPaddle2,
		TouchPad,
		Misc2,
		Misc3,
		Misc4,
		Misc5,
		Misc6
	}
}
