using System;
using SDL2;

namespace MoonWorks.Window
{
    public class OSWindow
    {
        internal IntPtr Handle { get; }
        public ScreenMode ScreenMode { get; }

        public OSWindow(WindowCreateInfo windowCreateInfo)
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

            ScreenMode = windowCreateInfo.ScreenMode;

            Handle = SDL.SDL_CreateWindow(
                windowCreateInfo.WindowTitle,
                SDL.SDL_WINDOWPOS_UNDEFINED,
                SDL.SDL_WINDOWPOS_UNDEFINED,
                (int)windowCreateInfo.WindowWidth,
                (int)windowCreateInfo.WindowHeight,
                windowFlags
            );
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

            SDL.SDL_SetWindowFullscreen(Handle, (uint) windowFlag);
        }
    }
}
