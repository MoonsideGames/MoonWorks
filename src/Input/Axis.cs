using MoonWorks.Math;
using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// Represents a specific joystick direction on a gamepad.
	/// </summary>
	public class Axis
	{
		public Gamepad Parent { get; }
		SDL.SDL_GamepadAxis SDL_Axis;

		public AxisCode Code { get; private set; }

		/// <summary>
		/// An axis value between -1 and 1.
		/// </summary>
		public float Value { get; private set; }

		public Axis(
			Gamepad parent,
			AxisCode code,
			SDL.SDL_GamepadAxis sdlAxis
		) {
			Parent = parent;
			SDL_Axis = sdlAxis;
			Code = code;
		}

		internal void Update()
		{
			Value = MathHelper.Normalize(
				SDL.SDL_GetGamepadAxis(Parent.Handle, SDL_Axis),
				short.MinValue, short.MaxValue,
				-1, 1
			);
		}
	}
}
