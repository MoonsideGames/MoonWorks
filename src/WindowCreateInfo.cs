namespace MoonWorks
{
	public struct WindowCreateInfo
	{
		public string WindowTitle;
		public uint WindowWidth;
		public uint WindowHeight;
		public ScreenMode ScreenMode;

		public WindowCreateInfo(
			string windowTitle,
			uint windowWidth,
			uint windowHeight,
			ScreenMode screenMode
		) {
			WindowTitle = windowTitle;
			WindowWidth = windowWidth;
			WindowHeight = windowHeight;
			ScreenMode = screenMode;
		}
	}
}
