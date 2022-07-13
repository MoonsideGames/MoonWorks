using MoonWorks.Math;
using SDL2;

namespace MoonWorks.Input
{
	public class Trigger
	{
		public Gamepad Parent { get; }
		public SDL.SDL_GameControllerAxis SDL_Axis;

		public TriggerCode Code { get; }

		/// <summary>
		/// A trigger value between 0 and 1.
		/// </summary>
		public float Value { get; private set; }

		public Trigger(
			Gamepad parent,
			TriggerCode code,
			SDL.SDL_GameControllerAxis sdlAxis
		) {
			Parent = parent;
			Code = code;
			SDL_Axis = sdlAxis;
		}

		internal void Update()
		{
			Value = MathHelper.Normalize(
				SDL.SDL_GameControllerGetAxis(Parent.Handle, SDL_Axis),
				0, short.MaxValue,
				0, 1
			);
		}
	}
}
