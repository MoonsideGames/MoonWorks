namespace MoonWorks
{
	/// <summary>
	/// All the information required for window creation.
	/// </summary>
	public struct WindowCreateInfo
	{
		/// <summary>
		/// The name of the window that will be displayed in the operating system.
		/// </summary>
		public string WindowTitle;
		/// <summary>
		/// The width of the window.
		/// </summary>
		public uint WindowWidth;
		/// <summary>
		/// The height of the window.
		/// </summary>
		public uint WindowHeight;
		/// <summary>
		/// Specifies if the window will be created in windowed mode or a fullscreen mode.
		/// </summary>
		public ScreenMode ScreenMode;
		/// <summary>
		/// Whether the window can be resized using the operating system's window dragging feature.
		/// </summary>
		public bool SystemResizable;
		/// <summary>
		/// Specifies if the window will open at the maximum desktop resolution.
		/// </summary>
		public bool StartMaximized;
		/// <summary>
		/// Specifies that a high pixel density window should be requested.
		/// If the system does not support high DPI, this will have no effect.
		/// </summary>
		public bool HighDPI;

		public WindowCreateInfo(
			string windowTitle,
			uint windowWidth,
			uint windowHeight,
			ScreenMode screenMode,
			bool systemResizable = false,
			bool startMaximized = false,
			bool highDPI = false
		)
		{
			WindowTitle = windowTitle;
			WindowWidth = windowWidth;
			WindowHeight = windowHeight;
			ScreenMode = screenMode;
			SystemResizable = systemResizable;
			StartMaximized = startMaximized;
			HighDPI = highDPI;
		}
	}
}
