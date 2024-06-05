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
		/// Specifies the swapchain composition. Use SDR unless you know what you're doing.
		/// </summary>
		public Graphics.SwapchainComposition SwapchainComposition;
		/// <summary>
		/// Specifies the presentation mode for the window. Roughly equivalent to V-Sync.
		/// </summary>
		public Graphics.PresentMode PresentMode;
		/// <summary>
		/// Whether the window can be resized using the operating system's window dragging feature.
		/// </summary>
		public bool SystemResizable;
		/// <summary>
		/// Specifies if the window will open at the maximum desktop resolution.
		/// </summary>
		public bool StartMaximized;

		public WindowCreateInfo(
			string windowTitle,
			uint windowWidth,
			uint windowHeight,
			ScreenMode screenMode,
			Graphics.PresentMode presentMode,
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
