using System;
using System.Collections.Generic;
using SDL2;

namespace MoonWorks
{
	public class Window : IDisposable
	{
		internal IntPtr Handle { get; }
		public ScreenMode ScreenMode { get; private set; }
		public uint Width { get; private set; }
		public uint Height { get; private set; }

		public bool Claimed { get; internal set; }
		public MoonWorks.Graphics.TextureFormat SwapchainFormat { get; internal set; }

		private bool IsDisposed;

		private static Dictionary<uint, Window> idLookup = new Dictionary<uint, Window>();

		public Window(WindowCreateInfo windowCreateInfo, SDL.SDL_WindowFlags flags)
		{
			if (windowCreateInfo.ScreenMode == ScreenMode.Fullscreen)
			{
				flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
			}
			else if (windowCreateInfo.ScreenMode == ScreenMode.BorderlessWindow)
			{
				flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
			}

			if (windowCreateInfo.SystemResizable)
			{
				flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
			}

			if (windowCreateInfo.StartMaximized)
			{
				flags |= SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
			}

			ScreenMode = windowCreateInfo.ScreenMode;

			Handle = SDL.SDL_CreateWindow(
				windowCreateInfo.WindowTitle,
				SDL.SDL_WINDOWPOS_UNDEFINED,
				SDL.SDL_WINDOWPOS_UNDEFINED,
				(int) windowCreateInfo.WindowWidth,
				(int) windowCreateInfo.WindowHeight,
				flags
			);

			Width = windowCreateInfo.WindowWidth;
			Height = windowCreateInfo.WindowHeight;

			idLookup.Add(SDL.SDL_GetWindowID(Handle), this);
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

		internal static Window Lookup(uint windowID)
		{
			return idLookup.ContainsKey(windowID) ? idLookup[windowID] : null;
		}

		internal void Show()
		{
			SDL.SDL_ShowWindow(Handle);
		}

		internal void SizeChanged(uint width, uint height)
		{
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

				idLookup.Remove(SDL.SDL_GetWindowID(Handle));
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
