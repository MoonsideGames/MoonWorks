using System;
using MoonWorks.Math;
using SDL2;

namespace MoonWorks.Input
{
	public class Axis
	{
		IntPtr GamepadHandle;
		SDL.SDL_GameControllerAxis SDL_Axis;

		public AxisCode Code { get; private set; }

		/// <summary>
		/// An axis value between -1 and 1.
		/// </summary>
		public float Value { get; private set; }

		public Axis(
			IntPtr gamepadHandle,
			AxisCode code,
			SDL.SDL_GameControllerAxis sdlAxis
		) {
			GamepadHandle = gamepadHandle;
			SDL_Axis = sdlAxis;
			Code = code;
		}

		internal void Update()
		{
			Value = MathHelper.Normalize(
				SDL.SDL_GameControllerGetAxis(GamepadHandle, SDL_Axis),
				short.MinValue, short.MaxValue,
				-1, 1
			);
		}
	}
}
