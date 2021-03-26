using System.Collections.Generic;
using SDL2;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks.Window;
using System.Text;

namespace MoonWorks
{
    public abstract class Game
    {
        public const double MAX_DELTA_TIME = 0.1;

        private bool quit = false;
        private double timestep;
        ulong currentTime = SDL.SDL_GetPerformanceCounter();
        double accumulator = 0;
        bool debugMode;

        public OSWindow Window { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public AudioDevice AudioDevice { get; }
        public Inputs Inputs { get; }

        private Dictionary<PresentMode, RefreshCS.Refresh.PresentMode> moonWorksToRefreshPresentMode = new Dictionary<PresentMode, RefreshCS.Refresh.PresentMode>
        {
            { PresentMode.Immediate, RefreshCS.Refresh.PresentMode.Immediate },
            { PresentMode.Mailbox, RefreshCS.Refresh.PresentMode.Mailbox },
            { PresentMode.FIFO, RefreshCS.Refresh.PresentMode.FIFO },
            { PresentMode.FIFORelaxed, RefreshCS.Refresh.PresentMode.FIFORelaxed }
        };

        public Game(
            WindowCreateInfo windowCreateInfo,
            PresentMode presentMode,
            int targetTimestep = 60,
            bool debugMode = false
        ) {
            timestep = 1.0 / targetTimestep;

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_TIMER | SDL.SDL_INIT_GAMECONTROLLER) < 0)
            {
                System.Console.WriteLine("Failed to initialize SDL!");
                return;
            }

            Logger.Initialize();

            Inputs = new Inputs();

            Window = new OSWindow(windowCreateInfo);

            GraphicsDevice = new GraphicsDevice(
                Window.Handle,
                moonWorksToRefreshPresentMode[presentMode],
                debugMode
            );

            AudioDevice = new AudioDevice();

            this.debugMode = debugMode;
        }

        public void Run()
        {
            while (!quit)
            {
                var newTime = SDL.SDL_GetPerformanceCounter();
                double frameTime = (newTime - currentTime) / (double)SDL.SDL_GetPerformanceFrequency();

                if (frameTime > MAX_DELTA_TIME)
                {
                    frameTime = MAX_DELTA_TIME;
                }

                currentTime = newTime;

                accumulator += frameTime;

                if (!quit)
                {
                    while (accumulator >= timestep)
                    {
                        Inputs.Mouse.Wheel = 0;

                        HandleSDLEvents();

                        Inputs.Update();
                        AudioDevice.Update();

                        Update(timestep);

                        accumulator -= timestep;
                    }

                    double alpha = accumulator / timestep;

                    Draw(timestep, alpha);
                }
            }
        }

        private void HandleSDLEvents()
        {
            while (SDL.SDL_PollEvent(out var _event) == 1)
            {
                switch (_event.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        quit = true;
                        break;

                    case SDL.SDL_EventType.SDL_TEXTINPUT:
                        HandleTextInput(_event);
                        break;

                    case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                        Inputs.Mouse.Wheel += _event.wheel.y;
                        break;
                }
            }
        }

        protected abstract void Update(double dt);

        protected abstract void Draw(double dt, double alpha);

        private void HandleTextInput(SDL2.SDL.SDL_Event evt)
        {
            // Based on the SDL2# LPUtf8StrMarshaler
            unsafe
            {
                int bytes = MeasureStringLength(evt.text.text);
                if (bytes > 0)
                {
                    /* UTF8 will never encode more characters
                        * than bytes in a string, so bytes is a
                        * suitable upper estimate of size needed
                        */
                    char* charsBuffer = stackalloc char[bytes];
                    int chars = Encoding.UTF8.GetChars(
                        evt.text.text,
                        bytes,
                        charsBuffer,
                        bytes
                    );

                    for (int i = 0; i < chars; i += 1)
                    {
                        Inputs.OnTextInput(charsBuffer[i]);
                    }
                }
            }
        }

		private unsafe static int MeasureStringLength(byte* ptr)
		{
			int bytes;
			for (bytes = 0; *ptr != 0; ptr += 1, bytes += 1);
			return bytes;
		}
    }
}
