using MoonWorks.Math;
using SDL3;

namespace MoonWorks.Input
{
	/// <summary>
	/// Represents a trigger input on a gamepad.
	/// </summary>
	public class Trigger
	{
		public Gamepad Parent { get; }
		public SDL.SDL_GamepadAxis SDL_Axis;

		public TriggerCode Code { get; }

		/// <summary>
		/// A trigger value between 0 and 1.
		/// </summary>
		public float Value { get; private set; }

		public Trigger(
			Gamepad parent,
			TriggerCode code,
			SDL.SDL_GamepadAxis sdlAxis
		) {
			Parent = parent;
			Code = code;
			SDL_Axis = sdlAxis;
		}

		internal void Update()
		{
			Value = MathHelper.Normalize(
				SDL.SDL_GetGamepadAxis(Parent.Handle, SDL_Axis),
				0, short.MaxValue,
				0, 1
			);
		}
	}
}
