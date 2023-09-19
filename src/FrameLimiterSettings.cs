namespace MoonWorks
{
	public enum FrameLimiterMode
	{
		/// <summary>
		/// The game will render at the maximum possible framerate that the computing resources allow. <br/>
		/// Note that this may lead to overheating, resource starvation, etc.
		/// </summary>
		Uncapped,
		/// <summary>
		/// The game will render no more than the specified frames per second.
		/// </summary>
		Capped
	}

	/// <summary>
	/// The Game's frame limiter setting. Specifies uncapped framerate or a maximum rendering frames per second value. <br/>
	/// Note that this is separate from the Game's Update timestep and can be a different value.
	/// </summary>
	public struct FrameLimiterSettings
	{
		public FrameLimiterMode Mode;
		/// <summary>
		/// If Mode is set to Capped, this is the maximum frames per second that will be rendered.
		/// </summary>
		public int Cap;

		public FrameLimiterSettings(
			FrameLimiterMode mode,
			int cap
		) {
			Mode = mode;
			Cap = cap;
		}
	}
}
