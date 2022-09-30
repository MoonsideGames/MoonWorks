namespace MoonWorks
{
	public struct WindowCreateInfo
	{
		public string WindowTitle;
		public uint WindowWidth;
		public uint WindowHeight;
		public ScreenMode ScreenMode;
		public PresentMode PresentMode;
		public bool SystemResizable;
		public bool StartMaximized;

		public WindowCreateInfo(
			string windowTitle,
			uint windowWidth,
			uint windowHeight,
			ScreenMode screenMode,
			PresentMode presentMode,
			bool systemResizable = false,
			bool startMaximized = false
		) {
			WindowTitle = windowTitle;
			WindowWidth = windowWidth;
			WindowHeight = windowHeight;
			ScreenMode = screenMode;
			PresentMode = presentMode;
			SystemResizable = systemResizable;
			StartMaximized = startMaximized;
		}
	}
}
