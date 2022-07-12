using System;
using MoonWorks.Math;
using SDL2;

namespace MoonWorks.Input
{
	public class Trigger
	{
		public IntPtr GamepadHandle;
		public SDL.SDL_GameControllerAxis SDL_Axis;

		public TriggerCode Code { get; }

		/// <summary>
		/// A trigger value between 0 and 1.
		/// </summary>
		public float Value { get; private set; }

		public Trigger(
			IntPtr gamepadHandle,
			TriggerCode code,
			SDL.SDL_GameControllerAxis sdlAxis
		) {
			GamepadHandle = gamepadHandle;
			Code = code;
			SDL_Axis = sdlAxis;
		}

		internal void Update()
		{
			Value = MathHelper.Normalize(
				SDL.SDL_GameControllerGetAxis(GamepadHandle, SDL_Axis),
				0, short.MaxValue,
				0, 1
			);
		}
	}
}
