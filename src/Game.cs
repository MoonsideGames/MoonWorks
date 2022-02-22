using System.Collections.Generic;
using SDL2;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks.Window;
using System.Text;
using System;
using System.Diagnostics;

namespace MoonWorks
{
    public abstract class Game
    {
        public TimeSpan MAX_DELTA_TIME = TimeSpan.FromMilliseconds(100);

        private bool quit = false;
        bool debugMode;

		private Stopwatch gameTimer;
        private TimeSpan timestep;
		private long previousTicks = 0;
        TimeSpan accumulatedElapsedTime = TimeSpan.Zero;
		// must be a power of 2 so we can do a bitmask optimization when checking worst case
		private const int PREVIOUS_SLEEP_TIME_COUNT = 128;
		private const int SLEEP_TIME_MASK = PREVIOUS_SLEEP_TIME_COUNT - 1;
		private TimeSpan[] previousSleepTimes = new TimeSpan[PREVIOUS_SLEEP_TIME_COUNT];
		private int sleepTimeIndex = 0;
		private TimeSpan worstCaseSleepPrecision = TimeSpan.FromMilliseconds(1);

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
            timestep = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / targetTimestep);
			gameTimer = Stopwatch.StartNew();

			for (int i = 0; i < previousSleepTimes.Length; i += 1)
			{
				previousSleepTimes[i] = TimeSpan.FromMilliseconds(1);
			}

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
				AdvanceElapsedTime();

				/* We want to wait until the next frame,
				 * but we don't want to oversleep. Requesting repeated 1ms sleeps and
				 * seeing how long we actually slept for lets us estimate the worst case
				 * sleep precision so we don't oversleep the next frame.
				 */
				while (accumulatedElapsedTime + worstCaseSleepPrecision < timestep)
				{
					System.Threading.Thread.Sleep(1);
					TimeSpan timeAdvancedSinceSleeping = AdvanceElapsedTime();
					UpdateEstimatedSleepPrecision(timeAdvancedSinceSleeping);
				}

				/* Now that we have slept into the sleep precision threshold, we need to wait
				 * for just a little bit longer until the target elapsed time has been reached.
				 * SpinWait(1) works by pausing the thread for very short intervals, so it is
				 * an efficient and time-accurate way to wait out the rest of the time.
				 */
				while (accumulatedElapsedTime < timestep)
				{
					System.Threading.Thread.SpinWait(1);
					AdvanceElapsedTime();
				}

				// Now that we are going to perform an update, let's handle SDL events.
				HandleSDLEvents();

				// Do not let any step take longer than our maximum.
                if (accumulatedElapsedTime > MAX_DELTA_TIME)
                {
                    accumulatedElapsedTime = MAX_DELTA_TIME;
                }

                if (!quit)
                {
                    while (accumulatedElapsedTime >= timestep)
                    {
                        Inputs.Mouse.Wheel = 0;

                        Inputs.Update();
                        AudioDevice.Update();

                        Update(timestep);

                        accumulatedElapsedTime -= timestep;
                    }

                    var alpha = accumulatedElapsedTime / timestep;

                    Draw(timestep, alpha);

                }

				GraphicsDevice.SubmitDestroyCommandBuffer();
            }

			GraphicsDevice.Dispose();
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

        protected abstract void Update(TimeSpan dt);

		// alpha refers to a percentage value between the current and next state
        protected abstract void Draw(TimeSpan dt, double alpha);

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

		private TimeSpan AdvanceElapsedTime()
		{
			long currentTicks = gameTimer.Elapsed.Ticks;
			TimeSpan timeAdvanced = TimeSpan.FromTicks(currentTicks - previousTicks);
			accumulatedElapsedTime += timeAdvanced;
			previousTicks = currentTicks;
			return timeAdvanced;
		}

		/* To calculate the sleep precision of the OS, we take the worst case
		 * time spent sleeping over the results of previous requests to sleep 1ms.
		 */
		private void UpdateEstimatedSleepPrecision(TimeSpan timeSpentSleeping)
		{
			/* It is unlikely that the scheduler will actually be more imprecise than
			 * 4ms and we don't want to get wrecked by a single long sleep so we cap this
			 * value at 4ms for sanity.
			 */
			var upperTimeBound = TimeSpan.FromMilliseconds(4);

			if (timeSpentSleeping > upperTimeBound)
			{
				timeSpentSleeping = upperTimeBound;
			}

			/* We know the previous worst case - it's saved in worstCaseSleepPrecision.
			 * We also know the current index. So the only way the worst case changes
			 * is if we either 1) just got a new worst case, or 2) the worst case was
			 * the oldest entry on the list.
			 */
			if (timeSpentSleeping >= worstCaseSleepPrecision)
			{
				worstCaseSleepPrecision = timeSpentSleeping;
			}
			else if (previousSleepTimes[sleepTimeIndex] == worstCaseSleepPrecision)
			{
				var maxSleepTime = TimeSpan.MinValue;
				for (int i = 0; i < previousSleepTimes.Length; i++)
				{
					if (previousSleepTimes[i] > maxSleepTime)
					{
						maxSleepTime = previousSleepTimes[i];
					}
				}
				worstCaseSleepPrecision = maxSleepTime;
			}

			previousSleepTimes[sleepTimeIndex] = timeSpentSleeping;
			sleepTimeIndex = (sleepTimeIndex + 1) & SLEEP_TIME_MASK;
		}

		private unsafe static int MeasureStringLength(byte* ptr)
		{
			int bytes;
			for (bytes = 0; *ptr != 0; ptr += 1, bytes += 1);
			return bytes;
		}
    }
}
