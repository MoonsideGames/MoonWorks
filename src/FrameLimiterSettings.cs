namespace MoonWorks
{
	public enum FrameLimiterMode
	{
		Uncapped,
		Capped
	}

	public struct FrameLimiterSettings
	{
		public FrameLimiterMode Mode;
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
