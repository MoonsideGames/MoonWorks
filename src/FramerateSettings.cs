namespace MoonWorks
{
	public enum FramerateMode
	{
		Uncapped,
		Capped
	}

	public struct FramerateSettings
	{
		public FramerateMode Mode;
		public int Cap;
	}
}
