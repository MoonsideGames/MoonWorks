using System;
using SDL2;

namespace MoonWorks
{
	public class Window : IDisposable
	{
		internal IntPtr Handle { get; }
		public ScreenMode ScreenMode { get; private set; }
		public uint Width { get; private set; }
		public uint Height { get; private set; }

		private bool IsDisposed;

		public Window(WindowCreateInfo windowCreateInfo)
		{
			var windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN;

			if (windowCreateInfo.ScreenMode == ScreenMode.Fullscreen)
			{
				windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
			}
			else if (windowCreateInfo.ScreenMode == ScreenMode.BorderlessWindow)
			{
				windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
			}

			if (windowCreateInfo.SystemResizable)
			{
				windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
			}

			ScreenMode = windowCreateInfo.ScreenMode;

			Handle = SDL.SDL_CreateWindow(
				windowCreateInfo.WindowTitle,
				SDL.SDL_WINDOWPOS_UNDEFINED,
				SDL.SDL_WINDOWPOS_UNDEFINED,
				(int) windowCreateInfo.WindowWidth,
				(int) windowCreateInfo.WindowHeight,
				windowFlags
			);

			Width = windowCreateInfo.WindowWidth;
			Height = windowCreateInfo.WindowHeight;
		}

		public void ChangeScreenMode(ScreenMode screenMode)
		{
			SDL.SDL_WindowFlags windowFlag = 0;

			if (screenMode == ScreenMode.Fullscreen)
			{
				windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
			}
			else if (screenMode == ScreenMode.BorderlessWindow)
			{
				windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
			}

			ScreenMode = screenMode;

			SDL.SDL_SetWindowFullscreen(Handle, (uint) windowFlag);
		}

		/// <summary>
		/// Resizes the window.
		/// Note that you are responsible for recreating any graphics resources that need to change as a result of the size change.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void SetWindowSize(uint width, uint height)
		{
			SDL.SDL_SetWindowSize(Handle, (int) width, (int) height);
			Width = width;
			Height = height;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					// dispose managed state (managed objects)
				}

				SDL.SDL_DestroyWindow(Handle);

				IsDisposed = true;
			}
		}

		~Window()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
