﻿using System;
using System.Collections.Generic;
using MoonWorks.Graphics;
using SDL3;

namespace MoonWorks
{
	/// <summary>
	/// Represents a window in the client operating system. <br/>
	/// Every Game has a MainWindow automatically. <br/>
	/// You can create additional Windows if you desire. They must be Claimed by the GraphicsDevice to be rendered to.
	/// </summary>
	public class Window : IDisposable
	{
		public IntPtr Handle { get; }
		public ScreenMode ScreenMode { get; private set; }
		public uint Width { get; private set; }
		public uint Height { get; private set; }
		internal Texture SwapchainTexture { get; set; }

		public bool Claimed { get; internal set; }
		public MoonWorks.Graphics.SwapchainComposition SwapchainComposition { get; internal set; }
		public MoonWorks.Graphics.TextureFormat SwapchainFormat { get; internal set; }

		public (int, int) Position
		{
			get
			{
				if (!SDL.SDL_GetWindowPosition(Handle, out var x, out var y))
				{
					Logger.LogError(SDL.SDL_GetError());
					return (0, 0);
				}

				return (x, y);
			}
		}

		public string Title { get; private set;}

		public bool RelativeMouseMode { get; private set; } = false;

		private bool IsDisposed;

		private static Dictionary<uint, Window> idLookup = new Dictionary<uint, Window>();

		private System.Action<uint, uint> SizeChangeCallback = null;

		public unsafe Window(WindowCreateInfo windowCreateInfo, SDL.SDL_WindowFlags flags)
		{
			if (windowCreateInfo.ScreenMode == ScreenMode.Fullscreen)
			{
				flags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
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

			var displayID = SDL.SDL_GetPrimaryDisplay();
			SDL.SDL_DisplayMode *displayMode = (SDL.SDL_DisplayMode*) SDL.SDL_GetCurrentDisplayMode(displayID);

			Handle = SDL.SDL_CreateWindow(
				windowCreateInfo.WindowTitle,
				windowCreateInfo.ScreenMode == ScreenMode.Windowed ? (int) windowCreateInfo.WindowWidth : displayMode->w,
				windowCreateInfo.ScreenMode == ScreenMode.Windowed ? (int) windowCreateInfo.WindowHeight : displayMode->h,
				flags
			);

			/* Requested size might be different in fullscreen, so let's just get the area */
			SDL.SDL_GetWindowSize(Handle, out var width, out var height);
			Width = (uint) width;
			Height = (uint) height;

			idLookup.Add(SDL.SDL_GetWindowID(Handle), this);
		}

		/// <summary>
		/// Changes the ScreenMode of this window.
		/// </summary>
		public unsafe void SetScreenMode(ScreenMode screenMode)
		{
			if (screenMode == ScreenMode.Fullscreen)
			{
				SDL.SDL_SetWindowFullscreen(Handle, true);
			}
			else
			{
				var displayID = SDL.SDL_GetDisplayForWindow(Handle);
				SDL.SDL_DisplayMode *displayMode = (SDL.SDL_DisplayMode*) SDL.SDL_GetCurrentDisplayMode(displayID);
				SDL.SDL_SetWindowPosition(Handle, displayMode->w / 2, displayMode->h / 2);
			}

			ScreenMode = screenMode;
		}

		/// <summary>
		/// Resizes the window. <br/>
		/// Note that you are responsible for recreating any graphics resources that need to change as a result of the size change.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public unsafe void SetSize(uint width, uint height)
		{
			SDL.SDL_SetWindowSize(Handle, (int) width, (int) height);
			Width = width;
			Height = height;

			if (ScreenMode == ScreenMode.Windowed)
			{
				var displayID = SDL.SDL_GetDisplayForWindow(Handle);
				SDL.SDL_DisplayMode *displayMode = (SDL.SDL_DisplayMode*) SDL.SDL_GetCurrentDisplayMode(displayID);
				SDL.SDL_SetWindowPosition(Handle, displayMode->w / 2, displayMode->h / 2);
			}

		}

		/// <summary>
		/// Sets the window position.
		/// </summary>
		public void SetPosition(int x, int y)
		{
			SDL.SDL_SetWindowPosition(Handle, x, y);
		}

		/// <summary>
		/// Sets the window position to the center of the display.
		/// </summary>
		public void SetPositionCentered()
		{
			var display = SDL.SDL_GetDisplayForWindow(Handle);
			SDL.SDL_GetDisplayUsableBounds(display, out var rect);
			SetPosition((rect.x + rect.w - (int) Width) / 2, (rect.y + rect.h - (int) Height) / 2);
		}

		/// <summary>
		/// Sets the window title.
		/// </summary>
		public void SetTitle(string title)
		{
			if (!SDL.SDL_SetWindowTitle(Handle, title))
			{
				Logger.LogError(SDL.SDL_GetError());
				return;
			}
			Title = title;
		}

		/// <summary>
		/// If set to true, the cursor is hidden, the mouse position is constrained to the window,
		/// and relative mouse motion will be reported even if the mouse is at the edge of the window.
		/// </summary>
		public void SetRelativeMouseMode(bool enabled)
		{
			if (!SDL.SDL_SetWindowRelativeMouseMode(Handle, enabled))
			{
				Logger.LogError(SDL.SDL_GetError());
				return;
			}

			RelativeMouseMode = enabled;
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

		/// <summary>
		/// You can specify a method to run when the window size changes.
		/// </summary>
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
