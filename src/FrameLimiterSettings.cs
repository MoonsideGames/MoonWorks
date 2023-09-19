namespace MoonWorks
{
	/// <summary>
	/// The Game's frame limiter mode. Specifies a maximum rendering frames per second value.
	/// </summary>
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
