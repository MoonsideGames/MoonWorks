using System;
using System.Collections.Generic;
using MoonWorks.Graphics;
using SDL2;

namespace MoonWorks
{
	public class Window : IDisposable
	{
		internal IntPtr Handle { get; }
		public ScreenMode ScreenMode { get; private set; }
		public uint Width { get; private set; }
		public uint Height { get; private set; }
		internal Texture SwapchainTexture { get; set; } = null;

		public bool Claimed { get; internal set; }
		public MoonWorks.Graphics.TextureFormat SwapchainFormat { get; internal set; }

		private bool IsDisposed;

		private static Dictionary<uint, Window> idLookup = new Dictionary<uint, Window>();

		private System.Action<uint, uint> SizeChangeCallback = null;

		public Window(WindowCreateInfo windowCreateInfo, SDL.SDL_WindowFlags flags)
		{
			if (windowCreateInfo.ScreenMode == ScreenMode.Fullscreen)
			{
				flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
			}
			else if (windowCreateInfo.ScreenMode == ScreenMode.BorderlessFullscreen)
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

		public void SetScreenMode(ScreenMode screenMode)
		{
			SDL.SDL_WindowFlags windowFlag = 0;

			if (screenMode == ScreenMode.Fullscreen)
			{
				windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
			}
			else if (screenMode == ScreenMode.BorderlessFullscreen)
			{
				windowFlag = SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
			}

			SDL.SDL_SetWindowFullscreen(Handle, (uint) windowFlag);

			if (screenMode == ScreenMode.Windowed)
			{
				SDL.SDL_SetWindowPosition(Handle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
			}

			ScreenMode = screenMode;
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

			if (ScreenMode == ScreenMode.Windowed)
			{
				SDL.SDL_SetWindowPosition(Handle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
			}
		}

		internal static Window Lookup(uint windowID)
		{
			return idLookup.ContainsKey(windowID) ? idLookup[windowID] : null;
		}

		internal void Show()
		{
			SDL.SDL_ShowWindow(Handle);
		}

		internal void HandleSizeChange(uint width, uint height)
		{
			Width = width;
			Height = height;

			if (SizeChangeCallback != null)
			{
				SizeChangeCallback(width, height);
			}
		}

		public void RegisterSizeChangeCallback(System.Action<uint, uint> sizeChangeCallback)
		{
			SizeChangeCallback = sizeChangeCallback;
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
