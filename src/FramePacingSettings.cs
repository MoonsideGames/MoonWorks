using System;

namespace MoonWorks
{
	/// <summary>
	/// Specifies the frame pacing strategy of the Game's tick loop.
	/// </summary>
	public enum FramePacingMode
	{
		/// <summary>
		/// The game will call Draw at the same pace as Update.
		/// The tick loop will wait on the swapchain right before events are processed to minimize visual latency.
		/// Note that this will lead to lower framerate in GPU-bound scenarios.
		/// </summary>
		LatencyOptimized,
		/// <summary>
		/// The game will call Draw at the same pace as Update.
		/// Has the same CPU power characteristics as LatencyOptimized, but with better GPU utilization at the cost of input latency.
		/// </summary>
		Capped,
		/// <summary>
		/// If the GraphicsDevice.PresentMode is set to VSync, the Draw rate will be limited by the monitor refresh rate.
		/// Otherwise the game will call Draw at the maximum possible framerate that the computing resources allow. <br/>
		/// Note that this may lead to overheating, resource starvation, etc. <br/>
		/// Even with Vsync on, the CPU can't sleep as often, so this setting may cause increased power consumption.
		/// </summary>
		Uncapped
	}

	/// <summary>
	/// The Game's frame pacing settings. Specifies uncapped framerate or a maximum rendering frames per second value. <br/>
	/// </summary>
	public struct FramePacingSettings
	{
		/// <summary>
		/// Specifies the frame pacing strategy of the Game's tick loop.
		/// </summary>
		public FramePacingMode Mode { get; private set;}

		/// <summary>
		/// Represents how often Game.Update will called.
		/// If Mode is set to Capped or LatencyOptimized, this also represents how often Game.Draw will be called.
		/// If Mode is set to Uncapped, Game.Draw will run as fast as possible.
		/// </summary>
		public TimeSpan Timestep { get; private set; }

		/// <summary>
		/// If a previous frame took too long, this is how many times Game.Update can be called to catch up before continuing.
		/// </summary>
		public int MaxUpdatesPerTick { get; private set; }

		/// <summary>
		/// The game will render at the same pace as the timestep.
		/// The tick loop will wait on the swapchain right before events are processed to minimize visual latency.
		/// Note that this will lead to lower throughput in GPU-bound scenarios.
		/// </summary>
		public static FramePacingSettings CreateLatencyOptimized(
			int timestepFPS,
			int maxUpdatesPerTick
		) {
			return new FramePacingSettings(
				FramePacingMode.LatencyOptimized,
				timestepFPS,
				maxUpdatesPerTick
			);
		}

		/// <summary>
		/// The game will render no more than the specified frames per second.
		/// The framerate limit may be different from the timestep.
		/// </summary>
		public static FramePacingSettings CreateCapped(
			int timestepFPS,
			int maxUpdatesPerTick
		) {
			return new FramePacingSettings(
				FramePacingMode.Capped,
				timestepFPS,
				maxUpdatesPerTick
			);
		}

		/// <summary>
		/// The game will render at the maximum possible framerate that the computing resources allow. <br/>
		/// Note that this may lead to overheating, resource starvation, etc. <br/>
		/// If the GraphicsDevice.PresentMode is set to VSYNC, the framerate will be limited by the monitor refresh rate.
		/// </summary>
		public static FramePacingSettings CreateUncapped(
			int timestepFPS,
			int maxUpdatesPerTick
		) {
			return new FramePacingSettings(
				FramePacingMode.Uncapped,
				timestepFPS,
				maxUpdatesPerTick
			);
		}

		private FramePacingSettings(
			FramePacingMode mode,
			int timestepFPS,
			int maxUpdatesPerTick
		) {
			Mode = mode;
			Timestep = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / timestepFPS);
			MaxUpdatesPerTick = maxUpdatesPerTick;
		}
	}
}
